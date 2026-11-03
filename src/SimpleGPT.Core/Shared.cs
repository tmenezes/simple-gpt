using SimpleGPT.Core.Clients;

namespace SimpleGPT.Core;

public static class Shared
{
    internal const string OpenAiBaseUrl = "https://api.openai.com";

    public static class ModelNames
    {
        // 6.1 series
        public const string Gpt6_1Sol   = "gpt-6.1-sol";
        public const string Gpt6_1Terra = "gpt-6.1-terra";
        public const string Gpt6_1Luna  = "gpt-6.1-luna";

        // 6 series
        public const string Gpt6Sol   = "gpt-6-sol";
        public const string Gpt6Terra = "gpt-6-terra";
        public const string Gpt6Luna  = "gpt-6-luna";

        // 5.6 series
        public const string Gpt5_6Sol   = "gpt-5.6-sol";
        public const string Gpt5_6Terra = "gpt-5.6-terra";
        public const string Gpt5_6Luna  = "gpt-5.6-luna";

        // other 5.x series
        public const string Gpt5_5      = "gpt-5.5";
        public const string Gpt5_5Pro   = "gpt-5.5-pro";
        public const string Gpt5_4      = "gpt-5.4";
        public const string Gpt5_4Mini  = "gpt-5.4-mini";
        public const string Gpt5_4Nano  = "gpt-5.4-nano";
        public const string Gpt5_4Pro   = "gpt-5.4-pro";

        // 4.x series (and older)
        public const string Gpt4_1 = "gpt-4.1"; 
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