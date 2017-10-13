using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Commerce.Plugin.Carts;

namespace Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks
{
    //public class WriteCartTotalsToContextBlockEx
    [PipelineDisplayName("Carts.WriteCartTotalsToContextBlockEx")]
    public class WriteCartTotalsToContextBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        public WriteCartTotalsToContextBlock()
          : base((string)null)
        {
        }

        public override Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires<Cart>(arg).IsNotNull<Cart>("The argument can not be null");
            Condition.Requires<Cart>(arg).IsNotNull<Cart>("The cart can not be null");

            //F21RileyRoseSetCartFulfillmentBlock Task< Cart > Run(CartFulfillmentArgument arg,  context);

            context.CommerceContext.Models.Add((Model)arg.Totals);
            return Task.FromResult<Cart>(arg);
        }
    }
  }
