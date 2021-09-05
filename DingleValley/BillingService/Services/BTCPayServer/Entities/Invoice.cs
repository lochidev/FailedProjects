using Newtonsoft.Json;


namespace BillingService.Services.BTCPayServer.Entities
{
    public class Invoice
    {
        [JsonProperty("amount")]
        public string Amount { get; init; }

        [JsonProperty("currency")]
        public string Currency { get; init; }
        [JsonProperty("metadata")]
        public MetaData MetaData { get; init; }
        [JsonProperty("checkout")]
        public Checkout Checkout { get; init; }

        [JsonProperty("id")]
        public string Id { get; init; }

        [JsonProperty("checkoutLink")]
        public string CheckoutLink { get; init; }

        [JsonProperty("createdTime")]
        public long CreatedTime { get; init; }

        [JsonProperty("expirationTime")]
        public long ExpirationTime { get; init; }

        [JsonProperty("monitoringTime")]
        public long MonitoringTime { get; init; }

        [JsonProperty("status")]
        public string Status { get; init; }

        [JsonProperty("additionalStatus")]
        public string AdditionalStatus { get; init; }
        public Invoice(string amount, string currency, Checkout checkout = null, MetaData metaData = null)
        {
            Amount = amount;
            Currency = currency;
            Checkout = checkout;
            MetaData = metaData;
        }
    }
    public record MetaData([JsonProperty("orderId")] string OrderId);
}
