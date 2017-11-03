/*
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Availability;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
*/

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Availability;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks
{
    // public class FilterCartFulfillmentOptionsBlockEx

        /// <summary>
        /// 
        /// </summary>
    [PipelineDisplayName("Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks.FilterCartFulfillmentOptionsBlockEx")]
    public class FilterCartFulfillmentOptionsBlockEx : PipelineBlock<CartArgument, IEnumerable<FulfillmentOption>, CommercePipelineExecutionContext>
    {
        private readonly IGetFulfillmentOptionsPipeline _getOptions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getOptionsPipeline"></param>
        public FilterCartFulfillmentOptionsBlockEx(IGetFulfillmentOptionsPipeline getOptionsPipeline)
          : base((string)null)
        {
            this._getOptions = getOptionsPipeline;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<IEnumerable<FulfillmentOption>> Run(CartArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires<CartArgument>(arg).IsNotNull<CartArgument>("The arg can not be null");
            Condition.Requires<Cart>(arg.Cart).IsNotNull<Cart>("The cart can not be null");
            Cart cart = arg.Cart;
            if (!cart.Lines.Any<CartLineComponent>())
            {
                CommercePipelineExecutionContext executionContext = context;
                string reason = await context.CommerceContext.AddMessage(context.CommerceContext.GetPolicy<KnownResultCodes>().ValidationError, "CartHasNoLines", new object[1]
                {
          (object) cart.Id
                }, string.Format("Cart '{0}' has no lines", (object)cart.Id));
                executionContext.Abort(reason, (object)context);
                executionContext = (CommercePipelineExecutionContext)null;
                return new List<FulfillmentOption>().AsEnumerable<FulfillmentOption>();
            }
            List<FulfillmentOption> list = (await this._getOptions.Run(string.Empty, context)).ToList<FulfillmentOption>();
            if (list.Any<FulfillmentOption>() && cart.Lines.Count == 1)
            {
                FulfillmentOption fulfillmentOption = list.FirstOrDefault<FulfillmentOption>((Func<FulfillmentOption, bool>)(o => o.FulfillmentType.Equals("SplitShipping", StringComparison.OrdinalIgnoreCase)));
                if (fulfillmentOption != null)
                    list.Remove(fulfillmentOption);
            }
            foreach (Component line in (IEnumerable<CartLineComponent>)arg.Cart.Lines)
            {
                if (line.GetComponent<CartProductComponent>().HasPolicy<AvailabilityAlwaysPolicy>())
                {
                    List<FulfillmentOption> source = list;
                    Func<FulfillmentOption, bool> func = (Func<FulfillmentOption, bool>)(p => p.FulfillmentType.Equals("ShipToMe", StringComparison.OrdinalIgnoreCase));
                   // Func<FulfillmentOption, bool> predicate;
                   // if (source.Any<FulfillmentOption>(predicate))
                    //    list.Remove(list.First<FulfillmentOption>((Func<FulfillmentOption, bool>)(p => p.FulfillmentType.Equals("ShipToMe", StringComparison.OrdinalIgnoreCase))));
                }
                else
                {
                    List<FulfillmentOption> source = list;
                    Func<FulfillmentOption, bool> func = (Func<FulfillmentOption, bool>)(p => p.FulfillmentType.Equals("Digital", StringComparison.OrdinalIgnoreCase));
                    //Func<FulfillmentOption, bool> predicate;
                   // if (source.Any<FulfillmentOption>(predicate))
                       // list.Remove(list.First<FulfillmentOption>((Func<FulfillmentOption, bool>)(p => p.FulfillmentType.Equals("Digital", StringComparison.OrdinalIgnoreCase))));
                }
            }
            return list.AsEnumerable<FulfillmentOption>();
        }
    }
}
