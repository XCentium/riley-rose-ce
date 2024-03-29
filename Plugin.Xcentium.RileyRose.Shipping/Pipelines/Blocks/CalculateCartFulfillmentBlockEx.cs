﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Xcentium.RileyRose.Shipping.Models;
using Plugin.Xcentium.RileyRose.Shipping.Util;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Management;

namespace Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    public class CalculateCartFulfillmentBlockEx : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {

        /// <summary>
        /// 
        /// </summary>

        private readonly IGetSellableItemPipeline _getSellableItemPipeline;

        /// <summary>
        /// 
        /// </summary>
        private readonly IGetItemByPathPipeline _getItemByPathPipeline;

        /// <summary>
        /// 
        /// </summary>
        private readonly IGetItemChildrenPipeline _getItemChildrenByPathPipeline;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getItemByPathPipeline"></param>
        /// <param name="getItemChildrenByPathPipeline"></param>
        /// <param name="getSellableItemPipeline"></param>
        public CalculateCartFulfillmentBlockEx(IGetItemByPathPipeline getItemByPathPipeline, IGetItemChildrenPipeline getItemChildrenByPathPipeline,
            IGetSellableItemPipeline getSellableItemPipeline) : base(null)
        {
            _getItemByPathPipeline = getItemByPathPipeline;
            _getSellableItemPipeline = getSellableItemPipeline;
            _getItemChildrenByPathPipeline = getItemChildrenByPathPipeline;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {

            var adjustments = arg.Adjustments;

            if (adjustments == null || !adjustments.Any()) return await Task.FromResult(arg);


            //Get Shipping Options from Sitecore
            var shippingOptions =
                await ShippingOptionsModel.GetData(_getItemByPathPipeline, _getItemChildrenByPathPipeline, context);

            //Calculate Custom Shipping Charges
            var shippingAwardedAdjustment = ShippingCalculator.GetShippingAdjustment(arg, shippingOptions, context);


            arg.Adjustments[0].Adjustment = shippingAwardedAdjustment.Adjustment;


            return await Task.FromResult(arg);
        }
    }
}
