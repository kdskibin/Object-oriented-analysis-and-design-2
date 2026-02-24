using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace source
{
    public class OllamaService
    {
        private string _baseUrl;
        private int _timeout;

        public OllamaService(string baseUrl = "http://localhost:11434")
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _timeout = 300000;
        }

        // обычный запрос (stream = false)

        public string SendChatRequest(string jsonBody)
        {
            var request = (HttpWebRequest)WebRequest.Create($"{_baseUrl}/api/chat");
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.Timeout = _timeout; // 5 минут

            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody);
            request.ContentLength = bodyBytes.Length;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bodyBytes, 0, bodyBytes.Length);
            }

            using var response = (HttpWebResponse)request.GetResponse();
            using var responseStream = response.GetResponseStream();
            using var reader = new StreamReader(responseStream, Encoding.UTF8);

            string text = reader.ReadToEnd();

            using var doc = JsonDocument.Parse(text);
            return doc.RootElement
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString() ?? string.Empty;
        }

        // потоковый запрос (stream = true)

        public string SendStreamingRequest(string jsonBody, Action<string> onToken)
        {
            var request = (HttpWebRequest)WebRequest.Create($"{_baseUrl}/api/chat");
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.Timeout = _timeout;

            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody);
            request.ContentLength = bodyBytes.Length;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bodyBytes, 0, bodyBytes.Length);
            }

            using var response = (HttpWebResponse)request.GetResponse();
            using var responseStream = response.GetResponseStream();
            using var reader = new StreamReader(responseStream, Encoding.UTF8);

            var fullResponse = new StringBuilder();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;

                if (root.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("content", out var tokenEl))
                {
                    string token = tokenEl.GetString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        fullResponse.Append(token);
                        onToken(token);
                    }
                }

                if (root.TryGetProperty("done", out var done) && done.GetBoolean())
                    break;
            }

            return fullResponse.ToString();
        }
    }
}