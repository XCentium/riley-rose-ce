using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plugin.Xcentium.RileyRose.Pipelines.Helper;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Pipelines;
using Sitecore.Framework.Conditions;
using Sitecore.Commerce.Core.Commands;


namespace Plugin.Xcentium.RileyRose.Pipelines.Blocks
{
    /// <summary>
    /// Customize Order Confirmation Number
    /// </summary>
    public class CustomizeOrderNumber : PipelineBlock<Order, Order, CommercePipelineExecutionContext>
    {
        private IConfiguration configuration;
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;

        public CustomizeOrderNumber(IConfiguration configuration, FindEntitiesInListCommand findEntitiesInListCommand)
        {
            this.configuration = configuration;
            _findEntitiesInListCommand = findEntitiesInListCommand;
        }

        /// <summary>
        /// Generate Order Confirmation Number
        /// </summary>
        /// <param name="order"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<Order> Run(Order order, CommercePipelineExecutionContext context)
        {
            Condition.Requires<Order>(order).IsNotNull<Order>("The order can not be null");

            var uniqueCode = Guid.NewGuid().ToString("B");
            var connection = configuration[Constants.AppSettings.RileyRoseInterfaceConnection];
            var initialOrderStatusCode = configuration[Constants.AppSettings.InitialOrderStatusCode];

            if (order.Components.Any())
            {
                //Get Contact Component which contains customer information
                var contactComponent = order.GetComponent<ContactComponent>();

                if (!string.IsNullOrEmpty(connection) && contactComponent != null)
                {
                    try
                    {
                        var sqlConnection = new SqlConnection(connection);
                        //open connection
                        sqlConnection.Open();
                        var sqlCommand = new SqlCommand(Constants.StoredProcedures.Names.InsertOrderDetails, sqlConnection);

                        //set paramter values
                        sqlCommand.Parameters.Add(Constants.StoredProcedures.Parameters.OrderGroupId, SqlDbType.UniqueIdentifier).Value = new Guid(order.FriendlyId);
                        sqlCommand.Parameters.Add(Constants.StoredProcedures.Parameters.CustomerId, SqlDbType.NVarChar).Value = contactComponent.CustomerId;
                        sqlCommand.Parameters.Add(Constants.StoredProcedures.Parameters.CustomerEmail, SqlDbType.NVarChar).Value = contactComponent.Email;
                        sqlCommand.Parameters.Add(Constants.StoredProcedures.Parameters.OrderConfirmationId, SqlDbType.Int);
                        sqlCommand.Parameters.Add(Constants.StoredProcedures.Parameters.TrackingNumber, SqlDbType.NVarChar).Value = string.Empty;
                        sqlCommand.Parameters.Add(Constants.StoredProcedures.Parameters.OrderStatus, SqlDbType.VarChar).Value = initialOrderStatusCode; //Initial Order Status will be 0002, which states that its NEW
                        sqlCommand.Parameters.Add(Constants.StoredProcedures.Parameters.PssTracking, SqlDbType.VarChar).Value = string.Empty;
                        sqlCommand.Parameters.Add(Constants.StoredProcedures.Parameters.RegDate, SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        sqlCommand.Parameters.Add(Constants.StoredProcedures.Parameters.Posted, SqlDbType.Bit).Value = true;
                        sqlCommand.Parameters.Add(Constants.StoredProcedures.Parameters.MsReplicationVersion, SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
                        sqlCommand.Parameters.Add(Constants.StoredProcedures.Parameters.ShipCDompany, SqlDbType.VarChar).Value = string.Empty;

                        sqlCommand.Parameters[Constants.StoredProcedures.Parameters.OrderConfirmationId].Direction = ParameterDirection.Output; //output parameter

                        //set command type to be a stored procedure
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        //Execute
                        sqlCommand.ExecuteNonQuery();
                        //close connection
                        sqlConnection.Close();
                        uniqueCode = sqlCommand.Parameters[Constants.StoredProcedures.Parameters.OrderConfirmationId].Value.ToString();
                    }
                    catch (Exception ex)
                    {
                        context.Logger.LogError($"Plugin.Xcentium.RileyRose.Pipelines.Blocks.CustomizeOrderNumber encountered an exception while generating orderConfirmationId. Exception Message: {ex.Message}, Details: {ex.StackTrace}");
                        uniqueCode = GetCustomOrderNumber(order, context, _findEntitiesInListCommand);
                    }
                }
            }

            order.OrderConfirmationId = uniqueCode;

            return Task.FromResult<Order>(order);
        }

        /// <summary>
        /// You may customized what is returned here based on number od existing orders or date or get number from external system
        /// </summary>
        /// <param name="order"></param>
        /// <param name="context"></param>
        /// <param name="findEntitiesInListCommand"></param>
        /// <returns></returns>
        private string GetCustomOrderNumber(Order order, CommercePipelineExecutionContext context, FindEntitiesInListCommand findEntitiesInListCommand)
        {
            //Get Contact Component which contains customer information
            var contactComponent = order.GetComponent<ContactComponent>();

            // get all existing orders.
            var orders = (IEnumerable<Order>)findEntitiesInListCommand.Process<Order>(context.CommerceContext, CommerceEntity.ListName<Order>(), 0, int.MaxValue).Result.Items;

            // Total orders
            var orderCount = orders.Count();

            // use the info you have to generate an appropriate order number. You may also use the data you have to call an external system.
            // in this instance we will just return the number of existing orders incremented by 1
            // Return order count and increment by 1 as the new order number.
            if (orders.Any())
            {
                var nextOrderNumber = 2000000000 + orderCount + 1;
                return nextOrderNumber.ToString();
            }

            // return a random guid if ther was an isssue retriving existing orders or all else failed.
            return Guid.NewGuid().ToString("B");
        }


    }
}
