using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    /// <summary>
    /// API controller for Project Invoice operations.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin, API-PACTShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectinvoice")]
    public class ProjectInvoiceController : ControllerBase
    {
        private readonly IProjectInvoiceService _service;
        private readonly IMapper _mapper;
        private readonly ICurrentUserContext _currentUserContext;

        public ProjectInvoiceController(IProjectInvoiceService service, IMapper mapper, ICurrentUserContext currentUserContext)
        {
            _service = service;
            _mapper = mapper;
            _currentUserContext = currentUserContext;
        }

        /// <summary>Retrieves a paginated list of Project Invoice records.</summary>
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] QueryParameters<string> query, [FromQuery] string? parentProject)
        {
            PaginatedResult<ProjectInvoiceDto> pagedResult = await _service.GetPagedProjectInvoicesAsync(query, parentProject);
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
        [HttpGet("invoice/id")]
        public async Task<IActionResult> GetById([FromQuery] int id)
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
        [HttpPut("invoice/id")]
        public async Task<IActionResult> Update([FromQuery] int id, [FromBody] ProjectInvoiceReq request)
        {
            ProjectInvoiceDto dto = _mapper.Map<ProjectInvoiceDto>(request);
            dto.InvoiceCounter = id;
            ProjectInvoiceDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<ProjectInvoiceRes>(updated));
        }

        /// <summary>Deletes a Project Invoice record.</summary>
        [HttpDelete("invoice/id")]
        public async Task<IActionResult> Delete([FromQuery] int id)
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

        /// <summary>Retrieves failed invoice import records for the current user.</summary>
        [HttpGet("import/failed")]
        public async Task<IActionResult> GetFailedInvoiceImport([FromQuery] QueryParameters<string> query)
        {
            var importedBy = _currentUserContext.UserId;
            var result = await _service.GetFailedInvoiceImportAsync(query, importedBy);
            return Ok(_mapper.Map<PaginationRes<InvoiceImportRowRes>>(result));
        }

        /// <summary>Retrieves a failed invoice import record by ID.</summary>
        [HttpGet("import/failed/{id}")]
        public async Task<IActionResult> GetFailedInvoiceImportById(int id)
        {
            var importedBy = _currentUserContext.UserId;
            var result = await _service.GetFailedInvoiceImportByIdAsync(id, importedBy);
            if (result == null)
                throw new KeyNotFoundException($"Failed Invoice Import with ID {id} not found.");
            return Ok(_mapper.Map<InvoiceImportRowRes>(result));
        }

        /// <summary>Updates and re-validates a failed invoice import record.</summary>
        [HttpPut("import/failed/{id}")]
        public async Task<IActionResult> SaveFailedInvoiceImport(int id, [FromBody] InvoiceImportRowReq request)
        {
            var importedBy = _currentUserContext.UserId;
            var dto = _mapper.Map<InvoiceImportRowDto>(request);
            var movedToInvoice = await _service.SaveFailedInvoiceImportAsync(id, dto, importedBy);
            return Ok(movedToInvoice);
        }

        /// <summary>Deletes a failed invoice import record by ID.</summary>
        [HttpDelete("import/failed/{id}")]
        public async Task<IActionResult> DeleteFailedInvoiceImportById(int id)
        {
            var importedBy = _currentUserContext.UserId;
            var deleted = await _service.DeleteFailedInvoiceImportByIdAsync(id, importedBy);
            return Ok(deleted);
        }

        /// <summary>Deletes all failed invoice import records for the current user.</summary>
        [HttpDelete("import/failed/user")]
        public async Task<IActionResult> DeleteFailedInvoiceImportByUser()
        {
            var importedBy = _currentUserContext.UserId;
            var deletedCount = await _service.DeleteFailedInvoiceImportByUserAsync(importedBy);
            return Ok(deletedCount > 0);
        }

        /// <summary>Imports invoice data from an Excel file.</summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportInvoice([FromBody] InvoiceImportReq request)
        {
            var importedBy = _currentUserContext.UserId;
            var dto = _mapper.Map<InvoiceImportDto>(request);
            var result = await _service.ImportInvoiceAsync(dto, importedBy);
            return Ok(_mapper.Map<InvoiceImportRes>(result));
        }
    }
}
