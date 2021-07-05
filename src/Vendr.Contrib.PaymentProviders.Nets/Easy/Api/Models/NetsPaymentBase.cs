using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.Api.Models
{
    public class NetsPaymentBase
    {
        [JsonProperty("checkout")]
        public NetsPaymentCheckout Checkout { get; set; }
    }

    public class NetsPaymentCheckout
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
