using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using MapsterMapper;

namespace Apha.PIMS.Application.Services
{
    public class AccessLevelService : IAccessLevelService
    {
        private readonly IAccessLevelRepository _repository;
        private readonly IMapper _mapper;

        public AccessLevelService(IAccessLevelRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        
        public async Task<List<AccessLevelDto>> GetAllAsync()
        {
            List<AccessLevel> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<AccessLevelDto>>(entities);
        }

       
        public async Task<List<AccessLevelDto>> GetBySystemIdAsync(int systemid)
        {
            List<AccessLevel> entities = await _repository.GetBySystemIdAsync(systemid);
            return _mapper.Map<List<AccessLevelDto>>(entities);
        }

        
        public async Task<AccessLevelDto?> GetByIdAsync(int systemid, int accesslevelid)
        {
            AccessLevel? entity = await _repository.GetByIdAsync(systemid, accesslevelid);
            return entity is null ? null : _mapper.Map<AccessLevelDto>(entity);
        }

        
        public async Task<AccessLevelDto> CreateAsync(AccessLevelDto dto)
        {
            if (dto is null)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError("Access level data is required.", "ACCESS_LEVEL_REQUIRED")
                };
                throw new BusinessValidationErrorException(errors);
            }

            bool alreadyExists = await _repository.ExistsAsync(dto.SystemId, dto.AccessLevelId);
            if (alreadyExists)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"AccessLevel (systemid={dto.SystemId}, accesslevelid={dto.AccessLevelId}) already exists.",
                        "ACCESS_LEVEL_ALREADY_EXISTS")
                };
                throw new BusinessValidationErrorException(errors);
            }

            AccessLevel entity = _mapper.Map<AccessLevel>(dto);
            AccessLevel created = await _repository.AddAsync(entity);
            return _mapper.Map<AccessLevelDto>(created);
        }

        public async Task<AccessLevelDto> UpdateAsync(AccessLevelDto dto)
        {
            if (dto is null)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError("Access level data is required.", "ACCESS_LEVEL_REQUIRED")
                };
                throw new BusinessValidationErrorException(errors);
            }

            bool exists = await _repository.ExistsAsync(dto.SystemId, dto.AccessLevelId);
            if (!exists)
                throw new KeyNotFoundException(
                    $"AccessLevel (systemid={dto.SystemId}, accesslevelid={dto.AccessLevelId}) was not found.");

            AccessLevel entity = _mapper.Map<AccessLevel>(dto);
            AccessLevel updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<AccessLevelDto>(updated);
        }

        
        public async Task DeleteAsync(int systemid, int accesslevelid)
        {
            bool exists = await _repository.ExistsAsync(systemid, accesslevelid);
            if (!exists)
                throw new KeyNotFoundException(
                    $"AccessLevel (systemid={systemid}, accesslevelid={accesslevelid}) was not found.");

            await _repository.DeleteAsync(systemid, accesslevelid);
        }

        public async Task<bool> ExistsAsync(int systemid, int accesslevelid)
        {
            return await _repository.ExistsAsync(systemid, accesslevelid);
        }
    }
}
