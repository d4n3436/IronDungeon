using Newtonsoft.Json;

namespace IronDungeon
{
    public class Config
    {
        public Config(string token, bool slowTyping)
        {
            Token = token;
            SlowTyping = slowTyping;
        }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("slow_typing")]
        public bool SlowTyping { get; set; }
    }
}