using SimpleGPT.Core.Clients;

namespace SimpleGPT.Core;

public interface IGptClientFactory
{
    IGptClient Create(string modelName, string apiKey);
}

public class GptClientFactory : IGptClientFactory
{
    public IGptClient Create(string apiKey) => Create(Constants.ModelNames.Gpt4Turbo, apiKey);
    public IGptClient Create(string modelName, string apiKey)
    {
        var isGpt3Turbo = modelName.StartsWith("gpt-3.5-turbo");
        return isGpt3Turbo
            ? new GptCompletionsClient(modelName, apiKey)
            : new GptClientAdapter(modelName, apiKey);
    }
}