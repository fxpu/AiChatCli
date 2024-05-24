using System.Text;
using FxPu.AiChatLib.Services;
using FxPu.AiChatLib.Utils;
using Microsoft.Extensions.Logging;

namespace FxPu.AiChatCli.Utils
{
    internal class CommandProcessor : ICommandProcessor
    {
        private readonly ILogger<CommandProcessor> _logger;
        private readonly IChatService _chatSvc;
        private readonly CommandParser _commandParser;
        private readonly string _help;

        public CommandProcessor(ILogger<CommandProcessor> logger, IChatService chatSvc)
        {
            _logger = logger;
            _chatSvc = chatSvc;

            // define commands
            _commandParser = new CommandParser()
    .AddDefinition("s", ":s - Submit question.", async (args, content) => await _chatSvc.SubitAsync(content))
    .AddDefinition("n", ":n - New chat.", async (args, content) => await _chatSvc.NewChatAsync())
    .AddDefinition("sc", ":sc <name> - set configuration.", async (args, content) => await _chatSvc.SetConfigurationAsync(args[0]))
    .AddDefinition("lc", ":lc - list configurations.", async (args, content) => await _chatSvc.ListConfigurationsAsync())
    .AddDefinition("h", ":h - Display help.", (args, content) => ValueTask.FromResult(new CommandResult(false, _help)))
        .AddDefinition("q", ":q - Quit the app.", (args, content) => throw new QuitException());

            // create help text
            _help = _commandParser.Help().Trim();
        }

        public async ValueTask RunAsync()

        {
            // write help and prompt
            Console.WriteLine(_help);
            Console.Write(Prompt());

            while (true)
            {
                var sb = new StringBuilder();
                var line = Console.ReadLine();

                // line does not start with :, normal line
                if (!line.StartsWith(":"))
                {
                    sb.AppendLine(line);
                    continue;
                }

                // parse command
                var content = sb.ToString();
                sb.Clear();
                _logger.LogTrace("Parse command {line}", line);
                var commandFunc = _commandParser.CreateCommandFunc(line, content);

                // execute command
                try
                {
                    var commandResult = await commandFunc();

                    // display result
                    Console.WriteLine(commandResult.Output.TrimEnd());
                }
                catch (QuitException)
                {
                    _logger.LogInformation("Quit program");
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e, "Error execute command.");
                    Console.WriteLine("Error: {0}", e.Message);
                }

                // write prompt
                Console.Write(Prompt());
            }
        }

        private string Prompt()
        {
            return ">";
        }

    }
}
