using FxPu.AiChatLib.Utils;

namespace FxPu.AiChatLib.Services
{
    public interface IChatService
    {
        ChatStatus GetStatus();
        ValueTask<IEnumerable<ChatConfiguration>> ListConfigurationsAsync();
        ValueTask NewChatAsync();
        ValueTask NewChatKeepSystemMessageAsync();
        ValueTask SetConfigurationAsync(string name);
        ValueTask SetSystemMessageAsync(string systemMessage);
        ValueTask<string?> SubitAsync(string question);
    }
}