using System.Diagnostics;
using System.Net.Http.Json;
using FxPu.Utils;
using Microsoft.Extensions.Logging;

namespace FxPu.LlmClient.Pp
{
    public class PpClient : ILlmClient
    {
        private ILogger<PpClient> _logger;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.perplexity.ai";

        public PpClient(ILogger<PpClient> logger, string apiKey)
        {
            _logger = logger;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async ValueTask<LlmChatCompletionResponse> GetChatCompletionAsync(LlmChatCompletionRequest llmChatCompletionRequest, CancellationToken cancellationToken = default)
        {
            Validate.IsNotNull(llmChatCompletionRequest)?.Throw<ArgumentNullException>();
            Validate.IsNotEmpty(llmChatCompletionRequest.ModelName)?.Throw<ArgumentException>();
            Validate.IsNotEmpty(llmChatCompletionRequest.Messages)?.Throw<ArgumentException>();

            // convert messages
            var messages = llmChatCompletionRequest.Messages.Select(m => PpChatMessage.FromLlmChatMessage(m)).ToList();

            // create request
            var request = new PpChatCompletionRequest
            {
                Model = llmChatCompletionRequest.ModelName,
                Messages = messages,
                ReturnCitations = true,
                ReturnRelatedQuestions = false
            };

            // send request
            _logger.LogTrace("Sending request");
            var sw = Stopwatch.StartNew();
            var httpResponseMessage = await _httpClient.PostAsJsonAsync("/chat/completions", request, cancellationToken);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var error = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogTrace("Error occured, throw LlmClientException");
                throw new LlmClientException($"Error get chat completion: {error}");
            }
            var response = await httpResponseMessage.Content.ReadFromJsonAsync<PpChatCompletionResponse>(cancellationToken);
            sw.Stop();
            _logger.LogTrace("Request took {ms}.", sw.ElapsedMilliseconds);

            return new LlmChatCompletionResponse
            {
                Message = PpChatMessage.ToLlmChatMessage(response.Choices.First().Message, response.Citations),
                CompletionTokens = response.Usage.CompletionTokens,
                PromptTokens = response.Usage.PromptTokens,
                TotalTokens = response.Usage.TotalTokens,
                ElapsedMilliseconds = sw.ElapsedMilliseconds
            };
        }
    }
}
