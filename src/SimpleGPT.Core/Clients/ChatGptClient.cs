using Azure.AI.OpenAI;
using System.Text;

namespace SimpleGPT.Core.Clients;

public class ChatGptClient : IChatGptClient
{
    private readonly string                   _modeName;
    private readonly OpenAIClient             _client;
    private readonly List<ChatRequestMessage> _messages = new();

    public string Conversation => _messages.Select(m => m.ToString()).Aggregate((a, b) => $"{a}\n{b}") ?? "";

    // ctor
    public ChatGptClient(string apiKey) : this(Constants.ModelNames.Gpt4Turbo, apiKey) { }
    public ChatGptClient(string modeName, string apiKey)
    {
        _modeName = modeName;
        _client = new OpenAIClient(apiKey);
    }

    // public
    public void AddChatMessage(ChatMessageType type, string message)
    {
        var chatMessage = CreateChatMessage(type.ToString(), message);
        _messages.Add(chatMessage);
    }

    public async Task<string> CallGpt()
    {
        var chatCompletionsOptions = new ChatCompletionsOptions(_modeName, _messages)
        {
            Temperature = 0.9f,
            MaxTokens = 256,
            PresencePenalty = 0.5f
        };

        var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
        var responseMessage = response.Value.Choices[0].Message;

        var chatMessage = CreateChatMessage(responseMessage.Role.ToString(), responseMessage.Content);
        _messages.Add(chatMessage);

        return responseMessage.Content;
    }

    // private
    private ChatRequestMessage CreateChatMessage(string chatMessageType, string message)
    {
        ChatRequestMessage chatMessage = chatMessageType.ToLowerInvariant() switch
        {
            "system" => new ChatRequestSystemMessage(message),
            "assistant" => new ChatRequestAssistantMessage(message),
            "user" => new ChatRequestUserMessage(message),
            _ => throw new ArgumentOutOfRangeException(nameof(chatMessageType))
        };
        return chatMessage;
    }
}