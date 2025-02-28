using System.ClientModel;
using System.Diagnostics;
using FxPu.AiChat.Utils;
using FxPu.LlmClient;
using FxPu.LlmClient.OpenAi;
using FxPu.LlmClient.Perplexity;
using FxPu.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FxPu.AiChat.Services
{
    public class ChatService : IChatService
    {
        private readonly ILogger<ChatService> _logger;
        private readonly ChatOptions _chatOptions;
        private readonly IList<LlmChatMessage> _messages;
        private readonly ILoggerFactory _loggerFactory;

        private ChatConfiguration _configuration = null!;
        private ILlmClient _llmClient = null!;
        private ChatConfiguration? _titleConfiguration;
        private ILlmClient ? _titleLlmClient;
        private ChatStatus _chatStatus;

        public ChatService(ILogger<ChatService> logger, IOptions<ChatOptions> chatOptionsFactory, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _chatOptions = chatOptionsFactory.Value;
            _loggerFactory = loggerFactory;

            _messages = new List<LlmChatMessage>();

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

            // add question
            _messages.Add(new LlmChatMessage { Role = LlmChatRole.User, Content = question });

            // ask the llm
            var request = new LlmChatCompletionRequest { Messages = _messages };
            var response = await _llmClient.GetChatCompletionAsync(request);

            // last tokens and time
            _chatStatus.LastTokenUsage = new TokenUsage(response.PromptTokens ?? 0, response.CompletionTokens ?? 0, response.TotalTokens ?? 0);
            _chatStatus.LastLlmDuration = TimeSpan.FromMilliseconds(response.ElapsedMilliseconds ?? 0);

            // add question and answer to messages
            var message = response.Message;
            _messages.Add(message);

            // update question number
            _chatStatus.QuestionNumber = _messages.Count(m => m.Role == LlmChatRole.Assistant) + 1;

            // wait for title task when not null
            if (titleTask != null)
            {
                _chatStatus.Title = await (ValueTask<string?>) titleTask;
            }

            return message.Content;
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

            // TODO: implement configuration
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
            _llmClient = CreateLlmClient(configuration);

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
            _messages.Add(new LlmChatMessage{ Role = LlmChatRole.System, Content = systemMessage });
            _chatStatus.IsSystemMessageSet = true;
        }

        public async ValueTask OpenSystemMessageAsync(string fileName)
        {
            Validate.IsNotEmpty(fileName, "File name is empty.").Throw<ChatException>();

            var foundFileName = SearchFile(fileName);
            Validate.IsNotNull(foundFileName, $"File {fileName} not found.").Throw<ChatException>();

            // read system  message from file
            var systemMessage = await File.ReadAllTextAsync(foundFileName);
            Validate.IsNotEmpty(systemMessage, $"File {fileName} contains no system message.").Throw<ChatException>();

            await SetSystemMessageAsync(systemMessage);
        }

        public async ValueTask NewChatKeepSystemMessageAsync()
        {
            // keep system message, new chat and set again
            var systemMessage = _messages.FirstOrDefault(m => m.Role == LlmChatRole.System);
            await NewChatAsync();
            if (systemMessage != null)
            {
                _messages.Add(systemMessage);
                _chatStatus.IsSystemMessageSet = true;
            }
        }

        private async ValueTask<string?> CreateTitleAsync(string question)
        {
            return null;
            //add first 100 chars as question for title
            //var titleQuestion = question.Length > 100 ? question.Substring(0, 100) : question;
            //var content = $"Summarize the following question in max. 3 words.Use the language of the question for the answer:\n{titleQuestion}";
        }

        private string? SearchFile(string fileName)
        {
            if (ValueHelper.IsEmpty(fileName))
            {   
                return null;
            }

            // direct hit?
            if (File.Exists(fileName))
            {
                return fileName;
            }

            // ~/.AiChat/SystemMessages
            var foundFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".AiChat", "SystemMessages", fileName);
            if (File.Exists(foundFileName))
            {
                return foundFileName;
            }

            // ~/.AiChat
            foundFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".AiChat", fileName);
            if (File.Exists(foundFileName))
            {
                return foundFileName;
            }

            // when fileName ends with .txt then return null => not found
            if (fileName.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return SearchFile($"{fileName}.txt");
        }



        private ILlmClient CreateLlmClient(ChatConfiguration configuration)
        {
            // open Ai
            if (configuration.Provider == LlmProvider.OpenAi)
            {
                _logger.LogTrace("Create OpenAi client.");
                var logger = _loggerFactory.CreateLogger<OpenAiClient>();
                var optionsFactory = new OptionsWrapper<LlmClientOptions>(new LlmClientOptions
                {
                    ApiKey = configuration.ApiKey,
                    ModelName = configuration.ModelName
                });
                return new OpenAiClient(logger, optionsFactory);
            }

            // Perplexity
            if (configuration.Provider == LlmProvider.Perplexity)
            {
                _logger.LogTrace("Create Perplexity client.");
                var logger = _loggerFactory.CreateLogger<PerplexityClient>();
                var optionsFactory = new OptionsWrapper<LlmClientOptions>(new LlmClientOptions
                {
                    ApiKey = configuration.ApiKey,
                    ModelName = configuration.ModelName
                });
                return new PerplexityClient(logger, optionsFactory);
            }

            throw new ArgumentException($"Provider {configuration.Provider} not supported.");
        }

    }
}