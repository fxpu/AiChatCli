namespace FxPu.LlmClient
{
    public class LlmClientOptions
    {
        public string? ApiEndpoint { get; set; }
        public string ApiKey { get; set; }
        public string ModelName { get; set; }
        public bool NoAutoInlineCitations { get; set; }
    }
}
