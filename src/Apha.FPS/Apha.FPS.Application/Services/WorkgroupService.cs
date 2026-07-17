using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using Npgsql;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for Workgroup CRUD and lookup operations.
    /// Orchestrates <see cref="IWorkgroupRepository"/> calls and enforces
    /// business rules extracted from frmMaintWorkGroup2 VBA and fps.workgroup triggers.
    /// </summary>
    public class WorkgroupService : IWorkgroupService
    {
        private readonly IWorkgroupRepository _repository;
        private readonly IMapper _mapper;

        public WorkgroupService(IWorkgroupRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GetPagedAsync — maps QueryParameters<string> to PaginationParameters<string>, then maps PagedData back to PaginatedResult
        /// <inheritdoc/>
        public async Task<PaginatedResult<WorkgroupDto>> GetPagedAsync(QueryParameters<string> query)
        {
            if (query is null)
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Query parameters cannot be null.", "WORKGROUP_INVALID_QUERY")
                ]);
            }

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.GetPagedAsync(filter);
            return _mapper.Map<PaginatedResult<WorkgroupDto>>(result);
        }

        // TRANSFORMENGINE: GetByKeyAsync — returns null if not found; caller (controller) maps null to 404
        /// <inheritdoc/>
        public async Task<WorkgroupDto?> GetByKeyAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("WorkGroupName cannot be null or empty.", "WORKGROUP_INVALID_KEY")
                ]);
            }

            var entity = await _repository.GetByKeyAsync(workGroupName);
            return entity is null ? null : _mapper.Map<WorkgroupDto>(entity);
        }

        // TRANSFORMENGINE: CreateAsync — VBA Form_BeforeUpdate guard: WorkGroupName + ProfitCentre required
        //   tI_WorkGroup trigger guard: duplicate WorkGroupName within active FpsYear is rejected
        /// <inheritdoc/>
        public async Task<WorkgroupDto> CreateAsync(WorkgroupDto dto)
        {
            if (dto is null)
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Workgroup data cannot be null.", "WORKGROUP_INVALID_DATA")
                ]);
            }

            // TRANSFORMENGINE: VBA Form_BeforeUpdate — WorkGroupName is a required field
            if (string.IsNullOrWhiteSpace(dto.WorkGroupName))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("WorkGroupName is required.", "WORKGROUP_NAME_REQUIRED")
                ]);
            }

            // TRANSFORMENGINE: VBA Form_BeforeUpdate — ProfitCentre (ResourceCentre) is a required field
            if (string.IsNullOrWhiteSpace(dto.ProfitCentre))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("ProfitCentre is required.", "WORKGROUP_PROFITCENTRE_REQUIRED")
                ]);
            }

            // TRANSFORMENGINE: tI_WorkGroup trigger guard — duplicate WorkGroupName within active FpsYear is rejected
            var exists = await _repository.ExistsAsync(dto.WorkGroupName);
            if (exists)
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"A workgroup with the name '{dto.WorkGroupName}' already exists for the active FPS year.",
                        "WORKGROUP_DUPLICATE_NAME")
                ]);
            }

            var entity = _mapper.Map<Workgroup>(dto);
            try
            {
                var created = await _repository.CreateAsync(entity);
                return _mapper.Map<WorkgroupDto>(created);
            }
            catch (Exception ex) when (IsCostCentreForeignKeyViolation(ex))
            {
                throw BuildCostCentreValidationException();
            }
        }

        // TRANSFORMENGINE: UpdateAsync — VBA Form_BeforeUpdate guard: WorkGroupName + ProfitCentre required
        //   originalWorkGroupName supports PK rename path; caller passes same value when no rename needed
        /// <inheritdoc/>
        public async Task<WorkgroupDto> UpdateAsync(string originalWorkGroupName, WorkgroupDto dto)
        {
            if (string.IsNullOrWhiteSpace(originalWorkGroupName))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Original WorkGroupName cannot be null or empty.", "WORKGROUP_INVALID_KEY")
                ]);
            }

            if (dto is null)
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Workgroup data cannot be null.", "WORKGROUP_INVALID_DATA")
                ]);
            }

            // TRANSFORMENGINE: VBA Form_BeforeUpdate — WorkGroupName is a required field
            if (string.IsNullOrWhiteSpace(dto.WorkGroupName))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("WorkGroupName is required.", "WORKGROUP_NAME_REQUIRED")
                ]);
            }

            // TRANSFORMENGINE: VBA Form_BeforeUpdate — ProfitCentre (ResourceCentre) is a required field
            if (string.IsNullOrWhiteSpace(dto.ProfitCentre))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("ProfitCentre is required.", "WORKGROUP_PROFITCENTRE_REQUIRED")
                ]);
            }

            // TRANSFORMENGINE: Pre-check that the target record exists before delegating to repository
            var exists = await _repository.ExistsAsync(originalWorkGroupName);
            if (!exists)
            {
                throw new KeyNotFoundException(
                    $"Workgroup '{originalWorkGroupName}' not found for the active FPS year.");
            }

            var entity = _mapper.Map<Workgroup>(dto);
            try
            {
                var updated = await _repository.UpdateAsync(originalWorkGroupName, entity);
                return _mapper.Map<WorkgroupDto>(updated);
            }
            catch (Exception ex) when (IsCostCentreForeignKeyViolation(ex))
            {
                throw BuildCostCentreValidationException();
            }
        }

        // TRANSFORMENGINE: DeleteAsync — no VBA-level cascade guard found; repository handles FK constraint errors
        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("WorkGroupName cannot be null or empty.", "WORKGROUP_INVALID_KEY")
                ]);
            }

            try
            {
                return await _repository.DeleteAsync(workGroupName);
            }
            catch (Exception ex) when (IsForeignKeyViolation(ex))
            {
                throw new BusinessValidationErrorException(new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        "There are associated records in the system so this record cannot be deleted.",
                        "WORKGROUPGRADE_FK_VIOLATION")
                });
            }
        }

        // TRANSFORMENGINE: GetAllProfitCentresAsync — lookup dropdown; delegates directly to repository
        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetAllProfitCentresAsync()
            => await _repository.GetAllProfitCentresAsync();

        // TRANSFORMENGINE: GetOwnersAsync — Owner dropdown; maps Manager entities to ManagerDto via AutoMapper
        /// <inheritdoc/>
        public async Task<IEnumerable<ManagerDto>> GetOwnersAsync()
        {
            var managers = await _repository.GetOwnersAsync();
            return _mapper.Map<IEnumerable<ManagerDto>>(managers);
        }

        // TRANSFORMENGINE: GetCostCentresByProfitCentreAsync — cascading CostCentre dropdown; filtered by ProfitCentre
        //   VBA Form_Current: Requeries CostCentre combo when ProfitCentre changes — maps to this AJAX endpoint
        /// <inheritdoc/>
        public async Task<IEnumerable<double?>> GetCostCentresByProfitCentreAsync(string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("ProfitCentre cannot be null or empty.", "WORKGROUP_PROFITCENTRE_REQUIRED")
                ]);
            }

            return await _repository.GetCostCentresByProfitCentreAsync(profitCentre);
        }

        // Screen-specific handling for the Maintain Workgroups foreign key constraints.
        // The violation surfaces as a PostgresException (SqlState 23503) usually wrapped inside a DbUpdateException.
        private static bool IsCostCentreForeignKeyViolation(Exception? ex)
            => IsForeignKeyViolation(ex);

        private static bool IsForeignKeyViolation(Exception? ex)
        {
            for (var current = ex; current is not null; current = current.InnerException)
            {
                if (current is PostgresException pgEx
                    && pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
                {
                    return true;
                }
            }

            return false;
        }

        private static BusinessValidationErrorException BuildCostCentreValidationException()
        {
            return new BusinessValidationErrorException(new List<BusinessValidationError>
            {
                new BusinessValidationError(
                    "The Cost center is not present in the Cost Center table. Please input Cost center which is already present in CostCenter table.",
                    "COSTCENTRE_FK_VIOLATION")
            });
        }
    }
}
