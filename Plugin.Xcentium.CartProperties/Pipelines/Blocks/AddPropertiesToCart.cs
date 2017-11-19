using System;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Xcentium.CartProperties.Pipelines.Blocks
{
    public class AddPropertiesToCart : PipelineBlock<CartEmailArgument, CartEmailArgument, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly IPersistEntityPipeline _persistEntityPersistEntityPipeline;

        /// <summary>
        /// 
        /// </summary>
        private readonly IFindEntityPipeline _findEntityPipeline;

        /// <summary>
        /// 
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="persistEntityPersistEntityPipeline"></param>
        /// <param name="findEntityPipeline"></param>
        /// <param name="serviceProvider"></param>
        public AddPropertiesToCart(IPersistEntityPipeline persistEntityPersistEntityPipeline,
            IFindEntityPipeline findEntityPipeline,
            IServiceProvider serviceProvider)
        {
            _persistEntityPersistEntityPipeline = persistEntityPersistEntityPipeline;
            _findEntityPipeline = findEntityPipeline;
            _serviceProvider = serviceProvider;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<CartEmailArgument> Run(CartEmailArgument arg, CommercePipelineExecutionContext context)
        {

            Condition.Requires<CartEmailArgument>(arg).IsNotNull<CartEmailArgument>(string.Format("{0}: arg can not be null", (object)this.Name));
            Condition.Requires<Cart>(arg.Cart).IsNotNull<Cart>(string.Format("{0}: The cart can not be null", (object)this.Name));
            Condition.Requires<string>(arg.Email).IsNotNullOrEmpty(string.Format("{0}: The customer email can not be null or empty", (object)this.Name));
            var cart = arg.Cart;

            if (cart.Lines.Any<CartLineComponent>())
            {

            }

            await Task.Delay(1);
            return arg;
        }
    }
}
