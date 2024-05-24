namespace FxPu.AiChatLib.Utils
{
    public class CommandResult
    {
        private bool _isError;
        private string? _Output;

        public CommandResult(string? output)
            : this(false, output)
        {
        }

        public CommandResult(bool isError, string? output)
        {
            _isError = isError;
            _Output = output?.Trim();
        }

        public bool IsError => _isError;
        public string? Output => _Output;
    }
}
