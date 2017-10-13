

//namespace Plugin.Xcentium.RileyRose.Payment.Pipelines.Blocks

// public class ApplyF21GiftCardBllock


//CalculateCartFulfillmentBlockEx

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Xcentium.RileyRose.Payment.Components; 
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Fulfillment; 
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Commerce.Plugin.Pricing;

namespace Plugin.Xcentium.RileyRose.Payment.Pipelines.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplyF21GiftCardBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    { 

        /// <summary>
        /// 
        /// </summary>
        
        public ApplyF21GiftCardBlock() : base(null)
        { 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {

            var adjustments = arg.Adjustments;
            //Create Ability to add multiple Gift Cards
            MultipleGiftCardPaymentComponent mpComponent = arg.Components.OfType<MultipleGiftCardPaymentComponent>().FirstOrDefault<MultipleGiftCardPaymentComponent>();
            MultipleGiftCardPaymentComponent mpComponent2 =   context.CommerceContext.Objects.OfType<MultipleGiftCardPaymentComponent>().FirstOrDefault<MultipleGiftCardPaymentComponent>();


            //decimal cartTotal = arg.Totals.GrandTotal.Amount;

            //If Valid Balance exists, Add Gift Card Payment / Else Remove 
            var giftCardAdjustments = GenerateGiftCardAdjustments(arg.Totals.GrandTotal.Amount, mpComponent);
            foreach (var gca in giftCardAdjustments)
            {
                //arg.Adjustments.Add(gca);
                //arg.Adjustments[0].Adjustment = shippingAwardedAdjustment.Adjustment;
            }




            /*
            if (adjustments == null || !adjustments.Any()) return await Task.FromResult(arg);

            //var postalPrice = PostalChargeHelper.GetRemotePostalCharge(arg, context).Result;
            var policy = context.GetPolicy<GlobalPhysicalFulfillmentPolicy>();
            //var postalPrice = policy.DefaultCartFulfillmentFee.Amount;

            //Get Shipping Options from Sitecore
            var shippingOptions = await ShippingOptionsModel.GetData(_getItemByPathPipeline, _getItemChildrenByPathPipeline, context);

            //Calculate Custom Shipping Charges
            var shippingAwardedAdjustment = ShippingCalculator.GetShippingAdjustment(arg, shippingOptions, context);


            arg.Adjustments[0].Adjustment = shippingAwardedAdjustment.Adjustment;
            */


            return await Task.FromResult(arg);
        }


        public List<CartLevelAwardedAdjustment> GenerateGiftCardAdjustments(decimal cartTotal, MultipleGiftCardPaymentComponent mpComponent)
        {
            List<CartLevelAwardedAdjustment> cartLevelAdjustements = new List<CartLevelAwardedAdjustment>();


            //Create Dynamic Gift Card from all submitted Gift Cards

            /*
            GiftCard card = new GiftCard()
            {
                Balance = giftCardPaymentComponent.Amount,
                GiftCardCode = giftCardPaymentComponent.GiftCardCode,
                OriginalAmount = giftCardPaymentComponent.Amount
            };
            */



            decimal giftCardAvailableBalance = mpComponent.GetAllGiftCardTotalsBalance();

            if (giftCardAvailableBalance <= 0) return null;



            decimal appliedGiftCardCredits = 0M;
            foreach (var gcPayment in mpComponent.GiftCardPaymentList)
            {
                //Create CartLevel Adjustment


                //Calc remaining cartotal
                var remainingCartTotal = cartTotal - appliedGiftCardCredits;

                //Determine How Much of the card balance to use (other communication to F21 gift card service is handled by front-end code)
                var amountFromGiftCard = gcPayment.OriginalBalance.Amount <= remainingCartTotal
                    ? gcPayment.OriginalBalance.Amount
                    : cartTotal;

                cartLevelAdjustements.Add(CreateGiftCardAdjustment
                    (
                        gcPayment.CardNumber,
                        new Money(gcPayment.OriginalBalance.CurrencyCode,
                            gcPayment.OriginalBalance.Amount))
                );

            }



            /*
            GiftCardPaymentComponent payment = new GiftCardPaymentComponent()
            {
                Name = "F21 GiftCard",
                Balance = new Money(card.Balance.CurrencyCode, amountFromGiftCard),
                Amount = new Money(card.Balance.CurrencyCode, amountFromGiftCard),
                GiftCard = card.AsReference(),
                GiftCardCode = mpComponent.GetDynamicGiftCardCode()
            };
            */

            /*
             * 
            //Calculate Custom Shipping Charges
            var shippingAwardedAdjustment = ShippingCalculator.GetShippingAdjustment(arg, shippingOptions, context);


            arg.Adjustments[0].Adjustment = shippingAwardedAdjustment.Adjustment;



            */



            return cartLevelAdjustements;
        }


        public static CartLevelAwardedAdjustment CreateGiftCardAdjustment(string cardNumber, Money adjustmentAmount)
        {
            CartLevelAwardedAdjustment awardedAdjustment = new CartLevelAwardedAdjustment();
            string str1 = "F21 GiftCard - " + cardNumber;
            awardedAdjustment.Name = str1;
            string str2 = "F21 GiftCard - " + cardNumber;
            awardedAdjustment.DisplayName = str2;
            awardedAdjustment.Adjustment = adjustmentAmount;
            awardedAdjustment.AdjustmentType = "F21 Gift Card";
            awardedAdjustment.AwardingBlock = "ApplyF21GiftCardBlock";
            return awardedAdjustment;
        }


    }
}
