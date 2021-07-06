using Newtonsoft.Json;
using System.Collections.Generic;

namespace Vendr.Contrib.PaymentProviders.Api.Models
{
    public class NetsPaymentRequest
    {
        [JsonProperty("order")]
        public NetsOrder Order { get; set; }

        [JsonProperty("checkout")]
        public NetsCheckout Checkout { get; set; }

        [JsonProperty("notifications")]
        public NetsNotifications Notifications { get; set; }

        [JsonProperty("paymentMethods")]
        public NetsPaymentMethod[] PaymentMethods { get; set; }
    }

    public class NetsConsumer
    {
        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("shippingAddress")]
        public NetsAddress ShippingAddress { get; set; }

        [JsonProperty("phoneNumber")]
        public NetsCustomerPhone PhoneNumber { get; set; }

        [JsonProperty("privatePerson")]
        public NetsCustomerName PrivatePerson { get; set; }

        [JsonProperty("company")]
        public NetsCompany Company { get; set; }
    }

    public class NetsAddress
    {
        [JsonProperty("addressLine1")]
        public string Line1 { get; set; }

        [JsonProperty("addressLine2")]
        public string Line2 { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public class NetsCustomerPhone
    {
        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }

    public class NetsCustomerName
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }
    }

    public class NetsCompany
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("contact")]
        public NetsCustomerName Contact { get; set; }
    }

    public class NetsCheckout
    {
        [JsonProperty("charge")]
        public bool Charge { get; set; }

        [JsonProperty("publicDevice")]
        public bool PublicDevice { get; set; }

        [JsonProperty("integrationType")]
        public string IntegrationType { get; set; }

        [JsonProperty("cancelUrl")]
        public string CancelUrl { get; set; }

        [JsonProperty("returnUrl")]
        public string ReturnUrl { get; set; }

        [JsonProperty("termsUrl")]
        public string TermsUrl { get; set; }

        [JsonProperty("appearance")]
        public NetsAppearance Appearance { get; set; }

        [JsonProperty("merchantHandlesConsumerData")]
        public bool MerchantHandlesConsumerData { get; set; }

        [JsonProperty("consumer")]
        public NetsConsumer Consumer { get; set; }
    }

    public class NetsNotifications
    {
        [JsonProperty("webhooks")]
        public NetsWebhook[] Webhooks { get; set; }
        
    }

    public class NetsWebhook
    {
        [JsonProperty("eventName")]
        public string EventName { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("authorization")]
        public string Authorization { get; set; }

        [JsonProperty("headers")]
        public List<Dictionary<string, string>> Headers { get; set; }
    }

    public class NetsAppearance
    {
        [JsonProperty("displayOptions")]
        public NetsDisplayOptions DisplayOptions { get; set; }

        [JsonProperty("textOptions")]
        public NetsTextOptions TextOptions { get; set; }
    }

    public class NetsDisplayOptions
    {
        [JsonProperty("showMerchantName")]
        public bool ShowMerchantName { get; set; }

        [JsonProperty("showOrderSummary")]
        public bool ShowOrderSummary { get; set; }
    }

    public class NetsTextOptions
    {
        [JsonProperty("completePaymentButtonText")]
        public string CompletePaymentButtonText { get; set; }
    }
}
