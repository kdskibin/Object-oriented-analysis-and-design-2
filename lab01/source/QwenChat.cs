using System.Text.Json.Nodes;

namespace source
{
    public class QwenChat: BaseChat
    {
        private bool _thinking;
        private int _topK;
        private float _topP;

        public QwenChat(float temperature, string systemPrompt, int maxTokens,
            bool thinking, int topK, float topP, OllamaService service): base("qwen3:14b", temperature, systemPrompt, maxTokens, service)
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

        protected override string BuildRequestJson(string userMessage, bool stream)
        {
            if (!_thinking)
                userMessage = "/no_think " + userMessage;
            return base.BuildRequestJson(userMessage, stream);
        }
    }
}