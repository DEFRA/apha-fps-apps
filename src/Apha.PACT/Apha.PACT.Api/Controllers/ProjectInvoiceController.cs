using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    /// <summary>
    /// API controller for Project Invoice operations.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectinvoice")]
    public class ProjectInvoiceController : ControllerBase
    {
        private readonly IProjectInvoiceService _service;
        private readonly IMapper _mapper;

        public ProjectInvoiceController(IProjectInvoiceService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Retrieves a paginated list of Project Invoice records.</summary>
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] QueryParameters<string> query, [FromQuery] string? parentProject)
        {
            PaginatedResult<ProjectInvoiceDto> pagedResult = await _service.GetPagedProjectInvoicesAsync(query, parentProject);
            return Ok(_mapper.Map<PaginationRes<ProjectInvoiceRes>>(pagedResult));
        }

        /// <summary>Retrieves a paginated list of Project Invoice records filtered by month.</summary>
        /// <param name="query">Query parameters for pagination, sorting, and filtering.</param>
        /// <param name="month">Optional month number to filter invoices (1-12).</param>
        /// <returns>A paginated list of project invoices for the specified month.</returns>
        [HttpGet("by-month")]
        public async Task<IActionResult> GetPagedProjectInvoicesByMonth([FromQuery] QueryParameters<string> query, [FromQuery] int? month)
        {
            if (month.HasValue && (month.Value < 1 || month.Value > 12))
            {
                return BadRequest("Month must be between 1 and 12");
            }

            PaginatedResult<ProjectInvoiceDto> pagedResult = await _service.GetPagedProjectInvoicesByMonthAsync(query, month);
            return Ok(_mapper.Map<PaginationRes<ProjectInvoiceRes>>(pagedResult));
        }

        /// <summary>Retrieves the YTD total Amount for project invoices.</summary>
        [HttpGet("total")]
        public async Task<IActionResult> GetTotal([FromQuery] string? parentProject)
        {
            decimal total = await _service.GetTotalAmountAsync(parentProject);
            return Ok(total);
        }

        /// <summary>Retrieves a Project Invoice record by invoice counter.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            ProjectInvoiceDto? item = await _service.GetByIdAsync(id);
            if (item is null)
                throw new KeyNotFoundException($"Project Invoice with ID {id} not found.");
            return Ok(_mapper.Map<ProjectInvoiceRes>(item));
        }

        /// <summary>Creates a new Project Invoice record.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProjectInvoiceReq request)
        {
            ProjectInvoiceDto dto = _mapper.Map<ProjectInvoiceDto>(request);
            ProjectInvoiceDto created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.InvoiceCounter }, _mapper.Map<ProjectInvoiceRes>(created));
        }

        /// <summary>Updates an existing Project Invoice record.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProjectInvoiceReq request)
        {
            ProjectInvoiceDto dto = _mapper.Map<ProjectInvoiceDto>(request);
            dto.InvoiceCounter = id;
            ProjectInvoiceDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<ProjectInvoiceRes>(updated));
        }

        /// <summary>Deletes a Project Invoice record.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _service.DeleteAsync(id);
            return Ok(deleted);
        }

        /// <summary>Retrieves monthly invoices summary pivoted by month, with optional filter, sort and pagination.</summary>
        [HttpGet("monthly-summary")]
        public async Task<IActionResult> GetMonthlyInvoicesSummary([FromQuery] QueryParameters<string> query)
        {
            MonthlyInvoicesPivotDto result = await _service.GetMonthlyInvoicesSummaryAsync(query);
            return Ok(_mapper.Map<MonthlyInvoicesPivotRes>(result));
        }

        /// <summary>Copies project invoices from source month to target month. Supports bulk copy (all invoices), selective copy by IDs, or selective copy by providing invoice records.</summary>
        [HttpPost("copy")]
        public async Task<IActionResult> CopyInvoices([FromBody] CopyInvoicesReq request, [FromQuery] string sourceMonth, [FromQuery] string destinationMonth)
        {
            // Map request to DTO
            CopyInvoicesDto dto = _mapper.Map<CopyInvoicesDto>(request);

            // Override DTO values with validated query parameters
            dto.SourceMonth = int.Parse(sourceMonth);
            dto.TargetMonth = int.Parse(destinationMonth);

            // Call service
            CopyInvoicesResultDto result = await _service.CopyInvoicesAsync(dto);

            return Ok(_mapper.Map<CopyInvoicesRes>(result));
        }
    }
}
