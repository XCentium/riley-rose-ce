using Plugin.Xcentium.RileyRose.Shipping.Models;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping.Util
{
    /// <summary>
    /// 
    /// </summary>
    public static class ShippingCalculator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cartTotal"></param>
        /// <param name="selectedShippingOption"></param>
        /// <returns></returns>
        public static Decimal CalculateShippingCost(decimal cartTotal, ShippingOption selectedShippingOption)
        {
            decimal definedCost = 0;
            

            foreach(var priceDef in selectedShippingOption.ShippingDefinitions)
            {
                if (IsInPriceRange(cartTotal, priceDef))
                {
                    return priceDef.Cost;
                }
            } 
            return definedCost;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="priceDef"></param>
        /// <returns></returns>
        public static bool IsInPriceRange(decimal amount, ShippingPriceDefinition priceDef)
        { 
            return (amount >= priceDef.MinAmount && amount <= priceDef.MaxAmount); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cart"></param>
        /// <param name="shippingOptions"></param>
        /// <returns></returns>
        public static ShippingOption GetSelectedShippingOption(Cart cart, List<ShippingOption> shippingOptions)
        {
            FulfillmentComponent fulfillmentComponent = cart.GetComponent<FulfillmentComponent>(); 
            return shippingOptions.FirstOrDefault(x => x.Name == fulfillmentComponent.FulfillmentMethod.Name); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cart"></param>
        /// <param name="shippingOptions"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static CartLevelAwardedAdjustment GetShippingAdjustment(Cart cart, List<ShippingOption> shippingOptions, CommercePipelineExecutionContext context)
        {

            CartLevelAwardedAdjustment awardedAdjustment = new CartLevelAwardedAdjustment();
            string currency = context.CommerceContext.CurrentCurrency();
            string fulfillment = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Fulfillment;

            var selectedShippingOption = GetSelectedShippingOption(cart, shippingOptions);
            decimal amount = CalculateShippingCost(cart.Totals.SubTotal.Amount, selectedShippingOption);

            string str1 = "ShippingFee";
            awardedAdjustment.Name = str1;
            string str2 = "ShippingFee";
            awardedAdjustment.DisplayName = str2;
            Money money = new Money(currency, amount);
            awardedAdjustment.Adjustment = money;
            awardedAdjustment.AdjustmentType = fulfillment;
            awardedAdjustment.AwardingBlock = "ShippingCalculator";
            return awardedAdjustment;
        }  

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputVal"></param>
        /// <returns></returns>
        public static decimal GetPriceValue(string inputVal)
        {
            decimal amount = 0;
            bool success = Decimal.TryParse(inputVal, out amount);
            return amount;
        }
    }
}
