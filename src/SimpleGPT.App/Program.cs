using SimpleGPT.Core;

namespace SimpleGPT
{
    internal class Program
    {
        private const string GPT_API_KEY = "< api-key-here >";

        static async Task Main(string[] args)
        {
            var gptClient = new GptClientFactory().Create(Shared.ModelNames.Gpt4Turbo, GPT_API_KEY);

            var knowledgeBase = "The Brazil store has discounts for member users. Guests can become a member and get discounts. Socks are cheaper today. most man are buying shoes lately.";
            var context = "You're talking to a shopper at a Checkout page of the Brazilian sport brand website. The shopper has socks in the cart. shopper is guest user";
            var question = "What can you suggest to this shopper?";

            var evaluator = new GptEvaluator(gptClient, knowledgeBase, context);
            var result = await evaluator.Evaluate(question);

            // showing results
            ConsoleLog("Knowledge: ", knowledgeBase, ConsoleColor.Cyan);
            ConsoleLog("Context  : ", context, ConsoleColor.Cyan);
            ConsoleLog("Question : ", question, ConsoleColor.Cyan);
            ConsoleLog("", "----------------------------------------");
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
}