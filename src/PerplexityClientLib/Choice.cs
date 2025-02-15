using System.Text.Json.Serialization;

namespace FxPu.Perplexity.Client
{
    public class Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }

        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }
}
