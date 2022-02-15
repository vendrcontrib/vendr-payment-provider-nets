using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.Api.Models
{
    public class NetsOrderItem
    {
        /// <summary>
        /// A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc).
        /// </summary>
        [JsonProperty("reference")]
        public string Reference { get; set; }

        /// <summary>
        /// The name of the product.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The quantity of the product.
        /// </summary>
        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// The defined unit of measurement for the product, for example pcs, liters, or kg.
        /// </summary>
        [JsonProperty("unit")]
        public string Unit { get; set; }

        /// <summary>
        /// The price per unit excluding VAT.
        /// </summary>
        [JsonProperty("unitPrice")]
        public int UnitPrice { get; set; }

        /// <summary>
        /// The tax/VAT rate (in percent multiplied by 100). For example, the value 2500 corresponds to 25%. Defaults to 0 if not provided.
        /// </summary>
        [JsonProperty("taxRate")]
        public int TaxRate { get; set; }

        /// <summary>
        /// The tax/VAT amount (unitPrice * quantity * taxRate / 10000). Defaults to 0 if not provided. taxAmount should include the total tax amount for the entire order item.
        /// </summary>
        [JsonProperty("taxAmount")]
        public int TaxAmount { get; set; }

        /// <summary>
        /// The total amount including VAT (netTotalAmount + taxAmount).
        /// </summary>
        [JsonProperty("grossTotalAmount")]
        public int GrossTotalAmount { get; set; }

        /// <summary>
        /// The total amount excluding VAT (unitPrice * quantity).
        /// </summary>
        [JsonProperty("netTotalAmount")]
        public int NetTotalAmount { get; set; }
    }
}
