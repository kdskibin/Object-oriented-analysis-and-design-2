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

        public QwenChatCreator(string modelname, bool thinking = true, float temperature = 0.5f,
            int topK = 40, float topP = 0.9f, OllamaService service = null) : base(temperature, service)
        {
            ModelName = modelname;
            Thinking = thinking;
            TopK = topK;
            TopP = topP;
        }

        public override BaseChat MakeChat(string systemPrompt = "", int maxTokens = 16384)
        {
            return new QwenChat(ModelName, Temperature, systemPrompt, maxTokens, Thinking, TopK, TopP, Service);
        }
    }
    public class GPTOssChatCreator : BaseChatCreator
    {
        public string ModelName;
        public string thinking_level;
        public int TopK;
        public float TopP;

        public GPTOssChatCreator(string modelname, string thinking_level = "medium", float temperature = 0.7f,
            int topK = 40, float topP = 0.9f, OllamaService service = null) : base(temperature, service)
        {
            ModelName = modelname;
            this.thinking_level = thinking_level;
            TopK = topK;
            TopP = topP;
        }

        public override BaseChat MakeChat(string systemPrompt = "", int maxTokens = 16384)
        {
            return new GPTOssChat(ModelName, Temperature, systemPrompt, maxTokens, thinking_level, TopK, TopP, Service);
        }
    }

    public class GemmaChatCreator : BaseChatCreator
    {
        public string ModelName;
        public int TopK;
        public float TopP;

        public GemmaChatCreator(string modelname, float temperature = 0.7f,
            int topK = 40, float topP = 0.9f, OllamaService service = null) : base(temperature, service)
        {
            ModelName = modelname;
            TopK = topK;
            TopP = topP;
        }

        public override BaseChat MakeChat(string systemPrompt = "", int maxTokens = 16384)
        {
            return new GemmaChat(ModelName, Temperature, systemPrompt, maxTokens, TopK, TopP, Service);
        }
    }
}
