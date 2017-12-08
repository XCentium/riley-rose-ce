using Sitecore.Commerce.Core;

namespace Plugin.Xcentium.RileyRose.Tax.Components
{
    public class VertexTax : Component
    {
        /// <summary>
        /// Unit Price tax based off
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Quantity applied to the unit price
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Line level discount
        /// </summary>
        public decimal LineDiscount { get; set; }

        /// <summary>
        /// Cart level discount
        /// </summary>
        public decimal CartLevelDiscount { get; set; }

        /// <summary>
        /// Total discount applied to the cart line
        /// </summary>
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// Grand total for tax purposes
        /// </summary>
        public decimal GrandPricePrice { get; set; }


        /// <summary>
        /// The Tax from Responsys
        /// </summary>
        public decimal Tax { get; set; }
    }
}
