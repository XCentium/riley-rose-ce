

using Microsoft.Extensions.Logging;
using Plugin.Xcentium.RileyRose.Shipping.Util;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping
{
    [PipelineDisplayName("Fulfillment.block.CalculateCartFulfillmentBlock")]
    public class CalculateCartFulfillmentBlockEx : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        public CalculateCartFulfillmentBlockEx()
          : base((string)null)
        {
        }

        public override Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires<Cart>(arg).IsNotNull<Cart>(string.Format("{0}: The cart cannot be null.", (object)this.Name));
            if (!arg.HasComponent<FulfillmentComponent>())
                return Task.FromResult<Cart>(arg);

            FulfillmentComponent fulfillmentComponent = arg.GetComponent<FulfillmentComponent>();
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
                return Task.FromResult<Cart>(arg);
            }
            string currency = context.CommerceContext.CurrentCurrency();
            GlobalPhysicalFulfillmentPolicy policy = context.GetPolicy<GlobalPhysicalFulfillmentPolicy>();
            Decimal amount = policy.DefaultCartFulfillmentFee.Amount;
            if (policy.DefaultCartFulfillmentFees.Any<Money>((Func<Money, bool>)(p => p.CurrencyCode.Equals(currency, StringComparison.OrdinalIgnoreCase))))
                amount = policy.DefaultCartFulfillmentFees.First<Money>((Func<Money, bool>)(p => p.CurrencyCode.Equals(currency, StringComparison.OrdinalIgnoreCase))).Amount;
            context.Logger.LogDebug(string.Format("{0} - Default Fulfillment Fee:{1} {2}", (object)this.Name, (object)currency, (object)amount), Array.Empty<object>());
            if (fulfillmentComponent is ElectronicFulfillmentComponent || fulfillmentComponent is SplitFulfillmentComponent)
                return Task.FromResult<Cart>(arg);
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
            IList<AwardedAdjustment> adjustments = arg.Adjustments;
            CartLevelAwardedAdjustment awardedAdjustment = new CartLevelAwardedAdjustment();
            string str1 = "FulfillmentFee";
            awardedAdjustment.Name = str1;
            string str2 = "FulfillmentFee";
            awardedAdjustment.DisplayName = str2;
            Money money = new Money(currency, amount);
            awardedAdjustment.Adjustment = money;

            string fulfillment = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Fulfillment;
            awardedAdjustment.AdjustmentType = fulfillment;
            int num = 0;
            awardedAdjustment.IsTaxable = num != 0;
            string name = this.Name;
            awardedAdjustment.AwardingBlock = name;


            //var ShippingAwardedAdjustment = ShippingCalculator.GetShippingAdjustment(arg, context);

            adjustments.Add((AwardedAdjustment)awardedAdjustment);
            //adjustments.Add((AwardedAdjustment)ShippingAwardedAdjustment);
            return Task.FromResult<Cart>(arg);
        }
    }
}