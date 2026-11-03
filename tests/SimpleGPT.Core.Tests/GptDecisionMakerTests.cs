using System.Net;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimpleGPT.Core.Tests;

public class GptDecisionMakerTests
{
    [Fact]
    public async Task EvaluatePredicate_SendsSharedInputAndReturnsProbability()
    {
        var handler = new StubHandler("{\"answers\":[{\"type\":\"predicate\",\"name\":\"damaged\",\"probability\":0.92}]}");
        using var client = new HttpClient(handler);
        var maker = new GptDecisionMaker(client, "test-key", Shared.ModelNames.Gpt6Luna, "custom instructions", "knowledge", "context");

        var result = await maker.Evaluate(new PredicateQuestion("damaged", "Is it damaged?"));

        result.IsRefused.Should().BeFalse();
        result.Answer.Should().Be(new PredicateAnswer("damaged", 0.92));
        handler.Method.Should().Be(HttpMethod.Post);
        handler.Uri.Should().Be(new Uri("https://api.openai.com/v1/decisions"));
        handler.Authorization.Should().Be("Bearer test-key");
        handler.ContentType.Should().Be("application/json");
        handler.Body!["model"]!.Value<string>().Should().Be(Shared.ModelNames.Gpt6Luna);
        var input = handler.Body["input"]!.Value<string>()!;
        input.Should().Contain("custom instructions").And.Contain("knowledge").And.Contain("context");
        var question = handler.Body["questions"]!.Single();
        question["type"]!.Value<string>().Should().Be("predicate");
        question["name"]!.Value<string>().Should().Be("damaged");
        question["instructions"]!.Value<string>().Should().Be("Is it damaged?");
    }

    [Fact]
    public async Task EvaluateChoice_PreservesStringAndBooleanValuesAndProbabilities()
    {
        var handler = new StubHandler("{\"answers\":[{\"type\":\"choice\",\"name\":\"route\",\"choice\":true,\"confidence\":0.8,\"probabilities\":[{\"value\":true,\"probability\":0.9},{\"value\":\"true\",\"probability\":0.1}]}]}");
        using var client = new HttpClient(handler);
        var maker = new GptDecisionMaker(client, "test-key", "knowledge", "context");
        var question = new ChoiceQuestion("route", "Choose a route", new[] { new DecisionChoice(true, "Boolean"), new DecisionChoice("true") });

        var result = await maker.Evaluate(question);

        result.Answer!.Choice.Value.Should().Be(true);
        result.Answer.Confidence.Should().Be(0.8);
        result.Answer.Probabilities.Should().Equal(new ChoiceProbability(true, 0.9), new ChoiceProbability("true", 0.1));
        var choices = handler.Body!["questions"]![0]!["choices"]!;
        choices[0]!["value"]!.Type.Should().Be(JTokenType.Boolean);
        choices[0]!["description"]!.Value<string>().Should().Be("Boolean");
        choices[1]!["value"]!.Type.Should().Be(JTokenType.String);
        choices[1]!["description"].Should().BeNull();
    }

    [Fact]
    public async Task EvaluateScore_SendsOrderedLevelsAndReturnsDistribution()
    {
        var handler = new StubHandler("{\"answers\":[{\"type\":\"score\",\"name\":\"severity\",\"score\":0.7,\"confidence\":0.6,\"probabilities\":[{\"value\":0,\"label\":\"low\",\"probability\":0.3},{\"value\":1,\"label\":\"high\",\"probability\":0.7}]}]}");
        using var client = new HttpClient(handler);
        var maker = new GptDecisionMaker(client, "test-key", "knowledge", "context");
        var question = new ScoreQuestion("severity", "How severe?", new[] { new DecisionScoreLevel("low"), new DecisionScoreLevel("high", "Blocked") });

        var result = await maker.Evaluate(question);

        result.Answer!.Score.Should().Be(0.7);
        result.Answer.Confidence.Should().Be(0.6);
        result.Answer.Probabilities.Should().Equal(new ScoreProbability(0, "low", 0.3), new ScoreProbability(1, "high", 0.7));
        var levels = handler.Body!["questions"]![0]!["levels"]!;
        levels[0]!["label"]!.Value<string>().Should().Be("low");
        levels[1]!["label"]!.Value<string>().Should().Be("high");
        levels[1]!["description"]!.Value<string>().Should().Be("Blocked");
    }

