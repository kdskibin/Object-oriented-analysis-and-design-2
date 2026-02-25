using System.Text;
using System.Text.Json.Nodes;

namespace source
{
    public abstract class BaseChat
    {
        public string SystemPrompt;
        public int MaxTokens;
        public List<ChatMessage> Context;

        protected string ModelName;
        protected float Temperature;
        protected OllamaService Service;

        protected BaseChat(string modelName, float temperature, string systemPrompt, int maxTokens, OllamaService service)
        {
            ModelName = modelName;
            Temperature = temperature;
            SystemPrompt = systemPrompt;
            MaxTokens = maxTokens;
            Service = service;
            Context = new List<ChatMessage>();

            if (!string.IsNullOrEmpty(systemPrompt))
                Context.Add(new ChatMessage("system", systemPrompt));
        }

        protected abstract JsonObject BuildOptions();

        // Собирает JSON-тело запроса к /api/chat.
        protected virtual JsonObject BuildRequestJson(string userMessage, bool stream)
        {
            Context.Add(new ChatMessage("user", userMessage));

            var messages = new JsonArray();
            foreach (var m in Context)
            {
                messages.Add(new JsonObject
                {
                    ["role"] = m.Role,
                    ["content"] = m.Content
                });
            }

            var body = new JsonObject
            {
                ["model"] = ModelName,
                ["messages"] = messages,
                ["stream"] = stream,
                ["options"] = BuildOptions()
            };
            return body;
        }

        // Генерация Без стриминга
        public virtual string GenerateWithoutStreaming(string request)
        {
            string json = BuildRequestJson(request, stream: false).ToJsonString();
            string response = Service.SendChatRequest(json);
            Context.Add(new ChatMessage("assistant", response));
            return response;
        }

        // Генерация СО стримингом
        public virtual string GenerateWithStreaming(string request, Action<string> onToken)
        {
            string json = BuildRequestJson(request, stream: true).ToJsonString();
            string response = Service.SendStreamingRequest(json, onToken);
            Context.Add(new ChatMessage("assistant", response));
            return response;
        }

        // Очистка контекста
        public virtual void ClearContext()
        {
            Context.Clear();
            if (!string.IsNullOrEmpty(SystemPrompt))
                Context.Add(new ChatMessage("system", SystemPrompt));
        }
    }
}