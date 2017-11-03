using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;

namespace Plugin.Xcentium.RileyRose.Payment.Components
{
    public class MultipleGiftCardPaymentComponent : Component
    {
        /// <summary>
        /// 
        /// </summary>
        public List<F21GiftCard> GiftCardPaymentList = new List<F21GiftCard>();


        public bool HasGiftCardByNumber(string cardNumber)
        {
            var result = GiftCardPaymentList.FirstOrDefault(x => x.CardNumber == cardNumber) ; 
            return (result!=null);
        }

        public bool AddGiftCard(string cardNumber, Money paymentAmount, Money originalBalance)
        {
            if (HasGiftCardByNumber(cardNumber)) return false;

            GiftCardPaymentList.Add
            (
                new F21GiftCard()
                {
                    CardNumber = cardNumber,
                    OriginalBalance = originalBalance,
                    PaymentAmount = paymentAmount,
                    CouponCode = cardNumber
                }
            );
            return true;
        }

        public bool RemoveGiftCard(string cardNumber)
        {
            if (!HasGiftCardByNumber(cardNumber)) return false;  
            GiftCardPaymentList.Remove(GiftCardPaymentList.FirstOrDefault(x => x.CardNumber == cardNumber)); 
            return true;
        }

        public decimal GetAllGiftCardTotalsBalance()
        {
            var mySum2 = GiftCardPaymentList.Select(x => x.OriginalBalance.Amount) ;
            var mySum = GiftCardPaymentList.Select(x => x.OriginalBalance.Amount).ToArray().Sum();
            return GiftCardPaymentList.Select(x => x.OriginalBalance.Amount).ToArray().Sum();  
        }

        public string GetDynamicGiftCardCode()
        {
            var dynamicCode = string.Join(",", GiftCardPaymentList.Select(x => x.CardNumber).ToArray());
            return (string.Join(",", GiftCardPaymentList.Select(x => x.CardNumber).ToArray())) ;
        }
    }
}
