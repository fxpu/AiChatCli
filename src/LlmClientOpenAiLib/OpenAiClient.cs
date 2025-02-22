using System.Diagnostics;
using FxPu.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;

namespace FxPu.LlmClient.OpenAi
{
    public class OpenAiClient : ILlmClient
    {
        private readonly ILogger<OpenAiClient> _logger;
        private readonly LlmClientOptions _options;

        private OpenAI.Chat.ChatClient _openAiClient;

        public OpenAiClient(ILogger<OpenAiClient> logger, IOptions<LlmClientOptions> optionsFactory)
        {
            _logger = logger;
            _options = optionsFactory.Value;

            _openAiClient = new OpenAIClient(_options.ApiKey).GetChatClient(_options.ModelName);
        }

        public async ValueTask<LlmChatCompletionResponse> GetChatCompletionAsync(LlmChatCompletionRequest llmChatCompletionRequest, CancellationToken cancellationToken = default)
        {
            Validate.IsNotNull(llmChatCompletionRequest)?.Throw<ArgumentNullException>();
            Validate.IsNotEmpty(llmChatCompletionRequest.Messages)?.Throw<ArgumentException>();

            // convert messages
            var messages = llmChatCompletionRequest.Messages.Select(m => FromLlmChatMessage(m)).ToList();

            // create options, not used at the moment
            // var options = new OpenAI.Chat.ChatCompletionRequestOptions;

            // send request
            _logger.LogTrace("Sending request");
            var sw = Stopwatch.StartNew();
            var response = await _openAiClient.CompleteChatAsync(messages);
            sw.Stop();
            _logger.LogTrace("Request took {ms}.", sw.ElapsedMilliseconds);

            return new LlmChatCompletionResponse
            {
                Message = new LlmChatMessage { Role = LlmChatRole.Assistant, Content = response.Value.Content.FirstOrDefault()?.Text },
                ElapsedMilliseconds = sw.ElapsedMilliseconds
            };
        }

        private OpenAI.Chat.ChatMessage FromLlmChatMessage(LlmChatMessage llmChatMessage)
        {
            OpenAI.Chat.ChatMessage openAiChatMessage = llmChatMessage.Role switch
            {
                LlmChatRole.System => new OpenAI.Chat.SystemChatMessage(llmChatMessage.Content),
                LlmChatRole.User => new OpenAI.Chat.UserChatMessage(llmChatMessage.Content),
                LlmChatRole.Assistant => new OpenAI.Chat.AssistantChatMessage(llmChatMessage.Content),
                _ => throw new ArgumentOutOfRangeException(nameof(llmChatMessage.Role))
            };

            return openAiChatMessage;
        }

    }
}
