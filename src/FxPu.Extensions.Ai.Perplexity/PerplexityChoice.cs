using System.Text.Json.Serialization;

namespace FxPu.Extensions.Ai.Perplexity
{
    class PerplexityChoice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }

        [JsonPropertyName("message")]
        public PerplexityChatMessage Message { get; set; }
    }
}
