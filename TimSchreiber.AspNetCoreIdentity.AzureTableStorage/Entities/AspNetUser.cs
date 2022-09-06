using System;
using System.Runtime.Serialization;
using TimSchreiber.AzureTableStorage.AutoIndex.Attributes;
using TimSchreiber.AzureTableStorage.AutoIndex.Entities;

namespace TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Entities
{
    public class AspNetUser : AutoIndexTableEntity
    {
        public AspNetUser()
        { }

        public AspNetUser(string id, string userName)
        {
            PartitionKey = id;
            RowKey = id;
            UserName = userName;
        }

        [IgnoreDataMember]
        public string Id => PartitionKey;

        [DataMember(Name = "LockoutEnd")]
        public string LockoutEndValue { get; set; }

        [IgnoreDataMember]
        public DateTimeOffset? LockoutEnd
        {
            get => !string.IsNullOrWhiteSpace(LockoutEndValue) && DateTimeOffset.TryParse(LockoutEndValue, out DateTimeOffset lockoutEnd)
                ? lockoutEnd
                : default;
            set => LockoutEndValue = value.HasValue
                ? value.ToString()
                : default;
        }

        [AutoIndex]
        public string NormalizedEmail { get; set; }

        [AutoIndex]
        public string NormalizedUserName { get; set; }

        public bool TwoFactorEnabled { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string SecurityStamp { get; set; }
        public string PasswordHash { get; set; }
        public bool EmailConfirmed { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
    }
}
