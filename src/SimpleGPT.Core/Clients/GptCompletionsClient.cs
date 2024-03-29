﻿using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SimpleGPT.Core.Clients
{
    public class GptCompletionsClient : IGptClient, IGptCompletionsClient
    {
        private readonly HttpClient     _client = new();
        private readonly string         _modeName;
        private readonly StringBuilder  _conversation;

        public string Conversation => _conversation.ToString();

        // constructors
        public GptCompletionsClient(string apiKey) : this(Shared.ModelNames.Gpt3TurboInstruct, apiKey) { }
        public GptCompletionsClient(string modeName, string apiKey)
        {
            var openAiUrl = $"{Shared.OpenAiBaseUrl}/v1/completions";
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _client.BaseAddress = new Uri(openAiUrl);

            _modeName = modeName;
            _conversation = new StringBuilder();
        }

        public Task<string> CallGpt(string userInput) => CallGpt(userInput, Shared.GptOptions.DefaultGeneralPurposes);
        public async Task<string> CallGpt(string userInput, GptCallOptions options)
        {
            _conversation.Append(userInput).Append("\n");

            var request = new HttpRequestMessage(HttpMethod.Post, "");
            request.Content = JsonContent.Create(new
            {
                model = _modeName,
                prompt = _conversation.ToString(),
                temperature = options.Temperature,
                max_tokens = options.MaxTokens,
                top_p = options.Top ?? 1,
                frequency_penalty = options.FrequencyPenalty,
                presence_penalty = options.PresencePenalty
            });
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // Get the response from the API
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseContent);
            var gptResponse = responseJson["choices"][0]["text"].ToString();

            // update conversation state
            _conversation.Append(gptResponse).Append("\n\n");

            return gptResponse;
        }

        public Task<string> GetCompletions(string userInput) => CallGpt(userInput);
        public Task<string> GetCompletions(string userInput, GptCallOptions options) => CallGpt(userInput, options);
    }
}