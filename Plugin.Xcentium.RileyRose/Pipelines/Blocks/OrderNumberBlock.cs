using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;
using Microsoft.Extensions.Logging;

using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Conditions;


namespace Plugin.Xcentium.RileyRose.Pipelines.Blocks
{
    public class OrderNumberBlock : PipelineBlock<Order, Order, CommercePipelineExecutionContext>
    {
        public override async Task<Order> Run(Order order, CommercePipelineExecutionContext context)
        {
            Condition.Requires<Order>(order).IsNotNull<Order>("The order can not be null");
            string uniqueCode = Guid.NewGuid().ToString("B");
            await Task.Delay(10);
            try
            {
                // get all orders in the system
                // add to order start and assign

               // uniqueCode = "asdf";
            }
            catch (Exception ex)
            {
                context.Logger.LogError(string.Format("{0}-UniqueCodeException: UniqueCode={1}|Stack={2}", (object)this.Name, (object)ex.Message, (object)ex.StackTrace));
            }
            order.OrderConfirmationId = uniqueCode;
            return order;
        }
    }
}
