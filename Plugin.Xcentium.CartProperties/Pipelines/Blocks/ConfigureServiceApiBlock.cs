using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Builder;
using Plugin.Xcentium.CartProperties.Models;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Xcentium.CartProperties.Pipelines.Blocks
{
    public class ConfigureServiceApiBlock : PipelineBlock<ODataConventionModelBuilder, ODataConventionModelBuilder, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        public ConfigureServiceApiBlock(IPersistEntityPipeline persistEntityPipeline)
        {
            _persistEntityPipeline = persistEntityPipeline;
        }

        public override Task<ODataConventionModelBuilder> Run(ODataConventionModelBuilder modelBuilder, CommercePipelineExecutionContext context)
        {
            Condition.Requires(modelBuilder).IsNotNull($"{base.Name}: The argument can not be null");

            var configuration = modelBuilder.Action("SetCartProperties");
            configuration.Parameter<string>("cartId");
            configuration.Parameter<Properties>("properties");
            configuration.ReturnsFromEntitySet<CommerceCommand>("Commands");
            return Task.FromResult(modelBuilder);
        }
    }
}
