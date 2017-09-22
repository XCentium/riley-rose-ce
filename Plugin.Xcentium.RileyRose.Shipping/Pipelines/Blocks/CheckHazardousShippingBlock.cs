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
    // public class CheckHazardousShipping
    [PipelineDisplayName("Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks.CheckHazardousShipping")]
    public class CheckHazardousShippingBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        private readonly GetSellableItemCommand _getSellableItemCommand;
        private readonly GetFulfillmentMethodsCommand _getFulfillmentMethodsCommand;
        
        public CheckHazardousShippingBlock(GetSellableItemCommand getSellableItemCommand, GetFulfillmentMethodsCommand getFulfillmentMethodsCommand)
            : base((string)null)
        {
            this._getSellableItemCommand = getSellableItemCommand;
            this._getFulfillmentMethodsCommand = getFulfillmentMethodsCommand;
        }
        public override async Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires<Cart>(arg).IsNotNull<Cart>(string.Format("{0}: the cart can not be null.", (object)this.Name));
            Cart tempCart = await new SetCartFulfillmentMethod(_getSellableItemCommand, _getFulfillmentMethodsCommand).SetFulfillment(arg, null, context);

            return await Task.FromResult<Cart>(tempCart);
        }
    }
}
