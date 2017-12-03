using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sitecore.Commerce.Core;

namespace Plugin.Xcentium.CartProperties.Components
{
    /// <summary>
    /// 
    /// </summary>
    public class GiftCardMessageComponent : Component
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Constants.Fields.RecipientEmail)]
        public string RecipientEmail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Constants.Fields.ConfirmRecipientEmail)]
        public string ConfirmRecipientEmail { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Constants.Fields.RecipientName)]
        public string RecipientName { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Constants.Fields.SenderEmail)]
        public string SenderEmail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Constants.Fields.SenderName)]
        public string SenderName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Constants.Fields.Message)]
        public string Message { get; set; }
    }
}
