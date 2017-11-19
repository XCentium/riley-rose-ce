using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Plugin.Xcentium.CartProperties.Components;
using Plugin.Xcentium.CartProperties.Models;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Carts;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Plugin.Xcentium.CartProperties.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class SetCartPropertiesCommand : CommerceCommand
    {
        private readonly FindEntityPipeline _findEntityPipeline;
        private readonly GetCartCommand _getCartCommand;

        /// <summary>
        /// 
        /// </summary>
        private readonly IGetCartPipeline _getCartPipeline;

        /// <summary>
        /// 
        /// </summary>
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findEntityPipeline"></param>
        /// <param name="getCartCommand"></param>
        /// <param name="getCartPipeline"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="persistEntityPipeline"></param>
        public SetCartPropertiesCommand(FindEntityPipeline findEntityPipeline, GetCartCommand getCartCommand, IGetCartPipeline getCartPipeline,
            IServiceProvider serviceProvider,
            IPersistEntityPipeline persistEntityPipeline) : base(serviceProvider)
        {
            _findEntityPipeline = findEntityPipeline;
            _getCartCommand = getCartCommand;
            _getCartPipeline = getCartPipeline;
            _persistEntityPipeline = persistEntityPipeline;
        }

        public async Task<Cart> Process(CommerceContext commerceContext, string cartId, Models.CartProperties cartProperties, CartLineProperties lineProperties)
        {
            try
            {



                

                var resolveCartArgument = new ResolveCartArgument(
                    commerceContext.CurrentShopName(),
                    cartId,
                    commerceContext.CurrentShopperId());

                var cart =
                    await this._getCartPipeline.Run(resolveCartArgument, commerceContext.GetPipelineContextOptions());

                var cart4 = GetCart(cartId, commerceContext);
                if (cart4 == null)
                {
                    return null;
                }

                // Set the custom fields on the cart
                if (cartProperties?.Properties != null && cartProperties.Properties.KeyValues.Any())
                {
                    cart.GetComponent<CartComponent>().Properties = cartProperties.Properties;
                }

                // Set the custom fields on the cartlines
                if (cart.Lines != null && cart.Lines.Any() && lineProperties != null &&
                    lineProperties.CartLineProperty.Any())
                {
                    foreach (var cartLineProperty in lineProperties.CartLineProperty)
                    {
                        var cartLineComponent = cart.Lines.FirstOrDefault(x => x.Id == cartLineProperty.CartLineId);
                        if (cartLineComponent != null)
                            cartLineComponent
                                .GetComponent<CartComponent>().Properties = cartLineProperty.Properties;
                    }
                }


                // Save the cart here to make sure the new flag is set
                var result = await this._persistEntityPipeline.Run(new PersistEntityArgument(cart), commerceContext.GetPipelineContextOptions());

                return result.Entity as Cart;
            }
            catch (Exception e)
            {
                return await Task.FromException<Cart>(e);
            }
        }

        private Cart GetCart(string cartId, CommerceContext commerceContext)
        {
            var shopName = commerceContext.CurrentShopName();
            var shopperId = commerceContext.CurrentShopperId();
            var customerId = commerceContext.CurrentCustomerId();

            var url =
                $"http://localhost:5000/api/Carts('{cartId}')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components($expand=ChildComponents)";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("ShopName", shopName);
            client.DefaultRequestHeaders.Add("ShopperId", shopperId);
            client.DefaultRequestHeaders.Add("Language", "en-US");
            client.DefaultRequestHeaders.Add("Environment", "RileyAuthoring");
            client.DefaultRequestHeaders.Add("CustomerId", customerId);
            client.DefaultRequestHeaders.Add("Currency", "USD");
            client.DefaultRequestHeaders.Add("Roles", "sitecore\\Pricer Manager|sitecore\\Promotioner Manager");


            try
            {

                var response = client.GetStringAsync(url).Result;


                var cart = JsonConvert.DeserializeObject<Cart>(response);
                client.Dispose();
                return cart;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                client.Dispose();
                return null;
            }



        }

    }
}
