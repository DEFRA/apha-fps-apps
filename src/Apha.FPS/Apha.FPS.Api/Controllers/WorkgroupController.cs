using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// REST API controller for WorkGroup maintenance operations.
    /// Migrated from <c>frmMaintWorkGroup2</c> (RecordSource: WorkGroup_MAP → fps.workgroup).
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [Route("api/v{version:apiVersion}/workgroup")]
    [ApiController]
    [ApiVersion("1.0")]
    public class WorkgroupController : ControllerBase
    {
        private readonly IWorkgroupService _workgroupService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initialises the <see cref="WorkgroupController"/> with its required dependencies.
        /// </summary>
        public WorkgroupController(IWorkgroupService workgroupService, IMapper mapper)
        {
            _workgroupService = workgroupService ?? throw new ArgumentNullException(nameof(workgroupService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paginated, optionally filtered and sorted list of workgroups for the active FPS year.
        /// </summary>
        /// <param name="query">Pagination, filter, and sort parameters.</param>
        /// <returns>Paginated list of <see cref="WorkgroupMaintenanceRes"/>.</returns>
        [HttpGet("paged")]
        public async Task<ActionResult<PaginationRes<WorkgroupMaintenanceRes>>> GetPagedAsync(
            [FromQuery] QueryParameters<string> query)
        {
            var result = await _workgroupService.GetPagedAsync(query);
            if (result == null)
            {
                throw new ArgumentException("Workgroup records not found.");
            }
            return Ok(_mapper.Map<PaginationRes<WorkgroupMaintenanceRes>>(result));
        }

        /// <summary>
        /// Returns a single workgroup by its WorkGroupName for the active FPS year.
        /// </summary>
        /// <param name="workGroupName">WorkGroup name (natural primary key component).</param>
        /// <returns><see cref="WorkgroupMaintenanceRes"/> if found.</returns>
        [HttpGet("{workGroupName}")]
        public async Task<ActionResult<WorkgroupMaintenanceRes>> GetByKeyAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
            {
                throw new ArgumentException("WorkGroupName cannot be null or empty.", nameof(workGroupName));
            }

            var dto = await _workgroupService.GetByKeyAsync(workGroupName);
            if (dto == null)
            {
                throw new KeyNotFoundException($"Workgroup '{workGroupName}' not found.");
            }
            return Ok(_mapper.Map<WorkgroupMaintenanceRes>(dto));
        }

        /// <summary>
        /// Creates a new workgroup record.
        /// Throws <see cref="ArgumentException"/> if WorkGroupName or ProfitCentre is missing.
        /// Throws <see cref="InvalidOperationException"/> if a workgroup with the same name already exists in the active FPS year.
        /// </summary>
        /// <param name="request">Workgroup creation request.</param>
        /// <returns>Created <see cref="WorkgroupMaintenanceRes"/>.</returns>
        [HttpPost]
        public async Task<ActionResult<WorkgroupMaintenanceRes>> CreateAsync([FromBody] WorkgroupMaintenanceReq request)
        {
            var dto = _mapper.Map<WorkgroupDto>(request);
            try
            {
                var created = await _workgroupService.CreateAsync(dto);
                return Ok(_mapper.Map<WorkgroupMaintenanceRes>(created));
            }
            catch (Exception ex) when (IsCostCentreForeignKeyViolation(ex))
            {
                throw BuildCostCentreValidationException();
            }
        }

        /// <summary>
        /// Updates an existing workgroup identified by
        /// Pass the original WorkGroupName in the route; use <c>request.WorkGroupName</c> to rename.
        /// Throws <see cref="KeyNotFoundException"/> if the workgroup does not exist.
        /// </summary>
        /// <param name="workGroupName">Original WorkGroup name (route parameter).</param>
        /// <param name="request">Workgroup update request.</param>
        /// <returns>Updated <see cref="WorkgroupMaintenanceRes"/>.</returns>
        [HttpPut("{workGroupName}")]
        public async Task<ActionResult<WorkgroupMaintenanceRes>> UpdateAsync(
            string workGroupName,
            [FromBody] WorkgroupMaintenanceReq request)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
            {
                throw new ArgumentException("WorkGroupName cannot be null or empty.", nameof(workGroupName));
            }

            var dto = _mapper.Map<WorkgroupDto>(request);
            try
            {
                var updated = await _workgroupService.UpdateAsync(workGroupName, dto);
                return Ok(_mapper.Map<WorkgroupMaintenanceRes>(updated));
            }
            catch (Exception ex) when (IsCostCentreForeignKeyViolation(ex))
            {
                throw BuildCostCentreValidationException();
            }
        }

        /// <summary>
        /// Deletes the workgroup with the given WorkGroupName in the active FPS year.
        /// </summary>
        /// <param name="workGroupName">WorkGroup name of the record to delete.</param>
        /// <returns>True if deletion succeeded.</returns>
        [HttpDelete("{workGroupName}")]
        public async Task<IActionResult> DeleteAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
            {
                throw new ArgumentException("WorkGroupName cannot be null or empty.", nameof(workGroupName));
            }

            var deleted = await DeleteWorkgroupAsync(workGroupName);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Workgroup '{workGroupName}' not found.");
            }
            return Ok(true);
        }

        private async Task<bool> DeleteWorkgroupAsync(string workGroupName)
        {
            try
            {
                return await _workgroupService.DeleteAsync(workGroupName);
            }
            catch (Exception ex) when (IsForeignKeyViolation(ex, "fk_workgroupgrade_workgroup"))
            {
                throw new BusinessValidationErrorException(new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        "There are associated records in the WorkgroupGrade table so this record cannot be deleted.",
                        "WORKGROUPGRADE_FK_VIOLATION")
                });
            }
        }

        /// <summary>
        /// Returns all available profit centre identifiers for the ResourceCentre dropdown.
        /// </summary>
        /// <returns>List of profit centre identifier strings.</returns>
        [HttpGet("profitcentres")]
        public async Task<ActionResult<IEnumerable<string>>> GetProfitCentresAsync()
        {
            var result = await _workgroupService.GetAllProfitCentresAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns all manager records for the Owner dropdown.
        /// Sourced from the fps/qryManager named query (vtblstaffactive JOIN vworkgroupgrade_general).
        /// </summary>
        /// <returns>List of <see cref="ManagerRes"/> records.</returns>
        [HttpGet("owners")]
        public async Task<ActionResult<IEnumerable<ManagerRes>>> GetOwnersAsync()
        {
            var managerDtos = await _workgroupService.GetOwnersAsync();
            return Ok(_mapper.Map<IEnumerable<ManagerRes>>(managerDtos));
        }

        /// <summary>
        /// Returns cost centre values linked to the given <paramref name="profitCentre"/>,
        /// for use in the cascading CostCentre dropdown.
        /// </summary>
        /// <param name="profitCentre">The selected profit centre code.</param>
        /// <returns>List of cost centre double values.</returns>
        [HttpGet("costcentres")]
        public async Task<ActionResult<IEnumerable<double?>>> GetCostCentresAsync([FromQuery] string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
            {
                throw new ArgumentException("ProfitCentre cannot be null or empty.", nameof(profitCentre));
            }

            var result = await _workgroupService.GetCostCentresByProfitCentreAsync(profitCentre);
            return Ok(result);
        }

        // Screen-specific handling for the Maintain Workgroups foreign key constraints.
        // Kept in this controller (not the shared ExceptionMiddleware) because the friendly messages
        // only apply to the Workgroup Maintenance screen. The violation surfaces as a
        // PostgresException (SqlState 23503) usually wrapped inside a DbUpdateException.
        private static bool IsCostCentreForeignKeyViolation(Exception? ex)
            => IsForeignKeyViolation(ex, "fk_workgroup_costcentre");

        private static bool IsForeignKeyViolation(Exception? ex, string constraintName)
        {
            for (var current = ex; current is not null; current = current.InnerException)
            {
                if (current is PostgresException pgEx
                    && pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation
                    && pgEx.ConstraintName?.Contains(constraintName, StringComparison.OrdinalIgnoreCase) == true)
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
