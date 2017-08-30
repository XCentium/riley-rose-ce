using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Xcentium.RileyRose.Payment.Pipelines.Blocks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Plugin.Xcentium.RileyRose.Payment
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfigureSitecore
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config
                .ConfigurePipeline<ICreateOrderPipeline>(d =>
                {
                    d.Add<CreateFederatedPaymentBlock>().Before<CreateOrderBlock>();
                })
            );
            services.RegisterAllCommands(assembly);

        }

    }
}
