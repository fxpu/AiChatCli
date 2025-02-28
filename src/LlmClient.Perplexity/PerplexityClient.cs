using System.Diagnostics;
using System.Net.Http.Json;
using FxPu.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FxPu.LlmClient.Perplexity
{
    public class PerplexityClient : ILlmClient
    {
        private readonly ILogger<PerplexityClient> _logger;
        private readonly LlmClientOptions _options;

        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.perplexity.ai";

        public PerplexityClient(ILogger<PerplexityClient> logger, IOptions<LlmClientOptions> optionsFactory)
        {
            _logger = logger;
            _options = optionsFactory.Value;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        }

        public async ValueTask<LlmChatCompletionResponse> GetChatCompletionAsync(LlmChatCompletionRequest llmChatCompletionRequest, CancellationToken cancellationToken = default)
        {
            Validate.IsNotNull(llmChatCompletionRequest)?.Throw<ArgumentNullException>();
            Validate.IsNotEmpty(llmChatCompletionRequest.Messages)?.Throw<ArgumentException>();

            // convert messages
            var messages = llmChatCompletionRequest.Messages.Select(m => FromLlmChatMessage(m)).ToList();

            // create request
            var request = new PerplexityChatCompletionRequest
            {
                Model = _options.ModelName,
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
            var response = await httpResponseMessage.Content.ReadFromJsonAsync<PerplexityChatCompletionResponse>(cancellationToken);
            sw.Stop();
            _logger.LogTrace("Request took {ms}.", sw.ElapsedMilliseconds);

            return new LlmChatCompletionResponse
            {
                Message = ToLlmChatMessage(response.Choices.First().Message, response.Citations),
                CompletionTokens = response.Usage.CompletionTokens,
                PromptTokens = response.Usage.PromptTokens,
                TotalTokens = response.Usage.TotalTokens,
                ElapsedMilliseconds = sw.ElapsedMilliseconds
            };
        }

        private LlmChatMessage ToLlmChatMessage(PerplexityChatMessage ppChatMessage, IEnumerable<string>? citations)
        {
            var role = ppChatMessage.Role switch
            {
                "system" => LlmChatRole.System,
                "user" => LlmChatRole.User,
                "assistant" => LlmChatRole.Assistant,
                _ => throw new ArgumentOutOfRangeException(nameof(ppChatMessage.Role))
            };

            var llmCitations = citations?.Select(c => new LlmCitation
            {
                Url = c
            });

            var llmCahtMessage = new LlmChatMessage
            {
                Role = role,
                Content = ppChatMessage.Content,
                Citations = llmCitations
            };

            // when auto inline citations, add citations as md links at the end.
            if (!_options.NoAutoInlineCitations)
            {
                llmCahtMessage.Content += LlmContentHelper.CitationsToMarkDowCitations(llmCitations, "\n\n---\n\n");
            }

            return llmCahtMessage;
        }

        private PerplexityChatMessage FromLlmChatMessage(LlmChatMessage llmChatMessage)
        {
            var role = llmChatMessage.Role switch
            {
                LlmChatRole.System => "system",
                LlmChatRole.User => "user",
                LlmChatRole.Assistant => "assistant",
                _ => throw new ArgumentOutOfRangeException(nameof(llmChatMessage.Role))
            };

            var ppChatMessage = new PerplexityChatMessage
            {
                Role = role,
                Content = llmChatMessage.Content
            };

            // when no auto inline citations, add citations as md links at the end for model history
            if (_options.NoAutoInlineCitations)
            {
                ppChatMessage.Content += LlmContentHelper.CitationsToMarkDowCitations(llmChatMessage.Citations, "\n\n---\n\n");
            }

            return ppChatMessage;
        }

    }
}
