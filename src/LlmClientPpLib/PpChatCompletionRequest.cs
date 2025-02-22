using System.Text.Json.Serialization;

namespace FxPu.LlmClient.Pp
{
    class PpChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "sonar-pro";

        [JsonPropertyName("messages")]
        public List<PpChatMessage> Messages { get; set; } = new();

        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; }

        [JsonPropertyName("temperature")]
        public float? Temperature { get; set; }

        [JsonPropertyName("top_p")]
        public float? TopP { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        [JsonPropertyName("return_citations")]
        public bool ReturnCitations { get; set; } = true;

        [JsonPropertyName("return_related_questions")]
        public bool ReturnRelatedQuestions { get; set; } = true;
    }
}
