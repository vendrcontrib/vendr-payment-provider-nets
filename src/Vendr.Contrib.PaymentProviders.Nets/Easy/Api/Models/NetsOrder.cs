using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.Api.Models
{
    public class NetsOrder
    {
        /// <summary>
        /// A reference to recognize this order. Usually a number sequence (order number).
        /// </summary>
        [JsonProperty("reference")]
        public string Reference { get; set; }

        /// <summary>
        /// The currency of the payment, for example 'DKK'.
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// The total amount of the order including VAT, if any. (Sum of all grossTotalAmounts in the order.)
        /// </summary>
        [JsonProperty("amount")]
        public int Amount { get; set; }

        /// <summary>
        /// A list of order items. At least one item must be specified.
        /// </summary>
        [JsonProperty("items")]
        public NetsOrderItem[] Items { get; set; }
    }
}
