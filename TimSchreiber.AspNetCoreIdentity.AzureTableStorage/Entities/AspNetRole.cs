using System.Runtime.Serialization;
using TimSchreiber.AzureTableStorage.AutoIndex.Attributes;
using TimSchreiber.AzureTableStorage.AutoIndex.Entities;

namespace TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Entities
{
    public class AspNetRole : AutoIndexTableEntity
    {
        public AspNetRole() { }

        public AspNetRole(string id)
            : base(id, id)
        { }

        [IgnoreDataMember]
        public string Id => PartitionKey;

        public string Name { get; set; }

        [AutoIndex]
        public string NormalizedName { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}
