namespace FxPu.LlmClient
{
    public class LlmChatCompletionRequest
    {
        public IEnumerable<LlmChatMessage> Messages { get; set; }
    }
}
