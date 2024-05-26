using FxPu.AiChatLib.Utils;

namespace FxPu.AiChatLib.Services
{
    public interface IChatService
    {
        ValueTask<IEnumerable<ChatConfiguration>> ListConfigurationsAsync();
        ValueTask NewChatAsync();
        ValueTask SetConfigurationAsync(string name);
        ValueTask<string?> SubitAsync(string question);
    }
}