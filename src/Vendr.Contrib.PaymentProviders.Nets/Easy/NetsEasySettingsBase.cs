using Vendr.Core.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders
{
    public class NetsEasySettingsBase : NetsSettingsBase
    {
        [PaymentProviderSetting(Name = "Accepted Payment Methods",
            Description = "A comma separated list of Payment Methods to accept.",
            SortOrder = 1000)]
        public string PaymentMethods { get; set; }

        [PaymentProviderSetting(Name = "Billing Company Property Alias",
            Description = "The order property alias containing company of the billing address (optional).",
            SortOrder = 1100)]
        public string BillingCompanyPropertyAlias { get; set; }

        [PaymentProviderSetting(Name = "Billing Phone Property Alias",
            Description = "The order property alias containing phone of the billing address.",
            SortOrder = 1200)]
        public string BillingPhonePropertyAlias { get; set; }

        [PaymentProviderSetting(Name = "Shipping Address (Line 1) Property Alias",
            Description = "The order property alias containing line 1 of the shipping address.",
            SortOrder = 1300)]
        public string ShippingAddressLine1PropertyAlias { get; set; }

        [PaymentProviderSetting(Name = "Shipping Address (Line 2) Property Alias",
            Description = "The order property alias containing line 2 of the shipping address.",
            SortOrder = 1400)]
        public string ShippingAddressLine2PropertyAlias { get; set; }

        [PaymentProviderSetting(Name = "Shipping Address Zip Code Property Alias",
            Description = "The order property alias containing the zip code of the shipping address.",
            SortOrder = 1500)]
        public string ShippingAddressZipCodePropertyAlias { get; set; }

        [PaymentProviderSetting(Name = "Shipping Address City Property Alias",
            Description = "The order property alias containing the city of the shipping address.",
            SortOrder = 1600)]
        public string ShippingAddressCityPropertyAlias { get; set; }

        [PaymentProviderSetting(Name = "Terms URL",
            Description = "The URL to the terms and conditions of your webshop.",
            SortOrder = 1700)]
        public string TermsUrl { get; set; }

        [PaymentProviderSetting(Name = "Merchant Terms URL",
            Description = "The URL to the privacy and cookie settings of your webshop",
            SortOrder = 1800)]
        public string MerchantTermsUrl { get; set; }

        [PaymentProviderSetting(Name = "Live Secret Key",
            Description = "Your live Nets secret key",
            SortOrder = 3100)]
        public string LiveSecretKey { get; set; }

        [PaymentProviderSetting(Name = "Live Checkout Key",
            Description = "Your live Nets checkout key",
            SortOrder = 3200)]
        public string LiveCheckoutKey { get; set; }

        [PaymentProviderSetting(Name = "Test Secret Key",
            Description = "Your test Nets secret key",
            SortOrder = 3300)]
        public string TestSecretKey { get; set; }

        [PaymentProviderSetting(Name = "Test Checkout Key",
            Description = "Your test Nets checkout key",
            SortOrder = 3400)]
        public string TestCheckoutKey { get; set; }
    }
}
