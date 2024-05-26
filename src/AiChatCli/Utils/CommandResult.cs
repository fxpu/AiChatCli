namespace FxPu.AiChatCli.Utils
{
    internal class CommandResult
    {
        private bool _isError;
        private string? _Output;
        private string? _newInput;

        public CommandResult(string? output, string? newInput = null)
            : this(false, output, newInput)
        {
        }

        public CommandResult(bool isError, string? output, string? newInput = null)
        {
            _isError = isError;
            _Output = output?.Trim();
            _newInput = newInput;
        }

        public bool IsError => _isError;
        public string? Output => _Output;
        public string? NewInput => _newInput;
    }
}
