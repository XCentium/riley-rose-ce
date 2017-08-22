using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;

namespace Plugin.Xcentium.RileyRose.Tax
{
    /// <summary>
    /// 
    /// </summary>
    public class VertexPolicy : Policy
    {

        /// <summary>
        /// 
        /// </summary>
        public VertexPolicy()
        {
            CompanyCode = "448120";
            AccountId = "2000196050";
            UserName = "wsapi";
            Password = "wsapi@";
            ClassCode = "2002";
            CustomerCode = "2002";
            InProductionMode = false;

            // ship from settings
            this.ShipFromAddressLine1 = "3880 N Mission Rd";
            this.ShipFromAddressLine2 = string.Empty;
            this.ShipFromAddressLine3 = "";
            this.ShipFromCity = "Los Angeles";
            this.ShipFromStateOrProvinceCode = "CA";
            this.ShipFromPostalCode = "90031";
            this.ShipFromCountryCode = "US";

        }

        /// <summary>
        /// 
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ClassCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CustomerCode { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public bool InProductionMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Password { get; set; }

        // ship from address
        /// <summary>
        /// 
        /// </summary>
        public string ShipFromName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ShipFromAddressLine1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ShipFromAddressLine2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ShipFromAddressLine3 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ShipFromCity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ShipFromStateOrProvinceCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ShipFromPostalCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ShipFromCountryCode { get; set; }



    }
}
