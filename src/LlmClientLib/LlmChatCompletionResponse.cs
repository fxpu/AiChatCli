namespace FxPu.LlmClient
{
    public class LlmChatCompletionResponse
    {
        public LlmChatMessage Message { get; set; }
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
        public int? TotalTokens { get; set; }
        public long? ElapsedMilliseconds { get; set; }
    }
}
