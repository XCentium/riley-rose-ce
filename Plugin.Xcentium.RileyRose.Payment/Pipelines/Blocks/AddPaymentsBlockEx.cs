

// Plugin.Xcentium.RileyRose.Payment.Pipelines.Blocks
// Decompiled with JetBrains decompiler
// Type: Sitecore.Commerce.Plugin.Payments.AddPaymentsBlock
// Assembly: Sitecore.Commerce.Plugin.Payments, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 91E4FF89-F629-46E7-83B8-216103263460
// Assembly location: C:\inetpub\CommerceAuthoring\Sitecore.Commerce.Plugin.Payments.dll

using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Plugin.Payments;

namespace Plugin.Xcentium.RileyRose.Payment.Pipelines.Blocks
{
    [PipelineDisplayName("Plugin.Xcentium.RileyRose.Payment.Pipelines.Blocks.addpayments")]
    public class AddPaymentsBlockEx : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        public AddPaymentsBlockEx()
            : base((string)null)
        {
        }

        public override async Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires<Cart>(arg).IsNotNull<Cart>(string.Format("{0}: The argument cannot be null.", (object)this.Name));
            CartPaymentsArgument argument;

            if (context.CommerceContext.Objects.OfType<CartPaymentsArgument>() == null) return arg;

            if (context.CommerceContext.Objects.OfType<CartPaymentsArgument>().FirstOrDefault().Payments != null)
            {
                argument = context.CommerceContext.Objects.OfType<CartPaymentsArgument>()
                    .FirstOrDefault<CartPaymentsArgument>();
            }
            else
            {
                return arg;
            }
             

            CommercePipelineExecutionContext executionContext;
            if (argument == null)
            {
                executionContext = context;
                string reason = await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "ArgumentNotFound", new object[1] { (object)typeof(CartPaymentsArgument).Name }, string.Format("Argument of type {0} was not found in context.", (object)typeof(CartPaymentsArgument).Name));
                executionContext.Abort(reason, (object)context);
                executionContext = (CommercePipelineExecutionContext)null;
                return (Cart)null;
            }
            Cart cart = argument.Cart;

            //Loop Through All Payments
            foreach (PaymentComponent payment in argument.Payments)
            {
                PaymentComponent p = payment;
                if (p != null)
                {
                    if (string.IsNullOrEmpty(p.Id))
                        p.Id = Guid.NewGuid().ToString("N");
                    context.Logger.LogInformation(string.Format("{0} - Adding Payment {1} Amount:{2}", (object)this.Name, (object)p.Id, (object)p.Amount.Amount), Array.Empty<object>());

                    //Check for valid currency type
                    if (string.IsNullOrEmpty(p.Amount.CurrencyCode))
                        p.Amount.CurrencyCode = context.CommerceContext.CurrentCurrency();
                    else if (!p.Amount.CurrencyCode.Equals(context.CommerceContext.CurrentCurrency(), StringComparison.OrdinalIgnoreCase))
                    {
                        executionContext = context;
                        string reason = await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "InvalidCurrency", new object[2] { (object)p.Amount.CurrencyCode, (object)context.CommerceContext.CurrentCurrency() }, string.Format("Invalid currency '{0}'. Valid currency is '{1}'.", (object)p.Amount.CurrencyCode, (object)context.CommerceContext.CurrentCurrency()));
                        executionContext.Abort(reason, (object)context);
                        executionContext = (CommercePipelineExecutionContext)null;
                        return (Cart)null;
                    }
                    if (context.GetPolicy<GlobalPricingPolicy>().ShouldRoundPriceCalc)
                    {
                        p.Amount.Amount = Decimal.Round(p.Amount.Amount, context.GetPolicy<GlobalPricingPolicy>().RoundDigits, context.GetPolicy<GlobalPricingPolicy>().MidPointRoundUp ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven);
                        context.Logger.LogDebug(string.Format("{0} - After Rounding: {1}", (object)this.Name, (object)p.Amount.Amount), Array.Empty<object>());
                    }
                    cart.SetComponent((Component)p);
                    p = (PaymentComponent)null;
                }
            }
            return cart;
        }
    }
}
