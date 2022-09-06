using System.Runtime.Serialization;
using TimSchreiber.AzureTableStorage.AutoIndex.Attributes;
using TimSchreiber.AzureTableStorage.AutoIndex.Entities;

namespace TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Entities
{
    public class AspNetUserRole : AutoIndexTableEntity
    {
        public AspNetUserRole() { }

        public AspNetUserRole(string userId, string roleId)
            : base(userId, roleId)
        { }

        [AutoIndex]
        public new string RowKey { get; set; }

        [IgnoreDataMember]
        public string UserId => PartitionKey;

        [IgnoreDataMember]
        public string RoleId => RowKey;
    }
}
