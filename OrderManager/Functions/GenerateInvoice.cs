using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace OrderManager.Functions
{
    public static class GenerateInvoice
    {
        [FunctionName("GenerateInvoice")]
        [StorageAccount("StorageAccount")]
        public static void Run(
            [QueueTrigger("orderreceivedqueue")]Order myQueueItem,
            IBinder outputBinder,
            ILogger log)
        {
            log.LogInformation($"Generate invoice for order: {myQueueItem.orderId}");

            using (var outputBlob = outputBinder.Bind<TextWriter>(new BlobAttribute($"invoices/{myQueueItem.orderId}")))
            {
                outputBlob.WriteLine($"Fattura generata il {DateTime.Now} per l'ordine {myQueueItem.orderId} del {myQueueItem.date}");
                outputBlob.WriteLine($"");
                outputBlob.WriteLine($"Cliente : {myQueueItem.custName}");
                outputBlob.WriteLine($"Domicilio: {myQueueItem.custAddress}");
                outputBlob.WriteLine($"Email: {myQueueItem.custEmail}");
                outputBlob.WriteLine($"");
                outputBlob.WriteLine($"Prezzo : {myQueueItem.price}€");
            }
        }
    }
}
