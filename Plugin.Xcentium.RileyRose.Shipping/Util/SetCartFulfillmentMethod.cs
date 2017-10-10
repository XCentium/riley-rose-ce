using CommerceServer.Core.Catalog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Fulfillment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Xcentium.RileyRose.Shipping.Components;

namespace Plugin.Xcentium.RileyRose.Shipping.Util
{
    public class SetCartFulfillmentMethod
    {
        private readonly GetSellableItemCommand _getSellableItemCommand;
        private readonly GetFulfillmentMethodsCommand _getFulfillmentMethodsCommand;
        private bool containsHazardousItems = false;

        public SetCartFulfillmentMethod(GetSellableItemCommand getSellableItemCommand, GetFulfillmentMethodsCommand getFulfillmentMethodsCommand) 
        {
            this._getSellableItemCommand = getSellableItemCommand;
            this._getFulfillmentMethodsCommand = getFulfillmentMethodsCommand;
        }


        public async Task<Cart> SetFulfillment(Cart cart, FulfillmentComponent origFulfillmentComponent, CommercePipelineExecutionContext context)
        { 
            Component existingFulfillmentComponent = cart.Components.FirstOrDefault(c => c is FulfillmentComponent); 

            List<Component> list1 = cart.Components.ToList<Component>();
            list1.RemoveAll((Predicate<Component>)(c => c is FulfillmentComponent));
            cart.Components = (IList<Component>)list1;

            foreach (CartLineComponent line in (IEnumerable<CartLineComponent>)cart.Lines)
            {
                List<Component> list2 = line.ChildComponents.ToList<Component>();
                list2.RemoveAll((Predicate<Component>)(c => c is FulfillmentComponent));
                List<Component> componentList = list2;
                line.ChildComponents = (IList<Component>)componentList;

                if(line.CartLineComponents != null && line.CartLineComponents.Any())
                {
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
            }

            if (containsHazardousItems)
            {
                //Set Cart to Ground Shippinig
                IEnumerable<FulfillmentMethod> fulfillmentMethods = await this._getFulfillmentMethodsCommand.Process(context.CommerceContext);
                var groundShipping = fulfillmentMethods.FirstOrDefault(f => f.Name.ToLower().Contains("ground"));
                //var FulfillmentComponent f = new FulfillmentComponent();
                FulfillmentComponent fc = new FulfillmentComponent();
                if (groundShipping != null)
                {
                    fc.FulfillmentMethod = new EntityReference(groundShipping.Id, "Standard Ground");
                    cart.SetComponent((Component)fc);
                } 
            }
            else
            {
                if (origFulfillmentComponent != null)
                {
                    cart.SetComponent(origFulfillmentComponent);
                }else
                {
                    if(existingFulfillmentComponent!=null) cart.SetComponent(existingFulfillmentComponent);
                }  
            }
            return await Task.FromResult<Cart>(cart);
        }
       
    }
}
