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
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin, API-PACTShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/monthlyoutput")]
    public class MonthlyOutputController : ControllerBase
    {
        private readonly IMonthlyOutputService _service;
        private readonly IMapper _mapper;
        private readonly ICurrentUserContext _currentUserContext;

        public MonthlyOutputController(IMonthlyOutputService service, IMapper mapper, ICurrentUserContext currentUserContext)
        {
            _service = service;
            _mapper = mapper;
            _currentUserContext = currentUserContext;
        }

        // ── Log ──────────────────────────────────────────────────────────────────

        [HttpGet("log/search")]
        public async Task<IActionResult> SearchAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? workGroup,
            [FromQuery] string? testCode,
            [FromQuery] string? buyer,
            [FromQuery] DateTime? dateImported,
            [FromQuery] double? month,
            [FromQuery] string? userId,
            [FromQuery] string? insertDelete)
        {
            var result = await _service.GetMonthlyOutputLogAsync(
                query, workGroup, testCode, buyer, dateImported, month, userId, insertDelete);

            return Ok(_mapper.Map<PaginationRes<MonthlyOutputLogRes>>(result));
        }

        // ── Live ─────────────────────────────────────────────────────────────────

        [HttpGet("live")]
        public async Task<IActionResult> GetLive(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? workGroup,
            [FromQuery] string? testCode,
            [FromQuery] string? buyer,
            [FromQuery] double? month)
        {
            var result = await _service.SearchLiveAsync(query, workGroup, testCode, buyer, month);
            return Ok(_mapper.Map<PaginationRes<MonthlyOutputRes>>(result));
        }

        [HttpGet("live/key")]
        public async Task<IActionResult> GetLiveByKey(
            [FromQuery] string testCode,
            [FromQuery] string buyer,
            [FromQuery] double month,
            [FromQuery] string workGroup)
        {
            var item = await _service.GetLiveByKeyAsync(testCode, buyer, month, workGroup);
            if (item is null)
                throw new KeyNotFoundException("MonthlyOutput record not found.");

            return Ok(_mapper.Map<MonthlyOutputRes>(item));
        }

        [HttpPut("live")]
        public async Task<IActionResult> UpdateLive([FromBody] MonthlyOutputReq request)
        {
            var dto = _mapper.Map<MonthlyOutputDto>(request);
            var updated = await _service.UpdateLiveAsync(dto);
            return Ok(_mapper.Map<MonthlyOutputRes>(updated));
        }

        [HttpDelete("live")]
        public async Task<IActionResult> DeleteLive(
            [FromQuery] string testCode,
            [FromQuery] string buyer,
            [FromQuery] double month,
            [FromQuery] string workGroup)
        {
            var deleted = await _service.DeleteLiveAsync(testCode, buyer, month, workGroup);
            return Ok(deleted);
        }

        // ── Staging ──────────────────────────────────────────────────────────────

        [HttpGet("staging")]
        public async Task<IActionResult> GetStaging([FromQuery] QueryParameters<string> query, [FromQuery] bool? passed)
        {
            var importedBy = _currentUserContext.UserId;
            var result = await _service.SearchStagingAsync(query, importedBy, passed);
            return Ok(_mapper.Map<PaginationRes<StagingMonthlyOutputRes>>(result));
        }

        [HttpGet("staging/{id:int}")]
        public async Task<IActionResult> GetStagingById(int id)
        {
            var importedBy = _currentUserContext.UserId;
            var item = await _service.GetStagingByIdAsync(id, importedBy);
            if (item is null)
                throw new KeyNotFoundException($"Staging MonthlyOutput record with ID {id} not found.");

            return Ok(_mapper.Map<StagingMonthlyOutputRes>(item));
        }

        [HttpPost("staging")]
        public async Task<IActionResult> CreateStaging([FromBody] StagingMonthlyOutputReq request)
        {
            var importedBy = _currentUserContext.UserId;
            var dto = _mapper.Map<StagingMonthlyOutputDto>(request);
            var created = await _service.CreateStagingAsync(dto, importedBy);
            return CreatedAtAction(nameof(GetStagingById), new { id = created.Id }, _mapper.Map<StagingMonthlyOutputRes>(created));
        }

        [HttpPut("staging/{id:int}")]
        public async Task<IActionResult> UpdateStaging(int id, [FromBody] StagingMonthlyOutputReq request)
        {
            var importedBy = _currentUserContext.UserId;
            var dto = _mapper.Map<StagingMonthlyOutputDto>(request);
            dto.Id = id;
            var updated = await _service.UpdateStagingAsync(dto, importedBy);
            return Ok(_mapper.Map<StagingMonthlyOutputRes>(updated));
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

        [HttpDelete("staging/user/failed")]
        public async Task<IActionResult> DeleteFailedStagingByUser()
        {
            var importedBy = _currentUserContext.UserId;
            var deletedCount = await _service.DeleteFailedStagingByUserAsync(importedBy);
            return Ok(deletedCount > 0);
        }

        [HttpPost("staging/import")]
        public async Task<IActionResult> ImportStaging([FromBody] MonthlyOutputImportReq request)
        {
            var importedBy = _currentUserContext.UserId;
            var dto = _mapper.Map<MonthlyOutputImportDto>(request);
            var result = await _service.ImportStagingAsync(dto, importedBy);
            return Ok(_mapper.Map<MonthlyOutputImportRes>(result));
        }

        [HttpPost("staging/validate")]
        public async Task<IActionResult> ValidateStaging()
        {
            var importedBy = _currentUserContext.UserId;
            var result = await _service.ValidateStagingAsync(importedBy);
            return Ok(_mapper.Map<MonthlyOutputValidateRes>(result));
        }

        [HttpPost("staging/makelive")]
        public async Task<IActionResult> MakeLive()
        {
            var importedBy = _currentUserContext.UserId;
            var result = await _service.MakeLiveAsync(importedBy);
            return Ok(_mapper.Map<MonthlyOutputMakeLiveRes>(result));
        }
    }
}
