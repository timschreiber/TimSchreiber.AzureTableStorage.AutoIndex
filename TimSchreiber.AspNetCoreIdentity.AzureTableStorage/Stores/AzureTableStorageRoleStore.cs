using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Entities;
using TimSchreiber.AzureTableStorage.AutoIndex.Repositories;

namespace TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Stores
{
    public class AzureTableStorageRoleStore : IRoleStore<IdentityRole>, IRoleClaimStore<IdentityRole>
    {
        private readonly IAutoIndexRepository<AspNetRole> _roleRepository;
        private readonly IAutoIndexRepository<AspNetRoleClaim> _roleClaimRepository;

        public AzureTableStorageRoleStore(IAutoIndexRepository<AspNetRole> roleRepository, IAutoIndexRepository<AspNetRoleClaim> roleClaimRepository)
        {
            _roleRepository = roleRepository;
            _roleClaimRepository = roleClaimRepository;
        }

        public async Task AddClaimAsync(IdentityRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            if(cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            var entity = new AspNetRoleClaim(role.Id, claim.Type, claim.Value);

            await _roleClaimRepository.AddAsync(entity);
        }

        public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            try
            {
                if(cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (role == null)
                    throw new ArgumentNullException(nameof(role));

                var entity = getRoleEntity(role);

                await _roleRepository.AddAsync(entity);

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Code = ex.Message, Description = ex.Message });
            }
        }

        public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (role == null)
                    throw new ArgumentNullException(nameof(role));

                var entity = await _roleRepository.GetAsync(role.Id, role.Id);

                if (entity != null)
                    await _roleRepository.DeleteAsync(entity);

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Code = ex.Message, Description = ex.Message });
            }
        }

        public void Dispose() { }

        public async Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            var entity = await _roleRepository.GetAsync(roleId, roleId);

            if (entity == null)
                return null;

            return getIdentityRole(entity);
        }

        public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            var entity = await _roleRepository.GetSingleOrDefaultByIndexedPropertyAsync(nameof(AspNetRole.NormalizedName), normalizedRoleName);

            if (entity == null)
                return null;

            return getIdentityRole(entity);
        }

        public async Task<IList<Claim>> GetClaimsAsync(IdentityRole role, CancellationToken cancellationToken = default)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            return (await _roleClaimRepository.GetAsync(role.Id))
                .Select(x => new Claim(x.ClaimType, x.ClaimValue)).ToList();
        }

        public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            if(cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            if(cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            if(cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.Name);
        }

        public async Task RemoveClaimAsync(IdentityRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            if(cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            var entity = new AspNetRoleClaim(role.Id, claim.Type, claim.Value);

            await _roleClaimRepository.DeleteAsync(entity);
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
        {
            if(cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            role.NormalizedName = normalizedName;

            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
        {
            if(cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            role.Name = roleName;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            try
            {
                if(cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (role == null)
                    throw new ArgumentNullException(nameof(role));

                var entity = getRoleEntity(role);

                await _roleRepository.UpdateAsync(entity);

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Code = ex.Message, Description = ex.Message });
            }
        }

        #region Private Methods
        private AspNetRole getRoleEntity(IdentityRole value)
        {
            return value == null
                ? default(AspNetRole)
                : new AspNetRole(value.Id)
                {
                    ConcurrencyStamp = value.ConcurrencyStamp,
                    Name = value.Name,
                    NormalizedName = value.NormalizedName
                };
        }

        private IdentityRole getIdentityRole(AspNetRole value)
        {
            return value == null
                ? default(IdentityRole)
                : new IdentityRole
                {
                    ConcurrencyStamp = value.ConcurrencyStamp,
                    Id = value.Id,
                    Name = value.Name,
                    NormalizedName = value.NormalizedName
                };
        }
        #endregion
    }
}
