using CommerceServer.Core.Catalog;
using Plugin.Xcentium.RileyRose.Shipping.Util;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks
{ 

    [PipelineDisplayName("Plugin.Xcentium.RileyRose.Shipping.F21RileyRoseSetCartFulfillmentBlock")]
    public class F21RileyRoseSetCartFulfillmentBlock : PipelineBlock<CartFulfillmentArgument, Cart, CommercePipelineExecutionContext>
    {
        private readonly GetSellableItemCommand _getSellableItemCommand;
        private readonly GetFulfillmentMethodsCommand _getFulfillmentMethodsCommand;
        //private bool containsHazardousItems = false;

        public F21RileyRoseSetCartFulfillmentBlock(GetSellableItemCommand getSellableItemCommand, GetFulfillmentMethodsCommand getFulfillmentMethodsCommand)
            : base((string)null)
        {
            this._getSellableItemCommand = getSellableItemCommand;
            this._getFulfillmentMethodsCommand = getFulfillmentMethodsCommand; 
        }
       
        public override async Task<Cart> Run(CartFulfillmentArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires<CartFulfillmentArgument>(arg).IsNotNull<CartFulfillmentArgument>(string.Format("{0}: The arg cannot be null", (object)this.Name));
            Condition.Requires<Cart>(arg.Cart).IsNotNull<Cart>(string.Format("{0}: The cart cannot be null", (object)this.Name));
            Condition.Requires<FulfillmentComponent>(arg.Fulfillment).IsNotNull<FulfillmentComponent>(string.Format("{0}: The fulfillment cannot be null", (object)this.Name));
            Cart cart = arg.Cart;
            // IEnumerable<FulfillmentMethod> fulfillmentMethods = await this._getFulfillmentMethodsCommand.Process(context.CommerceContext, order.GetComponent<OnHoldOrderComponent>().TemporaryCart.EntityTarget, party);

            /*

            List<Component> list1 = cart.Components.ToList<Component>();
            list1.RemoveAll((Predicate<Component>)(c => c is FulfillmentComponent));
            cart.Components = (IList<Component>)list1;
            
            foreach (CartLineComponent line in (IEnumerable<CartLineComponent>)cart.Lines)
            {
                List<Component> list2 = line.ChildComponents.ToList<Component>();
                list2.RemoveAll((Predicate<Component>)(c => c is FulfillmentComponent));
                List<Component> componentList = list2;
                line.ChildComponents = (IList<Component>)componentList;

                var componentId = line.CartLineComponents.FirstOrDefault(c => c is CartProductComponent).Id;


                //Check To See If any products are Hazardous => Ground Shipping
                var product = context.CommerceContext.GetObjects<Product>().FirstOrDefault(p => p.ProductId.Equals(componentId, StringComparison.OrdinalIgnoreCase)); 
                SellableItem sellableItem = await this._getSellableItemCommand.Process(context.CommerceContext, line.ItemId, false);
                bool isHazardous = sellableItem.GetComponent<F21RileyRosePrdComponent>().IsHazardous;

                if (isHazardous)
                {
                    containsHazardousItems = true;
                } 
            }

            if (containsHazardousItems)
            {
                //Set Cart to Ground Shippinig
                IEnumerable<FulfillmentMethod> fulfillmentMethods = await this._getFulfillmentMethodsCommand.Process(context.CommerceContext);
                var groundShipping = fulfillmentMethods.FirstOrDefault(f => f.Name.ToLower().Contains("ground"));
                //var FulfillmentComponent f = new FulfillmentComponent();
                FulfillmentComponent fc = new FulfillmentComponent();
                fc.FulfillmentMethod = new EntityReference(groundShipping.Id, "Standard Ground");
                cart.SetComponent((Component)fc);
            }else
            {
                cart.SetComponent((Component)arg.Fulfillment);
            }
            */
            Cart tempCart = await new SetCartFulfillmentMethod(_getSellableItemCommand, _getFulfillmentMethodsCommand).SetFulfillment(cart, arg.Fulfillment, context);
            //return await Task.FromResult<Cart>(cart);
            return await Task.FromResult<Cart>(tempCart);
        }
    } 
}
