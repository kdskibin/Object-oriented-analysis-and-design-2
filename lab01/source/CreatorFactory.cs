using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;

namespace source
{
    // Фабрика для создания экземпляров чат-моделей.
    public static class CreatorFactory
    {
        public static List<string> available_models = new List<string>() { "qwen3:14b", "qwen3:14b--thinking", "gemma3:12b", "gpt-oss:20b--low", "gpt-oss:20b--medium", "gpt-oss:20b--high" };
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

            if (key.Contains("gpt-oss"))
            {
                Match match = Regex.Match(modelName, @"--(\w+)");
                string thinking_level = match.Groups[1].Value; // "low", "medium" или "high"
                Debug.WriteLine(thinking_level);
                Debug.WriteLine(Regex.Replace(modelName, @"--(?:\w+)", ""));
                return new GPTOssChatCreator(Regex.Replace(modelName, @"--(?:\w+)", ""), thinking_level);
            }

            throw new Exception($"Модель «{modelName}» не поддерживается. Доступные модели: qwen, gemma, gptoss");
        }
    }
}