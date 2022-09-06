using System;
using System.Runtime.Serialization;
using TimSchreiber.AzureTableStorage.AutoIndex.Entities;
using TimSchreiber.AzureTableStorage.AutoIndex.Indexing;

namespace TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Entities
{
    public class AspNetUserToken : AutoIndexTableEntity
    {
        public AspNetUserToken()
        { }

        public AspNetUserToken(string userId, string loginProvider, string name)
            : base(userId, GetRowKey(loginProvider, name))
        { }

        [IgnoreDataMember]
        public string UserId => PartitionKey;

        [IgnoreDataMember]
        public string LoginProvider => RowKey.Split(new[] { Index.SEPARATOR }, 2, StringSplitOptions.RemoveEmptyEntries)[0];

        [IgnoreDataMember]
        public string Name => RowKey.Split(new[] { Index.SEPARATOR }, 2, StringSplitOptions.RemoveEmptyEntries)[1];

        public string Value { get; set; }

        public static string GetRowKey(string loginProvider, string name) => $"{loginProvider}{Index.SEPARATOR}{name}";
    }
}
