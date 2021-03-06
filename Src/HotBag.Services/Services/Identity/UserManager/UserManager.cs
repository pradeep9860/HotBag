﻿using System;
using System.Linq;
using System.Threading.Tasks;
using HotBag.AppUser;
using HotBag.Data;
using HotBag.DI.Base;
using HotBag.EntityFrameworkCore.Repository;
using HotBag.Identity.UserManagerResultDto;
using HotBag.Security.PasswordHasher;
using HotBag.Services.RepositoryFactory;
using Microsoft.EntityFrameworkCore;

namespace HotBag.Services.Identity
{
    public class UserManager : IUserManager, ITransientDependencies
    {
        private readonly IBaseRepository<HotBagUser, Guid> _userRepository;
        private readonly IBaseRepository<HotBagApplicationModule, long> _applicationModuleRepository;
        private readonly IBaseRepository<HotBagApplicationModulePermissionLevel, long> _roleApplicationModulePermissionLevelRepository;
        private readonly IBaseRepository<HotBagRoleApplicationModule, long> _roleApplicationModuleRepository;
        private readonly IBaseRepository<HotBagRole, long> _roleRepository;
        private readonly IBaseRepository<HotBagUserRoles, long> _userRoleRepository;

        public UserManager(
            IRepositoryFactory<HotBagUser, Guid> userRepository,
            IRepositoryFactory<HotBagUserRoles, long> userRoleRepository,
            IRepositoryFactory<HotBagRole, long> roleRepository,
            IRepositoryFactory<HotBagRoleApplicationModule, long> roleApplicationModuleRepository,
            IRepositoryFactory<HotBagApplicationModulePermissionLevel, long> roleApplicationModulePermissionLevelRepository,
            IRepositoryFactory<HotBagApplicationModule, long> applicationModuleRepository 
            )
        {
            _userRepository = userRepository.GetRepository();
            _applicationModuleRepository = applicationModuleRepository.GetRepository();
            this._roleApplicationModulePermissionLevelRepository = roleApplicationModulePermissionLevelRepository.GetRepository();
            this._roleApplicationModuleRepository = roleApplicationModuleRepository.GetRepository();
            this._roleRepository = roleRepository.GetRepository();
            this._userRoleRepository = userRoleRepository.GetRepository();
        } 

        public Task<bool> AddUserPermissions()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangeUserPasswordAsync(string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangeUserPasswordAsync(HotBagUser user, string newPassword)
        {
            throw new NotImplementedException();
        }

        public async Task<LoginResultDto> GetLoginAsync(string userNameOrEmail, string password)
        {
            var a = _userRepository.GetAll().ToList();
            var user = _userRepository.GetAll().FirstOrDefault(x =>
                 x.Email.ToLower() == userNameOrEmail.ToLower()
                 || x.Username.ToLower() == userNameOrEmail.ToLower()
            );

            return await GetLoginAsync(user, password);
        }

        public async Task<LoginResultDto> GetLoginAsync(HotBagUser user, string password)
        {
            var result = new LoginResultDto
            {
                IsLoginSuccess = false,
                LoginErrorMessage = "Unknown Error",
                User = null
            }; 

            if (user == null)
            {
                result.LoginErrorMessage = "Username or Password is invalid";
                return await Task.FromResult(result);
            } 

            if (!PasswordHasher.VerifyHashedPassword(user.HashedPassword, password))
            {
                result.LoginErrorMessage = "Username or Password is invalid";
                return await Task.FromResult(result); ;
            }

            var userStatusResult = CheckUserStatus(user);
            if (userStatusResult.isSuccess)
            {
                result.IsLoginSuccess = true;
                result.LoginErrorMessage = string.Empty;
                result.User = user;
                return await Task.FromResult(result); ;
            }
            else
            {
                result.IsLoginSuccess = false;
                result.LoginErrorMessage = userStatusResult.errorMessage;
                result.User = null;
                return await Task.FromResult(result); ;
            }
        }

        private (bool isSuccess, string errorMessage) CheckUserStatus(HotBagUser user)
        {
            var result = (false, "Unable to find result");
            switch (user.Status)
            {
                case UserStatus.Active:
                    result = (true, "User status is active");
                    break;
                case UserStatus.InActive:
                    result = (false, "User is not Active, Please contact with admin");
                    break;
                case UserStatus.EmailNotConfirmed:
                    result = (false, "Email not Confirmed, Please confirm your email by clicking link on email");
                    break;
                case UserStatus.Suspended:
                    result = (false, "User is suspended");
                    break;
                case UserStatus.PasswordExpired:
                    result = (false, "Password is expired, Please reset your password");
                    break;
                case UserStatus.PasswordNotCreated:
                    result = (false, "Password Not Created");
                    break;
                default:
                    break;
            }

            return result;
        }

        public Task<string> GetResetUserPasswordTokenAsync(HotBagUser user)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetResetUserPasswordTokenAsync(string emailAddress)
        {
            throw new NotImplementedException();
        }

        public async Task<HotBagUser> GetUserByEmailAsync(string emailAddress)
        {
            return await _userRepository.GetAll().FirstOrDefaultAsync(x =>
              x.Email.ToLower() == emailAddress.ToLower()
          );
        }

        public async Task<HotBagUser> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetAll().FirstOrDefaultAsync(x =>
               x.Username.ToLower() == username.ToLower()
           );
        }

        public async Task<HotBagUser> GetUserByUsernameOrEmailAsync(string usernameOrEmailAddress)
        {
            var user = await _userRepository.GetAll().FirstOrDefaultAsync(x =>
                x.Email.ToLower() == usernameOrEmailAddress.ToLower()
                || x.Username.ToLower() == usernameOrEmailAddress.ToLower()
           );
            return user;
        }

        public Task<bool> RemoveAllPermissions(HotBagUser user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveAllPermissions(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ResetUserPasswordAsync(string token, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreatePasswordAsync(string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreatePasswordAsync(HotBagUser user, string newPassword)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetAllPermissions(HotBagUser user)
        {
            var permissions = await (from userRole in _userRoleRepository.GetAll().Where(x => x.UserId == user.Id)
                                     join role in _roleRepository.GetAll() on userRole.RoleIdId equals role.Id
                                     join roleApplicationModule in _roleApplicationModuleRepository.GetAll() on role.Id equals roleApplicationModule.RoleId
                                     join applicatonModule in _applicationModuleRepository.GetAll() on roleApplicationModule.ApplicationModuleId equals applicatonModule.Id
                                     join applicationModulePermissionLevel in _roleApplicationModulePermissionLevelRepository.GetAll()
                                     on roleApplicationModule.ApplicationModulePermissionLevelId equals applicationModulePermissionLevel.Id
                                     select new
                                     { 
                                         PermissionName = $"{applicatonModule.ModuleName}.{applicationModulePermissionLevel.PermissionLevel.ToString()}", 
                                     }).Distinct().ToListAsync();

            var per = string.Join(",", permissions.Select(x => x.PermissionName));
            return per;
        }
    }
}
