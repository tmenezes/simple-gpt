using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimpleGPT.Core;

/// <summary>Evaluates typed questions with the OpenAI Decisions API.</summary>
public interface IGptDecisionMaker
{
    /// <summary>Evaluates a predicate question.</summary>
    /// <param name="question">The question to evaluate.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The predicate answer or a refusal.</returns>
    Task<DecisionResult<PredicateAnswer>> Evaluate(PredicateQuestion question, CancellationToken cancellationToken = default);

    /// <summary>Evaluates a choice question.</summary>
    /// <param name="question">The question to evaluate.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The choice answer or a refusal.</returns>
    Task<DecisionResult<ChoiceAnswer>> Evaluate(ChoiceQuestion question, CancellationToken cancellationToken = default);

    /// <summary>Evaluates a score question.</summary>
    /// <param name="question">The question to evaluate.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The score answer or a refusal.</returns>
    Task<DecisionResult<ScoreAnswer>> Evaluate(ScoreQuestion question, CancellationToken cancellationToken = default);
}

/// <summary>Evaluates questions against shared knowledge and context using the Decisions API.</summary>
public sealed class GptDecisionMaker : IGptDecisionMaker
{
    private const string DefaultInstructions = "You're a helper tool that evaluates questions based on knowledge and context.";

    private readonly HttpClient _httpClient;
    private readonly string     _apiKey;
    private readonly string     _input;
    private readonly JToken     _model;

    /// <summary>Initializes a decision maker with default instructions.</summary>
    /// <param name="httpClient">The HTTP client used to send requests; ownership remains with the caller.</param>
    /// <param name="apiKey">The OpenAI API key.</param>
    /// <param name="knowledgeBase">The shared knowledge used to evaluate questions.</param>
    /// <param name="context">The shared context used to evaluate questions.</param>
    public GptDecisionMaker(HttpClient httpClient, string apiKey, string knowledgeBase, string context)
        : this(httpClient, apiKey, Shared.ModelNames.Gpt6Luna, knowledgeBase, context) { }

    /// <summary>Initializes a decision maker with default instructions.</summary>
    /// <param name="httpClient">The HTTP client used to send requests; ownership remains with the caller.</param>
    /// <param name="apiKey">The OpenAI API key.</param>
    /// <param name="model">The model to use for the OpenAI API.</param>
    /// <param name="knowledgeBase">The shared knowledge used to evaluate questions.</param>
    /// <param name="context">The shared context used to evaluate questions.</param>
    public GptDecisionMaker(HttpClient httpClient, string apiKey, string model, string knowledgeBase, string context)
        : this(httpClient, apiKey, model, DefaultInstructions, knowledgeBase, context) { }

    /// <summary>Initializes a decision maker with custom instructions.</summary>
    /// <param name="httpClient">The HTTP client used to send requests; ownership remains with the caller.</param>
    /// <param name="apiKey">The OpenAI API key.</param>
    /// <param name="model">The model to use for the OpenAI API.</param>
    /// <param name="presetInstructions">The instructions shared by all questions.</param>
    /// <param name="knowledgeBase">The shared knowledge used to evaluate questions.</param>
    /// <param name="context">The shared context used to evaluate questions.</param>
    public GptDecisionMaker(HttpClient httpClient, string apiKey, string model, string presetInstructions, string knowledgeBase, string context)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = string.IsNullOrWhiteSpace(apiKey) ? throw new ArgumentException("An API key is required.", nameof(apiKey)) : apiKey;
        _model = string.IsNullOrWhiteSpace(model) ? throw new ArgumentException("A model is required.", nameof(model)) : model;

        ArgumentNullException.ThrowIfNull(presetInstructions);
        ArgumentNullException.ThrowIfNull(knowledgeBase);
        ArgumentNullException.ThrowIfNull(context);

