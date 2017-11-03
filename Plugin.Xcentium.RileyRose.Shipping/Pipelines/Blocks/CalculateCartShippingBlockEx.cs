using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Xcentium.RileyRose.Shipping.Models;
using Plugin.Xcentium.RileyRose.Shipping.Util;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    [PipelineDisplayName("Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks.CalculateCartShippingBlockEx")]
    public class CalculateCartShippingBlockEx : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        private readonly IGetSellableItemPipeline _getSellableItemPipeline;
        private readonly IGetItemByPathPipeline _getItemByPathPipeline;
        private readonly IGetItemChildrenPipeline _getItemChildrenByPathPipeline;
 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="getItemByPathPipeline"></param>
        /// <param name="getItemChildrenByPathPipeline"></param>
        /// <param name="getSellableItemPipeline"></param>
        public CalculateCartShippingBlockEx(IGetItemByPathPipeline getItemByPathPipeline, IGetItemChildrenPipeline getItemChildrenByPathPipeline,
          IGetSellableItemPipeline getSellableItemPipeline) : base(null)
        {
            _getItemByPathPipeline = getItemByPathPipeline;
            _getSellableItemPipeline = getSellableItemPipeline;
            _getItemChildrenByPathPipeline = getItemChildrenByPathPipeline;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            //Null check for cart
            Condition.Requires<Cart>(arg).IsNotNull<Cart>(string.Format("{0}: The cart cannot be null.", (object)this.Name));

            //Null check Fulfillment Component
            if (!arg.HasComponent<FulfillmentComponent>())
                return await Task.FromResult<Cart>(arg);

            //Get Fulfillment Component
            FulfillmentComponent fulfillmentComponent = arg.GetComponent<FulfillmentComponent>();

            //Check for CartLine Component
            if (!arg.Lines.Any<CartLineComponent>())
            {
                List<Component> list = arg.Components.ToList<Component>();
                list.Remove((Component)fulfillmentComponent);
                arg.Components = (IList<Component>)list;


                arg.Adjustments.Where<AwardedAdjustment>((Func<AwardedAdjustment, bool>)(a =>
                {
                    if (!string.IsNullOrEmpty(a.Name) && a.Name.Equals("FulfillmentFee", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(a.AdjustmentType))
                        return a.AdjustmentType.Equals(context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Fulfillment, StringComparison.OrdinalIgnoreCase);
                    return false;
                })).ToList<AwardedAdjustment>().ForEach((Action<AwardedAdjustment>)(a => arg.Adjustments.Remove(a)));
                return await Task.FromResult<Cart>(arg);
            }

            //Get Context Currency
            string currency = context.CommerceContext.CurrentCurrency();

            //Create new GlobalPhysicalFulfillment Policy
            GlobalPhysicalFulfillmentPolicy policy = context.GetPolicy<GlobalPhysicalFulfillmentPolicy>();
             
            //Set the CartLevelAwardedAdjustments
            CartLevelAwardedAdjustment awardedAdjustment = new CartLevelAwardedAdjustment();

            //Get the existing AwardedAdjustments
            IList<AwardedAdjustment> adjustments = arg.Adjustments;

            //Get Shipping Options from Sitecore
            var shippingOptions = await ShippingOptionsModel.GetData(_getItemByPathPipeline, _getItemChildrenByPathPipeline, context); 

            //Calculate Custom Shipping Charges
            var shippingAwardedAdjustment = ShippingCalculator.GetShippingAdjustment(arg, shippingOptions, context);

            //Add All Adjustments to the Cart
            //adjustments.Add((AwardedAdjustment)awardedAdjustment);
            adjustments.Add((AwardedAdjustment)shippingAwardedAdjustment);
            return await Task.FromResult<Cart>(arg);
        }
    }
}