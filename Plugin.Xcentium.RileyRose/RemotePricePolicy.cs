
namespace Plugin.Xcentium.RileyRose
{
    using Sitecore.Commerce.Core;

    /// <summary>
    /// 
    /// </summary>
    public class RemotePricePolicy : Policy
    {
        /// <summary>
        /// 
        /// </summary>
        public RemotePricePolicy()
        {
            this.DefaultPostagePrice = string.Empty;
            this.PostagePriceRequestUrl = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        public string PostagePriceRequestUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DefaultPostagePrice { get; set; }

    }
}
