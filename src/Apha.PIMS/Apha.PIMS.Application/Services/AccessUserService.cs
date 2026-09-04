using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
   
    public class AccessUserService : IAccessUserService
    {
        private readonly IAccessUserRepository _repository;
        private readonly IMapper _mapper;

        public AccessUserService(IAccessUserRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PaginatedResult<AccessUserDto>> GetPagedAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedAsync(parameters);
            return _mapper.Map<PaginatedResult<AccessUserDto>>(pagedData);
        }

        
        public async Task<List<AccessUserDto>> GetAllAsync()
        {
            List<AccessUser> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<AccessUserDto>>(entities);
        }

        
        public async Task<List<AccessUserDto>> GetBySystemIdAsync(int systemid)
        {
            List<AccessUser> entities = await _repository.GetBySystemIdAsync(systemid);
            return _mapper.Map<List<AccessUserDto>>(entities);
        }

        
        public async Task<List<AccessUserDto>> GetByNtLoginAsync(string ntlogin)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("NT login is required.", "NTLOGIN_REQUIRED")
                ]);

            List<AccessUser> entities = await _repository.GetByNtLoginAsync(ntlogin);
            return _mapper.Map<List<AccessUserDto>>(entities);
        }

        
        public async Task<AccessUserDto?> GetByIdAsync(int systemid, string ntlogin)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("NT login is required.", "NTLOGIN_REQUIRED")
                ]);

            AccessUser? entity = await _repository.GetByIdAsync(systemid, ntlogin);
            return entity is null ? null : _mapper.Map<AccessUserDto>(entity);
        }

       
        public async Task<AccessUserDto> CreateAsync(AccessUserDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            dto.NtLogin = string.IsNullOrWhiteSpace(dto.NtLogin) ? null : dto.NtLogin.Trim();
            dto.UserName = string.IsNullOrWhiteSpace(dto.UserName) ? null : dto.UserName.Trim();
            dto.UserEmail = string.IsNullOrWhiteSpace(dto.UserEmail) ? null : dto.UserEmail.Trim();

            if (string.IsNullOrWhiteSpace(dto.NtLogin))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("NT login is required.", "NTLOGIN_REQUIRED")
                ]);

            var usersForSystem = await _repository.GetBySystemIdAsync(dto.SystemId);

            bool duplicateNtLoginExists = usersForSystem.Any(u =>
                StringEqualsTrimmedIgnoreCase(u.NtLogin, dto.NtLogin));

            bool duplicateEmailExists = !string.IsNullOrWhiteSpace(dto.UserEmail)
                && usersForSystem.Any(u =>
                    !string.IsNullOrWhiteSpace(u.UserEmail)
                    && StringEqualsTrimmedIgnoreCase(u.UserEmail, dto.UserEmail));

            if (duplicateNtLoginExists && duplicateEmailExists)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("NTLogin and UserEmail already exist. Please enter unique values.", "ACCESS_USER_DUPLICATE_NTLOGIN_EMAIL")
                ]);

            if (duplicateNtLoginExists)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("NTLogin already exists. Please enter a unique NTLogin.", "ACCESS_USER_DUPLICATE_NTLOGIN")
                ]);

            if (duplicateEmailExists)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("UserEmail already exists.", "ACCESS_USER_DUPLICATE_EMAIL")
                ]);

            AccessUser entity = _mapper.Map<AccessUser>(dto);
            AccessUser created = await _repository.AddAsync(entity);
            return _mapper.Map<AccessUserDto>(created);
        }


        public async Task<AccessUserDto> UpdateAsync(AccessUserDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            dto.NtLogin = string.IsNullOrWhiteSpace(dto.NtLogin) ? null : dto.NtLogin.Trim();
            dto.UserName = string.IsNullOrWhiteSpace(dto.UserName) ? null : dto.UserName.Trim();
            dto.UserEmail = string.IsNullOrWhiteSpace(dto.UserEmail) ? null : dto.UserEmail.Trim();

            if (string.IsNullOrWhiteSpace(dto.NtLogin))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("NT login is required.", "NTLOGIN_REQUIRED")
                ]);

            bool exists = await _repository.ExistsAsync(dto.SystemId, dto.NtLogin);
            if (!exists)
                throw new KeyNotFoundException(
                    $"AccessUser (systemid={dto.SystemId}, ntlogin='{dto.NtLogin}') was not found.");

            var usersForSystem = await _repository.GetBySystemIdAsync(dto.SystemId);

            if (!string.IsNullOrWhiteSpace(dto.UserEmail))
            {
                bool duplicateEmailExists = usersForSystem.Any(u =>
                    !string.IsNullOrWhiteSpace(u.UserEmail)
                    && StringEqualsTrimmedIgnoreCase(u.UserEmail, dto.UserEmail)

                    && !StringEqualsTrimmedIgnoreCase(u.NtLogin, dto.NtLogin));

                if (duplicateEmailExists)
                    throw new BusinessValidationErrorException(
                    [
                        new BusinessValidationError("UserEmail already exists.", "ACCESS_USER_DUPLICATE_EMAIL")
                    ]);
            }

            AccessUser entity = _mapper.Map<AccessUser>(dto);
            AccessUser updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<AccessUserDto>(updated);
        }

        
        public async Task<bool> DeleteAsync(int systemid, string ntlogin)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("NT login is required.", "NTLOGIN_REQUIRED")
                ]);

            bool exists = await _repository.ExistsAsync(systemid, ntlogin);
            if (!exists)
                throw new KeyNotFoundException(
                    $"AccessUser (systemid={systemid}, ntlogin='{ntlogin}') was not found.");

            return await _repository.DeleteAsync(systemid, ntlogin);
        }

        public async Task<bool> ExistsAsync(int systemid, string ntlogin)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("NT login is required.", "NTLOGIN_REQUIRED")
                ]);

            return await _repository.ExistsAsync(systemid, ntlogin);
        }

        private static bool StringEqualsTrimmedIgnoreCase(string? left, string? right)
        {
            if (left is null || right is null)
            {
                return false;
            }

            return string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
