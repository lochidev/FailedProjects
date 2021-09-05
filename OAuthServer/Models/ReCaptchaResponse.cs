using Newtonsoft.Json;

namespace OAuthServer.Models
{
    public class ReCaptchaResponse
    {
        [JsonProperty("success")]
        public bool IsSuccess { get; set; }
        [JsonProperty("challenge_ts")]

        public DateTime DateTime { get; set; }
        [JsonProperty("hostname")]
        public string Hostname { get; set; }
    }
}
