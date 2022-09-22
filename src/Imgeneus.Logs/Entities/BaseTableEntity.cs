using Azure;
using Azure.Data.Tables;
using System;

namespace Imgeneus.Logs.Entities
{
    public record BaseTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;
        public DateTimeOffset? Timestamp { get; set; } = default!;
        public ETag ETag { get; set; } = default!;

        public BaseTableEntity(uint senderId)
        {
            PartitionKey = $"{senderId}";

            // This will ensure that the latest entries are added to the top of the table instead of at the bottom of the table.
            // More info here: https://stackoverflow.com/questions/40593939/how-to-retrieve-latest-record-using-rowkey-or-timestamp-in-azure-table-storage
            RowKey = $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks}";
        }
    }
}
