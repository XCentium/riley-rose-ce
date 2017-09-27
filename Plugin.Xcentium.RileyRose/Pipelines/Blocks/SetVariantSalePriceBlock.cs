using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommerceServer.Core.Catalog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Pipelines;

namespace Plugin.Xcentium.RileyRose.Pipelines.Blocks
{
    /// <summary>
    /// CalculateVariationsSellPriceBlock  and ICalculateVariationsSellPricePipeline 
    /// </summary>
    public class SetVariantSalePriceBlock : PipelineBlock<SellableItem, SellableItem, CommercePipelineExecutionContext>
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override  Task<SellableItem> Run(SellableItem arg, CommercePipelineExecutionContext context)
        {
            if (arg == null) return null;

            if (!arg.HasComponent<ItemVariationsComponent>()) return Task.FromResult<SellableItem>(arg);

            var product = context.CommerceContext.Objects.OfType<Product>().FirstOrDefault<Product>((Func<Product, bool>)(p => p.ProductId.Equals(arg.FriendlyId, StringComparison.OrdinalIgnoreCase)));
            if (product == null)
                return Task.FromResult<SellableItem>(arg);

            if (arg.HasComponent<PriceSnapshotComponent>())
                arg.Components.Remove((Component)arg.GetComponent<PriceSnapshotComponent>());

            var basePriceStr = arg.GetPolicy<PurchaseOptionMoneyPolicy>().SellPrice.Amount.ToString(CultureInfo.InvariantCulture);

            if (basePriceStr == string.Empty)
            {
                basePriceStr = product.ListPrice.ToString(CultureInfo.InvariantCulture);
            }
            

            var variantComponents = arg.GetComponent<ItemVariationsComponent>();

            if (variantComponents?.ChildComponents == null) return Task.FromResult<SellableItem>(arg);

            var variantComponentsChildren = variantComponents.ChildComponents.OfType<ItemVariationComponent>().ToList();

            if (!variantComponentsChildren.Any()) return Task.FromResult<SellableItem>(arg);

            var itemSalePrice = GetFirstDecimalFromString(basePriceStr);

            foreach (var variantComponentsChild in variantComponentsChildren)
            {
                if (variantComponentsChild.HasPolicy<PurchaseOptionMoneyPolicy>()) { variantComponentsChild.Policies.Remove(variantComponentsChild.GetPolicy<PurchaseOptionMoneyPolicy>()); }

                var newOptionMoneyPolicy = new PurchaseOptionMoneyPolicy();
                var currentCurrency = context.CommerceContext.CurrentCurrency();

                newOptionMoneyPolicy.SellPrice = new Money(currentCurrency, itemSalePrice);

                variantComponentsChild.SetPolicy(newOptionMoneyPolicy);

            }

            return Task.FromResult<SellableItem>(arg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
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
