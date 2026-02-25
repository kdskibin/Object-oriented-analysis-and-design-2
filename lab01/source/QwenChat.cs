using System.Text.Json.Nodes;

namespace source
{
    public class QwenChat: BaseChat
    {
        private bool _thinking;
        private int _topK;
        private float _topP;

        public QwenChat(string ModelName, float temperature, string systemPrompt, int maxTokens,
            bool thinking, int topK, float topP, OllamaService service): base(ModelName, temperature, systemPrompt, maxTokens, service)
        {
            _thinking = thinking;
            _topK = topK;
            _topP = topP;
        }

        protected override JsonObject BuildOptions()
        {
            return new JsonObject
            {
                ["temperature"] = Temperature,
                ["num_predict"] = MaxTokens,
                ["top_k"] = _topK,
                ["top_p"] = _topP
            };
        }

        protected override JsonObject BuildRequestJson(string userMessage, bool stream)
        {
            JsonObject base_json = base.BuildRequestJson(userMessage, stream);
            base_json["think"] = _thinking;
            return base_json;
        }
    }
}