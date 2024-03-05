using SimpleGPT.Core.Clients;

namespace SimpleGPT.Core;

public interface IGptClientFactory
{
    IGptClient Create(string modelName, string apiKey);
}

public class GptClientFactory : IGptClientFactory
{
    public IGptClient Create(string apiKey) => Create(Shared.ModelNames.Gpt4Turbo, apiKey);
    public IGptClient Create(string modelName, string apiKey)
    {
        var isGpt3TurboInstruct = modelName.StartsWith("gpt-3.5-turbo-instruct");
        return isGpt3TurboInstruct
            ? new GptCompletionsClient(modelName, apiKey)
            : new GptClientAdapter(modelName, apiKey);
    }
}