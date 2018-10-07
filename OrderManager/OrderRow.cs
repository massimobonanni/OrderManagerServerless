using System;

namespace OrderManager
{
    public class OrderRow
    {
        public OrderRow()
        { }

        public OrderRow(Order order)
        {
            orderId = order.orderId;
            custName = order.custName;
            custAddress = order.custAddress;
            custEmail = order.custEmail;
            cartId = order.cartId;
            date = order.date;
            price = order.price;
            isReported = false;
        }

        public string PartitionKey
        {
            get
            {
                return "Order";
            }
        }

        public string RowKey
        {
            get
            {
                return orderId;
            }
        }
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
