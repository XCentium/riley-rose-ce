using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.Xcentium.CartProperties.Components;
using Plugin.Xcentium.CartProperties.Models;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Carts;

namespace Plugin.Xcentium.CartProperties.Commands
{
    public class SetCartLinePropertiesCommand : CommerceCommand
    {

        /// <summary>
        /// 
        /// </summary>
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="persistEntityPipeline"></param>
        public SetCartLinePropertiesCommand(IServiceProvider serviceProvider,
            IPersistEntityPipeline persistEntityPipeline) : base(serviceProvider)
        {
            _persistEntityPipeline = persistEntityPipeline;
        }

        public async Task<Cart> Process(CommerceContext commerceContext, string cartId, CartLineProperties lineProperties, string baseUrl)
        {
            try
            {
                var gitfCardComponentSet = false;

                var cart = GetCart(cartId, commerceContext, baseUrl);
                if (cart == null)
                {
                    return null;
                }

                if (cart.Lines != null && cart.Lines.Any() && lineProperties != null &&
                    lineProperties.CartLineProperty.Any())
                {

                    foreach (var cartLineProperty in lineProperties.CartLineProperty)
                    {

                        var cartLineComponent = cart.Lines.FirstOrDefault(x => x.ItemId == cartLineProperty.CartLineId);
                        if (cartLineComponent != null)
                        {

                            var giftCardData =
                                cartLineProperty.Properties.KeyValues.FirstOrDefault(
                                    x => x.Key.ToLower() == Constants.Settings.Giftcarddata.ToLower());

                            if (giftCardData != null)
                            {
                                try
                                {
                                    var giftCardMessageComponent = JsonConvert.DeserializeObject<GiftCardMessageComponent>(giftCardData.Value.ToString());
                                    giftCardMessageComponent.Name = Constants.Settings.GiftCardMessageComponent;
                                    cartLineComponent.SetComponent(giftCardMessageComponent);
                                    gitfCardComponentSet = true;

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }

                        }

                    }
                }

                if (gitfCardComponentSet)
                {
                    var result = await this._persistEntityPipeline
                        .Run(new PersistEntityArgument(cart),
                        commerceContext.GetPipelineContextOptions());

                    return result.Entity as Cart;
                }

                return null;
            }
            catch (Exception e)
            {
                return await Task.FromException<Cart>(e);
            }
        }

        private Cart GetCart(string cartId, CommerceContext commerceContext, string baseUrl)
        {
            var url = string.Format(Constants.Settings.EndpointUrl, baseUrl, cartId);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.Settings.AppJson));
            client.DefaultRequestHeaders.Add(Constants.Settings.ShopName, commerceContext.CurrentShopName());
            client.DefaultRequestHeaders.Add(Constants.Settings.ShopperId, commerceContext.CurrentShopperId());
            client.DefaultRequestHeaders.Add(Constants.Settings.Language, commerceContext.CurrentLanguage());
            client.DefaultRequestHeaders.Add(Constants.Settings.Environment, commerceContext.Environment.Name);
            client.DefaultRequestHeaders.Add(Constants.Settings.CustomerId, commerceContext.CurrentCustomerId());
            client.DefaultRequestHeaders.Add(Constants.Settings.Currency, commerceContext.CurrentCurrency());
            client.DefaultRequestHeaders.Add(Constants.Settings.Roles, Constants.Settings.CartRoles);

            try
            {
                var cart = new Cart();

                var response = client.GetAsync(url).Result;

                if (response != null)
                {

                    var task = response.Content.ReadAsStreamAsync().ContinueWith(t =>
                    {
                        var stream = t.Result;
                        using (var reader = new StreamReader(stream))
                        {
                            var responseValue = reader.ReadToEnd();
                            cart = JsonConvert.DeserializeObject<Cart>(responseValue);
                        }
                    });

                    task.Wait();
                }

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
