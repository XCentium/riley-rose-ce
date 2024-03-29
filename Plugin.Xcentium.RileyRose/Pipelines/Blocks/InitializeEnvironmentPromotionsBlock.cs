﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializeEnvironmentPromotionsBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Xcentium.RileyRose
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.Coupons;
    using Sitecore.Commerce.Plugin.Fulfillment;
    using Sitecore.Commerce.Plugin.Promotions;
    using Sitecore.Commerce.Plugin.Rules;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines a block which bootstraps promotions.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{System.String, System.String,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName("RileyRose.InitializeEnvironmentPromotionsBlock")]
    public class InitializeEnvironmentPromotionsBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;
        private readonly IAddPromotionBookPipeline _addBookPipeline;
        private readonly IAddPromotionPipeline _addPromotionPipeline;
        private readonly IAddQualificationPipeline _addQualificationPipeline;
        private readonly IAddBenefitPipeline _addBenefitPipeline;
        private readonly IAddPrivateCouponPipeline _addPrivateCouponPipeline;
        private readonly IAddPublicCouponPipeline _addPublicCouponPipeline;
        private readonly IAddPromotionItemPipeline _addPromotionItemPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeEnvironmentPromotionsBlock"/> class.
        /// </summary>
        /// <param name="persistEntityPipeline">The persist entity pipeline.</param>
        /// <param name="addBookPipeline">The add book pipeline.</param>
        /// <param name="addPromotionPipeline">The add promotion pipeline.</param>
        /// <param name="addQualificationPipeline">The add qualification pipeline.</param>
        /// <param name="addBenefitPipeline">The add benefit pipeline.</param>
        /// <param name="addPrivateCouponPipeline">The add private coupon pipeline.</param>
        /// <param name="addPromotionItemPipeline">The add promotion item pipeline.</param>
        /// <param name="addPublicCouponPipeline">The add public coupon pipeline.</param>
        public InitializeEnvironmentPromotionsBlock(
            IPersistEntityPipeline persistEntityPipeline,
            IAddPromotionBookPipeline addBookPipeline,
            IAddPromotionPipeline addPromotionPipeline,
            IAddQualificationPipeline addQualificationPipeline,
            IAddBenefitPipeline addBenefitPipeline,
            IAddPrivateCouponPipeline addPrivateCouponPipeline,
            IAddPromotionItemPipeline addPromotionItemPipeline,
            IAddPublicCouponPipeline addPublicCouponPipeline)
        {
            this._persistEntityPipeline = persistEntityPipeline;
            this._addBookPipeline = addBookPipeline;
            this._addPromotionPipeline = addPromotionPipeline;
            this._addQualificationPipeline = addQualificationPipeline;
            this._addBenefitPipeline = addBenefitPipeline;
            this._addPrivateCouponPipeline = addPrivateCouponPipeline;
            this._addPromotionItemPipeline = addPromotionItemPipeline;
            this._addPublicCouponPipeline = addPublicCouponPipeline;
        }

        /// <summary>
        /// The run.
        /// </summary>
        /// <param name="arg">
        /// The argument.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override async Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            var artifactSet = "Environment.RileyRose.Promotions-1.0";

            // Check if this environment has subscribed to this Artifact Set
            if (!context.GetPolicy<EnvironmentInitializationPolicy>()
                .InitialArtifactSets.Contains(artifactSet))
            {
                return arg;
            }

            context.Logger.LogInformation($"{this.Name}.InitializingArtifactSet: ArtifactSet={artifactSet}");

            var book =
                await this._addBookPipeline.Run(
                    new AddPromotionBookArgument("RileyRose_PromotionBook")
                    {
                        DisplayName = "RileyRose Promotion Book",
                        Description = "This is the RileyRose promotion book"
                    },
                    context);

            await this.CreateCartFreeShippingPromotion(book, context);
            await this.CreateCartExclusive5PctOffCouponPromotion(book, context);
            await this.CreateCartExclusive5OffCouponPromotion(book, context);
            await this.CreateCart15PctOffCouponPromotion(book, context);
            await this.CreateDisabledPromotion(book, context);

            var date = DateTimeOffset.UtcNow;
            await this.CreateCart10PctOffCouponPromotion(book, context, date);
            System.Threading.Thread.Sleep(1); //// TO ENSURE CREATING DATE IS DIFFERENT BETWEEN THESE TWO PROMOTIONS
            await this.CreateCart10OffCouponPromotion(book, context, date);

            await this.CreateLineExclusive20PctOffCouponPromotion(book, context);
            await this.CreateLineExclusive20OffCouponPromotion(book, context);
            await this.CreateLine5PctOffCouponPromotion(book, context);
            await this.CreateLine5OffCouponPromotion(book, context);

            return arg;
        }

        #region Cart's Promotions

        /// <summary>
        /// Creates cart free shipping promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateCartFreeShippingPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await this._addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "CartFreeShippingPromotion", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1), "Free Shipping", "Free Shipping")
                    {
                        DisplayName = "Free Shipping",
                        Description = "Free shipping when Cart subtotal of $100 or more"
                    },
                    context);

            promotion =
                await this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartSubtotalCondition,
                            Name = CartsConstants.Conditions.CartSubtotalCondition,
                            Properties = new List<PropertyModel>
                                             {
                                                  new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                                  new PropertyModel { Name = "Subtotal", Value = "100", IsOperator = false, DisplayType = "System.Decimal" }
                                             }
                        }),
                    context);

            promotion =
                await this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = FulfillmentConstants.Conditions.CartHasFulfillmentCondition,
                            Name = FulfillmentConstants.Conditions.CartHasFulfillmentCondition,
                            Properties = new List<PropertyModel>()
                        }),
                    context);

            await this._addBenefitPipeline.Run(
               new PromotionActionModelArgument(
                   promotion,
                   new ActionModel
                   {
                       Id = Guid.NewGuid().ToString(),
                       LibraryId = FulfillmentConstants.Actions.CartFreeShippingAction,
                       Name = FulfillmentConstants.Actions.CartFreeShippingAction
                   }),
               context);

            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        /// <summary>
        /// Creates cart exclusive 5 percent off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateCartExclusive5PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
              await this._addPromotionPipeline.Run(
                  new AddPromotionArgument(book, "Cart5PctOffExclusiveCouponPromotion", DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddYears(1), "5% Off Cart (Exclusive Coupon)", "5% Off Cart (Exclusive Coupon)")
                  {
                      IsExclusive = true,
                      DisplayName = "5% Off Cart (Exclusive Coupon)",
                      Description = "5% off Cart with subtotal of $10 or more (Exclusive Coupon)"
                  },
                  context);

            promotion =
                await this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion =
                await this._addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Actions.CartSubtotalPercentOffAction,
                            Name = CartsConstants.Actions.CartSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "PercentOff", Value = "5", DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNEC5P"), context);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        /// <summary>
        /// Creates the cart exclusive5 off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateCartExclusive5OffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
              await this._addPromotionPipeline.Run(
                  new AddPromotionArgument(book, "Cart5OffExclusiveCouponPromotion", DateTimeOffset.UtcNow.AddDays(-3), DateTimeOffset.UtcNow.AddYears(1), "$5 Off Cart (Exclusive Coupon)", "$5 Off Cart (Exclusive Coupon)")
                  {
                      IsExclusive = true,
                      DisplayName = "$5 Off Cart (Exclusive Coupon)",
                      Description = "$5 off Cart with subtotal of $10 or more (Exclusive Coupon)"
                  },
                  context);

            promotion =
                await this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion =
                await this._addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Actions.CartSubtotalAmountOffAction,
                            Name = CartsConstants.Actions.CartSubtotalAmountOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "AmountOff", Value = "5", DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNEC5A"), context);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        /// <summary>
        /// Creates cart 15 percent off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateCart15PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
               await this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Cart15PctOffCouponPromotion", DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddYears(1), "15% Off Cart (Coupon)", "15% Off Cart (Coupon)")
                   {
                       DisplayName = "15% Off Cart (Coupon)",
                       Description = "15% off Cart with subtotal of $50 or more (Coupon)"
                   },
                   context);

            promotion =
                await this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartSubtotalCondition,
                            Name = CartsConstants.Conditions.CartSubtotalCondition,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "50", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion =
                await this._addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Actions.CartSubtotalPercentOffAction,
                            Name = CartsConstants.Actions.CartSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "PercentOff", Value = "15", DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNC15P"), context);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        /// <summary>
        /// Creates the cart10 PCT off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <param name="date">The date.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateCart10PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context, DateTimeOffset date)
        {
            var promotion =
               await this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Cart10PctOffCouponPromotion", date, date.AddYears(1), "10% Off Cart (Coupon)", "10% Off Cart (Coupon)")
                   {
                       DisplayName = "10% Off Cart (Coupon)",
                       Description = "10% off Cart with subtotal of $50 or more (Coupon)"
                   },
                   context);

            promotion =
                await this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartSubtotalCondition,
                            Name = CartsConstants.Conditions.CartSubtotalCondition,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "50", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion =
                await this._addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Actions.CartSubtotalPercentOffAction,
                            Name = CartsConstants.Actions.CartSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "PercentOff", Value = "10", DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNC10P"), context);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        /// <summary>
        /// Creates the cart10 off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <param name="date">The date.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateCart10OffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context, DateTimeOffset date)
        {
            var promotion =
               await this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Cart10OffCouponPromotion", date, date.AddYears(1), "$10 Off Cart (Coupon)", "$10 Off Cart (Coupon)")
                   {
                       DisplayName = "$10 Off Cart (Coupon)",
                       Description = "$10 off Cart with subtotal of $50 or more (Coupon)"
                   },
                   context);

            promotion =
                await this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartSubtotalCondition,
                            Name = CartsConstants.Conditions.CartSubtotalCondition,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "50", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion =
                await this._addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Actions.CartSubtotalAmountOffAction,
                            Name = CartsConstants.Actions.CartSubtotalAmountOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "AmountOff", Value = "10", DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNC10A"), context);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        /// <summary>
        /// Creates the disabled promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateDisabledPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
               await this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "DisabledPromotion", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1), "Disabled", "Disabled")
                   {
                       DisplayName = "Disabled",
                       Description = "Disabled"
                   },
                   context);

            promotion =
                await this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartSubtotalCondition,
                            Name = CartsConstants.Conditions.CartSubtotalCondition,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "5", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion =
                await this._addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Actions.CartSubtotalPercentOffAction,
                            Name = CartsConstants.Actions.CartSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "PercentOff", Value = "100", DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion.SetPolicy(new DisabledPolicy());
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        #endregion

        #region Line Promotions


        /// <summary>
        /// Creates line exclusive 20 percent off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateLineExclusive20PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
               await this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Line20PctOffExclusiveCouponPromotion", DateTimeOffset.UtcNow.AddDays(-3), DateTimeOffset.UtcNow.AddYears(1), "20% Off Item (Exclusive Coupon)", "20% Off Item (Exclusive Coupon)")
                   {
                       IsExclusive = true,
                       DisplayName = "20% Off Item (Exclusive Coupon)",
                       Description = "20% off any item with subtotal of $50 or more (Exclusive Coupon)"
                   },
                   context);

            promotion =
                await this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "25", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion =
                await this._addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Actions.CartAnyItemSubtotalPercentOffAction,
                            Name = CartsConstants.Actions.CartAnyItemSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "PercentOff", Value = "20", DisplayType = "System.Decimal" },
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "25", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNEL20P"), context);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        /// <summary>
        /// Creates the line exclusive $20 off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateLineExclusive20OffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
               await this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Line20OffExclusiveCouponPromotion", DateTimeOffset.UtcNow.AddDays(-4), DateTimeOffset.UtcNow.AddYears(1), "$20 Off Item (Exclusive Coupon)", "$20 Off Item (Exclusive Coupon)")
                   {
                       IsExclusive = true,
                       DisplayName = "$20 Off Item (Exclusive Coupon)",
                       Description = "$20 off any item with subtotal of $50 or more (Exclusive Coupon)"
                   },
                   context);

            promotion =
                await this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "25", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion =
                await this._addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Actions.CartAnyItemSubtotalAmountOffAction,
                            Name = CartsConstants.Actions.CartAnyItemSubtotalAmountOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "AmountOff", Value = "20", DisplayType = "System.Decimal" },
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "25", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNEL20A"), context);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        /// <summary>
        /// Creates line 5 percent off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateLine5PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
               await this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Line5PctOffCouponPromotion", DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddYears(1), "5% Off Item (Coupon)", "5% Off Item (Coupon)")
                   {
                       DisplayName = "5% Off Item (Coupon)",
                       Description = "5% off any item with subtotal of 10$ or more (Coupon)"
                   },
                   context);

            promotion =
                await this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion =
                await this._addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Actions.CartAnyItemSubtotalPercentOffAction,
                            Name = CartsConstants.Actions.CartAnyItemSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "PercentOff", Value = "5", DisplayType = "System.Decimal" },
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNL5P"), context);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        /// <summary>
        /// Creates line 5 amount off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateLine5OffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
               await this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Line5OffCouponPromotion", DateTimeOffset.UtcNow.AddDays(-6), DateTimeOffset.UtcNow.AddYears(1), "$5 Off Item (Coupon)", "$5 Off Item (Coupon)")
                   {
                       DisplayName = "$5 Off Item (Coupon)",
                       Description = "$5 off any item with subtotal of $10 or more (Coupon)"
                   },
                   context);

            promotion =
                await this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion =
                await this._addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Actions.CartAnyItemSubtotalAmountOffAction,
                            Name = CartsConstants.Actions.CartAnyItemSubtotalAmountOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "AmountOff", Value = "5", DisplayType = "System.Decimal" },
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNL5A"), context);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        #endregion
    }
}
