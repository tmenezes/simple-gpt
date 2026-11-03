namespace SimpleGPT.Core;

/// <summary>The result of a decision question, including a possible refusal.</summary>
/// <typeparam name="TAnswer">The answer type for the question.</typeparam>
public sealed class DecisionResult<TAnswer> where TAnswer : DecisionAnswer
{
    internal DecisionResult(TAnswer? answer, DecisionRefusal? refusal)
    {
        Answer = answer;
        Refusal = refusal;
    }

    /// <summary>Gets the answer, or null when the question was refused.</summary>
    public TAnswer? Answer { get; }
    /// <summary>Gets the refusal, or null when the question was answered.</summary>
    public DecisionRefusal? Refusal { get; }
    /// <summary>Gets whether the question was refused.</summary>
    public bool IsRefused => Refusal != null;
}

/// <summary>A named decision answer.</summary>
public abstract record DecisionAnswer(string? Name);

/// <summary>A predicate answer and its estimated probability.</summary>
/// <param name="Name">The question name.</param>
/// <param name="Probability">The probability that the condition is true.</param>
public sealed record PredicateAnswer(string? Name, double Probability) : DecisionAnswer(Name);

/// <summary>A probability assigned to a choice.</summary>
/// <param name="Value">The choice value.</param>
/// <param name="Probability">The probability assigned to the choice.</param>
public sealed record ChoiceProbability(DecisionChoiceValue Value, double Probability);

/// <summary>A choice answer with confidence and probabilities for each choice.</summary>
/// <param name="Name">The question name.</param>
/// <param name="Choice">The selected choice.</param>
/// <param name="Confidence">The confidence reported by the API.</param>
/// <param name="Probabilities">The choice probability distribution.</param>
public sealed record ChoiceAnswer(string? Name, DecisionChoiceValue Choice, double Confidence, IReadOnlyList<ChoiceProbability> Probabilities) : DecisionAnswer(Name);

/// <summary>A probability assigned to an ordered score level.</summary>
/// <param name="Value">The zero-based level index.</param>
/// <param name="Label">The level label.</param>
/// <param name="Probability">The probability assigned to the level.</param>
public sealed record ScoreProbability(long Value, string Label, double Probability);

/// <summary>A score answer with confidence and probabilities for each level.</summary>
/// <param name="Name">The question name.</param>
/// <param name="Score">The probability-weighted average of the level indices.</param>
/// <param name="Confidence">The confidence reported by the API.</param>
/// <param name="Probabilities">The level probability distribution.</param>
public sealed record ScoreAnswer(string? Name, double Score, double Confidence, IReadOnlyList<ScoreProbability> Probabilities) : DecisionAnswer(Name);

/// <summary>A refusal to answer a named decision question.</summary>
/// <param name="Name">The question name.</param>
public sealed record DecisionRefusal(string? Name) : DecisionAnswer(Name);