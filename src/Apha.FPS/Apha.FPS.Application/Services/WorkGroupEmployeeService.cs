// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — WorkGroupEmployeeService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Implemented CreateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto) — maps DTO → entity,
 *     delegates to IWorkGroupEmployeeRepository.CreateWorkGroupEmployeeAsync, maps result back to DTO.
 *   - Service now satisfies updated IWorkGroupEmployeeService interface (CreateWorkGroupEmployeeAsync added).
 *
 * PRESERVED:
 *   - GetWorkGroupEmployeeAsync, GetWorkGroupEmployeeByIdAsync, UpdateWorkGroupEmployeeAsync,
 *     DeleteWorkGroupEmployeeAsync implementations unchanged
 *   - Constructor DI pattern (IWorkGroupEmployeeRepository + IMapper) unchanged
 *   - ArgumentException / ArgumentNullException guard pattern preserved
 *   - No DbContext injected — all data access via repository interface
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: HrsAvail computation (HrsPaid - Leave - SickSpecial) is delegated to
 *     WorkGroupEmployeeRepository.CreateWorkGroupEmployeeAsync (Phase 4). Verify the repository
 *     sets HrsAvail before persisting so this service layer does not need to replicate it.
 *   - TRANSFORMENGINE TODO: Duplicate PactId guard — if the repository enforces unique PactId per
 *     FpsYear at DB level, a duplicate create will surface as a DbUpdateException. Consider catching
 *     and rethrowing as InvalidOperationException with a user-friendly message here if needed.
 */

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class WorkGroupEmployeeService : IWorkGroupEmployeeService
    {
        private readonly IWorkGroupEmployeeRepository _repository;
        private readonly IMapper _mapper;

        public WorkGroupEmployeeService(IWorkGroupEmployeeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<WorkGroupEmployeeDto>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(wgGrade);
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupEmployeeAsync(filter, wgGrade);
            return _mapper.Map<PaginatedResult<WorkGroupEmployeeDto>>(pagedData);
        }

        public async Task<WorkGroupEmployeeDto?> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pactId);
            var entity = await _repository.GetWorkGroupEmployeeByIdAsync(pactId);
            return _mapper.Map<WorkGroupEmployeeDto>(entity);
        }

        // TRANSFORMENGINE: CreateWorkGroupEmployeeAsync added — POST /api/v1/wgstaff
        // Maps WorkGroupEmployeeDto → WorkGroupEmployee entity, delegates create to repository,
        // maps persisted entity back to DTO. HrsAvail computation deferred to repository (Phase 4).
        public async Task<WorkGroupEmployeeDto> CreateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.PactId);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.WorkGroupGrade);

            var entity = _mapper.Map<WorkGroupEmployee>(dto);
            var created = await _repository.CreateWorkGroupEmployeeAsync(entity);
            return _mapper.Map<WorkGroupEmployeeDto>(created);
        }

        public async Task<WorkGroupEmployeeDto> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            var entity = _mapper.Map<WorkGroupEmployee>(dto);
            var updated = await _repository.UpdateWorkGroupEmployeeAsync(entity);
            return _mapper.Map<WorkGroupEmployeeDto>(updated);
        }

        public async Task<bool> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pactId);
            return await _repository.DeleteWorkGroupEmployeeAsync(pactId);
        }
    }
}
