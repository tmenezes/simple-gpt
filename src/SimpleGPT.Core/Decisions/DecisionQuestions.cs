namespace SimpleGPT.Core;

/// <summary>A named question evaluated against shared decision input.</summary>
public abstract class DecisionQuestion
{
    /// <summary>Initializes a named question.</summary>
    /// <param name="name">The name used to identify the answer.</param>
    /// <param name="instructions">The question to evaluate.</param>
    protected DecisionQuestion(string name, string instructions)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("A question name is required.", nameof(name)) : name;
        Instructions = string.IsNullOrWhiteSpace(instructions) ? throw new ArgumentException("Question instructions are required.", nameof(instructions)) : instructions;
    }

    /// <summary>Gets the name used to identify the answer.</summary>
    public string Name { get; }
    /// <summary>Gets the question instructions.</summary>
    public string Instructions { get; }
}

/// <summary>A question about whether a condition is true.</summary>
public sealed class PredicateQuestion : DecisionQuestion
{
    /// <summary>Initializes a predicate question.</summary>
    /// <param name="name">The name used to identify the answer.</param>
    /// <param name="instructions">The condition to evaluate.</param>
    public PredicateQuestion(string name, string instructions) : base(name, instructions) { }
}

/// <summary>A string or Boolean value available to a choice question.</summary>
public sealed record DecisionChoiceValue
{
    /// <summary>Initializes a string choice value.</summary>
    /// <param name="value">The string value.</param>
    public DecisionChoiceValue(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary>Initializes a Boolean choice value.</summary>
    /// <param name="value">The Boolean value.</param>
    public DecisionChoiceValue(bool value) => Value = value;

    /// <summary>Gets the string or Boolean value.</summary>
    public object Value { get; }

    /// <summary>Converts a string to a choice value.</summary>
    /// <param name="value">The string value.</param>
    public static implicit operator DecisionChoiceValue(string value) => new(value);

    /// <summary>Converts a Boolean to a choice value.</summary>
    /// <param name="value">The Boolean value.</param>
    public static implicit operator DecisionChoiceValue(bool value) => new(value);
}

/// <summary>An available choice and its optional description.</summary>
public sealed class DecisionChoice
{
    /// <summary>Initializes a choice.</summary>
    /// <param name="value">The string or Boolean choice value.</param>
    /// <param name="description">An optional description of the choice.</param>
    public DecisionChoice(DecisionChoiceValue value, string? description = null)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Description = description;
    }

    /// <summary>Gets the choice value.</summary>
    public DecisionChoiceValue Value { get; }
    /// <summary>Gets the optional description.</summary>
    public string? Description { get; }
}

/// <summary>A question selecting one of a fixed set of choices.</summary>
public sealed class ChoiceQuestion : DecisionQuestion
{
    /// <summary>Initializes a choice question.</summary>
    /// <param name="name">The name used to identify the answer.</param>
    /// <param name="instructions">The question to evaluate.</param>
    /// <param name="choices">Between 2 and 255 choices with unique values.</param>
    public ChoiceQuestion(string name, string instructions, IEnumerable<DecisionChoice> choices) : base(name, instructions)
    {
        ArgumentNullException.ThrowIfNull(choices);
        var items = choices.ToArray();
        if (items.Length < 2 || items.Length > 255 || items.Any(item => item == null) || items.Select(item => item.Value).Distinct().Count() != items.Length)
            throw new ArgumentException("Provide between 2 and 255 choices with unique values.", nameof(choices));
        Choices = Array.AsReadOnly(items);
    }

    /// <summary>Gets the available choices.</summary>
    public IReadOnlyList<DecisionChoice> Choices { get; }
}

/// <summary>An ordered scoring level and its optional description.</summary>
public sealed class DecisionScoreLevel
{
    /// <summary>Initializes a scoring level.</summary>
    /// <param name="label">The level label.</param>
    /// <param name="description">An optional description of the level.</param>
    public DecisionScoreLevel(string label, string? description = null)
    {
        Label = label ?? throw new ArgumentNullException(nameof(label));
        Description = description;
    }

    /// <summary>Gets the level label.</summary>
    public string Label { get; }
    /// <summary>Gets the optional description.</summary>
    public string? Description { get; }
}

/// <summary>A question rated against ordered levels.</summary>
public sealed class ScoreQuestion : DecisionQuestion
{
    /// <summary>Initializes a score question.</summary>
    /// <param name="name">The name used to identify the answer.</param>
    /// <param name="instructions">The question to evaluate.</param>
    /// <param name="levels">Between 2 and 10 ordered levels.</param>
    public ScoreQuestion(string name, string instructions, IEnumerable<DecisionScoreLevel> levels) : base(name, instructions)
    {
        ArgumentNullException.ThrowIfNull(levels);
        var items = levels.ToArray();
        if (items.Length < 2 || items.Length > 10 || items.Any(item => item == null))
            throw new ArgumentException("Provide between 2 and 10 levels.", nameof(levels));
        Levels = Array.AsReadOnly(items);
    }

    /// <summary>Gets the ordered scoring levels.</summary>
    public IReadOnlyList<DecisionScoreLevel> Levels { get; }
}