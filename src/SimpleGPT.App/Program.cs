using SimpleGPT.Core;

namespace SimpleGPT;

internal class Program
{
    private const string GPT_API_KEY = "<api-key-here>";

    static async Task Main(string[] args)
    {
        await RunDecisionMaker();
    }

    private static async Task RunDecisionMaker()
    {
        Console.WriteLine("Brazilian football decision examples:");
        Console.WriteLine("1. Predicate - did Flamengo win?");
        Console.WriteLine("2. Choice - which team scored the winning goal?");
        Console.WriteLine("3. Score - how exciting was the match?");
        Console.Write("Choose 1, 2, or 3: ");
        var selection = Console.ReadLine();
        if (selection != "1" && selection != "2" && selection != "3")
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        //var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        //if (string.IsNullOrWhiteSpace(apiKey))
        //{
        //    Console.WriteLine("Set the OPENAI_API_KEY environment variable first.");
        //    return;
        //}

        var knowledgeBase = "In football, the team with more goals wins the match.";
        var context = "In a fictional Campeonato Brasileiro match, Flamengo beat Palmeiras 2-1 with a winning goal in the final minute.";
        using var httpClient = new HttpClient();
        var decisionMaker = new GptDecisionMaker(httpClient, GPT_API_KEY, Shared.ModelNames.Gpt6Luna, knowledgeBase, context);

        ConsoleLog("Context: ", context, ConsoleColor.Cyan);
        switch (selection)
        {
            case "1":
                var predicate = await decisionMaker.Evaluate(new PredicateQuestion("flamengo_win", "Did Flamengo win the match?"));
                ConsoleLog("Result: ", predicate.IsRefused ? "Refused" : $"Win probability: {predicate.Answer!.Probability:P1}", ConsoleColor.Yellow);
                break;
            case "2":
                var choice = await decisionMaker.Evaluate(new ChoiceQuestion("winning_team", "Which team scored the winning goal?", new[]
                {
                    new DecisionChoice("Flamengo"),
                    new DecisionChoice("Palmeiras")
                }));
                ConsoleLog("Result: ", choice.IsRefused ? "Refused" : $"Team: {choice.Answer!.Choice.Value}; confidence: {choice.Answer.Confidence:P1}", ConsoleColor.Yellow);
                break;
            case "3":
                var score = await decisionMaker.Evaluate(new ScoreQuestion("excitement", "How exciting was the match based on the described events?", new[]
                {
                    new DecisionScoreLevel("Low",    "Few noteworthy events"),
                    new DecisionScoreLevel("Medium", "A competitive match"),
                    new DecisionScoreLevel("High",   "A dramatic finish")
                }));

                ConsoleLog("Result: ", score.IsRefused ? "Refused" : $"Excitement: {score.Answer!.Score:F2} out of 2; confidence: {score.Answer.Confidence:P1}", ConsoleColor.Yellow);
                if (score.Answer is { } answer)
                    ConsoleLog("Detail: ", string.Join(" | ", answer.Probabilities.Select(p => $"[Label: {p.Label}, Probability: {p.Probability:P1}]")), ConsoleColor.Yellow);

                break;
        }
    }

    private static async Task RunEvaluator()
    {
        var gptClient = new GptClientFactory().Create(Shared.ModelNames.Gpt5_4Mini, GPT_API_KEY);

        var knowledgeBase = "The Brazil store has discounts for member users. Guests can become a member and get discounts. Socks are cheaper today. most man are buying shoes lately.";
        var context = "You're talking to a shopper at a Checkout page of the Brazilian sport brand website. The shopper has socks in the cart. shopper is guest user";
        var question = "What can you suggest to this shopper?";

        var evaluator = new GptEvaluator(gptClient, knowledgeBase, context);
        var result = await evaluator.Evaluate(question);

        // showing results
        ConsoleLog("Knowledge: ", knowledgeBase, ConsoleColor.Cyan);
        ConsoleLog("Context  : ", context,       ConsoleColor.Cyan);
        ConsoleLog("Question : ", question,      ConsoleColor.Cyan);
        ConsoleLog("",            "----------------------------------------");
        ConsoleLog("Result   : ", result, ConsoleColor.Yellow);
        Console.ReadLine();
    }

    private static void ConsoleLog(string header, string message, ConsoleColor color = ConsoleColor.White, ConsoleColor bgColor = ConsoleColor.Black)
    {
        Console.BackgroundColor = bgColor;
        Console.ForegroundColor = color;
        Console.Write(header);
        Console.ResetColor();
        Console.WriteLine(message);
    }
}