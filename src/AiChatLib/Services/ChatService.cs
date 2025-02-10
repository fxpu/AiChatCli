using System.ClientModel;
using System.Diagnostics;
using FxPu.AiChat.Utils;
using FxPu.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.RealtimeConversation;

namespace FxPu.AiChat.Services
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
            var llmChatClient = _llmClient.GetChatClient(_configuration.ModelName);

            // recent messages
            var llmMessages = _messages.Select(m => ConvertMessage(m)).ToList();

            // add question
            llmMessages.Add(new OpenAI.Chat.UserChatMessage(question));

            // ask the llm
            var sw = Stopwatch.StartNew();
                var llmResult = await llmChatClient.CompleteChatAsync(llmMessages);
            sw.Stop();

            // last tokens and time
            //_chatStatus.LastTokenUsage = new TokenUsage(llmResponse.Usage.PromptTokens, llmResponse.Usage.CompletionTokens, llmResponse.Usage.TotalTokens);
            _chatStatus.LastLlmDuration = sw.Elapsed;

            // add question and answer to messages
            var answer = llmResult.Value.Content.FirstOrDefault()?.Text;
            _messages.Add(new ChatMessage { Role = "Assistant", Content = answer });


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
            if (_configuration.AzureOpenAiEndpoint == null)
            {
                if (_configuration.ApiEndpoint == null)
                {
                    _logger.LogTrace("Use www.openai.com endpoint.");
                    _llmClient = new OpenAIClient(_configuration.ApiKey);
                }
                else
                {
                    _logger.LogTrace("Use {endpoint}.", _configuration.ApiEndpoint);
                                                            _llmClient = new OpenAIClient(new ApiKeyCredential(_configuration.ApiKey), new OpenAIClientOptions { Endpoint = new Uri(_configuration.ApiEndpoint) });
                }
            }
            else
            {
                _logger.LogTrace("Use AzureOpenAiEndpoint {endpoint}.", _configuration.AzureOpenAiEndpoint);
                throw new NotImplementedException("Azure not supported.");
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
            ThrowIf.IsEmpty<ChatException>(fileName, $"File name is empty.");

            var foundFileName = SearchFile(fileName);
            ThrowIf.IsNull<ChatException>(foundFileName, $"File {fileName} not found.");

            // read system  message from file
            var systemMessage = await File.ReadAllTextAsync(foundFileName);
            ThrowIf.IsEmpty<ChatException>(systemMessage, $"File {fileName} contains no system message.");

            await SetSystemMessageAsync(systemMessage);
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
            return null;
            //add first 100 chars as question for title
            //var titleQuestion = question.Length > 100 ? question.Substring(0, 100) : question;
            //var content = $"Summarize the following question in max. 3 words.Use the language of the question for the answer:\n{titleQuestion}";
        }

        private string? SearchFile(string fileName)
        {
            if (Value.IsEmpty(fileName))
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

        

private OpenAI.Chat.ChatMessage ConvertMessage(ChatMessage message)
{
    return message.Role switch
    {
        "system" => new OpenAI.Chat.SystemChatMessage(message.Content),
        "user" => new OpenAI.Chat.UserChatMessage(message.Content),
        "assistant" => new OpenAI.Chat.AssistantChatMessage(message.Content),
        _ => throw new NotImplementedException($"Role {message.Role} not implemented.")
    };
}

        private void test()
        {
            
        }

    }
}