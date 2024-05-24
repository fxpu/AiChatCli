using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FxPu.AiChatLib.Utils;

namespace FxPu.AiChatLib.Services
{
    public interface IChatService
    {
        ValueTask<CommandResult> ListConfigurationsAsync();
        public ValueTask<CommandResult> NewChatAsync();
        ValueTask<CommandResult> SetConfigurationAsync(string name);
        public ValueTask<CommandResult> SubitAsync(string question);
    }
}
