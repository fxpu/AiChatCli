using System.Reflection;
using System.Text;
using FxPu.AiChatLib.Services;
using Microsoft.Extensions.Logging;

namespace FxPu.AiChatCli.Utils
{
    internal class CommandProcessor : ICommandProcessor
    {
        private readonly ILogger<CommandProcessor> _logger;
        private readonly Commands _commands;
        private readonly IChatService _chatSvc;

        public CommandProcessor(ILogger<CommandProcessor> logger, Commands commands, IChatService chatSvc)
        {
            _logger = logger;
            _commands = commands;
            _chatSvc = chatSvc;
        }

        public async ValueTask RunAsync()
        {
            // write help and prompt
            Console.WriteLine(":h - List commands.");
            Console.Write(Prompt());
            // loop input
            var inputSb = new StringBuilder();

            while (true)
            {
                var line = Console.ReadLine();

                // line does not start with :, normal line
                if (!line.StartsWith(":"))
                {
                    inputSb.AppendLine(line);
                    continue;
                }

                // invoke, maybe quit
                CommandResult commandResult = null!;
                try
                {
                    commandResult = await InvokeCommandAsync(line, inputSb.ToString());
                }
                catch (QuitAppException)
                    {
                    _logger.LogDebug("Quit app");
                    return;
                }

                
                // clear and set input?
                inputSb.Clear();
                if (commandResult.NewInput != null)
                {
                    inputSb.Append(commandResult.NewInput);
                }

                // display result
                Console.WriteLine(commandResult.Output);

                // write prompt
                Console.Write(Prompt());
            }
        }

        private string Prompt()
        {
            var status = _chatSvc.GetStatus();

            return status.QuestionNumber == 1 ? ">" : $"{status.QuestionNumber} >";
        }

        private async ValueTask<CommandResult> InvokeCommandAsync(string line, string? input)
        {
            // check line
            if (!line.StartsWith(":") || line.Length < 2)
            {
                return new CommandResult(true, "Invalid command.", input);
            }

            // parse line into arhgs. args[0] is the command
            var args = line.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // find method and command attribute for command
            MethodInfo? commandMethod = null;
            string? argsCsv = null;
            var methods = typeof(Commands).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var commandAttribute = method.GetCustomAttribute<CommandAttribute>();
                if (commandAttribute != null && commandAttribute.Command == args[0])
                {
                    commandMethod = method;
                    argsCsv = commandAttribute.Arguments;
                    break;
                }
            }

            // command not found
            if (commandMethod == null)
            {
                return new CommandResult(true, "Invalid command \":{args[0]}\".", input);
            }

            // check argument count
            if (argsCsv != null)
            {
                int argsCount = argsCsv.Split(',').Length;
                if (argsCount + 1 != args.Length)
                {
                    return new CommandResult(true, "Invalid number of arguments for command \":{args[0]}{argsCsv}\".", input);
                }
            }

            // invoke method and do exception handling
            try
            {
                return await (ValueTask<CommandResult>) commandMethod.Invoke(_commands, [args, input]);
            }
            catch (QuitAppException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error invoking command :{0}.", args[0]);
                return new CommandResult(true, e.Message, input);
            }
        }

    }
}
