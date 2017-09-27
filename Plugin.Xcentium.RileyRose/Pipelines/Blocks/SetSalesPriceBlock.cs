using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Pipelines;
using CommerceServer.Core.Catalog;

namespace Plugin.Xcentium.RileyRose.Pipelines.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    public class SetSalesPriceBlock : PipelineBlock<SellableItem, SellableItem, CommercePipelineExecutionContext>
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<SellableItem> Run(SellableItem arg, CommercePipelineExecutionContext context)
        {

            if (arg == null)
                return Task.FromResult<SellableItem>((SellableItem)null);

            var product = context.CommerceContext.Objects.OfType<Product>().FirstOrDefault<Product>((Func<Product, bool>)(p => p.ProductId.Equals(arg.FriendlyId, StringComparison.OrdinalIgnoreCase)));
            if (product == null)
                return Task.FromResult<SellableItem>(arg);

            if (arg.HasComponent<PriceSnapshotComponent>())
                arg.Components.Remove((Component)arg.GetComponent<PriceSnapshotComponent>());

            if (!product.HasProperty(RileyRoseConstants.Field.BasePrice) ||
                product[RileyRoseConstants.Field.BasePrice] == null)
            {
                return Task.FromResult<SellableItem>(arg);
            }

            var basePriceStr = product[RileyRoseConstants.Field.BasePrice].ToString();
            if (basePriceStr == string.Empty) return Task.FromResult<SellableItem>(arg);
            var listPrice = product.ListPrice;

            var optionMoneyPolicy = new PurchaseOptionMoneyPolicy();

            optionMoneyPolicy.SellPrice = arg.GetPolicy<PurchaseOptionMoneyPolicy>().SellPrice;

            arg.Policies.Remove((Policy)arg.GetPolicy<PurchaseOptionMoneyPolicy>());

            var newOptionMoneyPolicy = new PurchaseOptionMoneyPolicy();
            var currentCurrency = context.CommerceContext.CurrentCurrency();
            var itemSalePrice = GetFirstDecimalFromString(basePriceStr);
            if (itemSalePrice <= 0) return Task.FromResult<SellableItem>(arg);

            if (listPrice < itemSalePrice) return Task.FromResult<SellableItem>(arg);


            newOptionMoneyPolicy.SellPrice = new Money(currentCurrency, itemSalePrice);
            arg.SetPolicy((Policy)newOptionMoneyPolicy);
   
            return Task.FromResult<SellableItem>(arg);
        }

        private decimal GetFirstDecimalFromString(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0.00M;
            var decList = Regex.Split(str, @"[^0-9\.]+").Where(c => c != "." && c.Trim() != "").ToList();
            var decimalVal = decList.Any() ? decList.FirstOrDefault() : string.Empty;

            if (string.IsNullOrEmpty(decimalVal)) return 0.00M;
            decimal decimalResult = 0;
            decimal.TryParse(decimalVal, out decimalResult);
            return decimalResult;
        }
    }
}
