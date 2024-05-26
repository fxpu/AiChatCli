using System.Text;
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
        private string? _chatTitle;
        private TokenUsage? _lastTokenUsage;
        private ChatConfiguration _configuration;

        public ChatService(ILogger<ChatService> logger, IOptions<ChatOptions> chatOptionsFactory)
        {
            _logger = logger;
            _chatOptions = chatOptionsFactory.Value;

            _messages = new List<ChatMessage>();

            // use the first configuration as default
            SetConfigurationAndClient(_chatOptions.Configurations.First());
        }

        public ValueTask NewChatAsync()
        {
            _messages.Clear();
            _lastTokenUsage = null;

            return new ValueTask();
        }

        public async ValueTask<string?> SubitAsync(string question)
        {
            return "Hello";
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
            if (_configuration == configuration)
            {
                return;
            }

            _configuration = configuration;
            // TODO: change client and model
        }

        public ValueTask<IEnumerable<ChatConfiguration>> ListConfigurationsAsync()
        {
                return new ValueTask<IEnumerable<ChatConfiguration>>(_chatOptions.Configurations);
        }

    }
}
