using System;
using System.Collections.Generic;
using System.Text;

namespace source
{
    // Фабрика для создания экземпляров чат-моделей.
    public static class CreatorFactory
    {
        public static List<string> available_models = new List<string>() { "qwen3:14b", "qwen3:14b--thinking" };
        public static BaseChatCreator GetSuitableCreator(string modelName)
        {
            // Приводим имя модели к нижнему регистру и удаляем лишние пробелы
            string key = modelName.ToLower().Trim() ?? "";

            if (key.Contains("qwen"))
            {
                if (key.Contains("--thinking"))
                    return new QwenChatCreator(modelName.Replace("--thinking", ""), true);
                else
                    return new QwenChatCreator(modelName, false);
            }

            //if (key == "gemma")
            //{
            //    return new GemmaChatCreator();
            //}

            //if (key == "gptoss")
            //{
            //    return new GPTOssChatCreator();
            //}

            throw new Exception($"Модель «{modelName}» не поддерживается. Доступные модели: qwen, gemma, gptoss");
        }
    }
}