using Plugin.Xcentium.RileyRose.Tax;
using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Xcentium.RileyRose.Tax.Pipelines.Blocks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Tax;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Plugin.Xcentium.RileyRose.Tax
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>The configure services.</summary>
        /// <param name="services">The services.</param>

        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            Action<SitecorePipelinesConfigBuilder> actionDelegate = c => c
                .ConfigurePipeline<ICalculateCartLinesPipeline>(
                    d =>
                    {
                        d.Add<UpdateCalculateCartLinesTaxBlock>().After<CalculateCartLinesTaxBlock>();
                    })
                .ConfigurePipeline<ICalculateCartPipeline>(
                    d =>
                    {
                        d.Add<UpdateCalculateCartTaxBlock>().After<CalculateCartTaxBlock>();                       
                    }
               );

            services.Sitecore().Pipelines(actionDelegate);

        }
    }
}
