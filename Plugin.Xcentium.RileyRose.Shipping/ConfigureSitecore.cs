﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// <summary>
//   The SamplePlugin startup class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Xcentium.RileyRose.Shipping
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.Fulfillment;
    using Pipelines.Blocks; 
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Catalog.Cs;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// The carts configure sitecore class.
    /// </summary>
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config
               /*
                .ConfigurePipeline<ICalculateCartPipeline>(d =>
               {
                   d.Replace<CalculateCartFulfillmentBlock, CalculateCartFulfillmentBlockEx>();
               })
               */
                .ConfigurePipeline<IGetSellableItemPipeline>(d =>
               {
                   d.Add<F21RileyRoseSellableItemBlock>().After<TranslateProductBlock>();
               })

                /*
                 .ConfigurePipeline<ISetCartFulfillmentPipeline>(d =>
                {
                    d.Add<F21RileyRoseSetCartFulfillmentBlock>().After<SetCartFulfillmentBlock>();
                })

                 .ConfigurePipeline<ICalculateCartPipeline>(d =>
                {
                    d.Add< CalculateCartShippingBlockEx>().After<CalculateCartFulfillmentBlock>();
                })


                */
                .ConfigurePipeline<ISetCartFulfillmentPipeline>(d =>
                {
                    d.Replace<SetCartFulfillmentBlock,F21RileyRoseSetCartFulfillmentBlock>() ;
                })

                 .ConfigurePipeline<IAddCartLinePipeline>(d =>
                 {
                     d.Add<CheckHazardousShippingBlock>().Before<PersistCartBlock>();
                 })  

                .ConfigurePipeline<ISetCartFulfillmentPipeline>(d =>
                {
                    d.Add<ValidateCartFulfillmentBlock>().After<ValidateCartFulfillmentBlockEx>();
                })

                .ConfigurePipeline<ICalculateCartPipeline>(d =>
                {
                    d.Replace<CalculateCartTotalsBlock, CalculateCartTotalsBlockEx>();
                })

                .ConfigurePipeline<ICalculateCartPipeline>(d =>
                {
                    d.Replace<CalculateCartFulfillmentBlock, CalculateCartShippingBlockEx>(); 
                })

                .ConfigurePipeline<IGetFulfillmentOptionsPipeline>(d =>
               {
                   d.Replace<FilterCartFulfillmentOptionsBlock,FilterCartFulfillmentOptionsBlockEx>();
               })

                .ConfigurePipeline<IGetFulfillmentOptionsPipeline>(d =>
               {
                   //d.Replace<BasePipelineBlockRunner, BasePipelineBlockRunnerEx>();
               })

               );

            services.RegisterAllCommands(assembly);
        }
    }
}