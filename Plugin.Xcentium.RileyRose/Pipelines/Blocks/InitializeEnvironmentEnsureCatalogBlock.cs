// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializeEnvironmentEnsureCatalogBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Xcentium.RileyRose
{
    using System.Threading.Tasks;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Catalog.Cs;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Ensure RileyRose catalog has been loaded.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{System.String, System.String,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(RileyRoseConstants.Pipelines.Blocks.InitializeEnvironmentEnsureCatalogBlock)]
    public class InitializeEnvironmentEnsureCatalogBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly EnsureCatalogCommand _ensureCatalogCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeEnvironmentEnsureCatalogBlock"/> class.
        /// </summary>
        /// <param name="ensureCatalogCommand">
        /// The EnsureCatalog Command.
        /// </param>
        public InitializeEnvironmentEnsureCatalogBlock(EnsureCatalogCommand ensureCatalogCommand)
        {
            this._ensureCatalogCommand = ensureCatalogCommand;
        }

        /// <summary>
        /// The run.
        /// </summary>
        /// <param name="arg">
        /// The argument.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override async Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            await this._ensureCatalogCommand.Process(context.CommerceContext, "RileyRose_Master", @"[wwwroot]\data\Catalogs\RileyRose_Master.xml", "RileyRose_Inventory", @"[wwwroot]\data\Catalogs\RileyRose_Inventory.xml");

            await this._ensureCatalogCommand.Process(context.CommerceContext, "RileyRose_NextCubeMarketplace", @"[wwwroot]\data\Catalogs\RileyRose_NextCubeMarketplace.xml", "RileyRose_Inventory", @"[wwwroot]\data\Catalogs\RileyRose_Inventory.xml");

            return arg;
        }
    }
}
