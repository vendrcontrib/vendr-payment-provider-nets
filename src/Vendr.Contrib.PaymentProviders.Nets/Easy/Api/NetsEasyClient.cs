using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Vendr.Contrib.PaymentProviders.Api.Models;
using Vendr.Contrib.PaymentProviders.Api.Models;

namespace Vendr.Contrib.PaymentProviders.Api
{
    public class NetsEasyClient
    {
        private NetsEasyClientConfig _config;

        public NetsEasyClient(NetsEasyClientConfig config)
        {
            _config = config;
        }

        public NetsPaymentResult CreatePayment(NetsPaymentRequest data)
        {
            return Request("/v1/payments/", (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<NetsPaymentResult>());
        }

        public NetsPaymentDetails GetPayment(string paymentId)
        {
            return Request($"/v1/payments/{paymentId}", (req) => req
                .GetJsonAsync<NetsPaymentDetails>());
        }

        public string CancelPayment(string paymentId, object data)
        {
            return Request($"/v1/payments/{paymentId}/cancels", (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<string>());
        }

        public NetsCharge ChargePayment(string paymentId, object data)
        {
            return Request($"/v1/payments/{paymentId}/charges", (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<NetsCharge>());
        }

        public NetsRefund RefundPayment(string chargeId, object data)
        {
            return Request($"/v1/charges/{chargeId}/refunds", (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<NetsRefund>());
        }

        private TResult Request<TResult>(string url, Func<IFlurlRequest, Task<TResult>> func)
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

                result = func.Invoke(req).Result;
            }
            catch (FlurlHttpException ex)
            {
                throw;
            }

            return result;
        }
    }
}
