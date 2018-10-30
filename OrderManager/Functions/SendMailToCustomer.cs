using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace OrderManager.Functions
{
    public static class SendMailToCustomer
    {
        [FunctionName("SendMailToCustomer")]
        [StorageAccount("StorageAccount")]
        public static void Run([BlobTrigger("invoices/{filename}")] Stream myBlob,
            string filename,
            [Table("ordersTable", "Order", "{filename}", Filter = null, Take = 50)] OrderRow orderRow,
            ILogger log,
            [SendGrid(ApiKey = "SendGridApiKey")] out SendGridMessage message)
        {
            log.LogInformation($"File Processed : {filename}");
            log.LogInformation($"Order: {orderRow}");
            log.LogInformation($"Customer mail: {orderRow.custEmail}");

            message = CreateMailMessage(orderRow, myBlob);
        }

        private static SendGridMessage CreateMailMessage(OrderRow orderRow, Stream myBlob)
        {
            var message = new SendGridMessage()
            {
                Subject = "Azure Functions Invoice",
                From = new EmailAddress("azureinvoice@invoiceplatform.com")
            };
            message.AddTo(new EmailAddress(orderRow.custEmail));

            var buffer = ReadBufferFromStream(myBlob);

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(buffer);
            var text = System.Convert.ToBase64String(plainTextBytes);

            message.AddContent("text/plain", System.Text.Encoding.UTF8.GetString(plainTextBytes));
            message.AddAttachment("invoice.txt", text, "text/plain", "attachment", "Invoice File");

            return message;
        }

        private static char[] ReadBufferFromStream(Stream input)
        {
            char[] buffer = new char[input.Length];
            using (StreamReader reader = new StreamReader(input))
            {
                reader.Read(buffer, 0, (int)input.Length);
            }
            return buffer;
        }
    }
}
