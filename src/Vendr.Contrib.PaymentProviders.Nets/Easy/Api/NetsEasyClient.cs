using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Vendr.Contrib.PaymentProviders.Api.Models;

namespace Vendr.Contrib.PaymentProviders.Api
{
    public class NetsEasyClient
    {
        private readonly NetsEasyClientConfig _config;

        public NetsEasyClient(NetsEasyClientConfig config)
        {
            _config = config;
        }

        public async Task<NetsPaymentResult> CreatePaymentAsync(NetsPaymentRequest data)
        {
            return await Request("/v1/payments/", (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<NetsPaymentResult>());
        }

        public async Task<NetsPaymentDetails> GetPaymentAsync(string paymentId)
        {
            return await Request($"/v1/payments/{paymentId}", (req) => req
                .GetJsonAsync<NetsPaymentDetails>());
        }

        public async Task<string> CancelPaymentAsync(string paymentId, object data)
        {
            return await Request($"/v1/payments/{paymentId}/cancels", (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<string>());
        }

        public async Task<NetsCharge> ChargePaymentAsync(string paymentId, object data)
        {
            return await Request($"/v1/payments/{paymentId}/charges", (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<NetsCharge>());
        }

        public async Task<NetsRefund> RefundPaymentAsync(string chargeId, object data)
        {
            return await Request($"/v1/charges/{chargeId}/refunds", (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<NetsRefund>());
        }

        private async Task<TResult> Request<TResult>(string url, Func<IFlurlRequest, Task<TResult>> func)
        {
            var result = default(TResult);

            try
            {
                var req = new FlurlRequest(_config.BaseUrl + url)
                        .ConfigureRequest(x =>
                        {
                            var jsonSettings = new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.Include,
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            };
                            x.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
                        })
                        .WithHeader("Authorization", _config.Authorization);

                result = await func.Invoke(req);
            }
            catch (FlurlHttpException ex)
            {
                throw;
            }

            return result;
        }
    }
}
