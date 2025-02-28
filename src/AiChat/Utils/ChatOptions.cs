namespace FxPu.AiChat.Utils
{
    public class ChatOptions
    {
        public string? TitleConfigurationName { get; set; }
        public IEnumerable<ChatConfiguration> Configurations { get; set; }

        public static ChatOptions SAMPLE_OPTIONS => new ChatOptions
        {
            Configurations = [
                new ChatConfiguration
                    {
                        Name = "Default",
                        ApiKey = "<ApiKey>",
                        ModelName = "<ModelName>"
                    }
                ]
        };

    }
}