        _input = $"{presetInstructions}\nKnowledge base: {knowledgeBase}\nContext: {context}";
    }

    /// <inheritdoc />
    public Task<DecisionResult<PredicateAnswer>> Evaluate(PredicateQuestion question, CancellationToken cancellationToken = default) =>
        EvaluateCore(question, "predicate", answer => new PredicateAnswer(ReadName(answer), ReadNumber(answer, "probability")), cancellationToken);

    /// <inheritdoc />
    public Task<DecisionResult<ChoiceAnswer>> Evaluate(ChoiceQuestion question, CancellationToken cancellationToken = default) =>
        EvaluateCore(question, "choice", answer => new ChoiceAnswer(
            ReadName(answer), ReadChoiceValue(answer["choice"]), ReadNumber(answer, "confidence"),
            ReadProbabilities(answer).Select(item => new ChoiceProbability(ReadChoiceValue(item["value"]), ReadNumber(item, "probability"))).ToArray()), cancellationToken);

    /// <inheritdoc />
    public Task<DecisionResult<ScoreAnswer>> Evaluate(ScoreQuestion question, CancellationToken cancellationToken = default) =>
        EvaluateCore(question, "score", answer => new ScoreAnswer(
            ReadName(answer), ReadNumber(answer, "score"), ReadNumber(answer, "confidence"),
            ReadProbabilities(answer).Select(item => new ScoreProbability(ReadInteger(item, "value"), ReadString(item, "label"), ReadNumber(item, "probability"))).ToArray()), cancellationToken);

    // private methods
    private async Task<DecisionResult<TAnswer>> EvaluateCore<TAnswer>(DecisionQuestion question, string type, Func<JObject, TAnswer> parseAnswer, CancellationToken cancellationToken) where TAnswer : DecisionAnswer
    {
        ArgumentNullException.ThrowIfNull(question);
        var questionJson = new JObject
        {
            ["type"] = type,
            ["name"] = question.Name,
            ["instructions"] = question.Instructions
        };
        
        if (question is ChoiceQuestion choice)
            questionJson["choices"] = new JArray(choice.Choices.Select(item => CreateOption("value", JToken.FromObject(item.Value.Value), item.Description)));
        
        if (question is ScoreQuestion score)
            questionJson["levels"] = new JArray(score.Levels.Select(item => CreateOption("label", new JValue(item.Label), item.Description)));

        var body = new JObject
        {
            ["model"] = _model,
            ["input"] = _input,
            ["questions"] = new JArray(questionJson)
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{Shared.OpenAiBaseUrl}/v1/decisions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");
        
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var json = JObject.Parse(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        if (json["answers"] is not JArray answers || answers.Count != 1 || answers[0] is not JObject answer)
            throw new JsonException("The Decisions response must contain exactly one answer.");
        
        var name = ReadName(answer);
        if (name != question.Name)
            throw new JsonException("The Decisions answer name does not match the question.");
        
        var answerType = ReadString(answer, "type");
        if (answerType == "refusal")
            return new DecisionResult<TAnswer>(null, new DecisionRefusal(name));
        
        if (answerType != type)
            throw new JsonException($"Unexpected Decisions answer type: {answerType}.");
        
        return new DecisionResult<TAnswer>(parseAnswer(answer), null);
    }

    private static JObject CreateOption(string property, JToken value, string? description)
    {
        var option = new JObject { [property] = value };
        if (description != null)
            option["description"] = description;
        
        return option;
    }

    private static string? ReadName(JObject answer)
    {
        if (answer["name"] is not JToken name || (name.Type != JTokenType.String && name.Type != JTokenType.Null))
            throw new JsonException("The Decisions answer has an invalid name.");
       
        return name.Type == JTokenType.Null ? null : name.Value<string>();
    }

    private static string ReadString(JObject value, string property)
        => value[property]?.Type == JTokenType.String 
            ? value[property]!.Value<string>()! 
            : throw new JsonException($"Invalid Decisions field: {property}.");

    private static double ReadNumber(JObject value, string property)
        => value[property]?.Type is JTokenType.Integer or JTokenType.Float
            ? value[property]!.Value<double>()
            : throw new JsonException($"Invalid Decisions field: {property}.");

    private static long ReadInteger(JObject value, string property)
        => value[property]?.Type == JTokenType.Integer 
            ? value[property]!.Value<long>() 
            : throw new JsonException($"Invalid Decisions field: {property}.");

    private static DecisionChoiceValue ReadChoiceValue(JToken? value) => value?.Type switch
    {
        JTokenType.String => new DecisionChoiceValue(value.Value<string>()!),
        JTokenType.Boolean => new DecisionChoiceValue(value.Value<bool>()),
        _ => throw new JsonException("Invalid Decisions choice value.")
    };

    private static IEnumerable<JObject> ReadProbabilities(JObject answer)
    {
        if (answer["probabilities"] is not JArray probabilities || probabilities.Any(item => item is not JObject))
            throw new JsonException("Invalid Decisions probabilities.");
        
        return probabilities.Cast<JObject>();
    }
}