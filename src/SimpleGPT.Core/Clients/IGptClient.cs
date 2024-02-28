namespace SimpleGPT.Core.Clients;

public interface IGptClient
{
    Task<string> CallGpt(string userInput);
}

public interface IGptCompletionsClient
{
    string Conversation { get; }
    Task<string> GetCompletions(string userInput);
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
}