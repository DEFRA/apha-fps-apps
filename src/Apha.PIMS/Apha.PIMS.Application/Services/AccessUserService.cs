using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    // TRANSFORMENGINE: service orchestrates IAccessUserRepository; composite PK (systemid, ntlogin)
    public class AccessUserService : IAccessUserService
    {
        private readonly IAccessUserRepository _repository;
        private readonly IMapper _mapper;

        public AccessUserService(IAccessUserRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: returns all users across all systems — admin use only
        public async Task<List<AccessUserDto>> GetAllAsync()
        {
            List<AccessUser> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<AccessUserDto>>(entities);
        }

        // TRANSFORMENGINE: PIMS system-scoped listing — used for admin users grid filtered by system
        public async Task<List<AccessUserDto>> GetBySystemIdAsync(int systemid)
        {
            List<AccessUser> entities = await _repository.GetBySystemIdAsync(systemid);
            return _mapper.Map<List<AccessUserDto>>(entities);
        }

        // TRANSFORMENGINE: cross-system lookup by NT login — used for user search/autocomplete
        public async Task<List<AccessUserDto>> GetByNtLoginAsync(string ntlogin)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new ArgumentException("NT login is required.", nameof(ntlogin));

            List<AccessUser> entities = await _repository.GetByNtLoginAsync(ntlogin);
            return _mapper.Map<List<AccessUserDto>>(entities);
        }

        // TRANSFORMENGINE: returns nullable — controller maps null to 404; composite PK lookup
        public async Task<AccessUserDto?> GetByIdAsync(int systemid, string ntlogin)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new ArgumentException("NT login is required.", nameof(ntlogin));

            AccessUser? entity = await _repository.GetByIdAsync(systemid, ntlogin);
            return entity is null ? null : _mapper.Map<AccessUserDto>(entity);
        }

        // TRANSFORMENGINE: duplicate-user guard — throws InvalidOperationException if user already exists for this system+login
        public async Task<AccessUserDto> CreateAsync(AccessUserDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.NtLogin))
                throw new ArgumentException("NT login is required.", nameof(dto));

            bool alreadyExists = await _repository.ExistsAsync(dto.SystemId, dto.NtLogin);
            if (alreadyExists)
                throw new InvalidOperationException(
                    $"AccessUser (systemid={dto.SystemId}, ntlogin='{dto.NtLogin}') already exists.");

            AccessUser entity = _mapper.Map<AccessUser>(dto);
            AccessUser created = await _repository.AddAsync(entity);
            return _mapper.Map<AccessUserDto>(created);
        }

        // TRANSFORMENGINE: validate existence before update — throws KeyNotFoundException if not found
        public async Task<AccessUserDto> UpdateAsync(AccessUserDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.NtLogin))
                throw new ArgumentException("NT login is required.", nameof(dto));

            bool exists = await _repository.ExistsAsync(dto.SystemId, dto.NtLogin);
            if (!exists)
                throw new KeyNotFoundException(
                    $"AccessUser (systemid={dto.SystemId}, ntlogin='{dto.NtLogin}') was not found.");

            AccessUser entity = _mapper.Map<AccessUser>(dto);
            AccessUser updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<AccessUserDto>(updated);
        }

        // TRANSFORMENGINE: throws KeyNotFoundException if not found before delete
        public async Task<bool> DeleteAsync(int systemid, string ntlogin)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new ArgumentException("NT login is required.", nameof(ntlogin));

            bool exists = await _repository.ExistsAsync(systemid, ntlogin);
            if (!exists)
                throw new KeyNotFoundException(
                    $"AccessUser (systemid={systemid}, ntlogin='{ntlogin}') was not found.");

            return await _repository.DeleteAsync(systemid, ntlogin);
        }

        public async Task<bool> ExistsAsync(int systemid, string ntlogin)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new ArgumentException("NT login is required.", nameof(ntlogin));

            return await _repository.ExistsAsync(systemid, ntlogin);
        }
    }
}
