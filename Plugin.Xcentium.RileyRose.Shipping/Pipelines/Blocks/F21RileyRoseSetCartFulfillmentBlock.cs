using CommerceServer.Core.Catalog;
using Plugin.Xcentium.RileyRose.Shipping.Util;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks
{ 

    [PipelineDisplayName("Plugin.Xcentium.RileyRose.Shipping.F21RileyRoseSetCartFulfillmentBlock")]
    public class F21RileyRoseSetCartFulfillmentBlock : PipelineBlock<CartFulfillmentArgument, Cart, CommercePipelineExecutionContext>
    {
        private readonly GetSellableItemCommand _getSellableItemCommand;
        private readonly GetFulfillmentMethodsCommand _getFulfillmentMethodsCommand;
        //private bool containsHazardousItems = false;

        public F21RileyRoseSetCartFulfillmentBlock(GetSellableItemCommand getSellableItemCommand, GetFulfillmentMethodsCommand getFulfillmentMethodsCommand)
            : base((string)null)
        {
            this._getSellableItemCommand = getSellableItemCommand;
            this._getFulfillmentMethodsCommand = getFulfillmentMethodsCommand; 
        }
       
        public override async Task<Cart> Run(CartFulfillmentArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires<CartFulfillmentArgument>(arg).IsNotNull<CartFulfillmentArgument>(string.Format("{0}: The arg cannot be null", (object)this.Name));
            Condition.Requires<Cart>(arg.Cart).IsNotNull<Cart>(string.Format("{0}: The cart cannot be null", (object)this.Name));
            Condition.Requires<FulfillmentComponent>(arg.Fulfillment).IsNotNull<FulfillmentComponent>(string.Format("{0}: The fulfillment cannot be null", (object)this.Name));
            Cart cart = arg.Cart;
            
            Cart tempCart = await new SetCartFulfillmentMethod(_getSellableItemCommand, _getFulfillmentMethodsCommand).SetFulfillment(cart, arg.Fulfillment, context); 
            return await Task.FromResult<Cart>(tempCart);
        }
    } 
}
