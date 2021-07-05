using System;
using System.Web;
using System.Web.Mvc;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders.Nets
{
    [PaymentProvider("nets-easy", "Nets Easy", "Nets Easy payment provider")]
    public class NetsEasyPaymentProvider : PaymentProviderBase<NetsEasySettings>
    {
        public NetsEasyPaymentProvider(VendrContext vendr)
            : base(vendr)
        { }

        public override bool FinalizeAtContinueUrl => true;

        public override PaymentFormResult GenerateForm(OrderReadOnly order, string continueUrl, string cancelUrl, string callbackUrl, NetsEasySettings settings)
        {
            return new PaymentFormResult()
            {
                Form = new PaymentForm(continueUrl, FormMethod.Post)
            };
        }

        public override string GetCancelUrl(OrderReadOnly order, NetsEasySettings settings)
        {
            return string.Empty;
        }

        public override string GetErrorUrl(OrderReadOnly order, NetsEasySettings settings)
        {
            return string.Empty;
        }

        public override string GetContinueUrl(OrderReadOnly order, NetsEasySettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.ContinueUrl.MustNotBeNull("settings.ContinueUrl");

            return settings.ContinueUrl;
        }

        public override CallbackResult ProcessCallback(OrderReadOnly order, HttpRequestBase request, NetsEasySettings settings)
        {
            return new CallbackResult
            {
                TransactionInfo = new TransactionInfo
                {
                    AmountAuthorized = order.TransactionAmount.Value,
                    TransactionFee = 0m,
                    TransactionId = Guid.NewGuid().ToString("N"),
                    PaymentStatus = PaymentStatus.Authorized
                }
            };
        }
    }
}
