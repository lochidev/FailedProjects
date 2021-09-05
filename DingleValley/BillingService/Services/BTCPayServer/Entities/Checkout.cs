using Newtonsoft.Json;


namespace BillingService.Services.BTCPayServer.Entities
{
    public class Checkout
    {
        [JsonProperty("speedPolicy")]
        public string SpeedPolicy { get; init; }

        [JsonProperty("paymentMethods")]
        public string[] PaymentMethods { get; init; }

        [JsonProperty("expirationMinutes")]
        public long ExpirationMinutes { get; init; }

        [JsonProperty("monitoringMinutes")]
        public long MonitoringMinutes { get; init; }

        [JsonProperty("paymentTolerance")]
        public long PaymentTolerance { get; init; }

        [JsonProperty("redirectURL")]
        public string RedirectUrl { get; init; }

        [JsonProperty("redirectAutomatically")]
        public bool RedirectAutomatically { get; init; }

        [JsonProperty("defaultLanguage")]
        public string DefaultLanguage { get; init; }
    }
}
