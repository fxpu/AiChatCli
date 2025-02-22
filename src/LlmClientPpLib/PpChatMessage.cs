using System.Text.Json.Serialization;

namespace FxPu.LlmClient.Pp
{
    class PpChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
