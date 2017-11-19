// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Plugin.Xcentium.RileyRose.Pipelines.Blocks;

namespace Plugin.Xcentium.RileyRose
{
    using System.Reflection;

    using Microsoft.Extensions.DependencyInjection;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    /// <summary>
    /// The RileyRose configure class.
    /// </summary>
    /// <seealso cref="Sitecore.Framework.Configuration.IConfigureSitecore" />
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
            services.RegisterAllCommands(assembly);

            services.Sitecore().Pipelines(
                config => config.ConfigurePipeline<IInitializeEnvironmentPipeline>(
                        d =>
                            {
                                d.Add<InitializeEnvironmentGiftCardsBlock>()
                                    .Add<InitializeEnvironmentSellableItemsBlock>()
                                    .Add<InitializeEnvironmentPricingBlock>()
                                    .Add<InitializeEnvironmentPromotionsBlock>();
                            })

                   .ConfigurePipeline<IBootstrapPipeline>(
                    d => 
                        {
                            d
                            //.Add<InitializeEnvironmentEnsureCatalogBlock>()
                            .Add<ChangeFulfillmentOptionsBlock>();
                        })

                    .ConfigurePipeline<IAddCartLinePipeline>(configure => configure.Add<AddCartLineGiftCardBlock>().Before<PersistCartBlock>())

                    .ConfigurePipeline<IUpdateCartLinePipeline>(configure => configure.Add<AddCartLineGiftCardBlock>().Before<ICalculateCartPipeline>())

                     //// Registers the route for our custom command
                     //.ConfigurePipeline<IConfigureServiceApiPipeline>(c => c.Add<CartProperties.Pipelines.Blocks.ConfigureServiceApiBlock>())

                    
                    
                    
                    );
					

            services.ConfigureCartPipelines();
            services.ConfigureOrdersPipelines();

            // Register commands too.
            //services.RegisterAllCommands(assembly);
        }
    }
}