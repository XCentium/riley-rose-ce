using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping
{
    public static class Constants
    {
        public struct Shipping
        {

            /// <summary>
            /// 
            /// </summary>
            public const string CartNullText = "The cart cannot be null.";

            /// <summary>
            /// 
            /// </summary>
            public const string FulfillmentFee = "FulfillmentFee";

            /// <summary>
            /// 
            /// </summary>  
            public const string ShippingChargePath = "/sitecore/Commerce/Commerce Control Panel/Shared Settings/RileyRose Shipping Options";
            public const string ShippingFolderItemId = "{0B05F214-3652-4D63-ABC8-40E59FE5330E}";
            public const string IsHazardous = "IsHazardous";

            /// <summary>
            /// 
            /// </summary>
            public const string Lang = "en-US";



            /// <summary>
            /// 
            /// </summary>
            public const string Weight = "Weight";

            /// <summary>
            /// 
            /// </summary>
            public const string ItemId = "ItemID";

            /// <summary>
            /// 
            /// </summary>
            public const string SitecoreItem = "SitecoreItem_{0}";

            /// <summary>
            /// 
            /// </summary>
            public const string AllOthers = "All Others";

            /// <summary>
            /// 
            /// </summary>
            public const string CartLineNullText = "The cart lines cannot be null.";
        }
    }
}
