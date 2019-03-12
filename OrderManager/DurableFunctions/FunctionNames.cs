using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManager.DurableFunctions
{
    internal static class FunctionNames
    {
        public const string OrderReceiverDurable = "OrderReceiverDurable";
        public const string OrderManagerDurable = "OrderManagerDurable";
        public const string OrderStoreDurable = "OrderStoreDurable";
        public const string GenerateInvoiceDurable = "GenerateInvoiceDurable";
        public const string SendMailToCustomerDurable="SendMailToCustomerDurable";
       }
}
