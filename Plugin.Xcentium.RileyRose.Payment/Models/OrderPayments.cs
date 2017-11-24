using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Xcentium.RileyRose.Payment.Models
{
    public class OrderPayments
    {
        public string PrimaryPayment { get; set; }
        public string PrimaryToken { get; set; }
        public string PrimaryTokenName { get; set; }

        public bool GiftCardUsed { get; set; }

        public string TxnGuid { get; set; }
        public string Message { get; set; }
        public string MPan { get; set; }
        public string Exp { get; set; }
        public string Type { get; set; }
        public string ExpiresMonth { get; set; }
        public string ExpiresYear { get; set; }
        public string UId { get; set; }

    }
}
