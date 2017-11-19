using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.OData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Plugin.Xcentium.CartProperties.Commands;
using Plugin.Xcentium.CartProperties.Models;
using Sitecore.Commerce.Core;

namespace Plugin.Xcentium.CartProperties.Controller
{
    /// <summary>
    /// 
    /// </summary>
    public class CommandsController : CommerceController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="globalEnvironment"></param>
        public CommandsController(IServiceProvider serviceProvider, CommerceEnvironment globalEnvironment) : base(serviceProvider, globalEnvironment)
        {

        }



        [HttpPost]
        [Route("SetCartProperties()")]
        public async Task<IActionResult> SetCartProperties([FromBody] ODataActionParameters value)
        {
            var cartProperties1 = new Models.CartProperties();

            var newCartProperties = new Properties();
            var kvList = new List<KeyValue>();
            var kv1 = new KeyValue
            {
                Key = "Key1",
                Value = "Value1"
            };
            kvList.Add(kv1);
            var kv2 = new KeyValue
            {
                Key = "Key2",
                Value = "Value2"
            };
            kvList.Add(kv2);
            var kv3 = new KeyValue
            {
                Key = "Key3",
                Value = "Value3"
            };
            kvList.Add(kv3);
            newCartProperties.KeyValues = kvList;

            cartProperties1.Properties = newCartProperties;

            var cartLineProperties1 = new CartLineProperties();
            var cartlinePropertiesList1 = new List<CartLineProperty>();

            var cartLineProperty1 = new CartLineProperty
            {
                CartLineId = "842c488b7bc34d4699d5057f983690f8",
                Properties = newCartProperties
            };

            cartlinePropertiesList1.Add(cartLineProperty1);
            cartLineProperties1.CartLineProperty = cartlinePropertiesList1;
            //string json = JsonConvert.SerializeObject(newCartProperties);
            //string json1 = JsonConvert.SerializeObject(cartProperties1);
            //string json2 = JsonConvert.SerializeObject(cartLineProperties1);

            if (!this.ModelState.IsValid)
                return (IActionResult) new BadRequestObjectResult(this.ModelState);

            if (value.ContainsKey("cartId"))
            {
                object obj1 = value["cartId"];

                if (!string.IsNullOrEmpty(obj1?.ToString()))
                {
                    var cartId = obj1.ToString();

                    var cartProperties = new Models.CartProperties();
                    var cartLineProperties = new CartLineProperties();

                    if (value.ContainsKey("cartProperties"))
                    {
                        var obj2 = value["cartProperties"];

                        if (!string.IsNullOrEmpty(obj2?.ToString()))
                        {
                            cartProperties = JsonConvert.DeserializeObject<Models.CartProperties>(obj2.ToString());
                        }
                    }

                    if (value.ContainsKey("cartLineProperties"))
                    {
                        var obj3 = value["cartLineProperties"];

                        if (!string.IsNullOrEmpty(cartLineProperties?.ToString()))
                        {
                            cartLineProperties = JsonConvert.DeserializeObject<CartLineProperties>(obj3.ToString());
                        }
                    }

                    var command = this.Command<SetCartPropertiesCommand>();
                    await Task.Delay(1);
                    var runCommand = await command.Process(this.CurrentContext, cartId, cartProperties,
                        cartLineProperties);

                    return (IActionResult) new ObjectResult((object) command);

                }
            }
            return (IActionResult) new BadRequestObjectResult((object) value);

        }
    }
}
