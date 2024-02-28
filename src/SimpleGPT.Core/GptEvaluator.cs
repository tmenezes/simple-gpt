using System.Text;
using SimpleGPT.Core.Clients;

namespace SimpleGPT.Core;

public interface IGptEvaluator
{
    Task<string> Evaluate(string question);
}

public class GptEvaluator : IGptEvaluator
{
    private readonly IGptClient _gptClient;
    private readonly string    _presetInstructions = "You're a helper tool that evaluates and answers questions based on knowledge and context. Use kind language and assistant tone.";
    private readonly string    _knowledgeBase;
    private readonly string    _context;

    public GptEvaluator(IGptClient gptClient, string knowledgeBase, string context)
    {
        _gptClient = gptClient;
        _knowledgeBase = knowledgeBase;
        _context = context;
    }

    public GptEvaluator(IGptClient gptClient, string presetInstructions, string knowledgeBase, string context) 
        : this(gptClient, knowledgeBase, context)
    {
        _presetInstructions = presetInstructions;
    }


    public async Task<string> Evaluate(string question)
    {
        var sb = new StringBuilder();
        sb.Append(_presetInstructions);
        sb.Append($"Given the following knowledge base: {_knowledgeBase}.");
        sb.Append($"On this context: {_context}.");
        sb.Append($"Evaluate this: {question}");

        return await _gptClient.CallGpt(sb.ToString());
    }
}