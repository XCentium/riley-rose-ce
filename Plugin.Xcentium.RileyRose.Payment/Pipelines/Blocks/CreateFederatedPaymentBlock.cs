using System;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Xcentium.RileyRose.Payment.Pipelines.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateFederatedPaymentBlock : PipelineBlock<CartEmailArgument, CartEmailArgument, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// A cart with federate payment component
        /// </returns>
        public override async Task<CartEmailArgument> Run(CartEmailArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The cart can not be null");

            var cart = arg.Cart;
            if (!cart.HasComponent<FederatedPaymentComponent>())
            {
                return arg;
            }

            var payment = cart.GetComponent<FederatedPaymentComponent>();

            if (string.IsNullOrEmpty(payment.PaymentMethodNonce))
            {
                context.Abort(
                    await context.CommerceContext.AddMessage(
                    context.GetPolicy<KnownResultCodes>().Error,
                    "InvalidOrMissingPropertyValue",
                    new object[] { "PaymentMethodNonce" },
                    $"Invalid or missing value for property 'PaymentMethodNonce'."), context);

                return arg;
            }


            try
            {
                payment.TransactionId = GenerateRandomNo().ToString();
                payment.TransactionStatus = Constants.FederatedPayment.Success;

                payment.MaskedNumber = Constants.FederatedPayment.MaskedNumber;
                payment.ExpiresMonth = int.Parse(DateTime.Today.Month.ToString());
                payment.ExpiresYear = int.Parse(DateTime.Today.Year.ToString());
                payment.CardType = Constants.FederatedPayment.CardType;
                return arg;
            }
            catch (Exception ex)
            {
                context.Abort(

                 await context.CommerceContext.AddMessage(
                    context.GetPolicy<KnownResultCodes>().Error,
                    "InvalidClientPolicy",
                    new object[] { "PaypalPayment" },
                    $"{this.Name}. Invalid PaypalPayment { ex.Message }"), context);
                return arg;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GenerateRandomNo()
        {
            const int min = 1001;
            const int max = 999999999;
            var rdm = new Random();
            return rdm.Next(min, max);
        }
    }
}
