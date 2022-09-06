using Azure;
using Azure.Data.Tables;
using System;

namespace TimSchreiber.AzureTableStorage.AutoIndex.Entities
{
    public abstract class AutoIndexTableEntity : ITableEntity
    {
        public AutoIndexTableEntity() { }

        public AutoIndexTableEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
