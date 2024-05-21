using FxPu.AiChat.Cli.Utils;
using FxPu.AiChat.Services;
using Microsoft.Extensions.Logging;

namespace FxPu.AiChat.Cli
{
    internal class Processor
    {
        private readonly ILogger<Processor> _logger;
        private readonly IChatService _chatSvc;

        public Processor(ILogger<Processor> logger, IChatService chatSvc)
        {
            _logger = logger;
            _chatSvc = chatSvc;
        }

        public async ValueTask RunAsync()
        {
            var commandParser = new CommandParser()
                .AddDefinition("s", "s - Submit question.", async (args, content) => await _chatSvc.SubitAsync(content))
                .AddDefinition("n", "n - New chat.", async(args, content) => await _chatSvc.NewChatAsync());

            // TODO: implement loop
            var commandFunc = commandParser.CreateCommandFunc(":s", "Hallo");
            var commandResult = await commandFunc();


               
        }

    }
}
