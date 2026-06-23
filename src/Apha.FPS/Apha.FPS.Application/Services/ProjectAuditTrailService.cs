/*
 * TRANSFORMENGINE MIGRATION — ProjectAuditTrailService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — service implementation delegating to IProjectAuditTrailRepository
 *   - 5 service methods covering all 5 audit trail log tables in fps schema
 *   - Orchestrates PaginationParameters mapping (QueryParameters<string> → PaginationParameters<string>) via AutoMapper
 *   - Orchestrates entity-to-DTO mapping (PagedData<TEntity> → PaginatedResult<TDto>) via AutoMapper
 *   - Input validation (ArgumentException.ThrowIfNullOrWhiteSpace) applied before first await per phase rules
 *
 * PRESERVED:
 *   - No direct DbContext usage — all data access delegated to IProjectAuditTrailRepository
 *   - Async end-to-end (all methods are async Task<...>)
 *   - Business guard: parentProject required (non-null, non-whitespace) for all queries
 *   - Nullable date range preserved — optional per HTML prototype filter-from/filter-to inputs
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetStaffJobLogsAsync — parentProject passed to repository; repository impl responsible for JobCode-prefix/join translation (no parentproject column in staffjob_log)
 *   - TRANSFORMENGINE TODO: FpsYear parameter not yet surfaced at service boundary — add if multi-year audit requirement emerges
 */
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class ProjectAuditTrailService : IProjectAuditTrailService
    {
        private readonly IProjectAuditTrailRepository _repository;
        private readonly IMapper _mapper;

        public ProjectAuditTrailService(IProjectAuditTrailRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GetProjectLogsAsync — maps QueryParameters to PaginationParameters, delegates to repository, maps PagedData<ProjectLog> → PaginatedResult<ProjectLogDto>
        public async Task<PaginatedResult<ProjectLogDto>> GetProjectLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(parentProject);

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var data = await _repository.GetProjectLogsAsync(filter, parentProject, fromDate, toDate);
            return _mapper.Map<PaginatedResult<ProjectLogDto>>(data);
        }

        // TRANSFORMENGINE: GetStaffJobLogsAsync — parentProject passed to repository which translates to JobCode-prefix filter (no parentproject column in fps.staffjob_log)
        public async Task<PaginatedResult<StaffJobLogDto>> GetStaffJobLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(parentProject);

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var data = await _repository.GetStaffJobLogsAsync(filter, parentProject, fromDate, toDate);
            return _mapper.Map<PaginatedResult<StaffJobLogDto>>(data);
        }

        // TRANSFORMENGINE: GetTestRequirementLogsAsync — parentProject maps to ProjectBuyerCode/JobCode filter in repository
        public async Task<PaginatedResult<TestRequirementLogDto>> GetTestRequirementLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(parentProject);

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var data = await _repository.GetTestRequirementLogsAsync(filter, parentProject, fromDate, toDate);
            return _mapper.Map<PaginatedResult<TestRequirementLogDto>>(data);
        }

        // TRANSFORMENGINE: GetAnimalRequestLogsAsync — parentProject maps to JobCode-prefix filter in repository (fps.animalreq_log has no parentproject column)
        public async Task<PaginatedResult<AnimalRequestLogDto>> GetAnimalRequestLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(parentProject);

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var data = await _repository.GetAnimalRequestLogsAsync(filter, parentProject, fromDate, toDate);
            return _mapper.Map<PaginatedResult<AnimalRequestLogDto>>(data);
        }

        // TRANSFORMENGINE: GetAdditionalCostLogsAsync — parentProject maps to JobCode-prefix filter in repository (fps.additionalcosts_log has no parentproject column)
        public async Task<PaginatedResult<AdditionalCostLogDto>> GetAdditionalCostLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(parentProject);

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var data = await _repository.GetAdditionalCostLogsAsync(filter, parentProject, fromDate, toDate);
            return _mapper.Map<PaginatedResult<AdditionalCostLogDto>>(data);
        }
    }
}
