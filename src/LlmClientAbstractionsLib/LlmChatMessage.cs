namespace FxPu.LlmClient
{
    public class LlmChatMessage
    {
        public LlmChatRole Role { get; set; }
        public string Content { get; set; }
        public IEnumerable<LlmCitation>? Citations { get; set; }
    }
}
