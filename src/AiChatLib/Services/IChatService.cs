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
        public ValueTask<CommandResult> NewChatAsync();
        public ValueTask<CommandResult> SubitAsync(string question);
    }
}
