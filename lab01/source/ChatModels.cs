using System.Text.Json.Nodes;

namespace source
{
    public class QwenChat : BaseChat
    {
        private bool thinking;
        private int topK;
        private float topP;

        public QwenChat(string ModelName, float temperature, string systemPrompt, int maxTokens,
            bool thinking, int topK, float topP, OllamaService service) : base(ModelName, temperature, systemPrompt, maxTokens, service)
        {
            this.thinking = thinking;
            this.topK = topK;
            this.topP = topP;
        }

        protected override JsonObject BuildOptions()
        {
            return new JsonObject
            {
                ["temperature"] = Temperature,
                ["num_predict"] = MaxTokens,
                ["top_k"] = topK,
                ["top_p"] = topP
            };
        }

        protected override JsonObject BuildRequestJson(string userMessage, bool stream)
        {
            JsonObject base_json = base.BuildRequestJson(userMessage, stream);
            base_json["think"] = thinking;
            return base_json;
        }
    }

    public class GPTOssChat : BaseChat
    {
        private string thinking_level;
        private int topK;
        private float topP;

        public GPTOssChat(string ModelName, float temperature, string systemPrompt, int maxTokens,
            string thinking_level, int topK, float topP, OllamaService service) : base(ModelName, temperature, systemPrompt, maxTokens, service)
        {
            this.thinking_level = thinking_level;
            this.topK = topK;
            this.topP = topP;
        }

        protected override JsonObject BuildOptions()
        {
            return new JsonObject
            {
                ["temperature"] = Temperature,
                ["num_predict"] = MaxTokens,
                ["top_k"] = topK,
                ["top_p"] = topP
            };
        }

        protected override JsonObject BuildRequestJson(string userMessage, bool stream)
        {
            JsonObject base_json = base.BuildRequestJson(userMessage, stream);
            base_json["think"] = thinking_level;
            return base_json;
        }
    }

    public class GemmaChat : BaseChat
    {
        private int topK;
        private float topP;

        public GemmaChat(string ModelName, float temperature, string systemPrompt, int maxTokens,
            int topK, float topP, OllamaService service = null) : base(ModelName, temperature, systemPrompt, maxTokens, service)
        {
            this.topK = topK;
            this.topP = topP;
        }

        protected override JsonObject BuildOptions()
        {
            return new JsonObject
            {
                ["temperature"] = Temperature,
                ["num_predict"] = MaxTokens,
                ["top_k"] = topK,
                ["top_p"] = topP
            };
        }

        protected override JsonObject BuildRequestJson(string userMessage, bool stream)
        {
            JsonObject base_json = base.BuildRequestJson(userMessage, stream);
            return base_json;
        }
    }
}