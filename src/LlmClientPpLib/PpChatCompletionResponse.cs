using System.Text.Json.Serialization;

namespace FxPu.LlmClient.Pp
{
    class PpChatCompletionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("choices")]
        public List<PpChoice> Choices { get; set; }

        [JsonPropertyName("usage")]
        public PpUsage Usage { get; set; }

        [JsonPropertyName("citations")]
        public List<string> Citations { get; set; }

        [JsonPropertyName("related_questions")]
        public List<string> RelatedQuestions { get; set; }
    }
}
