using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Vendr.Common.Logging;
using Vendr.Contrib.PaymentProviders.Api.Models;
using Vendr.Core.Api;
using Vendr.Core.PaymentProviders;
using Vendr.Extensions;

namespace Vendr.Contrib.PaymentProviders
{
    public abstract class NetsPaymentProviderBase<TSelf, TSettings> : PaymentProviderBase<TSettings>
        where TSelf : NetsPaymentProviderBase<TSelf, TSettings>
        where TSettings : NetsSettingsBase, new()
    {
        protected readonly ILogger<TSelf> _logger;

        public NetsPaymentProviderBase(VendrContext vendr,
            ILogger<TSelf> logger)
            : base(vendr)
        {
            _logger = logger;
        }

        public override string GetCancelUrl(PaymentProviderContext<TSettings> ctx)
        {
            ctx.Settings.MustNotBeNull("settings");
            ctx.Settings.CancelUrl.MustNotBeNull("settings.CancelUrl");

            return ctx.Settings.CancelUrl;
        }

        public override string GetContinueUrl(PaymentProviderContext<TSettings> ctx)
        {
            ctx.Settings.MustNotBeNull("settings");
            ctx.Settings.ContinueUrl.MustNotBeNull("settings.ContinueUrl");

            return ctx.Settings.ContinueUrl;
        }

        public override string GetErrorUrl(PaymentProviderContext<TSettings> ctx)
        {
            ctx.Settings.MustNotBeNull("settings");
            ctx.Settings.ErrorUrl.MustNotBeNull("settings.ErrorUrl");

            return ctx.Settings.ErrorUrl;
        }

        protected async Task<NetsWebhookEvent> GetNetsWebhookEventAsync(PaymentProviderContext<TSettings> ctx, string webhookAuthorization)
        {
            NetsWebhookEvent netsWebhookEvent = null;

            if (ctx.AdditionalData.ContainsKey("Vendr_NetsEasyWebhookEvent"))
            {
                netsWebhookEvent = (NetsWebhookEvent)ctx.AdditionalData["Vendr_NetsEasyWebhookEvent"];
            }
            else
            {
                try
                {
                    var json = await ctx.Request.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(json))
                    {
                        // Verify "Authorization" header returned from webhook
                        VerifyAuthorization(ctx.Request, webhookAuthorization);

                        netsWebhookEvent = JsonConvert.DeserializeObject<NetsWebhookEvent>(json);

                        ctx.AdditionalData.Add("Vendr_NetsEasyWebhookEvent", netsWebhookEvent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Nets Easy - GetNetsWebhookEvent");
                }
            }

            return netsWebhookEvent;
        }

        protected static string ForceHttps(string url)
        {
            var uri = new UriBuilder(url);

            var hadDefaultPort = uri.Uri.IsDefaultPort;
            uri.Scheme = Uri.UriSchemeHttps;
            uri.Port = hadDefaultPort ? -1 : uri.Port;

            return uri.ToString();
        }

        private void VerifyAuthorization(HttpRequestMessage request, string webhookAuthorization)
        {
            string authHeader = request.Headers.GetValues("Authorization")?.FirstOrDefault();

            if (authHeader == null)
                throw new Exception("The authorization header is not present in the webhook event.");

            if (authHeader != webhookAuthorization)
                throw new Exception("The authorization in the webhook event could not be verified.");
        }
    }
}
