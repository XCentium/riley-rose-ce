using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Commerce.Plugin.Fulfillment;

namespace Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks
{
    //public class ValidateCartFulfillmentBlockEx
    [PipelineDisplayName("Fulfillment.block.ValidateCartFulfillmentBlockEx")]
    public class ValidateCartFulfillmentBlockEx : PipelineBlock<CartFulfillmentArgument, CartFulfillmentArgument, CommercePipelineExecutionContext>
    {
        private readonly IGetCartFulfillmentOptionsPipeline _getCartOptionsPipeline;
        private readonly IGetFulfillmentMethodsPipeline _getMethodsPipeline;

        public ValidateCartFulfillmentBlockEx(IGetCartFulfillmentOptionsPipeline getCartFulfillmentOptionsPipeline, IGetFulfillmentMethodsPipeline getFulfillmentMethodsPipeline)
          : base((string)null)
        {
            this._getCartOptionsPipeline = getCartFulfillmentOptionsPipeline;
            this._getMethodsPipeline = getFulfillmentMethodsPipeline;
        }

        public override async Task<CartFulfillmentArgument> Run(CartFulfillmentArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires<CartFulfillmentArgument>(arg).IsNotNull<CartFulfillmentArgument>(string.Format("{0}: The arg cannot be null", (object)this.Name));
            Condition.Requires<Cart>(arg.Cart).IsNotNull<Cart>(string.Format("{0}: The cart cannot be null", (object)this.Name));
            Condition.Requires<FulfillmentComponent>(arg.Fulfillment).IsNotNull<FulfillmentComponent>(string.Format("{0}: The fulfillment cannot be null", (object)this.Name));
            Cart cart = arg.Cart;
            CommercePipelineExecutionContext executionContext;
            if (!cart.Lines.Any<CartLineComponent>())
            {
                executionContext = context;
                string reason = await context.CommerceContext.AddMessage(context.CommerceContext.GetPolicy<KnownResultCodes>().ValidationError, "CartHasNoLines", new object[1]
                {
          (object) cart.Id
                }, string.Format("Cart '{0}' has no lines", (object)cart.Id));
                executionContext.Abort(reason, (object)context);
                executionContext = (CommercePipelineExecutionContext)null;
                return (CartFulfillmentArgument)null;
            }
            FulfillmentComponent fulfillment = arg.Fulfillment;
            EntityReference fulfillmentMethod = fulfillment.FulfillmentMethod;
            if (string.IsNullOrEmpty(fulfillmentMethod != null ? fulfillmentMethod.EntityTarget : (string)null) || string.IsNullOrEmpty(fulfillment.FulfillmentMethod.Name))
            {
                executionContext = context;
                string reason = await context.CommerceContext.AddMessage(context.CommerceContext.GetPolicy<KnownResultCodes>().ValidationError, "InvalidOrMissingProperty", new object[1]
                {
          (object) "FulfillmentMethod"
                }, "Invalid or missing value for property 'FulfillmentMethod'.");
                executionContext.Abort(reason, (object)context);
                executionContext = (CommercePipelineExecutionContext)null;
                return (CartFulfillmentArgument)null;
            }
            EntityReference newMethod = fulfillment.FulfillmentMethod;
            FulfillmentMethod method = (await this._getMethodsPipeline.Run(string.Empty, context)).FirstOrDefault<FulfillmentMethod>((Func<FulfillmentMethod, bool>)(o =>
            {
                if (o.Id.Equals(newMethod.EntityTarget, StringComparison.OrdinalIgnoreCase))
                    return o.Name.Equals(newMethod.Name, StringComparison.OrdinalIgnoreCase);
                return false;
            }));
            if (method != null)
            {
                if ((await this._getCartOptionsPipeline.Run(new CartArgument(cart), context)).Any<FulfillmentOption>((Func<FulfillmentOption, bool>)(o => o.FulfillmentType.Equals(method.FulfillmentType, StringComparison.OrdinalIgnoreCase))))
                    return arg;
            }
            executionContext = context;
            string reason1 = await context.CommerceContext.AddMessage(context.CommerceContext.GetPolicy<KnownResultCodes>().ValidationError, "InvalidFulfillment", new object[2]
            {
        (object) fulfillment.FulfillmentMethod.EntityTarget,
        (object) arg.Cart.Id
            }, string.Format("Fulfillment '{0}' is not a permitted fulfillment for cart '{1}'.", (object)fulfillment.FulfillmentMethod.EntityTarget, (object)arg.Cart.Id));
            executionContext.Abort(reason1, (object)context);
            executionContext = (CommercePipelineExecutionContext)null;
            return (CartFulfillmentArgument)null;
        }
    }
}
