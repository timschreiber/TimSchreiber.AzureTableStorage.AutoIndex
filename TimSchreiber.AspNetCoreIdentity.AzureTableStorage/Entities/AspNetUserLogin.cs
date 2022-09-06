using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using TimSchreiber.AzureTableStorage.AutoIndex.Attributes;
using TimSchreiber.AzureTableStorage.AutoIndex.Entities;
using TimSchreiber.AzureTableStorage.AutoIndex.Indexing;

namespace TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Entities
{
    public class AspNetUserLogin : AutoIndexTableEntity
    {
        public AspNetUserLogin() { }

        public AspNetUserLogin(string userId, string loginProvider, string providerKey)
            : base(userId, $"{GetRowKey(loginProvider, providerKey)}")
        { }

        [AutoIndex]
        public new string RowKey { get; set; }

        [IgnoreDataMember]
        public string UserId => PartitionKey;

        [IgnoreDataMember]
        public string LoginProvider => RowKey.Split(new[] { Index.SEPARATOR }, 2, StringSplitOptions.RemoveEmptyEntries)[0];

        [IgnoreDataMember]
        public string ProviderKey => RowKey.Split(new[] { Index.SEPARATOR }, 2, StringSplitOptions.RemoveEmptyEntries)[1];

        public string ProviderDisplayName { get; set; }

        public static string GetRowKey(string loginProvider, string providerKey) => $"{loginProvider}{Index.SEPARATOR}{providerKey}";
    }
}