    [Fact]
    public async Task Evaluate_ReturnsRefusalWithoutAnAnswer()
    {
        var handler = new StubHandler("{\"answers\":[{\"type\":\"refusal\",\"name\":\"question\"}]}");
        using var client = new HttpClient(handler);
        var maker = new GptDecisionMaker(client, "test-key", "", "");

        var result = await maker.Evaluate(new PredicateQuestion("question", "Evaluate"));

        result.IsRefused.Should().BeTrue();
        result.Answer.Should().BeNull();
        result.Refusal.Should().Be(new DecisionRefusal("question"));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"answers\":[]}")]
    [InlineData("{\"answers\":[{\"type\":\"predicate\",\"name\":\"wrong\",\"probability\":0.5}]}")]
    [InlineData("{\"answers\":[{\"type\":\"choice\",\"name\":\"q\"}]}")]
    [InlineData("{\"answers\":[{\"type\":\"predicate\",\"name\":\"q\"}]}")]
    [InlineData("{\"answers\":[{\"type\":\"predicate\",\"name\":\"q\",\"probability\":\"0.5\"}]}")]
    public async Task Evaluate_RejectsMalformedOrUnexpectedAnswers(string json)
    {
        using var client = new HttpClient(new StubHandler(json));
        var maker = new GptDecisionMaker(client, "test-key", "", "");

        Func<Task> act = async () => await maker.Evaluate(new PredicateQuestion("q", "Evaluate"));

        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task Evaluate_PropagatesHttpFailure()
    {
        using var client = new HttpClient(new StubHandler("{}", HttpStatusCode.Unauthorized));
        var maker = new GptDecisionMaker(client, "test-key", "", "");

        Func<Task> act = async () => await maker.Evaluate(new PredicateQuestion("q", "Evaluate"));

        (await act.Should().ThrowAsync<HttpRequestException>()).Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("{\"answers\":[{\"type\":\"choice\",\"name\":\"q\",\"choice\":1,\"confidence\":0.5,\"probabilities\":[]}]}")]
    [InlineData("{\"answers\":[{\"type\":\"choice\",\"name\":\"q\",\"choice\":true,\"confidence\":0.5,\"probabilities\":[{\"value\":true}]}]}")]
    public async Task EvaluateChoice_RejectsInvalidChoiceAnswers(string json)
    {
        using var client = new HttpClient(new StubHandler(json));
        var maker = new GptDecisionMaker(client, "test-key", "", "");
        var question = new ChoiceQuestion("q", "Choose", new[] { new DecisionChoice(true), new DecisionChoice(false) });

        Func<Task> act = async () => await maker.Evaluate(question);

        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task EvaluateScore_RejectsNonIntegerLevelIndex()
    {
        using var client = new HttpClient(new StubHandler("{\"answers\":[{\"type\":\"score\",\"name\":\"q\",\"score\":0.5,\"confidence\":0.5,\"probabilities\":[{\"value\":0.5,\"label\":\"low\",\"probability\":0.5}]}]}"));
        var maker = new GptDecisionMaker(client, "test-key", "", "");
        var question = new ScoreQuestion("q", "Score", new[] { new DecisionScoreLevel("low"), new DecisionScoreLevel("high") });

        Func<Task> act = async () => await maker.Evaluate(question);

        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task Evaluate_RespectsCancellation()
    {
        using var client = new HttpClient(new StubHandler("{}"));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var maker = new GptDecisionMaker(client, "test-key", "", "");

        Func<Task> act = async () => await maker.Evaluate(new PredicateQuestion("q", "Evaluate"), cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void Questions_ValidateRequiredFieldsAndChoiceLimits()
    {
        Action missingName = () => new PredicateQuestion("", "Evaluate");
        Action missingInstructions = () => new PredicateQuestion("q", "");
        Action oneChoice = () => new ChoiceQuestion("q", "Choose", new[] { new DecisionChoice("a") });
        Action duplicateChoices = () => new ChoiceQuestion("q", "Choose", new[] { new DecisionChoice("a"), new DecisionChoice("a") });
        Action tooManyChoices = () => new ChoiceQuestion("q", "Choose", Enumerable.Range(0, 256).Select(i => new DecisionChoice(i.ToString())));
        Action oneLevel = () => new ScoreQuestion("q", "Score", new[] { new DecisionScoreLevel("low") });
        Action tooManyLevels = () => new ScoreQuestion("q", "Score", Enumerable.Range(0, 11).Select(i => new DecisionScoreLevel(i.ToString())));

        missingName.Should().Throw<ArgumentException>();
        missingInstructions.Should().Throw<ArgumentException>();
        oneChoice.Should().Throw<ArgumentException>();
        duplicateChoices.Should().Throw<ArgumentException>();
        tooManyChoices.Should().Throw<ArgumentException>();
        oneLevel.Should().Throw<ArgumentException>();
        tooManyLevels.Should().Throw<ArgumentException>();
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly string _response;
        private readonly HttpStatusCode _status;

        public StubHandler(string response, HttpStatusCode status = HttpStatusCode.OK)
        {
            _response = response;
            _status = status;
        }

        public HttpMethod? Method { get; private set; }
        public Uri? Uri { get; private set; }
        public string? Authorization { get; private set; }
        public string? ContentType { get; private set; }
        public JObject? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Method = request.Method;
            Uri = request.RequestUri;
            Authorization = request.Headers.Authorization?.ToString();
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            Body = JObject.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            return new HttpResponseMessage(_status) { Content = new StringContent(_response, Encoding.UTF8, "application/json") };
        }
    }
}