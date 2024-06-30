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
        private ChatConfiguration? _titleConfiguration;
        private OpenAIClient? _titleLlmClient;
        private ChatStatus _chatStatus;

        public ChatService(ILogger<ChatService> logger, IOptions<ChatOptions> chatOptionsFactory)
        {
            _logger = logger;
            _chatOptions = chatOptionsFactory.Value;

            _messages = new List<ChatMessage>();

            // chat status
            _chatStatus = new ChatStatus();

            // use the first configuration as default
            SetConfigurationAndClient(_chatOptions.Configurations.First());

            SetTitleConfigurationAndClient();
        }

        public ValueTask NewChatAsync()
        {
            // clear message
            _messages.Clear();
            _chatStatus = new ChatStatus();
            _chatStatus.ConfigurationName = _configuration.Name;

            return new ValueTask();
        }

        public async ValueTask<string?> SubitAsync(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ChatException("No question - no answer :-)");
            }

            // when status.title is null, create with question in background
            ValueTask<string?>? titleTask = null;
            if (_chatStatus.Title == null && _titleConfiguration != null)
            {
                titleTask = CreateTitleAsync(question);
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

            // wait for title task when not null
            if (titleTask != null)
            {
                _chatStatus.Title = await (ValueTask<string?>) titleTask;
            }

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

        private void SetTitleConfigurationAndClient()
        {
            var configurationName = _chatOptions.TitleConfigurationName;
            if (configurationName == null)
            {
                return;
            }

            // get configuration, exit when null
            _titleConfiguration = _chatOptions.Configurations.SingleOrDefault(c => c.Name == configurationName);
            if (_titleConfiguration == null)
            {
                return;
            }

_titleLlmClient = _titleConfiguration.AzureOpenAiEndpoint == null
                ? new OpenAIClient(_titleConfiguration.ApiKey)
                : new OpenAIClient(new Uri(_titleConfiguration.AzureOpenAiEndpoint), new AzureKeyCredential(_titleConfiguration.ApiKey));
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

            // status
            _chatStatus.ConfigurationName = _configuration.Name;
        }

        public ValueTask<IEnumerable<ChatConfiguration>> ListConfigurationsAsync()
        {
            return new ValueTask<IEnumerable<ChatConfiguration>>(_chatOptions.Configurations);
        }

        public ChatStatus GetStatus() => _chatStatus;

        public async ValueTask SetSystemMessageAsync(string systemMessage)
            {
            if (string.IsNullOrEmpty(systemMessage))
            {
                throw new ChatException("No system message set.");
            }

            // create new chat and set system message
            await NewChatAsync();
            _messages.Add(new ChatMessage { Role = "system", Content = systemMessage });
            _chatStatus.IsSystemMessageSet = true;
            }

        public async ValueTask OpenSystemMessageAsync(string fileName)
        {

        }

        public async ValueTask NewChatKeepSystemMessageAsync()
        {
            // keep system message, new chat and set again
            var systemMessage = _messages.FirstOrDefault(m => m.Role == "system");
            await NewChatAsync();
            if (systemMessage != null)
            {
                _messages.Add(systemMessage);
                _chatStatus.IsSystemMessageSet = true;
            }
        }

        private async ValueTask<string?> CreateTitleAsync(string question)
        {
            if (_titleConfiguration == null)
            {
                return null;
            }

            var titeLlmOptions = new ChatCompletionsOptions
            {
                DeploymentName = _titleConfiguration.ModelName,
                ChoiceCount = 1,
                Temperature = 0.1f,
                FrequencyPenalty = null,
                PresencePenalty = null
            };

            // add first 100 chars as question for title
            var titleQuestion = question.Length > 100 ? question.Substring(0, 100) : question;
            var content = $"Summarize the following question in max. 3 words.Use the language of the question for the answer:\n{ titleQuestion}";
            titeLlmOptions.Messages.Add(new ChatRequestUserMessage(content));

            // ask the llm
            var sw = Stopwatch.StartNew();
            var titleLlmResponse = (await _titleLlmClient.GetChatCompletionsAsync(titeLlmOptions)).Value;
            var titleLlmChoice = titleLlmResponse.Choices[0];
            sw.Stop();

            _logger.LogTrace("title llm took {ms}.", sw.ElapsedMilliseconds);

            return titleLlmChoice.Message.Content;
        }

    }
}
