using System;
using System.Runtime.Serialization;
using TimSchreiber.AzureTableStorage.AutoIndex.Attributes;
using TimSchreiber.AzureTableStorage.AutoIndex.Entities;
using TimSchreiber.AzureTableStorage.AutoIndex.Indexing;

namespace TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Entities
{
    public class AspNetUserClaim : AutoIndexTableEntity
    {
        public AspNetUserClaim()
        { }

        public AspNetUserClaim(string userId, string claimType, string claimValue)
            : base(userId, GetRowKey(claimType, claimValue))
        { }

        [AutoIndex]
        public new string RowKey { get => base.RowKey; set => base.RowKey = value; }

        [IgnoreDataMember]
        public string UserId => PartitionKey;

        [IgnoreDataMember]
        public string ClaimType => RowKey.Split(new[] { Index.SEPARATOR }, 2, StringSplitOptions.RemoveEmptyEntries)[0];

        [IgnoreDataMember]
        public string ClaimValue => RowKey.Split(new[] { Index.SEPARATOR }, 2, StringSplitOptions.RemoveEmptyEntries)[1];

        public static string GetRowKey(string claimType, string claimValue) => $"{claimType}{Index.SEPARATOR}{claimValue}";
    }
}
