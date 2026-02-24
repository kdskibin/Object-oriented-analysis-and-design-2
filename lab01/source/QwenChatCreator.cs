using System;
using System.Collections.Generic;
using System.Text;

namespace source
{
    public class QwenChatCreator : BaseChatCreator
    {
        public bool Thinking;
        public int TopK;
        public float TopP;

        public QwenChatCreator(float temperature = 0.7f, bool thinking = true,
            int topK = 40, float topP = 0.9f, OllamaService service = null): base(temperature, service)
        {
            Thinking = thinking;
            TopK = topK;
            TopP = topP;
        }

        public override BaseChat MakeChat(string systemPrompt = "", int maxTokens = 32768)
        {
            return new QwenChat(Temperature, systemPrompt, maxTokens, Thinking, TopK, TopP, Service);
        }
    }
}
