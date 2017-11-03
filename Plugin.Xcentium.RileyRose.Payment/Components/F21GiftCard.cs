using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;

namespace Plugin.Xcentium.RileyRose.Payment.Components
{
    public class F21GiftCard 
    {
        /// <summary>
        /// 
        /// </summary>
        public string CardNumber { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public string CouponCode { get; set; }



        /// <summary>
        /// 
        /// </summary>
        public string PinNumber { get; set; }



        /// <summary>
        /// 
        /// </summary>
        public Money OriginalBalance { get; set; }



        /// <summary>
        /// 
        /// </summary>
        public Money PaymentAmount { get; set; }
    }
}
