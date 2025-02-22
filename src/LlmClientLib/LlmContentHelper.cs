using System.Text;

namespace FxPu.LlmClient
{
    public static class LlmContentHelper
    {
        public static string? CitationsToMarkDowCitations(IEnumerable<LlmCitation>? llmCitations, string? prefix = null)
        {
            if (llmCitations == null || !llmCitations.Any())
            {
                return null;
            }

            var sb = new StringBuilder(prefix);
            int i = 0;
            foreach (var citation in llmCitations)
            {
                i++;
                sb.Append($"[{i}]: {citation.Url}\n");
            }

            return sb.ToString();
        }

    }
}
