using Newtonsoft.Json;
using OrderManager;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using OrderManager.Functions;

namespace ClientConsole
{
    class Program
    {
        private static string OrderWebHook = "http://localhost:7071/api/ReceiveOrder";


        static void Main(string[] args)
        {
            ExecuteAsync(5).GetAwaiter().GetResult();

            Console.ReadLine();
        }

        private static async Task ExecuteAsync(int numberOfOrders)
        {
            for (int i = 0; i < numberOfOrders; i++)
            {
                var order = CreateOrder();

                using (HttpClient client = new HttpClient())
                {
                    var postContent = JsonConvert.SerializeObject(order);
                    Console.WriteLine($"SEND --> {postContent}");
                    HttpContent content = new StringContent(postContent);
                    var response = await client.PostAsync(OrderWebHook, content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"RECEIVE <-- {responseContent}");
                    Console.WriteLine();
                }
                await Task.Delay(Faker.NumberFaker.Number(10000));
            }
        }

        private static Order CreateOrder()
        {
            var order = new Order()
            {
                orderId = Guid.NewGuid().ToString(),
                custName = Faker.NameFaker.Name(),
                custAddress = $"{Faker.LocationFaker.Street()}, {Faker.LocationFaker.City()}",
                custEmail = "massimo.bonanni@microsoft.com",//Faker.InternetFaker.Email(),
                date = DateTime.Now,
                price = Faker.NumberFaker.Number(100000) / 100.00
            };

            return order;
        }
    }
}
