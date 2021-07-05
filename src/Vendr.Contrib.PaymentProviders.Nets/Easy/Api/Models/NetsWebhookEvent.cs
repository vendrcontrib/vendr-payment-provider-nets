using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vendr.Contrib.PaymentProviders.Api.Models
{
    public class NetsWebhookEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("data")]
        public JObject Data { get; set; }
    }
}
