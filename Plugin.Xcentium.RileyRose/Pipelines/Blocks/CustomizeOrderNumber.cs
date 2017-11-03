using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Pipelines;

using Microsoft.Extensions.Logging;
using Plugin.Xcentium.RileyRose.Pipelines.Helper;
using Sitecore.Framework.Conditions;


namespace Plugin.Xcentium.RileyRose.Pipelines.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomizeOrderNumber : PipelineBlock<Order, Order, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="order"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<Order> Run(Order order, CommercePipelineExecutionContext context)
        {
            Condition.Requires<Order>(order).IsNotNull<Order>("The order can not be null");
            var uniqueCode = Guid.NewGuid().ToString("B");
            try
            {
                var count = OrderNumberSingleton.Instance.GetOrderCount();
                var code = 20000000 + count;
                uniqueCode = code.ToString();
            }
            catch (Exception ex)
            {
                context.Logger.LogError(string.Format("{0}-UniqueCodeException: UniqueCode={1}|Stack={2}", (object)this.Name, (object)ex.Message, (object)ex.StackTrace));
            }
            order.OrderConfirmationId = uniqueCode;

            return Task.FromResult<Order>(order);
        }
    }
}
