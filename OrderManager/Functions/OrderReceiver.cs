using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderManager.Functions
{
    public static class OrderReceiver
    {
        [FunctionName("OrderReceiver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ReceiveOrder")] HttpRequest req,
            [Queue("orderreceivedqueue", Connection = "StorageAccount")] IAsyncCollector<Order> outputQueueItem,
            [Table("ordersTable", Connection = "StorageAccount")] IAsyncCollector<OrderRow> outputTable,
            ILogger log)
        {
            log.LogInformation($"Order received!");

            string jsonContent = await new StreamReader(req.Body).ReadToEndAsync();

            Order order = null;
            try
            {
                order = JsonConvert.DeserializeObject<Order>(jsonContent);

                await outputQueueItem.AddAsync(order);

                OrderRow orderRow = new OrderRow(order);
                await outputTable.AddAsync(orderRow);
            }
            catch (Exception ex)
            {
                log.LogError("Error during order received operation", ex);
                return new BadRequestObjectResult("Error during order received operation");
            }

            return (ActionResult)new OkObjectResult($"Order {order} received.");

        }
    }
}
