
// Plugin.Xcentium.RileyRose.Payment.Pipelines.Blocks
//public class ValidateGiftCardPaymentBlockEx 


// Decompiled with JetBrains decompiler
// Type: Sitecore.Commerce.Plugin.GiftCards.ValidateGiftCardPaymentBlock
// Assembly: Sitecore.Commerce.Plugin.GiftCards, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CDE13B34-D78F-4FCE-89DA-B1236C1596D1
// Assembly location: C:\inetpub\CommerceAuthoring\Sitecore.Commerce.Plugin.GiftCards.dll

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Plugin.Xcentium.RileyRose.Payment.Components;
using Sitecore.Commerce.Plugin.GiftCards;

namespace Plugin.Xcentium.RileyRose.Payment.Pipelines.Blocks
{
    [PipelineDisplayName("Plugin.Xcentium.RileyRose.Payment.Pipelines.Blocks.ValidateGiftCardPaymentBlock")]
    public class ValidateGiftCardPaymentBlockEx : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        private readonly IFindEntityPipeline _findEntityPipeline;

        public ValidateGiftCardPaymentBlockEx(IFindEntityPipeline findEntityPipeline)
            : base((string)null)
        {
            this._findEntityPipeline = findEntityPipeline;
        }

        public override async Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires<Cart>(arg).IsNotNull<Cart>(string.Format("{0}: The cart can not be null", (object)this.Name));
            CartPaymentsArgument argument = context.CommerceContext.Objects.OfType<CartPaymentsArgument>().FirstOrDefault<CartPaymentsArgument>();
            if (argument == null)
            {
                var executionContext = context;
                string reason = await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "ArgumentNotFound", new object[1] { (object)typeof(CartPaymentsArgument).Name }, string.Format("Argument of type {0} was not found in context.", (object)typeof(CartPaymentsArgument).Name));
                executionContext.Abort(reason, (object)context);
                executionContext = (CommercePipelineExecutionContext)null;
                return (Cart)null;
            }
            IEnumerable<PaymentComponent> payments = argument.Payments;


            //Name = "Federated"
            //((Sitecore.Commerce.Plugin.Payments.PaymentComponent[])argument.Payments)[0].PaymentMethod.Name

            if (argument.Payments.FirstOrDefault().PaymentMethod.Name != "Gift Card") return arg;
            //Get Submitted Gift Card Component
            var giftCardPaymentComponent = (GiftCardPaymentComponent)argument.Payments.FirstOrDefault();
            if (giftCardPaymentComponent == null) return arg;

            var giftCardCode = giftCardPaymentComponent.GiftCardCode;
            var giftCardPaymentAmount = giftCardPaymentComponent.Amount;
            decimal cartTotal = arg.Totals.GrandTotal.Amount;

            //Create Ability to add multiple Gift Cards
            MultipleGiftCardPaymentComponent mpComponent = arg.Components.OfType<MultipleGiftCardPaymentComponent>().FirstOrDefault<MultipleGiftCardPaymentComponent>();
            if (mpComponent == null) mpComponent = new MultipleGiftCardPaymentComponent();

            //Check for Adding Vs Removing GiftCard
            if (giftCardPaymentAmount.Amount > 0)
            {
                bool cardAddedSuccess = mpComponent.AddGiftCard(giftCardCode, giftCardPaymentComponent.Amount, giftCardPaymentComponent.Amount);
            }
            else
            {
                bool cardAddedSuccess = mpComponent.RemoveGiftCard(giftCardCode);
            }

            //Remove Existing CartPaymentArgument
            context.CommerceContext.Objects.Remove(context.CommerceContext.Objects.OfType<CartPaymentsArgument>().FirstOrDefault());
            argument.Payments = null;

            //If Valid Balance exists, Add Gift Card Payment / Else Remove 
            GiftCardPaymentComponent payment = GenerateDynamicGiftCardPaymentComponent(cartTotal, mpComponent, giftCardPaymentComponent);
            if (payment != null)
            {
                List<PaymentComponent> pcList = new List<PaymentComponent>();
                pcList.Add(payment);
                argument.Payments = pcList.AsEnumerable();
                arg.SetComponent((Component)payment);
            }
            else
            {
                arg.Components.Remove(arg.Components.OfType<GiftCardPaymentComponent>().FirstOrDefault());
            }

            context.CommerceContext.Objects.Add(argument);
            arg.SetComponent((Component)mpComponent);
            context.CommerceContext.Objects.Add(mpComponent);

            return arg;
        }

        public GiftCardPaymentComponent GenerateDynamicGiftCardPaymentComponent(decimal cartTotal, MultipleGiftCardPaymentComponent mpComponent, GiftCardPaymentComponent giftCardPaymentComponent)
        {
            //Create Dynamic Gift Card from all submitted Gift Cards

            GiftCard card = new GiftCard()
            {
                Balance = giftCardPaymentComponent.Amount,
                GiftCardCode = giftCardPaymentComponent.GiftCardCode,
                OriginalAmount = giftCardPaymentComponent.Amount
            };

            decimal amountFromGiftCard = 0M;
            decimal giftCardAvailableBalance = mpComponent.GetAllGiftCardTotalsBalance();

            if (giftCardAvailableBalance <= 0) return null;

            //Determine How Much of the card balance to use (other communication to F21 gift card service is handled by front-end code)
            if (giftCardAvailableBalance <= cartTotal) amountFromGiftCard = giftCardAvailableBalance;
            if (giftCardAvailableBalance > cartTotal) amountFromGiftCard = cartTotal;

            GiftCardPaymentComponent payment = new GiftCardPaymentComponent()
            {
                Name = "F21 GiftCard",
                Balance = new Money(card.Balance.CurrencyCode, amountFromGiftCard),
                Amount = new Money(card.Balance.CurrencyCode, amountFromGiftCard),
                GiftCard = card.AsReference(),
                GiftCardCode = mpComponent.GetDynamicGiftCardCode()
            };


            return payment;
        }
    }
}

