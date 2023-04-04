using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace OrderManager.DurableFunctions
{
    public static class OrderDurableFunction
    {
        [FunctionName(FunctionNames.OrderReceiverDurable)]
        public static async Task<HttpResponseMessage> HttpStart(
                [HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequestMessage req,
                [OrchestrationClient]DurableOrchestrationClient starter,
                ILogger log)
        {
            log.LogInformation($"[START ORCHESTRATOR CLIENT] --> Order Durable received!");

            string jsonContent = await req.Content.ReadAsStringAsync();
            string instanceId = null;
            Order order = null;
            try
            {
                order = JsonConvert.DeserializeObject<Order>(jsonContent);
                instanceId = await starter.StartNewAsync(FunctionNames.OrderManagerDurable, order);
                log.LogInformation($"Order received - started orchestration with ID = '{instanceId}'.");
            }
            catch (Exception ex)
            {
                log.LogError("Error during order received operation", ex);
            }

            return starter.CreateCheckStatusResponse(req, instanceId);
        }


        [FunctionName(FunctionNames.OrderManagerDurable)]
        public static async Task OrderManager([OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {
            log.LogInformation($"[START ORCHESTRATOR] --> {FunctionNames.OrderManagerDurable}!");
            var order = context.GetInput<Order>();
            order.orderId = context.InstanceId;

            bool result = await context.CallActivityWithRetryAsync<bool>(FunctionNames.OrderStoreDurable,
                new RetryOptions(TimeSpan.FromSeconds(1), 10), order);

            if (!result)
                return;

            var fileName = await context.CallActivityAsync<string>(FunctionNames.GenerateInvoiceDurable, order);

            order.fileName = fileName;

            await context.CallActivityAsync<string>(FunctionNames.SendMailToCustomerDurable, order);

        }

        [FunctionName(FunctionNames.OrderStoreDurable)]
        public static async Task<bool> OrderStore([ActivityTrigger] Order order,
            [Table("ordersTableDurable", Connection = "StorageAccount")] IAsyncCollector<OrderRow> outputTable,
            ILogger log)
        {
            log.LogInformation($"[START ACTIVITY] --> {FunctionNames.OrderStoreDurable} for orderId={order.orderId}");
            try
            {
                OrderRow orderRow = new OrderRow(order);
                await outputTable.AddAsync(orderRow);
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Error during saving in Storage Table");
                return false;
            }

            return true;
        }

        [FunctionName(FunctionNames.GenerateInvoiceDurable)]
        [StorageAccount("StorageAccount")]
        public static Task<string> GenerateInvoice([ActivityTrigger] Order order,
            IBinder outputBinder,
            ILogger log)
        {
            log.LogInformation($"[START ACTIVITY] --> {FunctionNames.GenerateInvoiceDurable} for order: {order.orderId}");
            var fileName = $"invoicesdurable/{order.orderId}.txt";
            using (var outputBlob = outputBinder.Bind<TextWriter>(new BlobAttribute(fileName)))
            {
                outputBlob.WriteLine($"Fattura generata il {DateTime.Now} per l'ordine {order.orderId} del {order.date}");
                outputBlob.WriteLine($"");
                outputBlob.WriteLine($"Cliente : {order.custName}");
                outputBlob.WriteLine($"Domicilio: {order.custAddress}");
                outputBlob.WriteLine($"Email: {order.custEmail}");
                outputBlob.WriteLine($"");
                outputBlob.WriteLine($"Prezzo : {order.price}€");
            }

            return Task.FromResult(fileName);
        }

        [FunctionName(FunctionNames.SendMailToCustomerDurable)]
        [StorageAccount("StorageAccount")]
        public static bool SendMailToCustomer([ActivityTrigger] Order order,
             [SendGrid(ApiKey = "SendGridApiKey")] out SendGridMessage message,
             IBinder invoiceBinder,
             ILogger log)
        {
            log.LogInformation($"[START ACTIVITY] --> {FunctionNames.SendMailToCustomerDurable} for order: {order.orderId}");
            log.LogInformation($"File Processed : {order.fileName}");
            log.LogInformation($"Order: {order}");
            log.LogInformation($"Customer mail: {order.custEmail}");
            using (var inputBlob = invoiceBinder.Bind<TextReader>(new BlobAttribute(order.fileName)))
            {
                message = CreateMailMessage(order, inputBlob);
            }
            return true;
        }

        private static SendGridMessage CreateMailMessage(Order order, TextReader inputBlob)
        {
            var message = new SendGridMessage()
            {
                Subject = "Azure Functions Invoice",
                From = new EmailAddress("azureinvoice@invoiceplatform.com")
            };
            message.AddTo(new EmailAddress(order.custEmail));

            var buffer = ReadBufferFromTextReader(inputBlob);

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(buffer);
            var text = System.Convert.ToBase64String(plainTextBytes);

            message.AddContent("text/plain", System.Text.Encoding.UTF8.GetString(plainTextBytes));
            var filename = order.fileName.Split('/').Last();
            message.AddAttachment(filename, text, "text/plain", "attachment", "Invoice File");

            return message;
        }

        private static char[] ReadBufferFromTextReader(TextReader inputBlob)
        {
            List<char> returnArray = new List<char>();

            char[] buffer = new char[1024];
            var index = 0;
            int count = 0;
            do
            {
                count = inputBlob.ReadBlock(buffer, index, 1024);
                index += count;
                returnArray.AddRange(buffer.Take(count));
            } while (count == 1024);

            return returnArray.ToArray();
        }
    }
}
