using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.Xcentium.RileyRose.Payment.Helper;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.GiftCards;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Xcentium.RileyRose.Payment.Pipelines.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateFederatedPaymentBlock : PipelineBlock<CartEmailArgument, CartEmailArgument, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// A cart with federate payment component
        /// </returns>
        public override async Task<CartEmailArgument> Run(CartEmailArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The cart can not be null");

            var cart = arg.Cart;


            if (!cart.HasComponent<FederatedPaymentComponent>())
            {
                return arg;
            }


            if (cart.HasComponent<FederatedPaymentComponent>())
            {
                var payment = cart.GetComponent<FederatedPaymentComponent>();

                if (string.IsNullOrEmpty(payment.PaymentMethodNonce))
                {
                    context.Abort(
                        await context.CommerceContext.AddMessage(
                            context.GetPolicy<KnownResultCodes>().Error,
                            "InvalidOrMissingPropertyValue",
                            new object[] { "PaymentMethodNonce" },
                            $"Invalid or missing value for property 'PaymentMethodNonce'."), context);

                    return arg;
                }


                try
                {
                    if (payment.PaymentMethodNonce.ToLower().Contains("c|") || payment.PaymentMethodNonce.ToLower().Contains("cg|"))
                    {
                        var strList = payment.PaymentMethodNonce.Split('|').ToList();

                        var dicQueryString = GetChasePaymentDataList(strList[1]);

                        if (dicQueryString != null)
                        {
                            var txnGuid = dicQueryString.ContainsKey("TxnGUID") ? dicQueryString["TxnGUID"] : string.Empty;
                            var message = dicQueryString.ContainsKey("message") ? dicQueryString["message"] : string.Empty;
                            var maskedCreditCardNumber = dicQueryString.ContainsKey("mPAN") ? dicQueryString["mPAN"] : string.Empty;
                            var expirationDate = dicQueryString.ContainsKey("exp") ? dicQueryString["exp"] : string.Empty;
                            var type = dicQueryString.ContainsKey("type") ? dicQueryString["type"] : string.Empty;
                            var uId = dicQueryString.ContainsKey("uID") ? dicQueryString["uID"] : string.Empty;

                            payment.TransactionId = txnGuid;
                            payment.TransactionStatus = message;
                            payment.MaskedNumber = maskedCreditCardNumber;

                            if (!string.IsNullOrEmpty(expirationDate))
                            {
                                payment.ExpiresMonth = int.Parse(expirationDate.Substring(0, 2));
                                payment.ExpiresYear = int.Parse(expirationDate.Substring(expirationDate.Length - 2, 2));
                            }

                            payment.CardType = type;
                            payment.PaymentMethodNonce = $"{strList[0]}|" +
                                                         $"{type}|" +
                                                         $"{maskedCreditCardNumber}|" +
                                                         $"{expirationDate}|" +
                                                         $"{uId}";
                        }
                    }
                    return arg;
                }
                catch (Exception ex)
                {
                    context.Abort(
                        await context.CommerceContext.AddMessage(
                            context.GetPolicy<KnownResultCodes>().Error,
                            "InvalidClientPolicy",
                            new object[] { "CreditCardPayment" },
                            $"{this.Name}. Invalid CreditCardPayment { ex.Message }"),
                            context);
                    return arg;
                }
            }

            return arg;

        }

        private Dictionary<string,string> GetChasePaymentDataList(string uid)
        {
            var connStr = "http://cf.reference.storefront.com";
            // http://cf.reference.storefront.com/rileyroseapi/checkout/GetPaymentInfo?uid=2C4465E2AC14ACA591D89E4C5EE8C1C4

            var url = $"{connStr}/{"rileyroseapi/checkout/GetPaymentInfo"}?uid={uid}";

            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) return null;
                using (var reader = new StreamReader(stream))
                {
                    var responseString = reader.ReadToEnd().Trim();
                    if (!string.IsNullOrEmpty(responseString))
                    {
                        try
                        {
                            var orderCounterResponse =
                                JsonConvert.DeserializeObject<GetPaymentResponse>(responseString);
                            if (orderCounterResponse.Success)
                            {

                                var queryResult = orderCounterResponse.Result;

                                if (!string.IsNullOrEmpty(queryResult) && queryResult.Contains("="))
                                {
                                    var dicQueryString =
                                        queryResult.Split('&')
                                            .ToDictionary(c => c.Split('=')[0],
                                                c => Uri.UnescapeDataString(c.Split('=')[1]));

                                    return dicQueryString;


                                }


                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GenerateRandomNo()
        {
            const int min = 1001;
            const int max = 999999999;
            var rdm = new Random();
            return rdm.Next(min, max);
        }
    }
}
