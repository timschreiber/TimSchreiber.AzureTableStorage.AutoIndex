using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Entities;
using TimSchreiber.AzureTableStorage.AutoIndex.Repositories;

namespace TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Stores
{
    public static class AzureTableStorageStoreExtensions
    {
        public static IdentityBuilder AddAzureTableStorageStores(this IdentityBuilder builder, Action<AzureTableStorageStoreOptions> startupAction)
        {
            var services = builder.Services;
            var options = new AzureTableStorageStoreOptions();
            startupAction(options);

            services.Configure<AutoIndexRepositoryOptions>(o =>
            {
                o.TablePrefix = options.TablePrefix;
                o.ChunkSize = options.ChunkSize;
                o.IndexTableSuffix = options.IndexTableSuffix;
                o.StorageConnectionString = options.StorageConnectionString;
            });

            services.AddScoped<IAutoIndexRepository<AspNetUser>, AutoIndexRepository<AspNetUser>>();
            services.AddScoped<IAutoIndexRepository<AspNetUserClaim>, AutoIndexRepository<AspNetUserClaim>>();
            services.AddScoped<IAutoIndexRepository<AspNetUserLogin>, AutoIndexRepository<AspNetUserLogin>>();
            services.AddScoped<IAutoIndexRepository<AspNetUserRole>, AutoIndexRepository<AspNetUserRole>>();
            services.AddScoped<IAutoIndexRepository<AspNetRole>, AutoIndexRepository<AspNetRole>>();
            services.AddScoped<IAutoIndexRepository<AspNetRoleClaim>, AutoIndexRepository<AspNetRoleClaim>>();
            services.AddScoped<IAutoIndexRepository<AspNetUserToken>, AutoIndexRepository<AspNetUserToken>>();

            builder.AddUserStore<AzureTableStorageUserStore>();
            builder.AddRoleStore<AzureTableStorageRoleStore>();

            return builder;
        }
    }
}
