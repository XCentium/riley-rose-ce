using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping.Models
{ 
    /// <summary>
    /// 
    /// </summary>
    public class ShippingOption
    { 
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ShortDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LongDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool AllowHazmat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ShippingPriceDefinition> ShippingDefinitions = new List<ShippingPriceDefinition>();
    } 
}
