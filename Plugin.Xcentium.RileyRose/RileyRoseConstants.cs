// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RileyRoseConstants.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Xcentium.RileyRose
{
    /// <summary>
    /// The RileyRose constants.
    /// </summary>
    public static class RileyRoseConstants
    {
        /// <summary>
        /// 
        /// </summary>
        public struct Field
        {
            /// <summary>
            /// 
            /// </summary>
            public const string BasePrice = "Sale Price";

            /// <summary>
            /// 
            /// </summary>
            public const string ListPrice = "ListPrice";
        }

        /// <summary>
        /// The name of the RileyRose pipelines.
        /// </summary>
        public static class Pipelines
        {
            /// <summary>
            /// The name of the RileyRose pipeline blocks.
            /// </summary>
            public static class Blocks
            {
                /// <summary>
                /// The bootstrap aw sellable items block name.
                /// </summary>
                public const string BootstrapAwSellableItemsBlock = "RileyRose.block.BootstrapAwSellableItems";

                /// <summary>
                /// The initialize environment gift cards block name.
                /// </summary>
                public const string InitializeEnvironmentGiftCardsBlock = "RileyRose.block.InitializeEnvironmentGiftCards";

                /// <summary>
                /// The add cart line gift card block nME.
                /// </summary>
                public const string AddCartLineGiftCardBlock = "RileyRose.block.AddCartLineGiftCard";

                /// <summary>
                /// The initialize environment ensure catalog block name.
                /// </summary>
                public const string InitializeEnvironmentEnsureCatalogBlock = "RileyRose.block.InitializeEnvironmentEnsureCatalog";
            }


        }
    }
}
