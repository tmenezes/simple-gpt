namespace SimpleGPT.Core.Clients;

public class GptClientAdapter : IGptClient
{
    private readonly ChatGptClient _chatGptClient;

    public GptClientAdapter(string apiKey) : this(Shared.ModelNames.Gpt4Turbo, apiKey) { }
    public GptClientAdapter(string modelName, string apiKey)
    {
        _chatGptClient = new ChatGptClient(modelName, apiKey);
    }

    public Task<string> CallGpt(string userInput)
    {
        _chatGptClient.AddChatMessage(ChatMessageType.User, userInput);
        return _chatGptClient.CallGpt();
    }
}