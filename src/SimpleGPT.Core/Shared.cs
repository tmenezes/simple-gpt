using SimpleGPT.Core.Clients;

namespace SimpleGPT.Core;

public static class Shared
{
    internal const string OpenAiBaseUrl = "https://api.openai.com";

    public static class ModelNames
    {
        public const string Gpt4 = "gpt-4";
        public const string Gpt4Turbo = "gpt-4-0125-preview";
        public const string Gpt3Turbo = "gpt-3.5-turbo";
        public const string Gpt3Turbo0125 = "gpt-3.5-turbo-0125";
        public const string Gpt3Turbo1106 = "gpt-3.5-turbo-1106";
        public const string Gpt3TurboInstruct = "gpt-3.5-turbo-instruct";
    }

    public static class GptOptions
    {
        public static GptCallOptions DefaultGeneralPurposes => new();
        public static GptCallOptions DefaultForEvaluator => new()
        {
            Temperature = 0.9f,
            MaxTokens = 256,
            PresencePenalty = 0.5f,
            Top = 1
        };
    }
}