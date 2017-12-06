using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
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
using Microsoft.Extensions.Configuration;
using Plugin.Xcentium.RileyRose.Tax.Components;

namespace Plugin.Xcentium.RileyRose.Tax.Pipelines.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateCalculateCartTaxBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 
        /// </summary>
        public static Party ShippingParty { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public UpdateCalculateCartTaxBlock(IConfiguration configuration) : base((string) null)
        {
            _configuration = configuration;
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
            Condition.Requires<Cart>(arg)
                .IsNotNull<Cart>($"{(object) this.Name}: The cart can not be null");
            if (!arg.HasComponent<FulfillmentComponent>()) return Task.FromResult<Cart>(arg);

            if (arg.GetComponent<FulfillmentComponent>() is SplitFulfillmentComponent)
                return Task.FromResult<Cart>(arg);

            var taxAdjustments = arg.Adjustments.Where(a =>
            {
                if (!string.IsNullOrEmpty(a.Name) &&
                    a.Name.Equals(Constants.Tax.TaxFee, StringComparison.OrdinalIgnoreCase) &&
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
                    if (!string.IsNullOrEmpty(a.Name) &&
                        a.Name.Equals(Constants.Tax.TaxFee, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrEmpty(a.AdjustmentType))
                        return a.AdjustmentType.Equals(context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Tax,
                            StringComparison.OrdinalIgnoreCase);
                    return false;
                }).ToList().ForEach(a => arg.Adjustments.Remove(a));

            }

            var language = context.CommerceContext.CurrentLanguage();

            var globalTaxPolicy = context.GetPolicy<GlobalTaxPolicy>();
            var defaultTaxRate = globalTaxPolicy.DefaultCartTaxRate;
            var taxRate = 0.00M;
            
            var vertexConfig = _configuration.GetSection("VertexTax"); 

            var endpoint = vertexConfig["Endpoint"];

            var useVertexEndpoint = false || !string.IsNullOrEmpty(endpoint);

            var productTaxList = new List<KeyValuePair<string, decimal>>();

            var prodList = new List<string>();

            if (arg.Lines.Any() && arg.HasComponent<PhysicalFulfillmentComponent>() && (useVertexEndpoint))
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
                        StreetAddress1 = vertexConfig["ShipFromAddressLine1"],
                        StreetAddress2 = vertexConfig["ShipFromAddressLine2"],
                        City = vertexConfig["ShipFromCity"],
                        MainDivision = vertexConfig["ShipFromStateOrProvinceCode"],
                        PostalCode = vertexConfig["ShipFromPostalCode"],
                        Country = vertexConfig["ShipFromCountryCode"]
                    };

                    var seller = new SellerType
                    {
                        Company = vertexConfig["CompanyCode"],
                        PhysicalOrigin = sellerLocation
                    };


                    //Login
                    var login = new LoginType
                    {
                        UserName = vertexConfig["UserName"], 
                        Password = vertexConfig["Password"],
                    };
                

                    //Customer
                    var customerCode =
                        new CustomerCodeType
                        {
                            classCode = vertexConfig["ClassCode"]
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

                    var itemsPlusShippingCnt = arg.Lines.Count + 1;
                    var reqLineItems = new LineItemISIType[itemsPlusShippingCnt];

                    var nItemCount = 0;

                    

                    foreach (var cartLineComponent in arg.Lines)
                    {
                        var whCategoryCode = cartLineComponent.ItemId; 
                        var cartProductComponent = cartLineComponent.GetComponent<CartProductComponent>();
                        var cartproduct = context.CommerceContext.Objects.OfType<CommerceServer.Core.Catalog.Product>().FirstOrDefault<CommerceServer.Core.Catalog.Product>((Func<CommerceServer.Core.Catalog.Product, bool>)(p => p.ProductId.Equals(cartProductComponent.Id, StringComparison.OrdinalIgnoreCase)));
                        if (cartproduct != null)
                        {
                            if (cartproduct.HasProperty(Constants.Tax.WhCategoryCode))
                            {
                                whCategoryCode = cartproduct[Constants.Tax.WhCategoryCode].ToString();
                                if (!string.IsNullOrEmpty(whCategoryCode))
                                {
                                    if (whCategoryCode.Length > 4)
                                    {
                                        whCategoryCode = whCategoryCode.Substring(0, 4);
                                    }
                                    prodList.Add(cartLineComponent.ItemId);

                                    context.Logger.LogInformation(
                                        $"{(object) this.Name} - Vertex ADDED Product with whCategoryCode: {(object) whCategoryCode}",
                                        Array.Empty<object>());

                                }
                            }
                        }


                        var product = new Product {productClass = whCategoryCode};

                        //var measure = new MeasureType {Value = cartLineComponent.Quantity};
                        var measure = new MeasureType {Value = 1.0M};

                        var prodAmount =
                            new AmountType {Value = Convert.ToDecimal(cartLineComponent.Totals.GrandTotal.Amount)};

                        var lineitem = new LineItemISIType
                        {
                            Seller = seller,
                            Product = product,
                            Quantity = measure,
                            UnitPrice = prodAmount,
                            taxDate = taxDate
                        };

                        reqLineItems[nItemCount] = lineitem;

                        cartproduct = null;
                        measure = null;
                        prodAmount = null;
                        lineitem = null;

                        nItemCount++;
                    }


                    var shipProduct = new Product {productClass = "SH"};

                    var shipMeasure = new MeasureType {Value = 1};

                    var shippingAmt = 0.0M;

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
                    var shipAmount = new AmountType {Value = Convert.ToDecimal(shippingAmt)};


                    var shipLineitem = new LineItemISIType
                    {
                        Seller = seller,
                        Product = shipProduct,
                        Quantity = shipMeasure,
                        UnitPrice = shipAmount,
                        taxDate = taxDate
                    };
                    reqLineItems[nItemCount] = shipLineitem;

                    shipProduct = null;
                    shipMeasure = null;
                    shipAmount = null;
                    shipLineitem = null;

                    reqInvoice.Customer = customer;
                    reqInvoice.Customer.Destination = buyerLocation;
                    reqInvoice.documentDate = DateTime.Now;
                    reqInvoice.transactionType = SaleTransactionType.SALE;
                    reqInvoice.Currency = currency;
                    reqInvoice.LineItem = reqLineItems;

                    envelope.Login = login;
                    envelope.Item = reqInvoice;
                    var resInvoice = new InvoiceResponseType();
                    try
                    {
                        var remoteAddress = new System.ServiceModel.EndpointAddress("http://10.110.10.68:8095/vertex-ws/services/CalculateTax60");

                        using (CalculateTaxWS60Client client = new CalculateTaxWS60Client(new System.ServiceModel.BasicHttpBinding(), remoteAddress))
                        {
                            client.calculateTax60(ref envelope);

                            resInvoice = (InvoiceResponseType)envelope.Item;

                            var resLineItems = resInvoice.LineItem;

                            taxRate = resInvoice.TotalTax.Value;


                            if (resLineItems != null && resLineItems.Length > 0)
                            {
                                context.Logger.LogInformation(
                                    $"{(object)this.Name} - Vertex ResLineItems Lines: {(object)resLineItems.Length}",
                                    Array.Empty<object>());

                                var cnt = 0;
                                foreach (var resLineItem in resLineItems)
                                {
                                    if (cnt < prodList.Count)
                                    {
                                        context.Logger.LogInformation(
                                            $"{(object) this.Name} - Vertex Line Adding Started For: {(object) prodList[cnt]}",
                                            Array.Empty<object>());

                                        var productId = prodList[cnt];
                                        var kvp = new KeyValuePair<string, decimal>(productId,
                                            resLineItem.TotalTax.Value);
                                        productTaxList.Add(kvp);

                                        context.Logger.LogInformation(
                                            $"{(object) this.Name} - Vertex Line Adding Ended For: {(object) prodList[cnt]}",
                                            Array.Empty<object>());
                                        cnt++;
                                    }
                                }
                            }
                            else
                            {
                                context.Logger.LogInformation(
                                    $"{(object)this.Name} - Vertex ResLineItems Lines: {(object)0}",
                                    Array.Empty<object>());
                            }


                            client.Close();


                            context.Logger.LogInformation($"{(object)this.Name} - Vertex Item Tax Rate2: {(object)taxRate}", Array.Empty<object>());

                        }
                    }
                    catch (Exception ex)
                    {
                        context.Logger.LogDebug(
                            $"{(object) this.Name} - Vertex Tax Error: {(object) ex.Message}",
                            Array.Empty<object>());

                        context.Logger.LogInformation($"{(object)this.Name} - Vertex Failed! : {(object)ex.Message}", Array.Empty<object>());


                    }

                    if (productTaxList.Any())
                    {
                        context.Logger.LogInformation($"{(object)this.Name} - Vertex Lines Tax Exists: {(object)productTaxList.Count}", Array.Empty<object>());

                        foreach (var cartLineComponent in arg.Lines)
                        {
                            var kvp = productTaxList.FirstOrDefault(x => x.Key == cartLineComponent.ItemId);
                            if (!kvp.Equals(default(KeyValuePair<string, decimal>)))
                            {
                                if (cartLineComponent.HasComponent<VertexTax>())
                                {
                                    var vtax = cartLineComponent.GetComponent<VertexTax>();

                                    if (kvp.Value == vtax.Tax)
                                    {
                                        continue;
                                    }
                                    vtax.Tax = kvp.Value;
                                    cartLineComponent.GetComponent<VertexTax>().Tax = kvp.Value;
                                }
                                else
                                {
                                    cartLineComponent.SetComponent(new VertexTax { Tax = kvp.Value });
                                }


                                context.Logger.LogInformation($"{(object)this.Name} - No Vertex Line Tax Found for: {(object)cartLineComponent.ItemId} Amount:{(object)kvp.Value}", Array.Empty<object>());

                            }
                            else
                            {
                                context.Logger.LogInformation($"{(object)this.Name} - No Vertex Line Tax Missing for: {(object)cartLineComponent.ItemId}", Array.Empty<object>());
                            }

                        }
                    }
                    else
                    {
                        context.Logger.LogInformation($"{(object)this.Name} - No Vertex Lines Tax: {(object)0}", Array.Empty<object>());


                    }

                    reqLineItems = null;
                    envelope = null;
                    reqInvoice = null;
                    resInvoice = null;
                    customerCode = null;
                    seller = null;
                    sellerLocation = null;
                    buyerLocation = null;
                    login = null;
                    customer = null;
                    currency = null;

                }
            }


            var currencyCode = context.CommerceContext.CurrentCurrency();

            context.Logger.LogDebug($"{(object) this.Name} - Policy: {(object) globalTaxPolicy.TaxCalculationEnabled}",Array.Empty<object>());

            var defaultCartTaxRate = globalTaxPolicy.DefaultCartTaxRate;


            var num1 = arg.Adjustments.Where<AwardedAdjustment>((Func<AwardedAdjustment, bool>) (p => p.IsTaxable))
                .Aggregate<AwardedAdjustment, Decimal>(Decimal.Zero,
                    (Func<Decimal, AwardedAdjustment, Decimal>)
                    ((current, adjustment) => current + adjustment.Adjustment.Amount));

            context.Logger.LogDebug($"{(object) this.Name} - Tax Rate: {(object) defaultCartTaxRate} Adjustments Total:{(object) num1}", Array.Empty<object>());

            var source = arg.Lines.Where<CartLineComponent>((Func<CartLineComponent, bool>) (line =>
            {
                if (globalTaxPolicy.TaxExemptTagsEnabled && line.HasComponent<CartProductComponent>())
                    return line.GetComponent<CartProductComponent>().Tags
                        .Select<Tag, string>((Func<Tag, string>) (t => t.Name))
                        .Contains<string>(globalTaxPolicy.TaxExemptTag,
                            (IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
                return false;
            }));

            var adjustmentLinesTotal = new Decimal();

            var action = (Action<CartLineComponent>) (l => adjustmentLinesTotal += l.Totals.SubTotal.Amount);
            source.ForEach<CartLineComponent>(action);
            var amount = (arg.Totals.SubTotal.Amount + num1 - adjustmentLinesTotal) * defaultCartTaxRate;

            if (taxRate > decimal.Zero){amount = taxRate;}

            var awardedAdjustment1 = new CartLevelAwardedAdjustment
            {
                Name = "TaxFee",
                DisplayName = "TaxFee"
            };


            var money = new Money(currencyCode, amount);
            awardedAdjustment1.Adjustment = money;
            var tax = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Tax;
            awardedAdjustment1.AdjustmentType = tax;
            awardedAdjustment1.AwardingBlock = this.Name;
            awardedAdjustment1.IsTaxable = false;
            var awardedAdjustment2 = awardedAdjustment1;
            arg.Adjustments.Add((AwardedAdjustment) awardedAdjustment2);
            

            return Task.FromResult<Cart>(arg);
        }
    }
}
