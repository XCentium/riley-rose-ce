using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Tax;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Plugin.Xcentium.CartProperties
{
    public class ConfigureSitecore : IConfigureSitecore
    {
                public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            Action<SitecorePipelinesConfigBuilder> actionDelegate = c => c
                .ConfigurePipeline<IConfigureServiceApiPipeline>(
                    d =>
                    {
                        d.Add<CartProperties.Pipelines.Blocks.ConfigureServiceApiBlock>();
                    });

              services.Sitecore().Pipelines(actionDelegate);
            // Register commands too.
            services.RegisterAllCommands(assembly);
        }
    }
}
