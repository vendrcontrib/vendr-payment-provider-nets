using Newtonsoft.Json;
using System.Collections.Generic;

namespace Vendr.Contrib.PaymentProviders.Api.Models
{
    public class NetsPaymentRequest
    {
        /// <summary>
        /// Specifies an order associated with a payment. An order must contain at least one order item. The amount of the order must match the sum of the specified order items.
        /// </summary>
        [JsonProperty("order")]
        public NetsOrder Order { get; set; }

        /// <summary>
        /// Defines the behavior and style of the checkout page.
        /// </summary>
        [JsonProperty("checkout")]
        public NetsCheckout Checkout { get; set; }

        /// <summary>
        /// Notifications allow you to subscribe to status updates for a payment.
        /// </summary>
        [JsonProperty("notifications")]
        public NetsNotifications Notifications { get; set; }

        /// <summary>
        /// Specifies an array of invoice fees added to the total price when invoice is used as the payment method.
        /// </summary>
        [JsonProperty("paymentMethods")]
        public NetsPaymentMethod[] PaymentMethods { get; set; }
    }

    public class NetsConsumer
    {
        /// <summary>
        /// Identifier of customer.
        /// </summary>
        [JsonProperty("reference")]
        public string Reference { get; set; }

        /// <summary>
        /// The email address.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The address of a customer (private or business).
        /// </summary>
        [JsonProperty("shippingAddress")]
        public NetsAddress ShippingAddress { get; set; }

        /// <summary>
        /// An international phone number.
        /// </summary>
        [JsonProperty("phoneNumber")]
        public NetsCustomerPhone PhoneNumber { get; set; }

        /// <summary>
        /// The name of a natural person.
        /// </summary>
        [JsonProperty("privatePerson")]
        public NetsCustomerName PrivatePerson { get; set; }

        /// <summary>
        /// A business consumer.
        /// </summary>
        [JsonProperty("company")]
        public NetsCompany Company { get; set; }
    }

    public class NetsAddress
    {
        /// <summary>
        /// The primary address line.
        /// </summary>
        [JsonProperty("addressLine1")]
        public string Line1 { get; set; }

        /// <summary>
        /// An additional address line.
        /// </summary>
        [JsonProperty("addressLine2")]
        public string Line2 { get; set; }

        /// <summary>
        /// The postal code.
        /// </summary>
        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        /// <summary>
        /// The city.
        /// </summary>
        [JsonProperty("city")]
        public string City { get; set; }

        /// <summary>
        /// A three-letter country code (ISO 3166-1), for example DNK.
        /// </summary>
        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public class NetsCustomerPhone
    {
        /// <summary>
        /// The country calling code, for example 001.
        /// </summary>
        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        /// <summary>
        /// The phone number (without the country code prefix).
        /// </summary>
        [JsonProperty("number")]
        public string Number { get; set; }
    }

    public class NetsCustomerName
    {
        /// <summary>
        /// The first name (also known as given name).
        /// </summary>
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// The last name (also known as surname/family name).
        /// </summary>
        [JsonProperty("lastName")]
        public string LastName { get; set; }
    }

    public class NetsCompany
    {
        /// <summary>
        /// The name of the company.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The name of a natural person.
        /// </summary>
        [JsonProperty("contact")]
        public NetsCustomerName Contact { get; set; }
    }

    public class NetsCheckout
    {
        /// <summary>
        /// If set to true, the transaction will be charged automatically after the reservation has been accepted. Default value is false if not specified.
        /// </summary>
        [JsonProperty("charge")]
        public bool Charge { get; set; }

        /// <summary>
        /// If set to true, the checkout will not load any user data, and also the checkout will not remember the current consumer on this device. Default value is false if not specified.
        /// </summary>
        [JsonProperty("publicDevice")]
        public bool PublicDevice { get; set; }

        /// <summary>
        /// Determines whether the checkout should be embedded in your webshop or if the checkout should be hosted by Nets on a separate page. Valid values are: 'EmbeddedCheckout' (default) or 'HostedPaymentPage'.
        /// </summary>
        [JsonProperty("integrationType")]
        public string IntegrationType { get; set; }

