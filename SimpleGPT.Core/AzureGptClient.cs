namespace SimpleGPT.Core;

public class AzureGptClient : IGptClient
{
    public string Conversation { get; }

    public Task<string> CallGpt(string userInput)
    {
        throw new NotImplementedException();
    }
}