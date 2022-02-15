using Vendr.Core.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders
{
    public class NetsEasyOneTimeSettings : NetsEasySettingsBase
    {
        [PaymentProviderSetting(Name = "Auto Capture",
            Description = "Flag indicating whether to immediately capture the payment, or whether to just authorize the payment for later (manual) capture.",
            SortOrder = 5000)]
        public bool AutoCapture { get; set; }
    }
}