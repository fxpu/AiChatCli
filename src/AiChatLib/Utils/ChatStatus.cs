namespace FxPu.AiChat.Utils
{
    public class ChatStatus
    {
        public ChatStatus()
        {
            QuestionNumber = 1;
            LastLlmDuration = TimeSpan.Zero;
            LastTokenUsage = new TokenUsage(0, 0, 0);
        }
        public int QuestionNumber { get; set; }
        public TimeSpan LastLlmDuration { get; set; }
        public TokenUsage LastTokenUsage { get; set; }
        public bool IsSystemMessageSet { get; set; }
        public string? ConfigurationName { get; set; }
        public string? Title { get; set; }
    }
}
