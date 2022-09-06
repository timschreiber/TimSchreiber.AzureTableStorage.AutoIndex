namespace TimSchreiber.AzureTableStorage.AutoIndex.Repositories
{
    public class AutoIndexRepositoryOptions
    {
        public string StorageConnectionString { get; set; }
        public string TablePrefix { get; set; }
        public string IndexTableSuffix { get; set; }
        public int ChunkSize { get; set; }
    }
}
