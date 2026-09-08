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
    
    public class AccessUserLevelService : IAccessUserLevelService
    {
        private readonly IAccessUserLevelRepository _repository;
        private readonly IMapper _mapper;

        public AccessUserLevelService(IAccessUserLevelRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        
        public async Task<PaginatedResult<AccessUserLevelDto>> GetPagedAccessUserLevelAllAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedAccessUserLevelAllAsync(parameters);
            return _mapper.Map<PaginatedResult<AccessUserLevelDto>>(pagedData);
        }

       
        public async Task<List<AccessUserLevelDto>> GetBySystemIdAsync(int systemid)
        {
            List<AccessUserLevel> entities = await _repository.GetBySystemIdAsync(systemid);
            return _mapper.Map<List<AccessUserLevelDto>>(entities);
        }

        
        public async Task<List<AccessUserLevelDto>> GetByUserAsync(int systemid, string ntlogin)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("NT login is required.", "NTLOGIN_REQUIRED")
                ]);

            List<AccessUserLevel> entities = await _repository.GetByUserAsync(systemid, ntlogin);
            return _mapper.Map<List<AccessUserLevelDto>>(entities);
        }

        
        public async Task<AccessUserLevelDto?> GetByIdAsync(int systemid, string ntlogin, int accesslevelid)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("NT login is required.", "NTLOGIN_REQUIRED")
                ]);

            AccessUserLevel? entity = await _repository.GetByIdAsync(systemid, ntlogin, accesslevelid);
            return entity is null ? null : _mapper.Map<AccessUserLevelDto>(entity);
        }

        
        public async Task<AccessUserLevelDto> CreateAsync(AccessUserLevelDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var errors = new List<BusinessValidationError>();
            if (dto.SystemId <= 0)
                errors.Add(new BusinessValidationError("A valid SystemId is required.", "SYSTEMID_REQUIRED"));
            if (dto.AccessLevelId <= 0)
                errors.Add(new BusinessValidationError("A valid AccessLevelId is required.", "ACCESSLEVELID_REQUIRED"));
            if (string.IsNullOrWhiteSpace(dto.NtLogin))
                errors.Add(new BusinessValidationError("NT login is required.", "NTLOGIN_REQUIRED"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            bool alreadyExists = await _repository.ExistsAsync(dto.SystemId, dto.NtLogin, dto.AccessLevelId);
            if (alreadyExists)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("User already exists. Please enter a unique User.", "ACCESS_USER_LEVEL_DUPLICATE")
                ]);

            AccessUserLevel entity = _mapper.Map<AccessUserLevel>(dto);
            AccessUserLevel created = await _repository.AddAsync(entity);
            return _mapper.Map<AccessUserLevelDto>(created);
        }

       
        public async Task<bool> DeleteAsync(int systemid, string ntlogin, int accesslevelid)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("NT login is required.", "NTLOGIN_REQUIRED")
                ]);

            bool exists = await _repository.ExistsAsync(systemid, ntlogin, accesslevelid);
            if (!exists)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"AccessUserLevel (systemid={systemid}, ntlogin='{ntlogin}', accesslevelid={accesslevelid}) was not found.",
                        "ACCESS_USER_LEVEL_NOT_FOUND")
                ]);

            return await _repository.DeleteAsync(systemid, ntlogin, accesslevelid);
        }

        public async Task<bool> ExistsAsync(int systemid, string ntlogin, int accesslevelid)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("NT login is required.", "NTLOGIN_REQUIRED")
                ]);

            return await _repository.ExistsAsync(systemid, ntlogin, accesslevelid);
        }
    }
}
