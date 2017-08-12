// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializeEnvironmentGiftCardsBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Habitat
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Availability;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Entitlements;
    using Sitecore.Commerce.Plugin.ManagedLists;
    using Sitecore.Commerce.Plugin.Pricing;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines a habitat GiftCard SellableItem during the environment initialization.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{System.String, System.String,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(HabitatConstants.Pipelines.Blocks.InitializeEnvironmentGiftCardsBlock)]
    public class InitializeEnvironmentGiftCardsBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;
        private readonly IDoesEntityExistPipeline doesEntityExistPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeEnvironmentGiftCardsBlock"/> class.
        /// </summary>
        /// <param name="persistEntityPipeline">The persist entity pipeline.</param>
        /// <param name="doesEntityExistPipeline">The does entity exist pipeline.</param>
        public InitializeEnvironmentGiftCardsBlock(IPersistEntityPipeline persistEntityPipeline, IDoesEntityExistPipeline doesEntityExistPipeline)
        {
            this._persistEntityPipeline = persistEntityPipeline;
            this.doesEntityExistPipeline = doesEntityExistPipeline;
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
            const string ArtifactSet = "Environment.Habitat.GiftCards-1.0";

            // Check if this environment has subscribed to this Artifact Set
            if (!context.GetPolicy<EnvironmentInitializationPolicy>().InitialArtifactSets.Contains(ArtifactSet))
            {
                return arg;
            }

            context.Logger.LogInformation($"{this.Name}.InitializingArtifactSet: ArtifactSet={ArtifactSet}");

            //// GIFT CARD TEMPLATE
            var itemId = $"{CommerceEntity.IdPrefix<SellableItem>()}GiftCardV2";
            var entityExist = await this.doesEntityExistPipeline.Run(new FindEntityArgument(typeof(SellableItem), itemId), context.CommerceContext.GetPipelineContextOptions());
            if (!entityExist)
            {
                var giftCardSellableItem = new SellableItem
                {
                    Id = $"{CommerceEntity.IdPrefix<SellableItem>()}GiftCardV2",
                    Name = "Default GiftCard V2",
                    Policies = new List<Policy>
                            {
                                new AvailabilityAlwaysPolicy()
                            },
                    Components = new List<Component>
                            {
                                new ListMembershipsComponent { Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() } }
                            }
                };

                await this._persistEntityPipeline.Run(new PersistEntityArgument(giftCardSellableItem), context);
            }

            //// GIFT CARD SAMPLE
            itemId = $"{CommerceEntity.IdPrefix<SellableItem>()}6042986";
            entityExist = await this.doesEntityExistPipeline.Run(new FindEntityArgument(typeof(SellableItem), itemId), context.CommerceContext.GetPipelineContextOptions());
            if (entityExist)
            {
                return arg;
            }

            var giftCard = new SellableItem
            {
                Id = itemId,
                Name = "Default GiftCard",
                Policies = new List<Policy>
                        {
                            new AvailabilityAlwaysPolicy()
                        },
                Components = new List<Component>
                        {
                            new ListMembershipsComponent { Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() } },
                            new ItemVariationsComponent
                                {
                                    ChildComponents = new List<Component>
                                                            {
                                                                new ItemVariationComponent
                                                                    {
                                                                        Id = "56042986",
                                                                        Name = "Gift Card",
                                                                        Policies = new List<Policy>
                                                                                        {
                                                                                            new AvailabilityAlwaysPolicy(),
                                                                                            new ListPricingPolicy(new List<Money> { new Money("USD", 25M), new Money("CAD", 26M) })
                                                                                        }
                                                                    },
                                                                new ItemVariationComponent
                                                                    {
                                                                        Id = "56042987",
                                                                        Name = "Gift Card",
                                                                        Policies = new List<Policy>
                                                                                        {
                                                                                            new AvailabilityAlwaysPolicy(),
                                                                                            new ListPricingPolicy(new List<Money> { new Money("USD", 50M), new Money("CAD", 51M) })
                                                                                        }
                                                                    },
                                                                new ItemVariationComponent
                                                                    {
                                                                        Id = "56042988",
                                                                        Name = "Gift Card",
                                                                        Policies = new List<Policy>
                                                                                        {
                                                                                            new AvailabilityAlwaysPolicy(),
                                                                                            new ListPricingPolicy(new List<Money> { new Money("USD", 100M), new Money("CAD", 101M) })
                                                                                        }
                                                                    }
                                                            }
                                }
                        }
            };

           await this._persistEntityPipeline.Run(new PersistEntityArgument(giftCard), context);

            return arg;
        }
    }
}
