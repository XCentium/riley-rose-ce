using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks
{

    [PipelineDisplayName("Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks.CalculateCartTotalsBlock")]
    public class CalculateCartTotalsBlockEx : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        public CalculateCartTotalsBlockEx()
          : base((string)null)
        {
        }

        public override Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires<Cart>(arg).IsNotNull<Cart>(string.Format("{0}: The cart can not be null", (object)this.Name));
            string currencyCode = context.CommerceContext.CurrentCurrency();
            GlobalPricingPolicy globalPricingPolicy = context.GetPolicy<GlobalPricingPolicy>();
            Decimal amount = new Decimal();
            foreach (AwardedAdjustment adjustment in (IEnumerable<AwardedAdjustment>)arg.Adjustments)
            {
                if (globalPricingPolicy.ShouldRoundPriceCalc)
                    adjustment.Adjustment.Amount = Decimal.Round(adjustment.Adjustment.Amount, globalPricingPolicy.RoundDigits, globalPricingPolicy.MidPointRoundUp ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven);
                amount += adjustment.Adjustment.Amount;
            }
            Decimal lineAdjustmentsTotal = new Decimal();
            arg.Lines.ForEach<CartLineComponent>((Action<CartLineComponent>)(l =>
            {
                foreach (AwardedAdjustment adjustment in (IEnumerable<AwardedAdjustment>)l.Adjustments)
                {
                    if (globalPricingPolicy.ShouldRoundPriceCalc)
                        adjustment.Adjustment.Amount = Decimal.Round(adjustment.Adjustment.Amount, globalPricingPolicy.RoundDigits, globalPricingPolicy.MidPointRoundUp ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven);
                    lineAdjustmentsTotal += adjustment.Adjustment.Amount;
                }
            }));
            Decimal num = arg.Lines.Aggregate<CartLineComponent, Decimal>(Decimal.Zero, (Func<Decimal, CartLineComponent, Decimal>)((c, a) => c + a.Totals.SubTotal.Amount));
            arg.Totals.AdjustmentsTotal = new Money(currencyCode, amount);
            arg.Totals.GrandTotal = new Money(currencyCode, amount + num + lineAdjustmentsTotal);
            return Task.FromResult<Cart>(arg);
        }
    }
}
