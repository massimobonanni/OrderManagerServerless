using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace OrderManager
{
    //public static class GenerateInvoice
    //{
    //    [FunctionName("GenerateInvoice")]
    //    [StorageAccount("StorageAccount")]
    //    public static void Run([QueueTrigger("myqueue")]Order myQueueItem, 
    //        IBinder outputBinder,
    //        ILogger log)
    //    {
    //       log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

    //        using (var outputBlob = outputBinder.Bind<TextWriter>(new BlobAttribute($"invoices/{myQueueItem.orderId}.txt")))
    //        {
    //            outputBlob.WriteLine($"Fattura generata il {DateTime.Now} per l'ordine {myQueueItem.orderId} del {myQueueItem.date}");
    //            outputBlob.WriteLine($"");
    //            outputBlob.WriteLine($"Cliente : {myQueueItem.custName}");
    //            outputBlob.WriteLine($"Domicilio: {myQueueItem.custAddress}");
    //            outputBlob.WriteLine($"Email: {myQueueItem.custEmail}");
    //            outputBlob.WriteLine($"");
    //            outputBlob.WriteLine($"Prezzo : {myQueueItem.price}€");
    //        }
    //    }
    //}
}
