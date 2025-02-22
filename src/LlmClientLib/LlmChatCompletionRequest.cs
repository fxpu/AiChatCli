namespace FxPu.LlmClient
{
    public class LlmChatCompletionRequest
    {
        public string ModelName { get; set; }
        public IEnumerable<LlmChatMessage> Messages { get; set; }
    }
}
