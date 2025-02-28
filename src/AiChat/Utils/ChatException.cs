﻿namespace FxPu.AiChat.Utils
{
    public class ChatException : Exception
    {
        public ChatException()
        {
        }

        public ChatException(string? message) : base(message)
        {
        }

        public ChatException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}
