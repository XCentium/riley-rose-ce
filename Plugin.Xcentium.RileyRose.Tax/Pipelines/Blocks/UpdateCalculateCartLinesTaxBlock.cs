using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Commerce.Plugin.Tax;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using F21Vertax.VertexO;

namespace Plugin.Xcentium.RileyRose.Tax.Pipelines.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateCalculateCartLinesTaxBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// 
        /// </summary>
        public static Party ShippingParty { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires<Cart>(arg).IsNotNull<Cart>(string.Format("{0}: The cart can not be null", (object)this.Name));
            Condition.Requires<IList<CartLineComponent>>(arg.Lines).IsNotNull<IList<CartLineComponent>>(string.Format("{0}: The cart lines can not be null", (object)this.Name));

            // get all lines that have fulfillment methods applied
            var list = arg.Lines.Where(line =>
            {
                if (line != null)
                    return line.HasComponent<FulfillmentComponent>();
                return false;
            }).Select(l => l).ToList();
            if (!list.Any())
            {
                return await Task.FromResult(arg);
            }


            var currencyCode = context.CommerceContext.CurrentCurrency();
            var globalTaxPolicy = context.GetPolicy<GlobalTaxPolicy>();
            var defaultItemTaxRate = globalTaxPolicy.DefaultCartTaxRate;
            var taxRate = defaultItemTaxRate;
            var globalPricingPolicy = context.GetPolicy<GlobalPricingPolicy>();
            var vertexTaxPolicy = context.GetPolicy<VertexPolicy>();

            context.Logger.LogInformation("Vertex Pass:" + vertexTaxPolicy.Password);

            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.WriteLine(addr);
                    context.Logger.LogInformation("IP:" + addr);
                }
            }

            context.Logger.LogDebug($"{(object) this.Name} - Policy:{(object) globalTaxPolicy.TaxCalculationEnabled}", Array.Empty<object>());
            context.Logger.LogDebug($"{(object) this.Name} - Item Tax Rate:{(object) defaultItemTaxRate}", Array.Empty<object>());
            foreach (var cartLineComponent in list)
            {
                if (globalTaxPolicy.TaxExemptTagsEnabled && cartLineComponent.HasComponent<CartProductComponent>())
                {
                    var tags = cartLineComponent.GetComponent<CartProductComponent>().Tags;

                    var taxExcemptedTag = tags.Where(x=>x.Name.Contains(globalTaxPolicy.TaxExemptTag));
                    if(taxExcemptedTag.Any()) { continue;}

                }

                // Only calculate where there is a shipping address
                if (!(cartLineComponent.ChildComponents.OfType<FulfillmentComponent>().FirstOrDefault() is PhysicalFulfillmentComponent)) { continue;}

                var cartComponent = cartLineComponent.GetComponent<PhysicalFulfillmentComponent>();
                ShippingParty = cartComponent?.ShippingParty;

                if (ShippingParty != null)
                {

                    var envelope = new VertexEnvelope();
                    var reqInvoice = new InvoiceRequestType();


                    //Seller
                    var sellerLocation = new LocationType
                    {
                        StreetAddress1 = vertexTaxPolicy.ShipFromAddressLine1,
                        StreetAddress2 = vertexTaxPolicy.ShipFromAddressLine2,
                        City = vertexTaxPolicy.ShipFromCity,
                        MainDivision = vertexTaxPolicy.ShipFromStateOrProvinceCode,
                        PostalCode = vertexTaxPolicy.ShipFromPostalCode,
                        Country = vertexTaxPolicy.ShipFromCountryCode
                    };

                    var seller = new SellerType
                    {
                        Company = vertexTaxPolicy.CompanyCode,
                        PhysicalOrigin = sellerLocation
                    };


                    //Login
                    var login = new LoginType
                    {
                        UserName = "wsapi_xc", //vertexTaxPolicy.UserName,
                        Password = "wsapi_xc@!", //vertexTaxPolicy.Password
                    };
                    context.Logger.LogInformation("Vertex Pass:" + login.Password);

                    //Customer
                    var customerCode =
                        new CustomerCodeType
                        {
                            classCode = vertexTaxPolicy.ClassCode
                        };

                    var customer = new CustomerType { CustomerCode = customerCode };

                    var buyerLocation = new LocationType
                    {
                        StreetAddress1 = ShippingParty.Address1,
                        StreetAddress2 = ShippingParty.Address2,
                        City = ShippingParty.City,
                        MainDivision = ShippingParty.StateCode,
                        PostalCode = ShippingParty.ZipPostalCode,
                        Country = ShippingParty.CountryCode
                    };

                    //Currency
                    var currency = new CurrencyType
                    {
                        isoCurrencyCodeAlpha = context.CommerceContext.CurrentCurrency()
                    };


                    //Date
                    var taxDate = DateTime.Now;

                    //Items
                    var reqLineItems = new LineItemISIType[2];

                    var product = new Product { productClass = cartLineComponent.ItemId };

                    var measure = new MeasureType { Value = cartLineComponent.Quantity };

                    var prodAmount =
                        new AmountType { Value = Convert.ToDecimal(cartLineComponent.UnitListPrice.Amount) };

                    var lineitem = new LineItemISIType
                    {
                        Seller = seller,
                        Product = product,
                        Quantity = measure,
                        UnitPrice = prodAmount,
                        taxDate = taxDate
                    };

                    reqLineItems[0] = lineitem;

                    product = null; measure = null; prodAmount = null; lineitem = null;


                    var shipProduct = new Product { productClass = "SH" };

                    var shipMeasure = new MeasureType { Value = 1 };

                    decimal shippingAmt = 0.0M;

                    if (cartLineComponent.HasComponent<FulfillmentComponent>())
                    {
                        var fulfillmentComponent = cartLineComponent.GetComponent<FulfillmentComponent>();

                        if (fulfillmentComponent is PhysicalFulfillmentComponent)
                        {
                            var adjustment = cartLineComponent.Adjustments.FirstOrDefault(x => x.Name == "FulfillmentFee");
                            if (adjustment != null && adjustment.IsTaxable)
                            {
                                shippingAmt = adjustment.Adjustment.Amount;
                            }
                        }

                    }
                    var shipAmount = new AmountType { Value = Convert.ToDecimal(shippingAmt) };


                    var shipLineitem = new LineItemISIType
                    {
                        Seller = seller,
                        Product = shipProduct,
                        Quantity = shipMeasure,
                        UnitPrice = shipAmount,
                        taxDate = taxDate
                    };
                    reqLineItems[1] = shipLineitem;

                    shipProduct = null; shipMeasure = null; shipAmount = null; shipLineitem = null;

                    reqInvoice.Customer = customer;
                    reqInvoice.Customer.Destination = buyerLocation;
                    reqInvoice.documentDate = DateTime.Now;
                    reqInvoice.transactionType = SaleTransactionType.SALE;
                    reqInvoice.Currency = currency;
                    reqInvoice.LineItem = reqLineItems;

                    envelope.Login = login;
                    envelope.Item = reqInvoice;

                    try
                    {
                        var remoteAddress = new System.ServiceModel.EndpointAddress("http://10.110.10.68:8095/vertex-ws/services/CalculateTax60");

                        using (CalculateTaxWS60Client client = new CalculateTaxWS60Client(new System.ServiceModel.BasicHttpBinding(), remoteAddress))
                        {
                            client.calculateTax60(ref envelope);

                            var resInvoice = envelope.Item as InvoiceResponseType;

                            if (resInvoice != null) taxRate = resInvoice.TotalTax.Value;

                            context.Logger.LogInformation($"{(object)this.Name} - Vertex Item Tax Rate: {(object)taxRate}", Array.Empty<object>());

                            resInvoice = null;

                            client.Close();
                        }
                    }
                    catch (Exception ex)
                    {

                        context.Logger.LogInformation($"{(object)this.Name} - Vertex Failed1 : {(object)ex.Message}", Array.Empty<object>());


                        context.Logger.LogInformation("Vertex Failed1");

                    }



                    reqLineItems = null; envelope = null; reqInvoice = null;  customerCode = null; seller = null; sellerLocation = null; buyerLocation = null; login = null; customer = null; currency = null;

                }

                var subTotal = cartLineComponent.Adjustments.Where(a => a.IsTaxable)
                    .Aggregate(decimal.Zero, (current, adjustment) => current + adjustment.Adjustment.Amount);
                var subTotalRound = new Money(currencyCode,
                    (cartLineComponent.Totals.SubTotal.Amount + subTotal) * taxRate);
                if (globalPricingPolicy.ShouldRoundPriceCalc)
                    subTotalRound.Amount = decimal.Round(subTotalRound.Amount, globalPricingPolicy.RoundDigits,
                        globalPricingPolicy.MidPointRoundUp ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven);
                var adjustments = cartLineComponent.Adjustments;
                var awardedAdjustment = new CartLineLevelAwardedAdjustment
                {
                    Name = Constants.Tax.TaxFee,
                    DisplayName = Constants.Tax.TaxFee,
                    Adjustment = subTotalRound
                };

                var tax = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Tax;
                awardedAdjustment.AdjustmentType = tax;

                awardedAdjustment.AwardingBlock = Name;
                var taxableFlag = 0;
                awardedAdjustment.IsTaxable = taxableFlag != 0;
                var includeinGrandTotalFlag = 0;
                awardedAdjustment.IncludeInGrandTotal = includeinGrandTotalFlag != 0;
                adjustments.Add(awardedAdjustment);

            }

            return await Task.FromResult(arg);
        }
    }
}
