using FxPu.AiChat.Utils;

namespace FxPu.AiChat.Services
{
    public interface IChatService
    {
        ChatStatus GetStatus();
        ValueTask<IEnumerable<ChatConfiguration>> ListConfigurationsAsync();
        ValueTask NewChatAsync();
        ValueTask NewChatKeepSystemMessageAsync();
        ValueTask OpenSystemMessageAsync(string fileName);
        ValueTask SetConfigurationAsync(string name);
        ValueTask SetSystemMessageAsync(string systemMessage);
        ValueTask<string?> SubitAsync(string question);
    }
}