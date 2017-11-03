using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping.Util
{
    /// <summary>
    /// 
    /// </summary>
    public static class SitecoreUtil
    { 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemPath"></param>
        /// <param name="getItemByPathPipeline"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<ItemModel> GetSitecoreItemByPath(string itemPath, IGetItemByPathPipeline getItemByPathPipeline, CommercePipelineExecutionContext context)
        {
            var itemModelArgument =
            new ItemModelArgument(itemPath)
            {
                Language = Constants.Shipping.Lang
            };
            var sitecoreItem = await getItemByPathPipeline.Run(itemModelArgument, context);
            return sitecoreItem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="getItemChildrenByPathPipeline"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ItemModel>> GetSitecoreItemChildrenByItemId(string itemID, IGetItemChildrenPipeline getItemChildrenByPathPipeline, CommercePipelineExecutionContext context)
        {
            var itemModelArgument =
            new ItemModelArgument(itemID)
            {
                Language = Constants.Shipping.Lang
            };
            var sitecoreItem = await getItemChildrenByPathPipeline.Run(itemModelArgument, context);
            return sitecoreItem;
        }
    }
}


