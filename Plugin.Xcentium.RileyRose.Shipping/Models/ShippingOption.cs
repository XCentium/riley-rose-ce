using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping.Models
{ 
    public class ShippingOption
    { 
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public bool AllowHazmat { get; set; }

        public List<ShippingPriceDefinition> ShippingDefinitions = new List<ShippingPriceDefinition>();
    } 
}
