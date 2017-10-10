using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Diagnostics;

namespace Plugin.Xcentium.RileyRose.Pipelines.Helper
{
    /// <summary>
    /// 
    /// </summary>
    public static class PostalChargeHelper
    {

        /// <summary>
        /// 
        /// </summary>
        private static readonly HttpClient Client = new HttpClient();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<decimal> GetRemotePostalCharge(Cart arg, CommercePipelineExecutionContext context)
        {


            var productList = new List<object>();
            foreach (var cartItem in arg.Lines)
            {
                var cartComponent = cartItem.ChildComponents.OfType<CartProductComponent>().FirstOrDefault();

                productList.Add
                (
                    new
                    {
                        product = cartComponent?.Id,
                        qty = cartItem.Quantity,
                        program_id = "CA"
                    }
                );
            }

            if (arg.Lines.Any() && arg.HasComponent<PhysicalFulfillmentComponent>())
            {

                var remotePricePolicy = context.GetPolicy<RemotePricePolicy>();

                var fulfillmentComponent = arg.GetComponent<PhysicalFulfillmentComponent>();

                var postageSelection = fulfillmentComponent.FulfillmentMethod.Name;


                var values = new Dictionary<string, string>
                {
                    {"cartTotal", ""},
                    {"postageSelection", postageSelection}
                };

                try
                {
                    var content = new FormUrlEncodedContent(values);

                    var response = await Client.PostAsync(remotePricePolicy.PostagePriceRequestUrl, content);

                    var responseString = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(responseString))
                    {
                        var resp = JsonConvert.DeserializeObject<PriceResponse>(responseString);

                        return await Task.FromResult(resp.PostalCost);
                    }

                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex, "GetRemotePostalCharge");

                }

                var defaultPostagePrice = 0m;

                decimal.TryParse(remotePricePolicy.DefaultPostagePrice, out defaultPostagePrice);

                return await Task.FromResult(defaultPostagePrice);

            }

            return await Task.FromResult(0.0m);
        }
    }
}
