// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Plugin.Xcentium.RileyRose.Pipelines.Blocks;
using Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks;
using Plugin.Xcentium.RileyRose.Tax.Pipelines.Blocks;


namespace Plugin.Xcentium.RileyRose
{
    using Microsoft.Extensions.DependencyInjection;

    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Catalog.Cs;
    using Sitecore.Commerce.Plugin.Coupons;
    using Sitecore.Commerce.Plugin.Fulfillment;
    using Sitecore.Commerce.Plugin.Inventory.Cs;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Commerce.Plugin.Payments;
    using Sitecore.Commerce.Plugin.Promotions;
    using Sitecore.Commerce.Plugin.Tax;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    /// <summary>
    /// The services extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// The configure cart pipelines.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        /// <returns>
        /// The <see cref="IServiceCollection"/>.
        /// </returns>
        public static IServiceCollection ConfigureCartPipelines(this IServiceCollection services)
        {
            services.Sitecore().Pipelines(config => config
                .ConfigurePipeline<ICalculateCartLinesPipeline>(builder => builder
                    .Add<PopulateCartLineItemsBlock>()
                    .Add<CalculateCartLinesPriceBlock>()
                    .Add<ValidateCartLinesPriceBlock>()
                    .Add<CalculateCartLinesSubTotalsBlock>()
                    .Add<CalculateCartLinesFulfillmentBlock>()
                    .Add<CalculateCartLinesFulfillmentBlockEx>().After<CalculateCartLinesFulfillmentBlock>()
                    .Add<ValidateCartCouponsBlock>()
                    .Add<CalculateCartLinesPromotionsBlock>()
                   // .Add<CalculateCartLinesTaxBlock>()
                     .Add<UpdateCalculateCartLinesTaxBlock>()
                    .Add<CalculateCartLinesTotalsBlock>())

               .ConfigurePipeline<ICalculateCartPipeline>(builder => builder
                    .Add<CalculateCartSubTotalsBlock>()
                    .Add<CalculateCartFulfillmentBlock>()
                    .Add<CalculateCartFulfillmentBlockEx>().After<CalculateCartFulfillmentBlock>()
                    .Add<CalculateCartPromotionsBlock>()
                   // .Add<CalculateCartTaxBlock>()
                     .Add<UpdateCalculateCartTaxBlock>()
                    .Add<CalculateCartTotalsBlock>()
                    .Add<CalculateCartPaymentsBlock>())

                .ConfigurePipeline<ICalculateSellableItemSellPricePipeline>(builder => builder
                        .Add<Plugin.Xcentium.RileyRose.Pipelines.Blocks.SetSalesPriceBlock>().After<CalculateSellableItemSellPriceBlock>()
                )

                .ConfigurePipeline<IOrderPlacedPipeline>(builder => builder
                    .Replace<OrderPlacedAssignConfirmationIdBlock, CustomizeOrderNumber>())


                .ConfigurePipeline<ICalculateVariationsSellPricePipeline>(builder => builder
                    .Add<Plugin.Xcentium.RileyRose.Pipelines.Blocks.SetVariantSalePriceBlock>().After<CalculateVariationsSellPriceBlock>())


              .ConfigurePipeline<IAddPaymentsPipeline>(builder => builder.Add<ValidateCartHasFulfillmentBlock>().After<ValidateCartAndPaymentsBlock>()));


            return services;
        }

        /// <summary>
        /// The configure orders pipelines.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        /// <returns>
        /// The <see cref="IServiceCollection"/>.
        /// </returns>
        public static IServiceCollection ConfigureOrdersPipelines(this IServiceCollection services)
        {
            services.Sitecore().Pipelines(config => config
                .ConfigurePipeline<IItemOrderedPipeline>(builder => builder
                    .Add<UpdateItemsOrderedInventoryBlock>()));

            return services;
        }
    }
}
