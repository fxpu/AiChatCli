using System.Text;
using FxPu.AiChatLib.Utils;

namespace FxPu.AiChatCli.Utils
{
    record CommandDefinition(string command, string description, Func<string[], string, string?>? validationFunc, Func<string[], string, ValueTask<CommandResult>> commandFunc);

    internal class CommandParser
    {
        private IList<CommandDefinition> _commandDefinitions = new List<CommandDefinition>();

        public CommandParser AddDefinition(string command, string description, Func<string[], string, ValueTask<CommandResult>> commandFunc)
            => AddDefinition(command, description, null, commandFunc);

        public CommandParser AddDefinition(string command, string description, Func<string[], string, string?>? validationFunc, Func<string[], string, ValueTask<CommandResult>> commandFunc)
        {
            _commandDefinitions.Add(new CommandDefinition(command, description, validationFunc, commandFunc));

            return this;
        }

        public Func<ValueTask<CommandResult>> CreateCommandFunc(string line, string content)
        {
            if (!line.StartsWith(":") || line.Length < 2)
            {
                return () => ValueTask.FromResult(new CommandResult(true, "Invalid command."));
            }

            // parse line into command and args. Ignore first : char
            var splitted = line.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var command = splitted[0];
            var args = splitted.Skip(1).ToArray();

            // loop definitions and return if maches
            foreach (var commandDefinition in _commandDefinitions)
            {
                if (command.Equals(commandDefinition.command, StringComparison.InvariantCultureIgnoreCase))
                {
                    // validate arguments
                    if (commandDefinition.validationFunc != null)
                    {
                        var validationError = commandDefinition.validationFunc!(args, content);
                        if (validationError != null)
                        {
                            return () => ValueTask.FromResult(new CommandResult(true, validationError));
                        }
                    }

                    // return command func
                    return async () => await commandDefinition.commandFunc(args, content);
                }
            }

            //  command not found
            return () => ValueTask.FromResult(new CommandResult(true, $"Invalid command \":{command}\", use \":h\" for help."));
        }

        public string Help()
        {
            var sb = new StringBuilder();
            foreach (var commandDefinition in _commandDefinitions)
            {
                sb.AppendLine(commandDefinition.description);
            }

            return sb.ToString();
        }

    }
}
