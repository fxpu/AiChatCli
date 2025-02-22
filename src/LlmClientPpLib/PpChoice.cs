using System.Text.Json.Serialization;

namespace FxPu.LlmClient.Pp
{
    class PpChoice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }

        [JsonPropertyName("message")]
        public PpChatMessage Message { get; set; }
    }
}
