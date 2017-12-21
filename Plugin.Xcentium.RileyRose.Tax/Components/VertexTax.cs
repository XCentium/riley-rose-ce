using Sitecore.Commerce.Core;

namespace Plugin.Xcentium.RileyRose.Tax.Components
{
    public class VertexTax : Component
    {

        /// <summary>
        /// Subtotal before any discount
        /// </summary>
        public decimal AmountBeforeDiscount { get; set; }

        /// <summary>
        /// Line level discount
        /// </summary>
        public decimal LineDiscount { get; set; }

        /// <summary>
        /// Cart level discount
        /// </summary>
        public decimal CartLevelDiscount { get; set; }

        /// <summary>
        /// Grand total for tax purposes
        /// </summary>
        public decimal AmountToTaxAfterDiscounts { get; set; }


        /// <summary>
        /// The Tax from Responsys
        /// </summary>
        public decimal Tax { get; set; }
    }
}
