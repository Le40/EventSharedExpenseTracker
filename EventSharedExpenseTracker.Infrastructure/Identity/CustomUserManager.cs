using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Infrastructure.Identity
{
    public class CustomUserManager<TUser> : UserManager<TUser> where TUser : ApplicationUser
    {
        private readonly IUserRepository _customUserRepository;

        public CustomUserManager(
            IUserStore<TUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUser> passwordHasher,
            IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<TUser>> logger,
            IUserRepository customUserRepository)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _customUserRepository = customUserRepository;
        }

        public override async Task<IdentityResult> CreateAsync(TUser user, string password)
        {
            var customUser = new CustomUser
            {
                CustomUserName = user.CustomUserName,
                //ApplicationUserId = user.Id
            };

            _customUserRepository.Create(customUser);
            user.CustomUser = customUser;

            return await base.CreateAsync(user, password);
        }

        public override async Task<IdentityResult> DeleteAsync(TUser user)
        {
            var customUser = await _customUserRepository.GetByIdAsync(user.CustomUserId);
            if (customUser != null)
            {
                _customUserRepository.Delete(customUser);
            }

            return await base.DeleteAsync(user);
        }

        public override async Task<IdentityResult> UpdateAsync(TUser user)
        {
            var customUser = await _customUserRepository.GetByIdAsync(user.CustomUserId);
            if (customUser != null)
            {
                customUser.CustomUserName = user.CustomUserName;
                _customUserRepository.Update(customUser);
            }

            return await base.UpdateAsync(user);
        }
    }
}
