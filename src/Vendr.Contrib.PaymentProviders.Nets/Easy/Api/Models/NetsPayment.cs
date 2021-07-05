using Newtonsoft.Json;
using System;

namespace Vendr.Contrib.PaymentProviders.Api.Models
{
    public class NetsPaymentDetails
    {
        [JsonProperty("payment")]
        public NetsPayment Payment { get; set; }
    }

    public class NetsPayment : NetsPaymentBase
    {
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty("orderDetails")]
        public NetsOrderDetails OrderDetails { get; set; }

        [JsonProperty("summary")]
        public NetsSummary Summary { get; set; }
    }

    public class NetsOrderDetails
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }
    }

    public class NetsSummary
    {
        [JsonProperty("cancelledAmount")]
        public int CancelledAmount { get; set; }

        [JsonProperty("chargedAmount")]
        public int ChargedAmount { get; set; }

        [JsonProperty("refundedAmount")]
        public int RefundedAmount { get; set; }

        [JsonProperty("reservedAmount")]
        public int ReservedAmount { get; set; }
    }
}
