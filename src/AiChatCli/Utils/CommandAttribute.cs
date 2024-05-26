namespace FxPu.AiChatCli.Utils
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal class CommandAttribute : Attribute
    {
        public string Command { get; }
        public string? Arguments { get; }
        public string Description { get; }

        public CommandAttribute(string command, string description)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public CommandAttribute(string command, string? arguments, string description)
            : this(command, description)
        {
            Arguments = arguments;
        }

    }
}
