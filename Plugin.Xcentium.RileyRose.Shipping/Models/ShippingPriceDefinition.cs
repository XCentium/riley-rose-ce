﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Shipping.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ShippingPriceDefinition
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
        public decimal MinAmount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal MaxAmount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal Cost { get; set; } 
    }
}
