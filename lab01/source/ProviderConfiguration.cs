using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace source
{
    public abstract class ProviderConfiguration
    {
        public string base_url { get; protected set; }
    }

    public class OllamaConfiguration : ProviderConfiguration
    {
        public string ip { get; protected set; }
        public string port { get; protected set; }
        public OllamaConfiguration(string ip, string port)
        {
            this.ip = ip;
            this.port = port;
            base_url = "http://" + $"{ip}" + $":{port}";
        }
    }
}
