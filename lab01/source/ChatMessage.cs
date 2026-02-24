using System;
using System.Collections.Generic;
using System.Text;

namespace source
{
    public class ChatMessage
    {
        public string Role; // user, system, assistant, tool
        public string Content; // Текст сообщения

        public ChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }
}
