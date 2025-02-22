using System.Text.Json.Serialization;

namespace FxPu.LlmClient.Pp
{
    class PpChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        public static LlmChatMessage ToLlmChatMessage(PpChatMessage ppChatMessage, IEnumerable<string>? citations)
        {
            var role = ppChatMessage.Role switch
            {
                "system" => LlmChatRole.System,
                "user" => LlmChatRole.User,
                "assistant" => LlmChatRole.Assistant,
                _ => throw new ArgumentOutOfRangeException(nameof(ppChatMessage.Role))
            };

            var llmCitations = citations?.Select(c => new LlmCitation
            {
                Url = c
            });

            return new LlmChatMessage
            {
                Role = role,
                Content = ppChatMessage.Content,
                Citations = llmCitations
            };
        }

        public static PpChatMessage FromLlmChatMessage(LlmChatMessage llmChatMessage)
        {
            var role = llmChatMessage.Role switch
            {
                LlmChatRole.System => "system",
                LlmChatRole.User => "user",
                LlmChatRole.Assistant => "assistant",
                _ => throw new ArgumentOutOfRangeException(nameof(llmChatMessage.Role))
            };

            var ppChatMessage = new PpChatMessage
            {
                Role = role,
                Content = llmChatMessage.Content
            };
            // add citations to content
            var citationsText = LlmContentHelper.CitationsToMarkDowCitations(llmChatMessage.Citations, "\n");
            ppChatMessage.Content += citationsText;

            return ppChatMessage;
        }

    }
}
