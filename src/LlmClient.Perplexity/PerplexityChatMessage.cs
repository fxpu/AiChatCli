using System.Text.Json.Serialization;

namespace FxPu.LlmClient.Perplexity
{
    class PerplexityChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
