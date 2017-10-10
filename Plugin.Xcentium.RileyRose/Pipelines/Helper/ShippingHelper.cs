using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Services.Core.Model;

namespace Plugin.Xcentium.RileyRose.Pipelines.Helper
{
    /// <summary>
    /// 
    /// </summary>
    public static class ShippingHelper
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static decimal GetFirstDecimalFromString(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0.00M;
            var decList = Regex.Split(str, @"[^0-9\.]+").Where(c => c != "." && c.Trim() != "").ToList();
            var decimalVal = decList.Any() ? decList.FirstOrDefault() : string.Empty;

            if (string.IsNullOrEmpty(decimalVal)) return 0.00M;
            decimal decimalResult = 0;
            decimal.TryParse(decimalVal, out decimalResult);
            return decimalResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        public static PropertiesModel TranslateToProperties(ItemModel itemModel)
        {
            var propertiesModelInitial = new PropertiesModel();
            var str = string.Format(Constants.Shipping.SitecoreItem, itemModel[Constants.Shipping.ItemId] as string);
            propertiesModelInitial.Name = str;
            var propertiesModelTranslated = propertiesModelInitial;
            foreach (var keyValuePair in itemModel)
                if (keyValuePair.Value is string)
                    propertiesModelTranslated.SetPropertyValue(keyValuePair.Key, (string)keyValuePair.Value);
                else
                    propertiesModelTranslated.SetPropertyValue(keyValuePair.Key, keyValuePair.Value);
            return propertiesModelTranslated;
        }
    }
}
