using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OrderManager.ReportGenerator;

namespace Microsoft.WindowsAzure.Storage.Table
{
    public static class CloudTableExtensions
    {

        public static async Task UpdateOrdersAsync(this CloudTable table, IEnumerable<ReportOrderRow> ordersToReport)
        {
            if (ordersToReport.Any())
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                foreach (var item in ordersToReport)
                {
                    batchOperation.InsertOrReplace(item);
                }
                await table.ExecuteBatchAsync(batchOperation);
            }
        }

        public static async Task<IEnumerable<ReportOrderRow>> GetOrdersToReport(this CloudTable table)
        {
            var ordersToReport = new List<ReportOrderRow>();

            TableQuery<ReportOrderRow> rangeQuery = new TableQuery<ReportOrderRow>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                        "Order"),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForBool("isReported", QueryComparisons.Equal,
                        false)));

            var token = default(TableContinuationToken);

            do
            {
                var query = await table.ExecuteQuerySegmentedAsync(rangeQuery, token);

                foreach (ReportOrderRow entity in query)
                {
                    ordersToReport.Add(entity);
                }
            } while (token != null);

            return ordersToReport;
        }
    }
}
