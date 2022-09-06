namespace TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Stores
{
    public class AzureTableStorageStoreOptions
    {
        public string StorageConnectionString { get; set; }
        public string TablePrefix { get; set; }
        public string IndexTableSuffix { get; set; }
        public int ChunkSize { get; set; }
    }
}
