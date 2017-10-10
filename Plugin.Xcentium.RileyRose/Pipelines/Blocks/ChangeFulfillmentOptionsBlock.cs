using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Framework.Pipelines;

namespace Plugin.Xcentium.RileyRose.Pipelines.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    public class ChangeFulfillmentOptionsBlock :
        PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly IFindEntityPipeline _findEntityPipeline;
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="persistEntityPipeline"></param>
        /// <param name="findEntityPipeline"></param>
        public ChangeFulfillmentOptionsBlock(IPersistEntityPipeline persistEntityPipeline,
            IFindEntityPipeline findEntityPipeline)
        {
            _persistEntityPipeline = persistEntityPipeline;
            _findEntityPipeline = findEntityPipeline;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            // a new fulfilment policy based on result from web service.
            var fulfillmentPolicy = new GlobalPhysicalFulfillmentPolicy
            {
                PolicyId = typeof(GlobalPhysicalFulfillmentPolicy).Name,
                MaxShippingWeight = 50,
                MeasurementUnits = "Inches",
                WeightUnits = "Lbs",
                DefaultCartFulfillmentFee = new Money("USD", 14M),
                DefaultCartFulfillmentFees =
                    new List<Money> { new Money("USD", 24M), new Money("CAD", 24M) },
                DefaultItemFulfillmentFee = new Money("USD", 34M),
                DefaultItemFulfillmentFees =
                    new List<Money> { new Money("USD", 44M), new Money("CAD", 44M) },
                FulfillmentFees =
                    new List<FulfillmentFee>
                    {
                        new FulfillmentFee
                        {
                            Name = "Express",
                            Fee = new Money("USD", 11)
                        },
                        new FulfillmentFee
                        {
                            Name = "Next Day",
                            Fee = new Money("USD", 12)
                        },
                        new FulfillmentFee
                        {
                            Name = "Standard Ground",
                            Fee = new Money("USD", 13)
                        }
                    }
            };


            var existingEnvironment =
                _findEntityPipeline.Run(
                    new FindEntityArgument(typeof(CommerceEnvironment), "Entity-CommerceEnvironment-RileyRoseAuthoring"),
                    context).Result as CommerceEnvironment;
            if (existingEnvironment != null)
                try
                {


                    existingEnvironment.Policies.Remove(existingEnvironment.GetPolicy<GlobalPhysicalFulfillmentPolicy>());

                    existingEnvironment.SetPolicy(fulfillmentPolicy);

                    //var pos = 0;
                    //foreach (var environmentPolicy in existingEnvironment.Policies)
                    //{
                    //    if (environmentPolicy.PolicyId == typeof(GlobalPhysicalFulfillmentPolicy).Name)
                    //        break;
                    //    pos++;
                    //}

                    //if (pos > 0)
                    //{
                    //    existingEnvironment.Policies.RemoveAt(pos);
                    //    existingEnvironment.Policies.Add(fulfillmentPolicy);
                    //    existingEnvironment.IsPersisted = true;
                    //}
                }
                catch (Exception ex)
                {
                    context.Logger.LogError(string.Format("Exception in ChangeFulfillmentOptionsBlock:{0}", ex.Message));
                }

            try
            {
                _persistEntityPipeline.Run(new PersistEntityArgument(existingEnvironment), context).Wait();
            }
            catch (Exception ex)
            {
                context.Logger.LogError(string.Format("Exception in ChangeFulfillmentOptionsBlock - {0}", ex));

            }

            return Task.FromResult(arg);
        }


    }
}