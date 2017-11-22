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

        // 
        [HttpPost]
        [Route("SetCartLineProperties()")]
        public async Task<IActionResult> SetCartLineProperties([FromBody] ODataActionParameters value)
        {

            if (!this.ModelState.IsValid)
                return (IActionResult)new BadRequestObjectResult(this.ModelState);

            if (!value.ContainsKey("cartId")) return (IActionResult) new BadRequestObjectResult((object) value);
            var id = value["cartId"];

            if (string.IsNullOrEmpty(id?.ToString())) return (IActionResult) new BadRequestObjectResult((object) value);

            var cartId = id.ToString();

            var cartLineProperties = new CartLineProperties();
            if (value.ContainsKey("cartLineProperties"))
            {
                var cartlinePropObj = value["cartLineProperties"];

                if (!string.IsNullOrEmpty(cartLineProperties?.ToString()))
                {
                    cartLineProperties = JsonConvert.DeserializeObject<CartLineProperties>(cartlinePropObj.ToString());
                }
            }

            var command = this.Command<SetCartLinePropertiesCommand>();
            await Task.Delay(1);
            var runCommand = await command.Process(this.CurrentContext, cartId, cartLineProperties);

            return (IActionResult)new ObjectResult((object)runCommand);
        }

    }
}
