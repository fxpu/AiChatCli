using System.Reflection;
using System.Text;
using FxPu.AiChatLib.Services;
using Microsoft.Extensions.Logging;

namespace FxPu.AiChatCli.Utils
{
    internal class Commands
    {
        private readonly ILogger<Commands> _logger;
        private readonly IChatService _chatSvc;

        public Commands(ILogger<Commands> logger, IChatService chatSvc)
        {
            _logger = logger;
            _chatSvc = chatSvc;
        }


        [Command("h", "List commands.")]
        public ValueTask<CommandResult> ListCommandsAsync(string[] args, string? input)
        {
            var sb = new StringBuilder();
            var methods = typeof(Commands).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var ca = method.GetCustomAttribute<CommandAttribute>();
                if (ca != null)
                {
                    sb.Append($":{ca.Command} ");
                    if (ca.Arguments != null)
                    {
                        sb.Append($"{ca.Arguments} ");
                    }
                    sb.AppendLine($"- {ca.Description}");
                }
            }

            return new ValueTask<CommandResult>(new CommandResult(sb.ToString(), input));
        }

        [Command("s", "Submits the question.")]
        public async ValueTask<CommandResult> SubmitAsync(string[] args, string? input)
        {
            // write ... to console
            Console.WriteLine("...");

            // ask the llm
            var output = await _chatSvc.SubitAsync(input);

            return new CommandResult(output);
        }


        [Command("n", "New chat.")]
        public async ValueTask<CommandResult> NewChatAsync(string[] args, string? input)
        {
            // new chat and clear console
            await _chatSvc.NewChatAsync();
            Console.Clear();

            return new CommandResult(null);
        }

        [Command("sts", "Displays status e.g. duration, token usage etc.")]
        public async ValueTask<CommandResult> StatusAsync(string[] args, string? input)
        {
            var status = _chatSvc.GetStatus();
            var sb = new StringBuilder();
            sb.AppendLine("Status:");
            sb.Append($"Last duration {status.LastLlmDuration.Seconds} seconds");
            sb.AppendLine($"Prompt tokens {status.LastTokenUsage.PromptTokens}, completion tokens {status.LastTokenUsage.CompletionTokens}, total tokens {status.LastTokenUsage.TotalTokens}");

            return new CommandResult(sb.ToString(), input);
        }
        [Command("lc", "List configurations.")]
        public async ValueTask<CommandResult> ListConfigurationsAsync(string[] args, string? input)
        {
            var configurations = await _chatSvc.ListConfigurationsAsync();
            var i = 0;
            var sb = new StringBuilder();
            sb.AppendLine("Configurations:");
            foreach (var configuration in configurations)
            {
                sb.AppendLine($"{++i}. {configuration.Name}");
            }

            return new CommandResult(sb.ToString(), input);
        }

        [Command("sc", "<name>", "Sets the configuration.")]
        public async ValueTask<CommandResult> SetConfigurationAsync(string[] args, string? input)
        {
            // set configuration
            await _chatSvc.SetConfigurationAsync(args[1]);

            return new CommandResult("Configuration se.", input);
        }

        [Command("cls", "Clears the screen.")]
        public ValueTask<CommandResult> ClearScreenAsync(string[] args, string? input)
        {
            Console.Clear();
            return new ValueTask<CommandResult>(new CommandResult(null));
        }

        [Command("q", "Quit the App.")]
        public ValueTask<CommandResult> QuitAppAsync(string[] args, string? input)
        {
            throw new QuitAppException();
        }
    }
}
