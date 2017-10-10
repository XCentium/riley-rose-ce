using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Commerce.Plugin.Pricing;

namespace Plugin.Xcentium.RileyRose.Pipelines.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    public class CalculateCartLinesFulfillmentBlockEx : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {

            var adjustments = arg.Adjustments;

            if (adjustments == null || !adjustments.Any()) return await Task.FromResult(arg);

            //var postalPrice = PostalChargeHelper.GetRemotePostalCharge(arg, context).Result;
            var policy = context.GetPolicy<GlobalPhysicalFulfillmentPolicy>();
            //var postalPrice = policy.DefaultCartFulfillmentFee.Amount;

            var postalPrice = 9.99M;

            var currency = context.CommerceContext.CurrentCurrency();

            var money = new Money(currency, postalPrice);
            arg.Adjustments[0].Adjustment = money;



            return await Task.FromResult(arg);
        }
    }
}
