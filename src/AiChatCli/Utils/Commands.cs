using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;
using System.Text;
using FxPu.AiChat.Services;
using Microsoft.Extensions.Logging;

namespace FxPu.AiChat.Cli.Utils
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
            var output = await _chatSvc.SubmitAsync(input);

            return new CommandResult(output);
        }


        [Command("sv", "Submits the question amd uses OutTb.exe as Viewer.")]
        public async ValueTask<CommandResult> SubmitAndOpenViewerAsync(string[] args, string? input)
        {
            // write ... to console
            Console.WriteLine("...");

            // ask the llm
            var output = await _chatSvc.SubmitAsync(input);

            // get title from status
            var chatStatus = _chatSvc.GetStatus();
            var title = chatStatus.Title ?? "AiChatCli";

            // start a pipe and OutTb.exe
            await using var pipeStream = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            var pipeHandle = pipeStream.GetClientHandleAsString();
            Process.Start(new ProcessStartInfo("OutTb.exe")
            {
                UseShellExecute = false,
                Arguments = $"-p {pipeHandle} -t \"{title}\""
            });
            pipeStream.DisposeLocalCopyOfClientHandle();

            // wait for llm and write answer to pipe
            await using var writer = new StreamWriter(pipeStream, Encoding.UTF8);
            await writer.WriteAsync(output);
            await writer.FlushAsync();

            // wait for pipe consumption
#pragma warning disable CA1416 // Validate platform compatibility
            pipeStream.WaitForPipeDrain();
#pragma warning restore CA1416 // Validate platform compatibility

            return new CommandResult(output);
        }

        [Command("nc", "New chat.")]
        public async ValueTask<CommandResult> NewChatAsync(string[] args, string? input)
        {
            // new chat and clear console
            await _chatSvc.NewChatAsync();
            Console.Clear();

            return new CommandResult(null);
        }

        [Command("ncs", "New chat, keep system message.")]
        public async ValueTask<CommandResult> NewChatKeepSystemMessageAsync(string[] args, string? input)
        {
            // new chat and clear console
            await _chatSvc.NewChatKeepSystemMessageAsync();
            Console.Clear();

            return new CommandResult("system message set.");
        }

        [Command("dst", "Displays status e.g. duration, token usage etc.")]
        public async ValueTask<CommandResult> StatusAsync(string[] args, string? input)
        {
            var status = _chatSvc.GetStatus();
            var sb = new StringBuilder();
            sb.AppendLine("Status:");
            sb.Append($"Last duration {status.LastLlmDuration.Seconds} seconds");
            sb.AppendLine($"Prompt tokens {status.LastTokenUsage.PromptTokens}, completion tokens {status.LastTokenUsage.CompletionTokens}, total tokens {status.LastTokenUsage.TotalTokens}");
            sb.AppendLine($"Configuration {status.ConfigurationName}");
            sb.AppendLine($"System message set {status.IsSystemMessageSet}");

            return new CommandResult(sb.ToString(), input);
        }

        [Command("ss", "[<file>]", "Set system message.")]
        public async ValueTask<CommandResult> SetSystemMessageAsync(string[] args, string? input)
        {
            if (args.Length == 2)
            {
                // open with file name in arg1
                await _chatSvc.OpenSystemMessageAsync(args[1]);
            }
            else
            {
                // set input as system message
                await _chatSvc.SetSystemMessageAsync(input);
            }

            return new CommandResult("System message set.");
        }

        [Command("lcf", "List configurations.")]
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

        [Command("scf", "<name>", "Sets the configuration.")]
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
