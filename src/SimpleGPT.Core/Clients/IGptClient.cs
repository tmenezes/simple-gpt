namespace SimpleGPT.Core.Clients;

public interface IGptClient
{
    Task<string> CallGpt(string userInput);
    Task<string> CallGpt(string userInput, GptCallOptions options);
}

public interface IGptCompletionsClient
{
    string Conversation { get; }
    Task<string> GetCompletions(string userInput);
    Task<string> GetCompletions(string userInput, GptCallOptions options);
}


public enum ChatMessageType
{
    System = 0,
    Assistant = 1,
    User = 2
}

public interface IChatGptClient
{
    string Conversation { get; }
    void AddChatMessage(ChatMessageType type, string message);
    Task<string> CallGpt();
    Task<string> CallGpt(GptCallOptions options);
}