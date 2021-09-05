using System.Text.Json.Serialization;

namespace DingleValleyRebooted.Server.Models
{
    public class WebhookDelivery
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("invoiceId")]
        public string InvoiceId { get; set; }
    }
}