        /// <summary>
        /// Specifies where your customer will return after a canceled payment when using a hosted checkout page. See also the integrationType property.
        /// </summary>
        [JsonProperty("cancelUrl")]
        public string CancelUrl { get; set; }

        /// <summary>
        /// Specifies where your customer will return after a completed payment when using a hosted checkout page. See also the integrationType property.
        /// </summary>
        [JsonProperty("returnUrl")]
        public string ReturnUrl { get; set; }

        /// <summary>
        /// The URL to the terms and conditions of your webshop.
        /// </summary>
        [JsonProperty("termsUrl")]
        public string TermsUrl { get; set; }

        /// <summary>
        /// The URL to the privacy and cookie settings of your webshop.
        /// </summary>
        [JsonProperty("merchantTermsUrl")]
        public string MerchantTermsUrl { get; set; }

        /// <summary>
        /// Defines the appearance of the checkout page.
        /// </summary>
        [JsonProperty("appearance")]
        public NetsAppearance Appearance { get; set; }

        /// <summary>
        /// Allows you to initiate the checkout with customer data so that your customer only need to provide payment details.
        /// </summary>
        [JsonProperty("merchantHandlesConsumerData")]
        public bool MerchantHandlesConsumerData { get; set; }

        /// <summary>
        /// Contains information about the customer.
        /// </summary>
        [JsonProperty("consumer")]
        public NetsConsumer Consumer { get; set; }
    }

    public class NetsNotifications
    {
        /// <summary>
        /// The list of webhooks. The maximum number of webhooks is 32.
        /// </summary>
        [JsonProperty("webhooks")]
        public NetsWebhook[] Webhooks { get; set; }
        
    }

    public class NetsWebhook
    {
        /// <summary>
        /// The name of the event you want to subscribe to.
        /// </summary>
        [JsonProperty("eventName")]
        public string EventName { get; set; }

        /// <summary>
        /// The callback is sent to this URL. Must be HTTPS to ensure a secure communication. Maximum allowed length of the URL is 256 characters.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// The credentials that will be sent in the HTTP Authorization request header of the callback. Must be between 8 and 32 characters long and contain alphanumeric characters.
        /// </summary>
        [JsonProperty("authorization")]
        public string Authorization { get; set; }

        /// <summary>
        /// An of custom HTTP headers (name and value) to be sent with the HTTP callback request.
        /// </summary>
        [JsonProperty("headers")]
        public List<KeyValuePair<string, string>> Headers { get; set; }
    }

    public class NetsAppearance
    {
        /// <summary>
        /// Controls what is displayed on the checkout page.
        /// </summary>
        [JsonProperty("displayOptions")]
        public NetsDisplayOptions DisplayOptions { get; set; }

        /// <summary>
        /// Controls what text is displayed on the checkout page.
        /// </summary>
        [JsonProperty("textOptions")]
        public NetsTextOptions TextOptions { get; set; }
    }

    public class NetsDisplayOptions
    {
        /// <summary>
        /// If set to true, displays the merchant name above the checkout. Default value is true when using a HostedPaymentPage.
        /// </summary>
        [JsonProperty("showMerchantName")]
        public bool ShowMerchantName { get; set; }

        /// <summary>
        /// If set to true, displays the order summary above the checkout. Default value is true when using a HostedPaymentPage.
        /// </summary>
        [JsonProperty("showOrderSummary")]
        public bool ShowOrderSummary { get; set; }
    }

    public class NetsTextOptions
    {
        /// <summary>
        /// Overrides payment button text. The following predefined values are allowed: 'pay', 'purchase', 'order', 'book', 'reserve', 'signup', 'subscribe', 'accept'. The payment button text is localized.
        /// </summary>
        [JsonProperty("completePaymentButtonText")]
        public string CompletePaymentButtonText { get; set; }
    }
}
