using System.Diagnostics;
using Azure;
using Azure.AI.OpenAI;
using FxPu.AiChatLib.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FxPu.AiChatLib.Services
{
    public class ChatService : IChatService
    {
        private readonly ILogger<ChatService> _logger;
        private readonly ChatOptions _chatOptions;
        private readonly IList<ChatMessage> _messages;
        private ChatConfiguration _configuration = null!;
        private OpenAIClient _llmClient = null!;
        private ChatStatus _chatStatus;

        public ChatService(ILogger<ChatService> logger, IOptions<ChatOptions> chatOptionsFactory)
        {
            _logger = logger;
            _chatOptions = chatOptionsFactory.Value;

            _messages = new List<ChatMessage>();

            // use the first configuration as default
            SetConfigurationAndClient(_chatOptions.Configurations.First());

            // chat status
            _chatStatus = new ChatStatus();
        }

        public ValueTask NewChatAsync()
        {
            // clear message, keep system message
            var systemMessage = _messages.FirstOrDefault(m => m.Role == "system");
            _messages.Clear();
            if (systemMessage != null)
            {
                _messages.Add(systemMessage);
            }
            _chatStatus = new ChatStatus();

            return new ValueTask();
        }

        public async ValueTask<string?> SubitAsync(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ChatException("No question - no answer :-)");
            }

            // llm options
            var llmOptions = new ChatCompletionsOptions
            {
                DeploymentName = _configuration.ModelName,
                ChoiceCount = 1,
                Temperature = null,
                FrequencyPenalty = null,
                PresencePenalty = null
            };

            // recent messages
            foreach (var message in _messages)
            {
                ChatRequestMessage llmMessage = message.Role switch
                {
                    "system" => new ChatRequestSystemMessage(message.Content),
                    "user" => new ChatRequestUserMessage(message.Content),
                    "assistant" => new ChatRequestAssistantMessage(message.Content),
                    _ => throw new NotImplementedException($"Role {message.Role} not implemented.")
                };
                llmOptions.Messages.Add(llmMessage);
            }

            // add question
            llmOptions.Messages.Add(new ChatRequestUserMessage(question));

            // ask the llm
            var sw = Stopwatch.StartNew();
            var llmResponse = (await _llmClient.GetChatCompletionsAsync(llmOptions)).Value;
            var llmChoice = llmResponse.Choices[0];
            sw.Stop();

            // last tokens and time
            _chatStatus.LastTokenUsage = new TokenUsage(llmResponse.Usage.PromptTokens, llmResponse.Usage.CompletionTokens, llmResponse.Usage.TotalTokens);
            _chatStatus.LastLlmDuration = sw.Elapsed;

            // add question and answer to messages
            _messages.Add(new ChatMessage { Role = "user", Content = question });
            var answer = llmChoice.Message.Content;
            _messages.Add(new ChatMessage { Role = "assistant", Content = answer });

            // update question number
            _chatStatus.QuestionNumber = _messages.Count(m => m.Role == "assistant") + 1;

            return answer;
        }

        public ValueTask SetConfigurationAsync(string name)
        {
            var configuration = _chatOptions.Configurations.SingleOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (configuration == null)
            {
                throw new ChatException($"Configuration \"{name} not found.");
            }

            SetConfigurationAndClient(configuration);

            return new ValueTask();
        }

        private void SetConfigurationAndClient(ChatConfiguration configuration)
        {
            // nothing to do?
            if (_configuration == configuration)
            {
                return;
            }

            // set configuration and llm client
            _configuration = configuration;
            if (_configuration.AzureOpenAiEndpoint == null)
            {
                _logger.LogTrace("Use www.openai.com endpoint.");
                _llmClient = new OpenAIClient(_configuration.ApiKey);
            }
            else
            {
                _logger.LogTrace("Use AzureOpenAiEndpoint {endpoint}.", _configuration.AzureOpenAiEndpoint);
                _llmClient = new OpenAIClient(new Uri(_configuration.AzureOpenAiEndpoint), new AzureKeyCredential(_configuration.ApiKey));
            }
        }

        public ValueTask<IEnumerable<ChatConfiguration>> ListConfigurationsAsync()
        {
            return new ValueTask<IEnumerable<ChatConfiguration>>(_chatOptions.Configurations);
        }

        public ChatStatus GetStatus() => _chatStatus;

    }
}
