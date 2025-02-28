namespace FxPu.AiChat.Utils
{
    public class ChatConfiguration
    {
        public string? Name { get; set; }
        public LlmProvider Provider { get; set; }
        public string? ApiEndpoint { get; set; }
        public string ApiKey { get; set; }
        public string ModelName { get; set; }
    }
}
