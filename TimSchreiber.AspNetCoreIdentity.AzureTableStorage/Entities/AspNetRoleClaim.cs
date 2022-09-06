using System;
using System.Runtime.Serialization;
using TimSchreiber.AzureTableStorage.AutoIndex.Entities;
using TimSchreiber.AzureTableStorage.AutoIndex.Indexing;

namespace TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Entities
{
    public class AspNetRoleClaim : AutoIndexTableEntity
    {
        public AspNetRoleClaim()
        { }

        public AspNetRoleClaim(string roleId, string claimType, string claimValue)
            : base(roleId, GetRowKey(claimType, claimValue))
        { }

        [IgnoreDataMember]
        public string RoleId => PartitionKey;

        [IgnoreDataMember]
        public string ClaimType => RowKey.Split(new[] { Index.SEPARATOR }, 2, StringSplitOptions.RemoveEmptyEntries)[0];

        [IgnoreDataMember]
        public string ClaimValue => RowKey.Split(new[] { Index.SEPARATOR }, 2, StringSplitOptions.RemoveEmptyEntries)[1];

        public static string GetRowKey(string claimType, string claimValue) => $"{claimType}{Index.SEPARATOR}{claimValue}";
    }
}
