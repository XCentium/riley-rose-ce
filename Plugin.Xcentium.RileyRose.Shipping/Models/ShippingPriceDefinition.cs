using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping.Models
{
    public class ShippingPriceDefinition
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal Cost { get; set; } 
    }
}
