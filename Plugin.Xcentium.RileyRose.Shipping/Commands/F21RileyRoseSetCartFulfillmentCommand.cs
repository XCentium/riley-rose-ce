using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping.Commands
{
    using System;
    using System.Threading.Tasks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Fulfillment;

    /// <summary>
    /// Defines the SampleCommand command.
    /// </summary>
    public class F21RileyRoseSetCartFulfillmentCommand : CommerceCommand
    {
        private readonly GetSellableItemCommand _getSellableItemCommand;
        private readonly GetFulfillmentMethodsCommand _getFulfillmentMethodsCommand;
       // private bool containsHazardousItems = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="F21RileyRoseSetCartFulfillmentCommand"/> class.
        /// </summary>
        /// <param name="pipeline">
        /// The pipeline.
        /// </param>
        /// <param name="serviceProvider">The service provider</param>

        public F21RileyRoseSetCartFulfillmentCommand(GetSellableItemCommand getSellableItemCommand, GetFulfillmentMethodsCommand getFulfillmentMethodsCommand)
             
        {
            this._getSellableItemCommand = getSellableItemCommand;
            this._getFulfillmentMethodsCommand = getFulfillmentMethodsCommand;
        }

        /// <summary>
        /// The process of the command
        /// </summary>
        /// <param name="commerceContext">
        /// The commerce context
        /// </param>
        /// <param name="parameter">
        /// The parameter for the command
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<bool> Process(CommerceContext commerceContext, object parameter)
        {
            /*
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var arg = new SampleArgument(parameter);
                var result = await this._pipeline.Run(arg, new CommercePipelineExecutionContextOptions(commerceContext));

                return result;
            }
            */
            return await Task.FromResult<bool>(true);
        }
    }
}
