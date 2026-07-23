using OpenAI;
using OpenAI.Chat;

namespace SimpleGPT.Core.Clients;

public class ChatGptClient : IChatGptClient
{
    private readonly OpenAIClient      _client;
    private readonly ChatClient        _chatClient;
    private readonly List<ChatMessage> _messages = new();

    public string Conversation => _messages.Select(m => m.ToString()).Aggregate((a, b) => $"{a}\n{b}") ?? "";

    // ctor
    public ChatGptClient(string apiKey) : this(Shared.ModelNames.Gpt4Turbo, apiKey) { }
    public ChatGptClient(string modeName, string apiKey)
    {
        _client = new OpenAIClient(apiKey);
        _chatClient = _client.GetChatClient(modeName);
    }

    // public
    public void AddChatMessage(ChatMessageType type, string message)
    {
        var chatMessage = CreateChatMessage(type.ToString(), message);
        _messages.Add(chatMessage);
    }

    public Task<string> CallGpt() => CallGpt(Shared.GptOptions.DefaultGeneralPurposes);
    public async Task<string> CallGpt(GptCallOptions options)
    {
        var chatCompletionsOptions = new ChatCompletionOptions
        {
            Temperature = options.Temperature,
            MaxOutputTokenCount = options.MaxTokens,
            PresencePenalty = options.PresencePenalty
        };

        var response = await _chatClient.CompleteChatAsync(_messages, chatCompletionsOptions);
        var responseContent = response.Value.Content.First().Text;

        var chatMessage = CreateChatMessage(response.Value.Role.ToString(), responseContent);
        _messages.Add(chatMessage);

        return responseContent;
    }

    // private
    private ChatMessage CreateChatMessage(string chatMessageType, string message)
    {
        ChatMessage chatMessage = chatMessageType.ToLowerInvariant() switch
        {
            "system" => new SystemChatMessage(message),
            "assistant" => new AssistantChatMessage(message),
            "user" => new UserChatMessage(message),
            _ => throw new ArgumentOutOfRangeException(nameof(chatMessageType))
        };
        return chatMessage;
    }
}