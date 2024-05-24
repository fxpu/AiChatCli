using FxPu.AiChatLib.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FxPu.AiChatLib.Services
{
    public class ChatService : IChatService
    {
        private readonly ILogger<ChatService> _logger;
        private readonly ChatOptions _chatOptions;

        public ChatService(ILogger<ChatService> logger, IOptions<ChatOptions> chatOptionsFactory)
        {
            _logger = logger;
            _chatOptions = chatOptionsFactory.Value;
        }

        public async ValueTask<CommandResult> NewChatAsync()
        {
            return new CommandResult(false, "New chat created.");
        }

        public async ValueTask<CommandResult> SubitAsync(string question)
        {
            return new CommandResult(false, "Hello!");
        }

    }
}
