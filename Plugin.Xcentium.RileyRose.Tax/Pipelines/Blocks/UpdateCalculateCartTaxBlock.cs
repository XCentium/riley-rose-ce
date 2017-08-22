using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Commerce.Plugin.Tax;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Framework.Rules;
using F21Vertax.VertexO;

namespace Plugin.Xcentium.RileyRose.Tax.Pipelines.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateCalculateCartTaxBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {


        /// <summary>
        /// 
        /// </summary>
        public static Party ShippingParty { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public UpdateCalculateCartTaxBlock() : base((string)null)
        {

        }

        private JsonSerializerSettings _serializerSettings = null;
        private JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (_serializerSettings != null) return _serializerSettings;
                lock (this)
                {
                    _serializerSettings = new JsonSerializerSettings();
                    _serializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    _serializerSettings.Converters.Add(new StringEnumConverter());
                }
                return _serializerSettings;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires<Cart>(arg).IsNotNull<Cart>(string.Format("{0}: The cart can not be null", (object)this.Name));
            if (!arg.HasComponent<FulfillmentComponent>()) return Task.FromResult<Cart>(arg);

            if (arg.GetComponent<FulfillmentComponent>() is SplitFulfillmentComponent) return Task.FromResult<Cart>(arg);

            var taxAdjustments = arg.Adjustments.Where(a =>
            {
                if (!string.IsNullOrEmpty(a.Name) && a.Name.Equals(Constants.Tax.TaxFee, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(a.AdjustmentType))
                    return a.AdjustmentType.Equals(context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Tax,
                        StringComparison.OrdinalIgnoreCase);
                return false;
            }).ToList();




            if (taxAdjustments.Any())
            {
                // remove adjustments
                arg.Adjustments.Where(a =>
                {
                    if (!string.IsNullOrEmpty(a.Name) && a.Name.Equals(Constants.Tax.TaxFee, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrEmpty(a.AdjustmentType))
                        return a.AdjustmentType.Equals(context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Tax,
                            StringComparison.OrdinalIgnoreCase);
                    return false;
                }).ToList().ForEach(a => arg.Adjustments.Remove(a));


                var language = context.CommerceContext.CurrentLanguage();
               
                var globalTaxPolicy = context.GetPolicy<GlobalTaxPolicy>();
                var defaultTaxRate = globalTaxPolicy.DefaultCartTaxRate;
                var taxRate = defaultTaxRate;
                var vertexTaxPolicy = context.GetPolicy<VertexPolicy>();


                if (arg.Lines.Any() && arg.HasComponent<PhysicalFulfillmentComponent>())
                {
                    var cartComponent = arg.GetComponent<PhysicalFulfillmentComponent>();
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
                            UserName = vertexTaxPolicy.UserName,
                            Password = vertexTaxPolicy.Password
                        };

                        //Customer
                        var customerCode =
                            new CustomerCodeType
                            {
                                classCode = vertexTaxPolicy.ClassCode
                            };

                        var customer = new CustomerType {CustomerCode = customerCode};

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

                        int itemsPlusShippingCnt = arg.Lines.Count + 1;
                        var reqLineItems = new LineItemISIType[itemsPlusShippingCnt];

                        var nItemCount = 0;

                        foreach (var cartLineComponent in arg.Lines)
                        {

                            var product = new Product {productClass = cartLineComponent.ItemId};

                            var measure = new MeasureType {Value = cartLineComponent.Quantity};

                            var prodAmount =
                                new AmountType {Value = Convert.ToDecimal(cartLineComponent.UnitListPrice.Amount)};

                            var lineitem = new LineItemISIType
                            {
                                Seller = seller,
                                Product = product,
                                Quantity = measure,
                                UnitPrice = prodAmount,
                                taxDate = taxDate
                            };

                            reqLineItems[nItemCount] = lineitem;

                            product = null; measure = null; prodAmount = null; lineitem = null;

                            nItemCount++;
                        }


                        var shipProduct = new Product {productClass = "SH"};

                        var shipMeasure = new MeasureType {Value = 1};

                        decimal shippingAmt = 0.0M;

                        if (arg.HasComponent<FulfillmentComponent>())
                        {
                            var fulfillmentComponent = arg.GetComponent<FulfillmentComponent>();

                            if (fulfillmentComponent is PhysicalFulfillmentComponent)
                            {
                                var adjustment = arg.Adjustments.FirstOrDefault(x => x.Name == "FulfillmentFee");
                                if (adjustment != null && adjustment.IsTaxable)
                                {
                                    shippingAmt = adjustment.Adjustment.Amount;
                                }
                            }

                        }
                        var shipAmount = new AmountType {Value = Convert.ToDecimal(shippingAmt) };


                        var shipLineitem = new LineItemISIType
                        {
                            Seller = seller,
                            Product = shipProduct,
                            Quantity = shipMeasure,
                            UnitPrice = shipAmount,
                            taxDate = taxDate
                        };
                        reqLineItems[nItemCount] = shipLineitem;

                        shipProduct = null; shipMeasure = null; shipAmount = null; shipLineitem = null;

                        reqInvoice.Customer = customer;
                        reqInvoice.Customer.Destination = buyerLocation;
                        reqInvoice.documentDate = DateTime.Now;
                        reqInvoice.transactionType = SaleTransactionType.SALE;
                        reqInvoice.Currency = currency;
                        reqInvoice.LineItem = reqLineItems;

                        envelope.Login = login;
                        envelope.Item = reqInvoice;

                        var resInvoice = new InvoiceResponseType();

                        using (CalculateTaxWS60Client client = new CalculateTaxWS60Client())
                        {
                            client.calculateTax60(ref envelope);

                            resInvoice = (InvoiceResponseType)envelope.Item;


                            taxRate = resInvoice.TotalTax.Value;

                            client.Close();
                        }


                        reqLineItems = null; envelope = null; reqInvoice = null; resInvoice = null; customerCode = null; seller = null; sellerLocation = null; buyerLocation = null; login = null; customer = null; currency = null;

                    }
                }


                var currencyCode = context.CommerceContext.CurrentCurrency();

                context.Logger.LogDebug(string.Format("{0} - Policy: {1}", (object)this.Name, (object)globalTaxPolicy.TaxCalculationEnabled), Array.Empty<object>());
                var defaultCartTaxRate = globalTaxPolicy.DefaultCartTaxRate;
                var num1 = arg.Adjustments.Where<AwardedAdjustment>((Func<AwardedAdjustment, bool>)(p => p.IsTaxable)).Aggregate<AwardedAdjustment, Decimal>(Decimal.Zero, (Func<Decimal, AwardedAdjustment, Decimal>)((current, adjustment) => current + adjustment.Adjustment.Amount));
                context.Logger.LogDebug(string.Format("{0} - Tax Rate: {1} Adjustments Total:{2}", (object)this.Name, (object)defaultCartTaxRate, (object)num1), Array.Empty<object>());
                var source = arg.Lines.Where<CartLineComponent>((Func<CartLineComponent, bool>)(line =>
                {
                    if (globalTaxPolicy.TaxExemptTagsEnabled && line.HasComponent<CartProductComponent>())
                        return line.GetComponent<CartProductComponent>().Tags.Select<Tag, string>((Func<Tag, string>)(t => t.Name)).Contains<string>(globalTaxPolicy.TaxExemptTag, (IEqualityComparer<string>)StringComparer.InvariantCultureIgnoreCase);
                    return false;
                }));
                var adjustmentLinesTotal = new Decimal();
                var action = (Action<CartLineComponent>)(l => adjustmentLinesTotal += l.Totals.SubTotal.Amount);
                source.ForEach<CartLineComponent>(action);
                var amount = (arg.Totals.SubTotal.Amount + num1 - adjustmentLinesTotal) * defaultCartTaxRate;
                if (taxRate > Decimal.Zero)
                {
                    var awardedAdjustment1 = new CartLevelAwardedAdjustment();
                    var str1 = "TaxFee";
                    awardedAdjustment1.Name = str1;
                    var str2 = "TaxFee";
                    awardedAdjustment1.DisplayName = str2;
                    var money = new Money(currencyCode, amount);
                    awardedAdjustment1.Adjustment = money;
                    var tax = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Tax;
                    awardedAdjustment1.AdjustmentType = tax;
                    var name = this.Name;
                    awardedAdjustment1.AwardingBlock = name;
                    var num2 = 0;
                    awardedAdjustment1.IsTaxable = num2 != 0;
                    var awardedAdjustment2 = awardedAdjustment1;
                    arg.Adjustments.Add((AwardedAdjustment)awardedAdjustment2);
                }


            }

            return Task.FromResult<Cart>(arg);
        }
    }
}
