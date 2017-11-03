using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Plugin.Xcentium.RileyRose.Pipelines.Helper
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class OrderNumberSingleton
    {
        private string _sitecoreUrl = "http://cf.reference.storefront.com/rileyroseapi/order/GetOrderCount";

        /// <summary>
        /// 
        /// </summary>
        private OrderNumberSingleton()
        {
            SetOrderCount();
        }

        private void SetOrderCount()
        {
           // var uri = $"{sitecoreUrl}/rileyroseapi/order/GetOrderCount";

            var request = (HttpWebRequest)WebRequest.Create(_sitecoreUrl);
            var response = (HttpWebResponse)request.GetResponse();
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) return;
                using (var reader = new StreamReader(stream))
                {
                    var responseString = reader.ReadToEnd().Trim();
                    if (!string.IsNullOrEmpty(responseString))
                    {
                        try
                        {
                            var orderCounterResponse =
                                JsonConvert.DeserializeObject<OrderCounterResponse>(responseString);
                            if (orderCounterResponse.Success)
                            {
                                OrderCount = orderCounterResponse.Count;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public int OrderCount { get; set; }

        private static OrderNumberSingleton _instance;

        /// <summary>
        /// 
        /// </summary>
        public static OrderNumberSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OrderNumberSingleton();
                }
                return _instance;

            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetOrderCount()
        {
            var nextCount = OrderCount + 1;
            OrderCount = nextCount;
            return OrderCount;
        }

        /// <summary>
        /// 
        /// </summary>
        public string SitecoreUrl
        {
            get
            {
                return _sitecoreUrl;
            }

            set
            {
                _sitecoreUrl = value;
            }
        }
    }
}
