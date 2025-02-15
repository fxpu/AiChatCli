using System.Text.Json.Serialization;
using FxPu.Perplexity.Client;

namespace Fxpu.Perplexity.Api.Client
{
    public class PerplexityResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }

        [JsonPropertyName("usage")]
        public PerplexityUsage Usage { get; set; }

        [JsonPropertyName("citations")]
        public List<string> Citations { get; set; }

        [JsonPropertyName("related_questions")]
        public List<string> RelatedQuestions { get; set; }
    }
}
