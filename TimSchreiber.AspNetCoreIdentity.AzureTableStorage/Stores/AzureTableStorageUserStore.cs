using Azure.Data.Tables;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Entities;
using TimSchreiber.AzureTableStorage.AutoIndex.Entities;
using TimSchreiber.AzureTableStorage.AutoIndex.Repositories;
using TimSchreiber.EnumerableExtensions;

namespace TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Stores
{
    public class AzureTableStorageUserStore :
        IUserStore<IdentityUser>,
        IUserPasswordStore<IdentityUser>,
        IUserEmailStore<IdentityUser>,
        IUserLoginStore<IdentityUser>,
        IUserRoleStore<IdentityUser>,
        IUserSecurityStampStore<IdentityUser>,
        IUserClaimStore<IdentityUser>,
        IUserAuthenticationTokenStore<IdentityUser>,
        IUserTwoFactorStore<IdentityUser>,
        IUserPhoneNumberStore<IdentityUser>,
        IUserLockoutStore<IdentityUser>,
        IQueryableUserStore<IdentityUser>
    {
        private readonly IAutoIndexRepository<AspNetUser> _userRepository;
        private readonly IAutoIndexRepository<AspNetUserClaim> _userClaimRepository;
        private readonly IAutoIndexRepository<AspNetUserLogin> _userLoginRepository;
        private readonly IAutoIndexRepository<AspNetUserRole> _userRoleRepository;
        private readonly IAutoIndexRepository<AspNetRole> _roleRepository;
        private readonly IAutoIndexRepository<AspNetUserToken> _userTokenRepository;

        public AzureTableStorageUserStore(IAutoIndexRepository<AspNetUser> userRepository,
            IAutoIndexRepository<AspNetUserClaim> userClaimRepository,
            IAutoIndexRepository<AspNetUserLogin> userLoginRepository,
            IAutoIndexRepository<AspNetUserRole> userRoleRepository,
            IAutoIndexRepository<AspNetRole> roleRepository,
            IAutoIndexRepository<AspNetUserToken> userTokenRepository)
        {
            _userRepository = userRepository;
            _userClaimRepository = userClaimRepository;
            _userLoginRepository = userLoginRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _userTokenRepository = userTokenRepository;
        }

        #region IQueryableUserStore<IdentityUser> Members
        public IQueryable<IdentityUser> Users => _userRepository.GetAsync()
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult()
            .Select(x => getIdentityUser(x))
            .AsQueryable();
        #endregion

        #region IUserStore<IdentityUser> Members
        public async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            try
            {
                if(cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                var aspNetUser = getAspNetUser(user);

                await _userRepository.AddAsync(aspNetUser);

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Code = ex.Message, Description = ex.Message });
            }
        }

        public async Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                var entity = await _userRepository.GetAsync(user.Id, user.Id);

                if (entity != null)
                {
                    await _userRepository.DeleteAsync(entity);
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Code = ex.Message, Description = ex.Message });
            }
        }

        public async Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var userEntity = await _userRepository.GetAsync(userId, userId);

            return getIdentityUser(userEntity);
        }

        public async Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(normalizedUserName))
                throw new ArgumentNullException(nameof(normalizedUserName));

            var userEntity = await _userRepository.GetSingleOrDefaultByIndexedPropertyAsync(nameof(AspNetUser.NormalizedUserName), normalizedUserName);

            return getIdentityUser(userEntity);
        }

        public Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.NormalizedUserName = normalizedName;

            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.UserName = userName;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                var entity = getAspNetUser(user);

                await _userRepository.UpdateAsync(entity);

                return IdentityResult.Success;

            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Code = ex.Message, Description = ex.Message });
            }
        }

        public void Dispose() { }
        #endregion

        #region IUserPasswordStore<ApplicationUser> Members
        public Task SetPasswordHashAsync(IdentityUser user, string passwordHash, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.PasswordHash = passwordHash;

            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
        }
        #endregion

        #region IUserEmailStore<ApplicationUser> Members
        public Task SetEmailAsync(IdentityUser user, string email, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.Email = email;

            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.EmailConfirmed = confirmed;

            return Task.CompletedTask;
        }

        public async Task<IdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(normalizedEmail))
                throw new ArgumentNullException(nameof(normalizedEmail));

            var userEntity = await _userRepository.GetFirstOrDefaultByIndexedPropertyAsync(nameof(AspNetUser.NormalizedEmail), normalizedEmail);

            return getIdentityUser(userEntity);
        }

        public Task<string> GetNormalizedEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(IdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.NormalizedEmail = normalizedEmail;

            return Task.CompletedTask;
        }
        #endregion

        #region IUserLoginStore<ApplicationUser> Members

        public async Task AddLoginAsync(IdentityUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (login == null)
                throw new ArgumentNullException(nameof(login));

            if (string.IsNullOrWhiteSpace(login.LoginProvider))
                throw new ArgumentNullException(nameof(login.LoginProvider));

            if (string.IsNullOrWhiteSpace(login.ProviderKey))
                throw new ArgumentNullException(nameof(login.ProviderKey));

            var loginEntity = new AspNetUserLogin(user.Id, login.LoginProvider, login.ProviderKey)
            {
                ProviderDisplayName = login.ProviderDisplayName
            };

            await _userLoginRepository.AddAsync(loginEntity);
        }

        public async Task RemoveLoginAsync(IdentityUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(loginProvider))
                throw new ArgumentNullException(nameof(loginProvider));

            if (string.IsNullOrWhiteSpace(providerKey))
                throw new ArgumentNullException(nameof(providerKey));

            var loginEntity = new AspNetUserLogin(user.Id, loginProvider, providerKey);

            await _userLoginRepository.DeleteAsync(loginEntity);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return (await _userLoginRepository.GetAsync(user.Id))
                .Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName))
                .ToList();
        }

        public async Task<IdentityUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(loginProvider))
                throw new ArgumentNullException(nameof(loginProvider));

            if (string.IsNullOrWhiteSpace(providerKey))
                throw new ArgumentNullException(nameof(providerKey));

            var login = await _userLoginRepository.GetFirstOrDefaultByIndexedPropertyAsync(nameof(AspNetUserLogin.RowKey), AspNetUserLogin.GetRowKey(loginProvider, providerKey));
            
            if (login == null)
                return default;

            var entity = await _userRepository.GetAsync(login.UserId, login.UserId);

            return getIdentityUser(entity);
        }
        #endregion

        #region IUserRoleStore<ApplicationUser> Members
        public async Task AddToRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException(nameof(roleName));

            var role = await _roleRepository.GetSingleOrDefaultByIndexedPropertyAsync(nameof(AspNetRole.NormalizedName), roleName);
            if (role == null)
                throw new ArgumentOutOfRangeException(nameof(roleName), "Role does not exist");

            var userRole = await _userRoleRepository.GetAsync(user.Id, role.Id);
            if (userRole != null)
                return;

            userRole = new AspNetUserRole(user.Id, role.Id);

            await _userRoleRepository.AddAsync(userRole);
        }

        public async Task RemoveFromRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException(nameof(roleName));

            var role = await _roleRepository.GetSingleOrDefaultByIndexedPropertyAsync(nameof(AspNetRole.NormalizedName), roleName);
            if (role == null)
                throw new ArgumentOutOfRangeException(nameof(roleName), "Role does not exist");

            var userRole = new AspNetUserRole(user.Id, role.Id);

            await _userRoleRepository.DeleteAsync(userRole);
        }

        public async Task<IList<string>> GetRolesAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var roleIds = (await _userRoleRepository.GetAsync(user.Id)).Select(x => x.RoleId);

            var roleNames = new List<string>();
            foreach(var chunk in roleIds.InChunksOf(25))
            {
                var filter = string.Join(" or ", chunk.Select(x => $"(PartitionKey eq '{x}' and RowKey eq '{x}')"));
                roleNames.AddRange((await _roleRepository.QueryAsync(filter)).Select(x => x.Name));
            }

            return (roleNames).ToList();
        }

        public async Task<bool> IsInRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException(nameof(roleName));

            var role = await _roleRepository.GetSingleOrDefaultByIndexedPropertyAsync(nameof(AspNetRole.NormalizedName), roleName);
            if (role == null)
                return false;

            var userRole = await _userRoleRepository.GetAsync(user.Id, role.Id);

            return userRole != null;
        }

        public async Task<IList<IdentityUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException(nameof(roleName));

            var role = await _roleRepository.GetSingleOrDefaultByIndexedPropertyAsync(nameof(AspNetRole.NormalizedName), roleName);
            if (role == null)
                return new List<IdentityUser>();

            var userIds = (await _userRoleRepository.GetByIndexedPropertyAsync(nameof(AspNetUserRole.RowKey), role.Id)).Select(x => x.UserId);

            return (await getIdentityUsersByIds(userIds)).ToList();
        }
        #endregion

        #region IUserSecurityStampStore<ApplicationUser> Members
        public Task SetSecurityStampAsync(IdentityUser user, string stamp, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.SecurityStamp = stamp;

            return Task.CompletedTask;
        }

        public Task<string> GetSecurityStampAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.SecurityStamp);
        }
        #endregion

        #region IUserClaimStore<ApplicationUser> Members
        public async Task<IList<Claim>> GetClaimsAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return (await _userClaimRepository.GetAsync(user.Id)).Select(x => new Claim(x.ClaimType, x.ClaimValue)).ToList();
        }

        public async Task AddClaimsAsync(IdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (claims == null)
                throw new ArgumentNullException(nameof(claims));

            var claimEntities = claims.Select(x => new AspNetUserClaim(user.Id, x.Type, x.Value));

            foreach(var claimEntity in claimEntities)
                await _userClaimRepository.AddAsync(claimEntity);
        }

        public async Task ReplaceClaimAsync(IdentityUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            if (newClaim == null)
                throw new ArgumentNullException(nameof(newClaim));

            var oldClaimEntity = await _userClaimRepository.GetAsync(user.Id, AspNetUserClaim.GetRowKey(claim.Type, claim.Value));
            var newClaimEntity = new AspNetUserClaim(user.Id, newClaim.Type, newClaim.Value);

            await _userClaimRepository.DeleteAsync(oldClaimEntity);
            await _userClaimRepository.AddAsync(newClaimEntity);
        }

        public async Task RemoveClaimsAsync(IdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (claims == null)
                throw new ArgumentNullException(nameof(claims));

            if (claims.Any())
            {
                var claimEntities = claims.Select(x => new AspNetUserClaim(user.Id, x.Type, x.Value));
                foreach (var claimEntity in claimEntities)
                    await _userClaimRepository.DeleteAsync(claimEntity);
            }
        }

        public async Task<IList<IdentityUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            var userIds = (await _userClaimRepository.GetByIndexedPropertyAsync(nameof(AspNetUserClaim.RowKey), AspNetUserClaim.GetRowKey(claim.Type, claim.Value)))
                .Select(x => x.UserId);

            return (await getIdentityUsersByIds(userIds)).ToList();
        }
        #endregion

        #region IUserAuthenticationTokenStore<ApplicationUser> Members
        public async Task SetTokenAsync(IdentityUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(loginProvider))
                throw new ArgumentNullException(nameof(loginProvider));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var entity = await _userTokenRepository.GetAsync(user.Id, AspNetUserToken.GetRowKey(loginProvider, name));

            if (entity != null)
            {
                entity.Value = value;
                await _userTokenRepository.UpdateAsync(entity);
            }
            else
            {
                entity = new AspNetUserToken(user.Id, loginProvider, name) { Value = value };
                await _userTokenRepository.AddAsync(entity);
            }
        }

        public async Task RemoveTokenAsync(IdentityUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(loginProvider))
                throw new ArgumentNullException(nameof(loginProvider));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var entity = await _userTokenRepository.GetAsync(user.Id, AspNetUserToken.GetRowKey(loginProvider, name));

            if (entity != null)
                await _userTokenRepository.DeleteAsync(entity);
        }

        public async Task<string> GetTokenAsync(IdentityUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(loginProvider))
                throw new ArgumentNullException(nameof(loginProvider));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var entity = await _userTokenRepository.GetAsync(user.Id, AspNetUserToken.GetRowKey(loginProvider, name));

            return entity?.UserId;
        }
        #endregion

        #region IUserTwoFactorStore<ApplicationUser> Members
        public Task SetTwoFactorEnabledAsync(IdentityUser user, bool enabled, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.TwoFactorEnabled = enabled;

            return Task.CompletedTask;
        }

        public Task<bool> GetTwoFactorEnabledAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.TwoFactorEnabled);
        }
        #endregion

        #region IUserPhoneNumberStore<ApplicationUser> Members
        public Task SetPhoneNumberAsync(IdentityUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.PhoneNumber = phoneNumber;

            return Task.CompletedTask;
        }

        public Task<string> GetPhoneNumberAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.PhoneNumberConfirmed = confirmed;

            return Task.CompletedTask;
        }
        #endregion

        #region IUserLockoutStore<ApplicationUser> Members
        public Task<DateTimeOffset?> GetLockoutEndDateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.LockoutEnd);
        }

        public Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.LockoutEnd = lockoutEnd;

            return Task.CompletedTask;
        }

        public Task<int> IncrementAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(++user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.AccessFailedCount = 0;

            return Task.CompletedTask;
        }

        public Task<int> GetAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(IdentityUser user, bool enabled, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.LockoutEnabled = enabled;

            return Task.CompletedTask;
        }
        #endregion

        #region Private Methods
        private IdentityUser getIdentityUser(AspNetUser entity)
        {
            if (entity == default)
                return default;

            var result = new IdentityUser();

            populateIdentityUser(result, entity);

            return result;
        }

        private void populateIdentityUser(IdentityUser identityUser, AspNetUser entity)
        {
            identityUser.AccessFailedCount = entity.AccessFailedCount;
            identityUser.ConcurrencyStamp = entity.ConcurrencyStamp;
            identityUser.Email = entity.Email;
            identityUser.EmailConfirmed = entity.EmailConfirmed;
            identityUser.Id = entity.Id;
            identityUser.LockoutEnabled = entity.LockoutEnabled;
            identityUser.LockoutEnd = entity.LockoutEnd;
            identityUser.NormalizedEmail = entity.NormalizedEmail;
            identityUser.NormalizedUserName = entity.NormalizedUserName;
            identityUser.PasswordHash = entity.PasswordHash;
            identityUser.PhoneNumber = entity.PhoneNumber;
            identityUser.PhoneNumberConfirmed = entity.PhoneNumberConfirmed;
            identityUser.SecurityStamp = entity.SecurityStamp;
            identityUser.TwoFactorEnabled = entity.TwoFactorEnabled;
            identityUser.UserName = entity.UserName;
        }

        private AspNetUser getAspNetUser(IdentityUser identityUser)
        {
            if (identityUser == default)
                return default;

            var result = new AspNetUser();

            populateAspNetUser(result, identityUser);

            return result;
        }

        private void populateAspNetUser(AspNetUser entity, IdentityUser identityUser)
        {
            entity.AccessFailedCount = identityUser.AccessFailedCount;
            entity.ConcurrencyStamp = identityUser.ConcurrencyStamp;
            entity.Email = identityUser.Email;
            entity.EmailConfirmed = identityUser.EmailConfirmed;
            entity.PartitionKey = identityUser.Id;
            entity.RowKey = identityUser.Id;
            entity.LockoutEnabled = identityUser.LockoutEnabled;
            entity.LockoutEnd = identityUser.LockoutEnd;
            entity.NormalizedEmail = identityUser.NormalizedEmail;
            entity.NormalizedUserName = identityUser.NormalizedUserName;
            entity.PasswordHash = identityUser.PasswordHash;
            entity.PhoneNumber = identityUser.PhoneNumber;
            entity.PhoneNumberConfirmed = identityUser.PhoneNumberConfirmed;
            entity.SecurityStamp = identityUser.SecurityStamp;
            entity.TwoFactorEnabled = identityUser.TwoFactorEnabled;
            entity.UserName = identityUser.UserName;
        }
        private async Task<IEnumerable<IdentityUser>> getIdentityUsersByIds(IEnumerable<string> userIds)
        {
            var result = new List<IdentityUser>();

            foreach (var chunk in userIds.InChunksOf(25))
            {
                var filter = string.Join(" or ", chunk.Select(x => $"(PartitionKey eq '{x}' and RowKey eq '{x}')"));
                result.AddRange((await _userRepository.QueryAsync(filter)).Select(x => getIdentityUser(x)));
            }

            return result;
        }
        #endregion
    }
}
