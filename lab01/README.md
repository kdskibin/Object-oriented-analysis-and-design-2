# LMGUI (Language Model User Interface)

##### Предоставляет графический интерфейс для использования языковых моделей Ollama. Вся коммуникация осуществляется через протокол HTTP. В первую очередь работа нацелена на исопльзования паттерна "Factory Method", используя язык программирования C# и WinForms. Клиентская часть - приложение с простым GUI. Главная функция клиента это взаимодействие с сервером (отправка и полуение сообщений). Серверная часть, Ollama, обрабатывает запрос пользователя.

## Структура репозитория

* **static**

    Статичные файлы, не изменяемые в процессе работы программы (изображения и т.п.)

* **configs**

    `json` файл с конфигурацией провайдера языковых моделей. На данный момент поддерживается только Ollama.

## Описание проблемы:

Различные языковые модели требуют различных параметров (temperature, thinking, top_k, repetition_penalty...) для наивысшего качества генерируемых ответов. Паттерн **"Фабричный Метод"** как никакой другой подходит для реализации функционала создания и использования различных моделей при общих базовых фукнциях и атрибутах.

## Решение

Выбор за тем, какую модель инстанцировать определяется пользователем. Для создания базового креатора `BaseChatCreator` используется статичный метод `GetSuitableCreator`. Данный метод является единственным местом в программе с условями, изменение которых отвечает за расширяемость поддержки новых моделей. Фрагмент кода данной функции:

```csharp
        public static BaseChatCreator GetSuitableCreator(string modelName)
        {
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
```

Стоит отметить, что классы `QwenChatCreator GemmaChatModel GPTOssChatModel` являются хранилищем параметров модели по умолчанию. Праметры по умолчанию - парметры, определенные создателем модели и считаются лучшими для вопросно-ответного использования модели.

## Диаграммы UML

### Диаграмма классов

Концепция текущего подхода может быть описана диаграммой классов

![Концептуальная диаграмма классов](lab01/static/OOAP_Lab1.png)

Такая архитектура позволяет обеспечить расширяемость функционала с минимальной переработкой существующего кода.

## Альтернатива без ислпользования паттерна

Без использования паттерна **"Factory Method"** создание подобного приложения потребовало бы множество ветвлений для проверок, экземпляром какого класса является обьект. Обеспечить сравнимый уровень расширяемости приложения практически невыполнимая задача. Рассмотрим пример кода без использования паттерна:

```csharp
using source;
using System;

public class Program
{
    private static OllamaService service = new OllamaService(); // сервис один на все приложение

    public static string AskModel(string modelName, string userMessage)
    {
        BaseChat chat;

        // 1. Определяем, какую модель создать (if/else по имени)
        if (modelName.Contains("qwen"))
        {
            bool thinking = modelName.Contains("--thinking");
            // приходится явно передавать все параметры, даже если они стандартные
            chat = new QwenChat(
                modelName.Replace("--thinking", "").Trim(),
                temperature: 0.5f,
                systemPrompt: "",
                maxTokens: 16384,
                thinking: thinking,
                topK: 40,
                topP: 0.9f,
                service: service
            );
        }
        else if (modelName.Contains("gptoss"))
        {
            string thinkingLevel = "medium";
            if (modelName.Contains("--low")) thinkingLevel = "low";
            else if (modelName.Contains("--high")) thinkingLevel = "high";

            chat = new GPTOssChat(
                Regex.Replace(modelName, @"--\w+", "").Trim(),
                temperature: 0.7f,
                systemPrompt: "",
                maxTokens: 16384,
                thinking_level: thinkingLevel,
                topK: 40,
                topP: 0.9f,
                service: service
            );
        }
        else if (modelName.Contains("gemma"))
        {
            chat = new GemmaChat(
                modelName,
                temperature: 0.7f,
                systemPrompt: "",
                maxTokens: 16384,
                topK: 40,
                topP: 0.9f,
                service: service
            );
        }
        else
        {
            throw new NotSupportedException($"Модель {modelName} не поддерживается");
        }

        // 2. Выполняем запрос
        return chat.GenerateWithoutStreaming(userMessage);
    }
}
```

Без создателей чата `ChatCreators` поддержка кода превращается в кошмар. В любом месте, где требуется создать модель, потребуется дублировать все `if/else`. Под различные сценарии использования (RAG-система, перевод, написание кода, вопросно-оветная система...) точно потребуются различные модели. Создание одного `Creator` для каждой модели может решить проблему расширяемости приложения. Просто меняем необходимые параметры у обьекта `Creator` и создаем нужную модель, подходящую под текущую задачу.