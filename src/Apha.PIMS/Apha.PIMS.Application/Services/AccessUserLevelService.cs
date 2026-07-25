using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    // TRANSFORMENGINE: service orchestrates IAccessUserLevelRepository; three-column composite PK (systemid, ntlogin, accesslevelid); no update — assignment table
    public class AccessUserLevelService : IAccessUserLevelService
    {
        private readonly IAccessUserLevelRepository _repository;
        private readonly IMapper _mapper;

        public AccessUserLevelService(IAccessUserLevelRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: paged access user levels with repository-level filter/sort/paging
        public async Task<PaginatedResult<AccessUserLevelDto>> GetPagedAccessUserLevelAllAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedAccessUserLevelAllAsync(parameters);
            return _mapper.Map<PaginatedResult<AccessUserLevelDto>>(pagedData);
        }

        // TRANSFORMENGINE: returns all user-level assignments for a given system
        public async Task<List<AccessUserLevelDto>> GetBySystemIdAsync(int systemid)
        {
            List<AccessUserLevel> entities = await _repository.GetBySystemIdAsync(systemid);
            return _mapper.Map<List<AccessUserLevelDto>>(entities);
        }

        // TRANSFORMENGINE: returns all level assignments for a given user — used for user access management grid
        public async Task<List<AccessUserLevelDto>> GetByUserAsync(int systemid, string ntlogin)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new ArgumentException("NT login is required.");

            List<AccessUserLevel> entities = await _repository.GetByUserAsync(systemid, ntlogin);
            return _mapper.Map<List<AccessUserLevelDto>>(entities);
        }

        // TRANSFORMENGINE: returns nullable — controller maps null to 404; three-column composite PK lookup
        public async Task<AccessUserLevelDto?> GetByIdAsync(int systemid, string ntlogin, int accesslevelid)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new ArgumentException("NT login is required.");

            AccessUserLevel? entity = await _repository.GetByIdAsync(systemid, ntlogin, accesslevelid);
            return entity is null ? null : _mapper.Map<AccessUserLevelDto>(entity);
        }

        // TRANSFORMENGINE: duplicate-assignment guard — throws InvalidOperationException if assignment already exists
        public async Task<AccessUserLevelDto> CreateAsync(AccessUserLevelDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (dto.SystemId <= 0)
                throw new ArgumentException("A valid SystemId is required.");
            if (dto.AccessLevelId <= 0)
                throw new ArgumentException("A valid AccessLevelId is required.");
            if (string.IsNullOrWhiteSpace(dto.NtLogin))
                throw new ArgumentException("NT login is required.");

            bool alreadyExists = await _repository.ExistsAsync(dto.SystemId, dto.NtLogin, dto.AccessLevelId);
            if (alreadyExists)
                throw new InvalidOperationException(
                    $"AccessUserLevel (systemid={dto.SystemId}, ntlogin='{dto.NtLogin}', accesslevelid={dto.AccessLevelId}) already exists.");

            AccessUserLevel entity = _mapper.Map<AccessUserLevel>(dto);
            AccessUserLevel created = await _repository.AddAsync(entity);
            return _mapper.Map<AccessUserLevelDto>(created);
        }

        // TRANSFORMENGINE: throws KeyNotFoundException if assignment not found before delete
        public async Task<bool> DeleteAsync(int systemid, string ntlogin, int accesslevelid)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new ArgumentException("NT login is required.");

            bool exists = await _repository.ExistsAsync(systemid, ntlogin, accesslevelid);
            if (!exists)
                throw new KeyNotFoundException(
                    $"AccessUserLevel (systemid={systemid}, ntlogin='{ntlogin}', accesslevelid={accesslevelid}) was not found.");

            return await _repository.DeleteAsync(systemid, ntlogin, accesslevelid);
        }

        public async Task<bool> ExistsAsync(int systemid, string ntlogin, int accesslevelid)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new ArgumentException("NT login is required.");

            return await _repository.ExistsAsync(systemid, ntlogin, accesslevelid);
        }
    }
}
