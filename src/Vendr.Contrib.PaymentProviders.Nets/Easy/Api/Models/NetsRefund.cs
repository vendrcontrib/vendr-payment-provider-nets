using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.Api.Models
{
    public class NetsRefund
    {
        [JsonProperty("refundId")]
        public string RefundId { get; set; }
    }
}
