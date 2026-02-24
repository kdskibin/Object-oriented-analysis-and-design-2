using System;
using System.Collections.Generic;
using System.Text;

namespace source
{
    public abstract class BaseChatCreator
    {
        public float Temperature;

        protected OllamaService Service;

        protected BaseChatCreator(float temperature = 0.7f, OllamaService service = null)
        {
            Temperature = temperature;
            Service = service ?? new OllamaService();
        }

        public abstract BaseChat MakeChat(string systemPrompt = "", int maxTokens = 32768);
    }
}
