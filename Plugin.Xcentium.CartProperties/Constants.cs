using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.CartProperties
{
    public class Constants
    {
        public struct Settings
        {
            public const string EndpointUrl = "{0}/api/Carts('{1}')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components($expand=ChildComponents)";

            public const string AppJson = "application/json";
            public const string ShopperId = "ShopperId";
            public const string CartRoles = "sitecore\\Pricer Manager|sitecore\\Promotioner Manager";
            public const string CartId = "cartId";
            public const string CartLineProperties = "cartLineProperties";
            public const string ShopName = "ShopName";
            public const string Language = "Language";
            public const string Environment = "Environment";
            public const string GeoLocation = "GeoLocation";
            public const string CustomerId = "CustomerId";
            public const string Currency = "Currency";
            public const string Roles = "Roles";
            public const string Giftcarddata = "giftcarddata";
            public const string GiftCardMessageComponent = "GiftCardMessageComponent";

        }

        public struct Fields
        {
            public const string RecipientEmail = "recipientEmail";
            public const string ConfirmRecipientEmail = "confirmRecipientEmail";
            public const string RecipientName = "recipientName";
            public const string SenderEmail = "senderEmail";
            public const string SenderName = "senderName";
            public const string Message = "message";
        }
    }
}
