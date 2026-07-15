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
    /// API controller for MonthlyTime live records, staging imports, and import logs.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin, API-PACTShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/monthlytime")]
    public class MonthlyTimeController : ControllerBase
    {
        private readonly IMonthlyTimeService _service;
        private readonly IMapper _mapper;
        private readonly ICurrentUserContext _currentUserContext;

        public MonthlyTimeController(IMonthlyTimeService service, IMapper mapper, ICurrentUserContext currentUserContext)
        {
            _service = service;
            _mapper = mapper;
            _currentUserContext = currentUserContext;
        }

        [HttpGet("live")]
        public async Task<IActionResult> GetLive(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? workGroup,
            [FromQuery] string? timeCode,
            [FromQuery] string? pactStaffId,
            [FromQuery] string? parentProject,
            [FromQuery] double? month)
        {
            var result = await _service.SearchLiveAsync(query, workGroup, timeCode, pactStaffId, parentProject, month);
            return Ok(_mapper.Map<PaginationRes<MonthlyTimeRes>>(result));
        }

        [HttpGet("live/key")]
        public async Task<IActionResult> GetLiveByKey(
            [FromQuery] string pactStaffId,
            [FromQuery] string timeCode,
            [FromQuery] double month,
            [FromQuery] string parentProject)
        {
            var item = await _service.GetLiveByKeyAsync(pactStaffId, timeCode, month, parentProject);
            if (item is null)
                throw new KeyNotFoundException("MonthlyTime record not found.");

            return Ok(_mapper.Map<MonthlyTimeRes>(item));
        }

        [HttpPut("live")]
        public async Task<IActionResult> UpdateLive([FromBody] MonthlyTimeReq request)
        {
            var dto = _mapper.Map<MonthlyTimeDto>(request);
            var updated = await _service.UpdateLiveAsync(dto);
            return Ok(_mapper.Map<MonthlyTimeRes>(updated));
        }

        [HttpDelete("live")]
        public async Task<IActionResult> DeleteLive(
            [FromQuery] string pactStaffId,
            [FromQuery] string timeCode,
            [FromQuery] double month,
            [FromQuery] string parentProject)
        {
            var deleted = await _service.DeleteLiveAsync(pactStaffId, timeCode, month, parentProject);
            return Ok(deleted);
        }

        [HttpGet("staging")]
        public async Task<IActionResult> GetStaging([FromQuery] QueryParameters<string> query, [FromQuery] bool? passed)
        {
            var importedBy = _currentUserContext.UserId;
            var result = await _service.SearchStagingAsync(query, importedBy, passed);
            return Ok(_mapper.Map<PaginationRes<StagingMonthlyTimeRes>>(result));
        }

        [HttpGet("staging/{id:int}")]
        public async Task<IActionResult> GetStagingById(int id)
        {
            var importedBy = _currentUserContext.UserId;
            var item = await _service.GetStagingByIdAsync(id, importedBy);
            if (item is null)
                throw new KeyNotFoundException($"Staging MonthlyTime record with ID {id} not found.");

            return Ok(_mapper.Map<StagingMonthlyTimeRes>(item));
        }

        [HttpPost("staging")]
        public async Task<IActionResult> CreateStaging([FromBody] StagingMonthlyTimeReq request)
        {
            var importedBy = _currentUserContext.UserId;
            var dto = _mapper.Map<StagingMonthlyTimeDto>(request);
            var created = await _service.CreateStagingAsync(dto, importedBy);
            return CreatedAtAction(nameof(GetStagingById), new { id = created.Id }, _mapper.Map<StagingMonthlyTimeRes>(created));
        }

        [HttpPut("staging/{id:int}")]
        public async Task<IActionResult> UpdateStaging(int id, [FromBody] StagingMonthlyTimeReq request)
        {
            var importedBy = _currentUserContext.UserId;
            var dto = _mapper.Map<StagingMonthlyTimeDto>(request);
            dto.Id = id;
            var updated = await _service.UpdateStagingAsync(dto, importedBy);
            return Ok(_mapper.Map<StagingMonthlyTimeRes>(updated));
        }

        [HttpDelete("staging/{id:int}")]
        public async Task<IActionResult> DeleteStaging(int id)
        {
            var importedBy = _currentUserContext.UserId;
            var deleted = await _service.DeleteStagingAsync(id, importedBy);
            return Ok(deleted);
        }

        [HttpDelete("staging/user")]
        public async Task<IActionResult> DeleteAllStagingByUser()
        {
            var importedBy = _currentUserContext.UserId;
            var deletedCount = await _service.DeleteAllStagingByUserAsync(importedBy);
            return Ok(deletedCount > 0);
        }

        [HttpPost("staging/import")]
        public async Task<IActionResult> ImportStaging([FromBody] MonthlyTimeImportReq request)
        {
            var importedBy = _currentUserContext.UserId;
            var dto = _mapper.Map<MonthlyTimeImportDto>(request);
            var result = await _service.ImportStagingAsync(dto, importedBy);
            return Ok(_mapper.Map<MonthlyTimeImportRes>(result));
        }

        [HttpPost("staging/validate")]
        public async Task<IActionResult> ValidateStaging()
        {
            var importedBy = _currentUserContext.UserId;
            var result = await _service.ValidateStagingAsync(importedBy);
            return Ok(_mapper.Map<MonthlyTimeValidateRes>(result));
        }

        [HttpPost("staging/makelive")]
        public async Task<IActionResult> MakeLive()
        {
            var importedBy = _currentUserContext.UserId;
            var result = await _service.MakeLiveAsync(importedBy);
            return Ok(_mapper.Map<MonthlyTimeMakeLiveRes>(result));
        }

        [HttpGet("log/search")]
        public async Task<IActionResult> SearchAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? workGroup,
            [FromQuery] string? timeCode,
            [FromQuery] string? pactStaffId,
            [FromQuery] string? parentProject,
            [FromQuery] DateTime? dateImported,
            [FromQuery] double? month,
            [FromQuery] string? userId,
            [FromQuery] string? insertDelete)
        {
            var logFilter = new MonthlyTimeLogFilterDto
            {
                WorkGroup = workGroup,
                TimeCode = timeCode,
                PactStaffId = pactStaffId,
                ParentProject = parentProject,
                DateImported = dateImported,
                Month = month,
                UserId = userId,
                InsertDelete = insertDelete
            };

            var result = await _service.SearchAsync(query, logFilter);
            return Ok(_mapper.Map<PaginationRes<MonthlyTimeLogRes>>(result));
        }
    }
}
