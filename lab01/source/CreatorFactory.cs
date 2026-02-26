using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace source
{
    // Фабрика для создания экземпляров чат-моделей.
    public static class CreatorFactory
    {
        public static List<string> available_models = new List<string>() { "qwen3:14b", "qwen3:14b--thinking", "gemma3:12b", "gptoss:20b--low", "gptoss:20b--medium", "gptoss:20b--high" };
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

            if (key.Contains("gemma"))
            {
                return new GemmaChatCreator(modelName);
            }

            if (key.Contains("gptoss"))
            {
                string thinking_level = Regex.Match(modelName, @"--(\w+)").ToString();
                return new GPTOssChatCreator(Regex.Replace(modelName, @"--(?:\w+)", ""), thinking_level);
            }

            throw new Exception($"Модель «{modelName}» не поддерживается. Доступные модели: qwen, gemma, gptoss");
        }
    }
}