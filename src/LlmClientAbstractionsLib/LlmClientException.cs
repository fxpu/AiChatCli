namespace FxPu.LlmClient
{
    public class LlmClientException : Exception
    {
        public LlmClientException()
        {
        }

        public LlmClientException(string? message, Exception? innerException) : base(message, innerException)
        {
        }


        public LlmClientException(string? message) : base(message)
        {
        }

    }
}
