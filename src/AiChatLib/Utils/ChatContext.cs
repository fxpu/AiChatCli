﻿using System.Collections.Generic;

namespace FxPu.AiChat.Utils
{
    public class ChatContext
    {
        public ChatConfiguration Configuration { get; set; }
        public IList<ChatMessage> Messages { get; set; }
    }
}
