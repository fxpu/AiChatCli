using System.Diagnostics;
using System.Text.RegularExpressions;
using FxPu.AiChat.Utils;
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
        private readonly List<ChatMessage> _messages;
        private readonly ILoggerFactory _loggerFactory;

        private readonly List<ChatConfiguration> _configurations;
        private ChatConfiguration _configuration = null!;
        private IChatClient _extChatClient = null!;
        private ChatConfiguration? _titleConfiguration;
        private IChatClient? _titleExtClient;
        private ChatStatus _chatStatus;


        public ChatService(ILogger<ChatService> logger, IOptions<Utils.ChatOptions> chatOptionsFactory, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _chatOptions = chatOptionsFactory.Value;
            _loggerFactory = loggerFactory;

            _messages = new();

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
                    _titleExtClient = CreateExtChatClient(_titleConfiguration);
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
            _messages.Clear();
            _chatStatus = new ChatStatus();
            _chatStatus.ConfigurationName = _configuration.Name;

            return new ValueTask();
        }

        public async ValueTask<string?> SubitAsync(string question)
        {
            Validate.IsNotEmpty(question, "No question - no answer :-)")?.Throw<ChatException>();

            // when status.title is null, create with question in background
            ValueTask<string?>? titleTask = null;
            if (_chatStatus.Title == null && _titleConfiguration != null)
            {
                titleTask = CreateTitleAsync(question);
            }

            // add question
            _messages.Add(new ChatMessage(ChatRole.User, question));

            // ask the llm
            var sw = Stopwatch.StartNew();
            var chatResponse = await _extChatClient.GetResponseAsync(_messages);
            sw.Stop();

            // last tokens and time
            _chatStatus.LastTokenUsage = new TokenUsage(chatResponse.Usage?.InputTokenCount ?? 0, chatResponse.Usage?.OutputTokenCount ?? 0, chatResponse.Usage?.TotalTokenCount ?? 0);
            _chatStatus.LastLlmDuration = sw.Elapsed;

            // add question and answer to messages
            var message = new ChatMessage(ChatRole.Assistant, chatResponse.Text);
            _messages.Add(message);

            // update question number
            _chatStatus.QuestionNumber = _messages.Count(m => m.Role == ChatRole.Assistant) + 1;

            // wait for title task when not null
            if (titleTask != null)
            {
                _chatStatus.Title = await (ValueTask<string?>) titleTask;
            }

            return message.Text;
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
            _extChatClient = CreateExtChatClient(configuration);

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
            _messages.Add(new ChatMessage(ChatRole.System, systemMessage));
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
            var msExtAiSystemMessage = _messages.FirstOrDefault(m => m.Role == ChatRole.System);
            await NewChatAsync();
            if (msExtAiSystemMessage != null)
            {
                _messages.Add(msExtAiSystemMessage);
                _chatStatus.IsSystemMessageSet = true;
            }
        }

        private async ValueTask<string?> CreateTitleAsync(string question)
        {
            //add first 100 chars as question for title
            var titleQuestion = question.Length > 100 ? question.Substring(0, 100) : question;
            var content = $"Summarize the following question in max. 6 words.Use the language of the question for the answer:\n{titleQuestion}";
            var msExtAiResponse = await _titleExtClient.GetResponseAsync([new ChatMessage(ChatRole.User, content)]);

            return msExtAiResponse.Text;
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



        private IChatClient CreateExtChatClient(ChatConfiguration configuration)
        {
            // open Ai
            if (configuration.Provider == LlmProvider.OpenAi)
            {
                _logger.LogTrace("Create OpenAi client.");

                return new OpenAIClient(configuration.ApiKey).GetChatClient(configuration.ModelName).AsIChatClient();
            }

            throw new ArgumentException($"Provider {configuration.Provider} not supported.");
        }

        private ChatConfiguration? GetConfiguration(string name)
        {
            // check per regex if name onyl contains numbers
            if (Regex.IsMatch(name, "^[0-9]+$"))
            {
                var index = int.Parse(name) - 1;
                return index >= 0 && index < _configurations.Count ? _configurations[index] : null;
            }

            return _configurations.SingleOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

    }
}