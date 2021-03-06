﻿
using HotBag.AppUser;
using HotBag.AppUserDto;
using HotBag.AutoMaper;
using HotBag.Data;
using HotBag.DI.Base;
using HotBag.EntityFrameworkCore.Repository;
using HotBag.EntityFrameworkCore.UnitOfWork;
using HotBag.ResultWrapper.ResponseModel;
using HotBag.Security.PasswordHasher;
using HotBag.Services.RepositoryFactory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotBag.Services.Identity
{
    public class RoleService : IRoleService, ITransientDependencies
    {
        private readonly IBaseRepository<HotBagRole, long> _repository;
        private readonly IBaseRepository<HotBagPasswordHistoryLog, long> _passwordHistoryLogRepository;
        private readonly IBaseRepository<HotBagUserStatusLog, long> _userStatusLogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IObjectMapper _objectMapper;
        public RoleService(
            IRepositoryFactory<HotBagRole, long> repository,
            IRepositoryFactory<HotBagPasswordHistoryLog, long> passwordHistoryLogRepository,
            IRepositoryFactory<HotBagUserStatusLog, long> userStatusLogRepository,
            IUnitOfWork unitOfWork, IObjectMapper objectMapper)
        {
            _repository = repository.GetRepository();
            this._passwordHistoryLogRepository = passwordHistoryLogRepository.GetRepository();
            this._userStatusLogRepository = userStatusLogRepository.GetRepository();
            _unitOfWork = unitOfWork;
            _objectMapper = objectMapper;
        }

        public async Task Delete(long id)
        {
            await this._repository.DeleteAsync(id);
        }

        public async Task<ResultDto<HotBagRoleDto>> Get(long id)
        {
            var result = await _repository.GetAsync(id);
            var res = _objectMapper.Map<HotBagRoleDto>(result);
            return new ResultDto<HotBagRoleDto>(res);
        }

        public async Task<ListResultDto<HotBagRoleDto>> GetAll(string searchText)
        {
            var result = _repository.GetAll();

            if (!string.IsNullOrEmpty(searchText))
            {
                result = result.Where(x => x.RoleName.ToLower().Trim().Contains(searchText.ToLower().Trim()));
            }

            //var totalCount = await result.CountAsync();
            var finalResult = result
                .Select(x => new HotBagRoleDto
                {
                    Id = x.Id,
                    RoleName = x.RoleName 
                });

            return new ListResultDto<HotBagRoleDto>(await Task.FromResult(finalResult.ToList()), "Roles");
        }

        public async Task<PagedResultDto<HotBagRoleDto>> GetAllPaged(int skip, int maxResultCount, string searchText)
        {
            var result = _repository.GetAll();

            if (!string.IsNullOrEmpty(searchText))
            {
                result = result.Where(x => x.RoleName.ToLower().Trim().Contains(searchText.ToLower().Trim()));
            }

            var totalCount = result.Count();
            var finalResult = result
                .Select(x => new HotBagRoleDto
                {
                    Id = x.Id,
                    RoleName = x.RoleName
                })
                .Skip(skip)
                .Take(maxResultCount); 

            return new PagedResultDto<HotBagRoleDto>(totalCount, await Task.FromResult(finalResult.ToList()), skip + maxResultCount < totalCount, "Total Data With summary");
        }

        public async Task<ResultDto<int>> GetCount()
        {
            return new ResultDto<int>(await _repository.CountAsync());
        }

        public async Task<ResultDto<HotBagRoleDto>> Save(HotBagRoleDto entity)
        {
            var saveModel = _objectMapper.Map<HotBagRole>(entity); 
            var result = await _repository.InsertAsync(saveModel); 
            await _unitOfWork.SaveChangesAsync();
            var res = _objectMapper.Map<HotBagRoleDto>(result);
            return new ResultDto<HotBagRoleDto>(res);
        }

        public async Task<ResultDto<HotBagRoleDto>> Update(HotBagRoleDto entity)
        {
            var updateModel = _objectMapper.Map<HotBagRole>(entity);
            var result = await _repository.UpdateAsync(updateModel);
            await _unitOfWork.SaveChangesAsync();
            var res = _objectMapper.Map<HotBagRoleDto>(result);
            return new ResultDto<HotBagRoleDto>(res);
        }
    }
}
