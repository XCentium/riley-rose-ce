using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose
{
    /// <summary>
    /// 
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// 
        /// </summary>
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
            public const string ShippingChargePath = "/sitecore/Commerce/Commerce Control Panel/Shared Settings/Shipping Charge";


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

        public struct AppSettings
        {
            public const string RileyRoseInterfaceConnection = "AppSettings:RileyRoseInterface";
            public const string InitialOrderStatusCode = "AppSettings:InitialOrderStatusCode";
        }

        public struct StoredProcedures
        {
            public struct Names
            {
                public const string InsertOrderDetails = "sp_RileyRose_InsertInitialOrderDetails";
            }

            public struct Parameters
            {
                public const string OrderGroupId = "@orderGroupId";
                public const string CustomerId = "@customerId";
                public const string CustomerEmail = "@customerEmail";
                public const string OrderConfirmationId = "@orderConfirmationId";
                public const string TrackingNumber = "@trackingNumber";
                public const string OrderStatus = "@orderStatus";
                public const string PssTracking = "@pssTracking";
                public const string RegDate = "@regDate";
                public const string Posted = "@posted";
                public const string PostDate = "@postDate";
                public const string MsReplicationVersion = "@msrepl_tran_version";
                public const string ShipCDompany = "@shipCompany";
            }
        }
    }
}
