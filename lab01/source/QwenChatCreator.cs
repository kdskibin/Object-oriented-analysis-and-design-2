using System;
using System.Collections.Generic;
using System.Text;

namespace source
{
    public class QwenChatCreator : BaseChatCreator
    {
        public string ModelName;
        public bool Thinking;
        public int TopK;
        public float TopP;

        public QwenChatCreator(string modelname, bool thinking = true, float temperature = 0.7f,
            int topK = 40, float topP = 0.9f, OllamaService service = null): base(temperature, service)
        {
            ModelName = modelname;
            Thinking = thinking;
            TopK = topK;
            TopP = topP;
        }

        public override BaseChat MakeChat(string systemPrompt = "", int maxTokens = 32768)
        {
            return new QwenChat(ModelName, Temperature, systemPrompt, maxTokens, Thinking, TopK, TopP, Service);
        }
    }
}
