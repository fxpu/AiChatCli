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

        public async ValueTask<CommandResult> NewChatAsync()
        {
            return new CommandResult(false, "New chat created.");
        }

        public async ValueTask<CommandResult> SubitAsync(string question)
        {
            return new CommandResult(false, "Hello!");
        }

        public ValueTask<CommandResult>   SetConfigurationAsync(string name)
        {
            var configuration = _chatOptions.Configurations.SingleOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (configuration == null)
            {
                return ValueTask.FromResult(new CommandResult(true, $"Configuration \"{name} not found."));
            }

            SetConfigurationAndClient(configuration);

            return ValueTask.FromResult(new CommandResult($"Configuration set to \"{_configuration.Name}\"."));
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

        public ValueTask<CommandResult> ListConfigurationsAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("List of configurations");
            var i = 0;
            foreach (var configuration in _chatOptions.Configurations)
            {
                i++; ;
                sb.AppendLine($"{i}. {configuration.Name} - model {configuration.ModelName}");
            }

            return ValueTask.FromResult(new CommandResult(sb.ToString()));
        }

    }
}
