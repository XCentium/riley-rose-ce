using Plugin.Xcentium.RileyRose.Shipping.Util;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping.Models
{

    /// <summary>
    /// 
    /// </summary>
    public static class ShippingOptionsModel
    {

        /// <summary>
        /// 
        /// </summary>
        public static List<ShippingOption> _shippingOptions = new List<ShippingOption>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getItemByPathPipeline"></param>
        /// <param name="getItemChildrenByPathPipeline"></param>
        /// <param name="commercePipelineExecutionContext"></param>
        /// <returns></returns>
        public static async Task<List<ShippingOption>> GetData(IGetItemByPathPipeline getItemByPathPipeline, IGetItemChildrenPipeline getItemChildrenByPathPipeline, CommercePipelineExecutionContext commercePipelineExecutionContext)
        {
            if(_shippingOptions.Any())
            {
                return _shippingOptions;
            } 

            var shippingOptionsFolder = await SitecoreUtil.GetSitecoreItemByPath(Constants.Shipping.ShippingChargePath, getItemByPathPipeline, commercePipelineExecutionContext);
            var shippingFolderId = shippingOptionsFolder["ItemID"].ToString();
            //var shippingOptions = await SitecoreUtil.GetSitecoreItemChildrenByItemId(Constants.Shipping.ShippingFolderItemId, _getItemChildrenByPathPipeline, context);
            var shippingOptions = await SitecoreUtil.GetSitecoreItemChildrenByItemId(shippingFolderId, getItemChildrenByPathPipeline, commercePipelineExecutionContext);

            //List<ShippingOption> _shippingOptions = new List<ShippingOption>(); 

            foreach (var opt in shippingOptions)
            {
                var newShippingOption = new ShippingOption()
                {
                    Name = opt["ItemName"].ToString(),
                    DisplayName = opt["DisplayName"].ToString(),
                    ShortDescription = opt["Short Description"].ToString(),
                    LongDescription = opt["Long Description"].ToString(),
                    AllowHazmat = opt["Allow Hazmat"].ToString() == "1" ? true : false
                };

                var shippingPriceDef = await SitecoreUtil.GetSitecoreItemChildrenByItemId(opt["ItemID"].ToString(), getItemChildrenByPathPipeline, commercePipelineExecutionContext);

                foreach (var priceDef in shippingPriceDef)
                {
                    newShippingOption.ShippingDefinitions.Add(new ShippingPriceDefinition()
                    {
                        Name = priceDef["ItemName"].ToString(),
                        DisplayName = priceDef["DisplayName"].ToString(),
                        MinAmount = ShippingCalculator.GetPriceValue(priceDef["Min Amount"].ToString() ),
                        MaxAmount = ShippingCalculator.GetPriceValue(priceDef["Max Amount"].ToString() ),
                        Cost = ShippingCalculator.GetPriceValue(priceDef["Cost"].ToString())
                    });
                }
                _shippingOptions.Add(newShippingOption);
            } 
            return _shippingOptions;
        }  
    }
}
