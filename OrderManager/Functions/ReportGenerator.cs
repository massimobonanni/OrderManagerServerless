using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace OrderManager.Functions
{
    public static class ReportGenerator
    {
        [FunctionName("ReportGenerator")]
        [StorageAccount("StorageAccount")]
        public static async Task Run([TimerTrigger("%RepoterScheduleTriggerTime%")] TimerInfo myTimer,
            [Table("ordersTable", "Order")] CloudTable table,
            IBinder reportBinder,
            ILogger log)
        {
            log.LogInformation($"ReportGenerator executed at: {DateTime.Now}");

            var ordersToReport = await table.GetOrdersToReport();

            if (ordersToReport.Any())
            {
                var reportDate = DateTime.Now;

                using (var reportBlob = reportBinder.Bind<TextWriter>(new BlobAttribute($"reports/{reportDate:yyyyMMddHHmmss}.rep")))
                {
                    reportBlob.WriteLine($"OrderId;Customer;OrderDate;Price");
                    double total = 0.0;

                    foreach (var order in ordersToReport)
                    {
                        log.LogInformation($"{order}");
                        reportBlob.WriteLine($"{order.orderId};{order.custName};{order.date};{order.price}");
                        total += order.price;
                        order.isReported = true;
                    }

                    reportBlob.WriteLine($";Total;;{total}");
                    log.LogInformation($"Total: {total}");
                }

                await table.UpdateOrdersAsync(ordersToReport);
            }
            else
            {
                log.LogWarning($"Nothing to report!");
            }

        }






        public class ReportOrderRow : TableEntity
        {
            public string orderId { get; set; }
            public string custName { get; set; }
            public string custAddress { get; set; }
            public string custEmail { get; set; }
            public string cartId { get; set; }
            public DateTime date { get; set; }
            public double price { get; set; }
            public bool isReported { get; set; }
            public string fileName { get; set; }

            public override String ToString()
            {
                return $"orderId={orderId}, custName={custName}, custAddress={custAddress}, custEmail={custEmail}, cartId={cartId}, date={date}, price={price}";
            }
        }
    }
}
