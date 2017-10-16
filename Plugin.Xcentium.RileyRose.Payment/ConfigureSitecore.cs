// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// <summary>
//   The SamplePlugin startup class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Sitecore.Commerce.Plugin.GiftCards;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Commerce.Plugin.Payments;

namespace Plugin.Xcentium.RileyRose.Payment
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.Fulfillment;
    using Pipelines.Blocks;  
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
                .ConfigurePipeline<ICreateOrderPipeline>(d =>
                {
                    d.Add<CreateFederatedPaymentBlock>().Before<CreateOrderBlock>();
                })
                .ConfigurePipeline<IAddPaymentsPipeline>(d =>
                {
                   d.Replace<AddPaymentsBlock, AddPaymentsBlockEx>();
                })
                .ConfigurePipeline<IAddPaymentsPipeline>(d =>
                {
                    d.Replace<ValidateGiftCardPaymentBlock, ValidateGiftCardPaymentBlockEx>();
                })

                .ConfigurePipeline<IAddPaymentsPipeline>(d =>
                {
                    d.Add<ValidateGiftCardPaymentBlock>().Before<CreateOrderBlock>();
                    
                })
                .ConfigurePipeline<ICalculateCartPipeline>(d =>
                {
                    //d.Add<ValidateGiftCardPaymentBlock>().Before<CalculateCartTotalsBlock>();

                })
             
            );

            services.RegisterAllCommands(assembly);
        }
    }
}