using CommerceServer.Core.Catalog;
using Plugin.Xcentium.RileyRose.Shipping.Models;
using Plugin.Xcentium.RileyRose.Shipping.Util;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks
{

    [PipelineDisplayName("SellableItemF21RileyRoseBlock")]
    public class F21RileyRoseSellableItemBlock : PipelineBlock<SellableItem, SellableItem, CommercePipelineExecutionContext>
    {
        private readonly IGetSellableItemPipeline _getSellableItemPipeline;
        private readonly IGetItemByPathPipeline _getItemByPathPipeline;
        private readonly IGetItemChildrenPipeline _getItemChildrenByPathPipeline;
 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getItemByPathPipeline"></param>
        /// <param name="getSellableItemPipeline"></param>
        public F21RileyRoseSellableItemBlock(IGetItemByPathPipeline getItemByPathPipeline, IGetItemChildrenPipeline getItemChildrenByPathPipeline,
            IGetSellableItemPipeline getSellableItemPipeline) : base(null)
        {
            _getItemByPathPipeline = getItemByPathPipeline;
            _getSellableItemPipeline = getSellableItemPipeline;
            _getItemChildrenByPathPipeline = getItemChildrenByPathPipeline;
        }

        public override async Task<SellableItem> Run(SellableItem sellableItem, CommercePipelineExecutionContext context)
        {
            try
            { 
                Condition.Requires(sellableItem).IsNotNull("The argument cannot be null");
                //var result = this._pipeline.Run(arg, context).Result;
                var product = context.CommerceContext.GetObjects<Product>().FirstOrDefault(p => p.ProductId.Equals(sellableItem.FriendlyId, StringComparison.OrdinalIgnoreCase));

                //new System.Linq.SystemCore_EnumerableDebugView(new System.Linq.SystemCore_EnumerableDebugView<CommerceServer.Core.Catalog.CatalogItemsDataSet.CatalogItem>(product.Information.CatalogItems).Items[0]._columns).Items[70]
                var testVal = product["IsHazardous"];

                if (product["IsHazardous"] == null)
                {
                    sellableItem.GetComponent<F21RileyRosePrdComponent>().IsHazardous = false;
                    //var test2 = sellableItem.GetComponent<F21RileyRosePrdComponent>().IsHazardous;
                }
                else
                {
                    sellableItem.GetComponent<F21RileyRosePrdComponent>().IsHazardous = (bool)product["IsHazardous"];
                }  
                 
                
                return await Task.FromResult<SellableItem>(sellableItem); 

            }
            catch(Exception ex)
            {
                var myMessage = ex.Message;
                return null;
            }

        } 
      
    }
}
