using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Vendr.Contrib.PaymentProviders.Api.Models;
using Vendr.Contrib.PaymentProviders.Api;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders
{
    [PaymentProvider("nets-easy-checkout-onetime", "Nets Easy (One Time)", "Nets Easy payment provider for one time payments")]
    public class NetsEasyOneTimePaymentProvider : NetsPaymentProviderBase<NetsEasyOneTimeSettings>
    {
        public NetsEasyOneTimePaymentProvider(VendrContext vendr)
            : base(vendr)
        { }

        public override bool CanCancelPayments => true;
        public override bool CanCapturePayments => true;
        public override bool CanRefundPayments => true;
        public override bool CanFetchPaymentStatus => true;

        // We'll finalize via webhook callback
        public override bool FinalizeAtContinueUrl => false;

        public override IEnumerable<TransactionMetaDataDefinition> TransactionMetaDataDefinitions => new[]{
            new TransactionMetaDataDefinition("netsEasyPaymentId", "Nets (Easy) Payment ID"),
            new TransactionMetaDataDefinition("netsEasyChargeId", "Nets (Easy) Charge ID"),
            new TransactionMetaDataDefinition("netsEasyRefundId", "Nets (Easy) Refund ID"),
            new TransactionMetaDataDefinition("netsEasyCancelId", "Nets (Easy) Cancel ID"),
            new TransactionMetaDataDefinition("netsEasyWebhookAuthKey", "Nets (Easy) Webhook Authorization")
        };

        public override PaymentFormResult GenerateForm(OrderReadOnly order, string continueUrl, string cancelUrl, string callbackUrl, NetsEasyOneTimeSettings settings)
        {
            var currency = Vendr.Services.CurrencyService.GetCurrency(order.CurrencyId);
            var currencyCode = currency.Code.ToUpperInvariant();

            // Ensure currency has valid ISO 4217 code
            if (!Iso4217.CurrencyCodes.ContainsKey(currencyCode))
            {
                throw new Exception("Currency must be a valid ISO 4217 currency code: " + currency.Name);
            }

            var orderAmount = AmountToMinorUnits(order.TransactionAmount.Value);

            var paymentMethods = settings.PaymentMethods?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .Select(s => s.Trim())
                   .ToArray();

            var paymentMethodId = order.PaymentInfo.PaymentMethodId;
            var paymentMethod = paymentMethodId != null ? Vendr.Services.PaymentMethodService.GetPaymentMethod(paymentMethodId.Value) : null;

            string paymentId = string.Empty;
            string paymentFormLink = string.Empty;

            var webhookAuthKey = Guid.NewGuid().ToString();

            try
            {
                var clientConfig = GetNetsEasyClientConfig(settings);
                var client = new NetsEasyClient(clientConfig);

                var items = order.OrderLines.Select(x => new NetsOrderItem
                {
                    Reference = x.Sku,
                    Name = x.Name,
                    Quantity = (int)x.Quantity,
                    Unit = "pcs",
                    UnitPrice = (int)AmountToMinorUnits(x.UnitPrice.Value.WithoutTax),
                    TaxRate = (int)AmountToMinorUnits(x.TaxRate.Value * 100),
                    TaxAmount = (int)AmountToMinorUnits(x.TotalPrice.Value.Tax),
                    GrossTotalAmount = (int)AmountToMinorUnits(x.TotalPrice.Value.WithTax),
                    NetTotalAmount = (int)AmountToMinorUnits(x.TotalPrice.Value.WithoutTax)
                });

                var shippingMethod = Vendr.Services.ShippingMethodService.GetShippingMethod(order.ShippingInfo.ShippingMethodId.Value);
                if (shippingMethod != null)
                {
                    items = items.Append(new NetsOrderItem
                    {
                        Reference = shippingMethod.Sku,
                        Name = shippingMethod.Name,
                        Quantity = 1,
                        Unit = "pcs",
                        UnitPrice = (int)AmountToMinorUnits(order.ShippingInfo.TotalPrice.Value.WithoutTax),
                        TaxRate = (int)AmountToMinorUnits(order.ShippingInfo.TaxRate.Value * 100),
                        TaxAmount = (int)AmountToMinorUnits(order.ShippingInfo.TotalPrice.Value.Tax),
                        GrossTotalAmount = (int)AmountToMinorUnits(order.ShippingInfo.TotalPrice.Value.WithTax),
                        NetTotalAmount = (int)AmountToMinorUnits(order.ShippingInfo.TotalPrice.Value.WithoutTax)
                    });
                }

                // Check adjustments on subtotal price
                if (order.SubtotalPrice.Adjustments.Count > 0)
                {
                    // Discounts
                    var discountAdjustments = order.SubtotalPrice.Adjustments.OfType<DiscountAdjustment>();
                    if (discountAdjustments.Any())
                    {
                        foreach (var discount in discountAdjustments)
                        {
                            var taxRate = (discount.Price.Tax / discount.Price.WithoutTax) * 100;

                            items = items.Append(new NetsOrderItem
                            {
                                Reference = discount.DiscountId.ToString(),
                                Name = discount.DiscountName,
                                Quantity = 1,
                                Unit = "pcs",
                                UnitPrice = (int)AmountToMinorUnits(discount.Price.WithoutTax),
                                TaxRate = (int)AmountToMinorUnits(taxRate),
                                TaxAmount = (int)AmountToMinorUnits(discount.Price.Tax),
                                GrossTotalAmount = (int)AmountToMinorUnits(discount.Price.WithTax),
                                NetTotalAmount = (int)AmountToMinorUnits(discount.Price.WithoutTax)
                            });
                        }
                    }

                    // Custom price adjustments
                    var priceAdjustments = order.SubtotalPrice.Adjustments.Except(discountAdjustments).OfType<PriceAdjustment>();
                    if (priceAdjustments.Any())
                    {
                        foreach (var adjustment in priceAdjustments)
                        {
                            var reference = Guid.NewGuid().ToString();
                            var taxRate = (adjustment.Price.Tax / adjustment.Price.WithoutTax) * 100;

                            items = items.Append(new NetsOrderItem
                            {
                                Reference = reference,
                                Name = adjustment.Name,
                                Quantity = 1,
                                Unit = "pcs",
                                UnitPrice = (int)AmountToMinorUnits(adjustment.Price.WithoutTax),
                                TaxRate = (int)AmountToMinorUnits(taxRate),
                                TaxAmount = (int)AmountToMinorUnits(adjustment.Price.Tax),
                                GrossTotalAmount = (int)AmountToMinorUnits(adjustment.Price.WithTax),
                                NetTotalAmount = (int)AmountToMinorUnits(adjustment.Price.WithoutTax)
                            });
                        }
                    }
                }

                // Check adjustments on total price
                if (order.TotalPrice.Adjustments.Count > 0)
                {
                    // Discounts
                    var discountAdjustments = order.TotalPrice.Adjustments.OfType<DiscountAdjustment>();
                    if (discountAdjustments.Any())
                    {
                        foreach (var discount in discountAdjustments)
                        {
                            var taxRate = (discount.Price.Tax / discount.Price.WithoutTax) * 100;

                            items = items.Append(new NetsOrderItem
                            {
                                Reference = discount.DiscountId.ToString(),
                                Name = discount.DiscountName,
                                Quantity = 1,
                                Unit = "pcs",
                                UnitPrice = (int)AmountToMinorUnits(discount.Price.WithoutTax),
                                TaxRate = (int)AmountToMinorUnits(taxRate),
                                TaxAmount = (int)AmountToMinorUnits(discount.Price.Tax),
                                GrossTotalAmount = (int)AmountToMinorUnits(discount.Price.WithTax),
                                NetTotalAmount = (int)AmountToMinorUnits(discount.Price.WithoutTax)
                            });
                        }
                    }

                    // Custom price adjustments
                    var priceAdjustments = order.TotalPrice.Adjustments.Except(discountAdjustments).OfType<PriceAdjustment>();
                    if (priceAdjustments.Any())
                    {
                        foreach (var adjustment in priceAdjustments)
                        {
                            var reference = Guid.NewGuid().ToString();
                            var taxRate = (adjustment.Price.Tax / adjustment.Price.WithoutTax) * 100;

                            items = items.Append(new NetsOrderItem
                            {
                                Reference = reference,
                                Name = adjustment.Name,
                                Quantity = 1,
                                Unit = "pcs",
                                UnitPrice = (int)AmountToMinorUnits(adjustment.Price.WithoutTax),
                                TaxRate = (int)AmountToMinorUnits(taxRate),
                                TaxAmount = (int)AmountToMinorUnits(adjustment.Price.Tax),
                                GrossTotalAmount = (int)AmountToMinorUnits(adjustment.Price.WithTax),
                                NetTotalAmount = (int)AmountToMinorUnits(adjustment.Price.WithoutTax)
                            });
                        }
                    }
                }

                // Check adjustments on transaction amount
                if (order.TransactionAmount.Adjustments.Count > 0)
                {
                    // Gift Card adjustments
                    var giftCardAdjustments = order.TransactionAmount.Adjustments.OfType<GiftCardAdjustment>();
                    if (giftCardAdjustments.Any())
                    {
                        foreach (var giftcard in giftCardAdjustments)
                        {
                            items = items.Append(new NetsOrderItem
                            {
                                Reference = giftcard.GiftCardId.ToString(),
                                Name = giftcard.GiftCardCode, //$"Gift Card - {giftcard.Code}",
                                Quantity = 1,
                                Unit = "pcs",
                                UnitPrice = (int)AmountToMinorUnits(giftcard.Amount),
                                TaxRate = (int)AmountToMinorUnits(order.TaxRate.Value * 100),
                                GrossTotalAmount = (int)AmountToMinorUnits(giftcard.Amount),
                                NetTotalAmount = (int)AmountToMinorUnits(giftcard.Amount)
                            });
                        }
                    }

                    // Custom Amount adjustments
                    var amountAdjustments = order.TransactionAmount.Adjustments.Except(giftCardAdjustments).OfType<AmountAdjustment>();
                    if (amountAdjustments.Any())
                    {
                        foreach (var amount in amountAdjustments)
                        {
                            var reference = Guid.NewGuid().ToString();

                            items = items.Append(new NetsOrderItem
                            {
                                Reference = reference,
                                Name = amount.Name,
                                Quantity = 1,
                                Unit = "pcs",
                                UnitPrice = (int)AmountToMinorUnits(amount.Amount),
                                TaxRate = (int)AmountToMinorUnits(order.TaxRate.Value * 100),
                                GrossTotalAmount = (int)AmountToMinorUnits(amount.Amount),
                                NetTotalAmount = (int)AmountToMinorUnits(amount.Amount)
                            });
                        }
                    }
                }

                string company = !string.IsNullOrWhiteSpace(settings.BillingCompanyPropertyAlias)
                    ? order.Properties[settings.BillingCompanyPropertyAlias]
                    : string.Empty;

                var country = order.ShippingInfo.CountryId.HasValue
                    ? Vendr.Services.CountryService.GetCountry(order.ShippingInfo.CountryId.Value)
                    : null;

                var region = country != null ? new RegionInfo(country.Code) : null;
                var countryIsoCode = region?.ThreeLetterISORegionName;

                // If only partial data about the consumer is sent,
                // then the consumer will not be created in Easy.

                var consumer = new NetsConsumer
                {
                    Reference = order.CustomerInfo.CustomerReference,
                    Email = order.CustomerInfo.Email,
                    ShippingAddress = new NetsAddress
                    {
                        Line1 = !string.IsNullOrWhiteSpace(settings.ShippingAddressLine1PropertyAlias)
                            ? order.Properties[settings.ShippingAddressLine1PropertyAlias] : "",
                        Line2 = !string.IsNullOrWhiteSpace(settings.ShippingAddressLine2PropertyAlias)
                            ? order.Properties[settings.ShippingAddressLine2PropertyAlias] : "",
                        PostalCode = !string.IsNullOrWhiteSpace(settings.ShippingAddressZipCodePropertyAlias)
                            ? order.Properties[settings.ShippingAddressZipCodePropertyAlias] : "",
                        City = !string.IsNullOrWhiteSpace(settings.ShippingAddressCityPropertyAlias)
                            ? order.Properties[settings.ShippingAddressCityPropertyAlias] : "",
                        Country = countryIsoCode
                    }
                };

                string phone = !string.IsNullOrWhiteSpace(settings.BillingPhonePropertyAlias)
                    ? order.Properties[settings.BillingPhonePropertyAlias]
                    : string.Empty;

                phone = phone?
                    .Replace(" ", "")
                    .Replace("-", "")
                    .Replace("-", "")
                    .Replace("(", "")
                    .Replace(")", "");

                if (!string.IsNullOrEmpty(phone) &&
                    Regex.Match(phone, @"^\+[0-9]{7,18}$").Success)
                {
                    var prefix = phone.Substring(0, 3);
                    var number = phone.Substring(3);

                    consumer.PhoneNumber = new NetsCustomerPhone
                    {
                        Prefix = prefix, // E.g "+45"
                        Number = number
                    };
                }

                // Fill either privateperson or company, not both.
                if (!string.IsNullOrWhiteSpace(company))
                {
                    consumer.Company = new NetsCompany
                    {
                        Name = company,
                        Contact = new NetsCustomerName
                        {
                            FirstName = order.CustomerInfo.FirstName,
                            LastName = order.CustomerInfo.LastName
                        }
                    };
                }
                else
                {
                    consumer.PrivatePerson = new NetsCustomerName
                    {
                        FirstName = order.CustomerInfo.FirstName,
                        LastName = order.CustomerInfo.LastName
                    };
                }

                var data = new NetsPaymentRequest
                {
                    Order = new NetsOrder
                    {
                        Reference = order.OrderNumber,
                        Currency = currencyCode,
                        Amount = (int)orderAmount,
                        Items = items.ToArray()
                    },
                    Checkout = new NetsCheckout
                    {
                        Charge = settings.AutoCapture,
                        IntegrationType = "HostedPaymentPage",
                        CancelUrl = cancelUrl,
                        ReturnUrl = continueUrl,
                        TermsUrl = settings.TermsUrl,
                        Appearance = new NetsAppearance
                        {
                            DisplayOptions = new NetsDisplayOptions
                            {
                                ShowMerchantName = true,
                                ShowOrderSummary = true
                            }
                        },
                        MerchantHandlesConsumerData = true,
                        Consumer = consumer
                    },
                    Notifications = new NetsNotifications
                    {
                        Webhooks = new NetsWebhook[]
                        {
                            new NetsWebhook
                            {
                                EventName = NetsEvents.PaymentCheckoutCompleted,
                                Url = ForceHttps(callbackUrl), // Must be https 
                                Authorization = webhookAuthKey,
                                // Need documentation from Nets/Nets what headers are for.
                                //Headers = new List<Dictionary<string, string>>
                                //{
                                //    new Dictionary<string, string>(1)
                                //    {
                                //        { "Referrer-Policy", "no-referrer-when-downgrade" }
                                //    }
                                //}
                            },
                            new NetsWebhook
                            {
                                EventName = NetsEvents.PaymentChargeCreated,
                                Url = ForceHttps(callbackUrl),
                                Authorization = webhookAuthKey
                            },
                            new NetsWebhook
                            {
                                EventName = NetsEvents.PaymentRefundCompleted,
                                Url = ForceHttps(callbackUrl),
                                Authorization = webhookAuthKey
                            },
                            new NetsWebhook
                            {
                                EventName = NetsEvents.PaymentCancelCreated,
                                Url = ForceHttps(callbackUrl),
                                Authorization = webhookAuthKey
                            }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(data);

                // Create payment
                var payment = client.CreatePayment(data);

                // Get payment id
                paymentId = payment.PaymentId;

                var paymentDetails = client.GetPayment(paymentId);
                if (paymentDetails != null)
                {
                    var uriBuilder = new UriBuilder(paymentDetails.Payment.Checkout.Url);
                    var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                    if (!string.IsNullOrEmpty(settings.Language))
                    {
                        query["language"] = settings.Language;
                    }

                    uriBuilder.Query = query.ToString();
                    paymentFormLink = uriBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<NetsEasyOneTimePaymentProvider>(ex, "Nets Easy - error creating payment.");
            }

            var checkoutKey = settings.TestMode ? settings.TestCheckoutKey : settings.LiveCheckoutKey;

            return new PaymentFormResult()
            {
                MetaData = new Dictionary<string, string>
                {
                    { "netsEasyPaymentId", paymentId },
                    { "netsEasyWebhookAuthKey", webhookAuthKey }
                },
                Form = new PaymentForm(paymentFormLink, FormMethod.Get)
            };
        }

        public override CallbackResult ProcessCallback(OrderReadOnly order, HttpRequestBase request, NetsEasyOneTimeSettings settings)
        {
            try
            {
                // Process callback

                var webhookAuthKey = order.Properties["netsEasyWebhookAuthKey"]?.Value;
                
                var clientConfig = GetNetsEasyClientConfig(settings);
                var client = new NetsEasyClient(clientConfig);

                var netsEvent = GetNetsWebhookEvent(client, request, webhookAuthKey);
                if (netsEvent != null)
                {
                    var paymentId = netsEvent.Data?.SelectToken("paymentId")?.Value<string>();

                    var payment = !string.IsNullOrEmpty(paymentId) ? client.GetPayment(paymentId) : null;
                    if (payment != null)
                    {
                        var amount = (long)payment.Payment.OrderDetails.Amount;

                        if (netsEvent.Event == NetsEvents.PaymentCheckoutCompleted)
                        {
                            return CallbackResult.Ok(new TransactionInfo
                            {
                                TransactionId = paymentId,
                                AmountAuthorized = AmountFromMinorUnits(amount),
                                PaymentStatus = GetPaymentStatus(payment)
                            });
                        }
                        else if (netsEvent.Event == NetsEvents.PaymentChargeCreated)
                        {
                            var chargeId = netsEvent.Data?.SelectToken("chargeId")?.Value<string>();

                            return CallbackResult.Ok(new TransactionInfo
                            {
                                TransactionId = paymentId,
                                AmountAuthorized = AmountFromMinorUnits(amount),
                                PaymentStatus = GetPaymentStatus(payment)
                            },
                            new Dictionary<string, string>
                            {
                                { "netsEasyChargeId", chargeId }
                            });
                        }
                        else if (netsEvent.Event == NetsEvents.PaymentCancelCreated)
                        {
                            var cancelId = netsEvent.Data?.SelectToken("cancelId")?.Value<string>();

                            return CallbackResult.Ok(new TransactionInfo
                            {
                                TransactionId = paymentId,
                                AmountAuthorized = AmountFromMinorUnits(amount),
                                PaymentStatus = GetPaymentStatus(payment)
                            },
                            new Dictionary<string, string>
                            {
                                { "netsEasyCancelId", cancelId }
                            });
                        }
                        else if (netsEvent.Event == NetsEvents.PaymentRefundCompleted)
                        {
                            var refundId = netsEvent.Data?.SelectToken("refundId")?.Value<string>();

                            return CallbackResult.Ok(new TransactionInfo
                            {
                                TransactionId = paymentId,
                                AmountAuthorized = AmountFromMinorUnits(amount),
                                PaymentStatus = GetPaymentStatus(payment)
                            },
                            new Dictionary<string, string>
                            {
                                { "netsEasyRefundId", refundId }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<NetsEasyOneTimePaymentProvider>(ex, "Nets Easy - ProcessCallback");
            }

            return CallbackResult.BadRequest();
        }

        public override ApiResult FetchPaymentStatus(OrderReadOnly order, NetsEasyOneTimeSettings settings)
        {
            // Get payment: https://tech.netspayment.com/easy/api/paymentapi#getPayment

            try
            {
                var clientConfig = GetNetsEasyClientConfig(settings);
                var client = new NetsEasyClient(clientConfig);

                var transactionId = order.TransactionInfo.TransactionId;

                // Get payment
                var payment = client.GetPayment(transactionId);
                if (payment != null)
                {
                    return new ApiResult()
                    {
                        TransactionInfo = new TransactionInfoUpdate()
                        {
                            TransactionId = GetTransactionId(payment),
                            PaymentStatus = GetPaymentStatus(payment)
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<NetsEasyOneTimePaymentProvider>(ex, "Nets Easy - FetchPaymentStatus");
            }

            return ApiResult.Empty;
        }

        public override ApiResult CancelPayment(OrderReadOnly order, NetsEasyOneTimeSettings settings)
        {
            // Cancel payment: https://tech.netspayment.com/easy/api/paymentapi#cancelPayment

            try
            {
                var clientConfig = GetNetsEasyClientConfig(settings);
                var client = new NetsEasyClient(clientConfig);

                var transactionId = order.TransactionInfo.TransactionId;

                var data = new
                {
                    amount = AmountToMinorUnits(order.TransactionInfo.AmountAuthorized.Value)
                };

                // Cancel charge
                client.CancelPayment(transactionId, data);

                return new ApiResult()
                {
                    TransactionInfo = new TransactionInfoUpdate()
                    {
                        TransactionId = transactionId,
                        PaymentStatus = PaymentStatus.Cancelled
                    }
                };
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<NetsEasyOneTimePaymentProvider>(ex, "Nets Easy - CancelPayment");
            }

            return ApiResult.Empty;
        }

        public override ApiResult CapturePayment(OrderReadOnly order, NetsEasyOneTimeSettings settings)
        {
            // Charge payment: https://tech.netspayment.com/easy/api/paymentapi#chargePayment

            try
            {
                var clientConfig = GetNetsEasyClientConfig(settings);
                var client = new NetsEasyClient(clientConfig);

                var transactionId = order.TransactionInfo.TransactionId;

                var data = new
                {
                    amount = AmountToMinorUnits(order.TransactionInfo.AmountAuthorized.Value)
                };

                var result = client.ChargePayment(transactionId, data);
                if (result != null)
                {
                    return new ApiResult()
                    {
                        MetaData = new Dictionary<string, string>
                        {
                            { "netsEasyChargeId", result.ChargeId }
                        },
                        TransactionInfo = new TransactionInfoUpdate()
                        {
                            TransactionId = transactionId,
                            PaymentStatus = PaymentStatus.Captured
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<NetsEasyOneTimePaymentProvider>(ex, "Nets Easy - CapturePayment");
            }

            return ApiResult.Empty;
        }

        public override ApiResult RefundPayment(OrderReadOnly order, NetsEasyOneTimeSettings settings)
        {
            // Refund payment: https://tech.netspayment.com/easy/api/paymentapi#refundPayment

            try
            {
                var clientConfig = GetNetsEasyClientConfig(settings);
                var client = new NetsEasyClient(clientConfig);

                var transactionId = order.TransactionInfo.TransactionId;
                var chargeId = order.Properties["netsEasyChargeId"]?.Value;

                var data = new
                {
                    invoice = order.OrderNumber,
                    amount = AmountToMinorUnits(order.TransactionInfo.AmountAuthorized.Value)
                };

                var result = client.RefundPayment(chargeId, data);
                if (result != null)
                {
                    return new ApiResult()
                    {
                        MetaData = new Dictionary<string, string>
                        {
                            { "netsEasyRefundId", result.RefundId }
                        },
                        TransactionInfo = new TransactionInfoUpdate()
                        {
                            TransactionId = transactionId,
                            PaymentStatus = PaymentStatus.Refunded
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<NetsEasyOneTimePaymentProvider>(ex, "Nets Easy - RefundPayment");
            }

            return ApiResult.Empty;
        }

        protected string GetTransactionId(NetsPaymentDetails paymentDetails)
        {
            return paymentDetails?.Payment?.PaymentId;
        }

        protected PaymentStatus GetPaymentStatus(NetsPaymentDetails paymentDetails)
        {
            var payment = paymentDetails.Payment;

            if (payment.Summary.RefundedAmount > 0)
                return PaymentStatus.Refunded;

            if (payment.Summary.CancelledAmount > 0)
                return PaymentStatus.Cancelled;

            if (payment.Summary.ChargedAmount > 0)
                return PaymentStatus.Captured;

            if (payment.Summary.ReservedAmount > 0)
                return PaymentStatus.Authorized;

            return PaymentStatus.Initialized;
        }

        protected NetsEasyClientConfig GetNetsEasyClientConfig(NetsEasySettingsBase settings)
        {
            var prefix = settings.TestMode ? "test-secret-key-" : "live-secret-key-";
            var secretKey = settings.TestMode ? settings.TestSecretKey : settings.LiveSecretKey;
            var auth = secretKey?.Trim().Replace(prefix, string.Empty);

            return new NetsEasyClientConfig
            {
                BaseUrl = $"https://{(settings.TestMode ? "test." : "")}api.dibspayment.eu",
                Authorization = auth
            };
        }

        protected NetsWebhookEvent GetNetsWebhookEvent(NetsEasyClient client, HttpRequestBase request, string webhookAuthorization)
        {
            NetsWebhookEvent netsWebhookEvent = null;

            if (HttpContext.Current.Items["Vendr_NetsEasyWebhookEvent"] != null)
            {
                netsWebhookEvent = (NetsWebhookEvent)HttpContext.Current.Items["Vendr_NetsEasyWebhookEvent"];
            }
            else
            {
                try
                {
                    if (request.InputStream.CanSeek)
                        request.InputStream.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(request.InputStream))
                    {
                        var json = reader.ReadToEnd();

                        if (!string.IsNullOrEmpty(json))
                        {
                            // Verify "Authorization" header returned from webhook
                            VerifyAuthorization(request, webhookAuthorization);

                            netsWebhookEvent = JsonConvert.DeserializeObject<NetsWebhookEvent>(json);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Vendr.Log.Error<NetsEasyOneTimePaymentProvider>(ex, "Nets Easy - GetNetsWebhookEvent");
                }

                HttpContext.Current.Items["Vendr_NetsEasyWebhookEvent"] = netsWebhookEvent;
            }

            return netsWebhookEvent;
        }

        private void VerifyAuthorization(HttpRequestBase request, string webhookAuthorization)
        {
            if (request.Headers["Authorization"] == null)
                throw new Exception("The authorization header is not present in the webhook event.");

            if (request.Headers["Authorization"] != webhookAuthorization)
                throw new Exception("The authorization in the webhook event could not be verified.");
        }

        public static string ForceHttps(string url)
        {
            var uri = new UriBuilder(url);

            var hadDefaultPort = uri.Uri.IsDefaultPort;
            uri.Scheme = Uri.UriSchemeHttps;
            uri.Port = hadDefaultPort ? -1 : uri.Port;

            return uri.ToString();
        }

    }
}
