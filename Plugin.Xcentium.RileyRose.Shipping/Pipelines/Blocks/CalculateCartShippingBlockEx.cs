

using Microsoft.Extensions.Logging;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping
{
    [PipelineDisplayName("Fulfillment.block.CalculateCartShippingBlockEx")]
    public class CalculateCartShippingBlockEx : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        private readonly IGetSellableItemPipeline _getSellableItemPipeline;
        private readonly IGetItemByPathPipeline _getItemByPathPipeline;
        private readonly IGetItemChildrenPipeline _getItemChildrenByPathPipeline;

        /*
        public CalculateCartShippingBlockEx()
          : base((string)null)
        {

        }
        */
        public CalculateCartShippingBlockEx(IGetItemByPathPipeline getItemByPathPipeline, IGetItemChildrenPipeline getItemChildrenByPathPipeline,
          IGetSellableItemPipeline getSellableItemPipeline) : base(null)
        {
            _getItemByPathPipeline = getItemByPathPipeline;
            _getSellableItemPipeline = getSellableItemPipeline;
            _getItemChildrenByPathPipeline = getItemChildrenByPathPipeline;
        }

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


            /*
            //Get Default Fee Amount
            Decimal amount = policy.DefaultCartFulfillmentFee.Amount;
            if (policy.DefaultCartFulfillmentFees.Any<Money>((Func<Money, bool>)(p => p.CurrencyCode.Equals(currency, StringComparison.OrdinalIgnoreCase))))
                amount = policy.DefaultCartFulfillmentFees.First<Money>((Func<Money, bool>)(p => p.CurrencyCode.Equals(currency, StringComparison.OrdinalIgnoreCase))).Amount;
            context.Logger.LogDebug(string.Format("{0} - Default Fulfillment Fee:{1} {2}", (object)this.Name, (object)currency, (object)amount), Array.Empty<object>());

            //Check for ElectronicFulfillment or SplitFulfillment
            if (fulfillmentComponent is ElectronicFulfillmentComponent || fulfillmentComponent is SplitFulfillmentComponent)
                return await Task.FromResult<Cart>(arg);

            //Check for FulfilmentFees - return if exist
            context.Logger.LogDebug(string.Format("{0} - Fulfillment Method:{1}", (object)this.Name, (object)fulfillmentComponent.FulfillmentMethod.Name), Array.Empty<object>());
            if (policy.FulfillmentFees.Any<FulfillmentFee>((Func<FulfillmentFee, bool>)(p =>
            {
                if (p.Name.Equals(fulfillmentComponent.FulfillmentMethod.Name, StringComparison.OrdinalIgnoreCase))
                    return p.Fee.CurrencyCode.Equals(context.CommerceContext.CurrentCurrency(), StringComparison.OrdinalIgnoreCase);
                return false;
            })))
            {
                amount = policy.FulfillmentFees.First<FulfillmentFee>((Func<FulfillmentFee, bool>)(p => p.Name.Equals(fulfillmentComponent.FulfillmentMethod.Name, StringComparison.OrdinalIgnoreCase))).Fee.Amount;
                context.Logger.LogDebug(string.Format("{0} - Specific fee:{1}", (object)this.Name, (object)fulfillmentComponent.FulfillmentMethod.Name), Array.Empty<object>());
            }

            //Get the existing AwardedAdjustments
            IList<AwardedAdjustment> adjustments = arg.Adjustments;

            //Set the CartLevelAwardedAdjustments
            CartLevelAwardedAdjustment awardedAdjustment = new CartLevelAwardedAdjustment();
            string str1 = "FulfillmentFee";
            awardedAdjustment.Name = str1;
            string str2 = "FulfillmentFee";
            awardedAdjustment.DisplayName = str2;
            Money money = new Money(currency, amount);
            awardedAdjustment.Adjustment = money;

            //Set the Adjustment Fulfillment property
            string fulfillment = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Fulfillment;
            awardedAdjustment.AdjustmentType = fulfillment;
            int num = 0;
            awardedAdjustment.IsTaxable = num != 0;
            string name = this.Name;

            //Set the Adjustment AwardingBlock
            awardedAdjustment.AwardingBlock = name;


            */

            //Set the CartLevelAwardedAdjustments
            CartLevelAwardedAdjustment awardedAdjustment = new CartLevelAwardedAdjustment();

            //Get the existing AwardedAdjustments
            IList<AwardedAdjustment> adjustments = arg.Adjustments;

            //Get Shipping Options from Sitecore
            var shippingOptions = await ShippingOptionsModel.GetData(_getItemByPathPipeline, _getItemChildrenByPathPipeline, context); 

            //Calculate Custom Shipping Charges
            var ShippingAwardedAdjustment = ShippingCalculator.GetShippingAdjustment(arg, shippingOptions, context);

            //Add All Adjustments to the Cart
            //adjustments.Add((AwardedAdjustment)awardedAdjustment);
            adjustments.Add((AwardedAdjustment)ShippingAwardedAdjustment);
            return await Task.FromResult<Cart>(arg);
        }
    }
}