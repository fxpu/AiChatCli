using System.ClientModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using FxPu.AiChat.Utils;
using FxPu.Extensions.Ai.Perplexity;
using FxPu.Utils;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;

namespace FxPu.AiChat.Services
{
    public class ChatService : IChatService
    {
        private readonly ILogger<ChatService> _logger;
        private readonly Utils.ChatOptions _chatOptions;
        private readonly List<ChatMessage> _llmMessages;

        private readonly List<ChatConfiguration> _configurations;
        private ChatConfiguration _configuration = null!;
        private IChatClient _llmChatClient = null!;
        private ChatConfiguration? _titleConfiguration;
        private IChatClient? _titleLlmClient;
        private ChatStatus _chatStatus;


        public ChatService(ILogger<ChatService> logger, IOptions<Utils.ChatOptions> chatOptionsFactory)
        {
            _logger = logger;
            _chatOptions = chatOptionsFactory.Value;

            _llmMessages = new();

            //  build configurations for each model in the raw configurations
            _configurations = new List<ChatConfiguration>();
            foreach (var rawConfiguration in _chatOptions.Configurations)
            {
                // parse comma seperated ModelNames and create a configuration for each model
                var modelNames = rawConfiguration.ModelName.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var modelName in modelNames)
                {
                    var name = rawConfiguration.Name ?? rawConfiguration.Provider.ToString();
                    var configuration = new ChatConfiguration
                    {
                        Name = modelNames.Length == 1 ? name : $"{name} {modelName}",
                        Provider = rawConfiguration.Provider,
                        ApiEndpoint = rawConfiguration.ApiEndpoint,
                        ApiKey = rawConfiguration.ApiKey,
                        ModelName = modelName
                    };
                    _configurations.Add(configuration);
                }
            }

            // title llm configuration
            if (_chatOptions.TitleConfigurationName != null)
            {
                _titleConfiguration = _configurations.SingleOrDefault(c => c.Name.Equals(_chatOptions.TitleConfigurationName, StringComparison.InvariantCultureIgnoreCase));
                if (_titleConfiguration != null)
                {
                    _titleLlmClient = CreateLlmChatClient(_titleConfiguration);
                }
            }

            // chat status
            _chatStatus = new ChatStatus();

            // use the first configuration as default
            SetConfigurationAndClient(_configurations.First());
        }

        public ValueTask NewChatAsync()
        {
            // clear message
            _llmMessages.Clear();
            _chatStatus = new ChatStatus();
            _chatStatus.ConfigurationName = _configuration.Name;

            return new ValueTask();
        }

        public async ValueTask<string?> SubmitAsync(string question)
        {
            Validate.IsNotEmpty(question, "No question - no answer :-)")?.Throw<ChatException>();

            // when status.title is null, create with question in background
            ValueTask<string?>? titleTask = null;
            if (_chatStatus.Title == null && _titleConfiguration != null)
            {
                titleTask = CreateTitleAsync(question);
            }

            // add question
            _llmMessages.Add(new ChatMessage(ChatRole.User, question));

            // ask the llm
            var sw = Stopwatch.StartNew();
            var llmChatResponse = await _llmChatClient.GetResponseAsync(_llmMessages);
            sw.Stop();

            // last tokens and time
            _chatStatus.LastTokenUsage = new TokenUsage(llmChatResponse.Usage?.InputTokenCount ?? 0, llmChatResponse.Usage?.OutputTokenCount ?? 0, llmChatResponse.Usage?.TotalTokenCount ?? 0);
            _chatStatus.LastLlmDuration = sw.Elapsed;

            // add question and answer to messages
            var llmMessage = new ChatMessage(ChatRole.Assistant, llmChatResponse.Text);
            _llmMessages.Add(llmMessage);

            // update question number
            _chatStatus.QuestionNumber = _llmMessages.Count(m => m.Role == ChatRole.Assistant) + 1;

            // wait for title task when not null
            if (titleTask != null)
            {
                _chatStatus.Title = await (ValueTask<string?>) titleTask;
            }

            return llmMessage.Text;
        }

        public ValueTask SetConfigurationAsync(string name)
        {
            var configuration = GetConfiguration(name);
            if (configuration == null)
            {
                throw new ChatException($"Configuration \"{name} not found.");
            }

            SetConfigurationAndClient(configuration);

            return ValueTask.CompletedTask;
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
            _llmChatClient = CreateLlmChatClient(configuration);

            // status
            _chatStatus.ConfigurationName = _configuration.Name;
        }

        public ValueTask<IEnumerable<ChatConfiguration>> ListConfigurationsAsync()
        {
            return new ValueTask<IEnumerable<ChatConfiguration>>(_configurations);
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
            _llmMessages.Add(new ChatMessage(ChatRole.System, systemMessage));
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
            var llmSystemMessage = _llmMessages.FirstOrDefault(m => m.Role == ChatRole.System);
            await NewChatAsync();
            if (llmSystemMessage != null)
            {
                _llmMessages.Add(llmSystemMessage);
                _chatStatus.IsSystemMessageSet = true;
            }
        }

        private async ValueTask<string?> CreateTitleAsync(string question)
        {
            //add first 100 chars as question for title
            var titleQuestion = question.Length > 100 ? question.Substring(0, 100) : question;
            var content = $"Summarize the following question in max. 6 words.Use the language of the question for the answer:\n{titleQuestion}";
            var llmChatResponse = await _titleLlmClient.GetResponseAsync([new ChatMessage(ChatRole.User, content)]);

            return llmChatResponse.Text;
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



        private IChatClient CreateLlmChatClient(ChatConfiguration configuration)
        {
            switch (configuration.Provider)
            {
                case LlmProvider.OpenAi:
                    _logger.LogTrace("Create OpenAi client.");
                    return new OpenAIClient(configuration.ApiKey).GetChatClient(configuration.ModelName).AsIChatClient();

                case LlmProvider.AzureOpenAi:
                    _logger.LogTrace("Create AzureOpenAi client.");
                    return new AzureOpenAIClient(new Uri(configuration.ApiEndpoint), new ApiKeyCredential(configuration.ApiKey)).GetChatClient(configuration.ModelName).AsIChatClient();

                case LlmProvider.Perplexity:
                    _logger.LogTrace("Create Perplexity client.");
                    return new PerplexityChatClient(configuration.ApiKey, configuration.ModelName);

                default:
                    throw new ArgumentException($"Provider {configuration.Provider} not supported.");
            }
        }

        private ChatConfiguration? GetConfiguration(string name)
        {
            // check per regex if name only contains numbers
            if (Regex.IsMatch(name, "^[0-9]+$"))
            {
                var index = int.Parse(name) - 1;
                return index >= 0 && index < _configurations.Count ? _configurations[index] : null;
            }

            return _configurations.SingleOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

    }
}