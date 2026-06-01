using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for PC Grade maintenance (frmMaintPCGrade).
    /// Supports read, create, update, and delete of profit centre grades,
    /// with trigger-derived FK validation enforced at service level.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/pcgrades")]
    public class ProfitCentreGradeController : ControllerBase
    {
        private readonly IProfitCentreGradeService _profitCentreGradeService;
        private readonly IMapper _mapper;

        public ProfitCentreGradeController(IProfitCentreGradeService profitCentreGradeService, IMapper mapper)
        {
            _profitCentreGradeService = profitCentreGradeService ?? throw new ArgumentNullException(nameof(profitCentreGradeService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paginated list of RC grades available for the given profit centre.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfitCentreGradesAsync([FromQuery] PaginationReq<string> query, [FromQuery] string profitCentre)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _profitCentreGradeService.GetProfitCentreGradesAsync(filter, profitCentre);
            return Ok(_mapper.Map<PaginationRes<ProfitCentreGradeRes>>(result));
        }

        /// <summary>
        /// Returns a paginated list of all profit centre grades for the maintenance DataGrid.
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetAllPagedAsync([FromQuery] PaginationReq<string> query)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _profitCentreGradeService.GetAllPagedAsync(filter);
            return Ok(_mapper.Map<PaginationRes<ProfitCentreGradeRes>>(result));
        }

        /// <summary>
        /// Returns a single profit centre grade by PcGrade code.
        /// </summary>
        [HttpGet("{pcGrade}")]
        public async Task<IActionResult> GetByIdAsync(string pcGrade)
        {
            var result = await _profitCentreGradeService.GetByIdAsync(pcGrade);
            if (result is null)
                return NotFound($"Profit centre grade '{pcGrade}' not found.");
            return Ok(_mapper.Map<ProfitCentreGradeRes>(result));
        }

        /// <summary>
        /// Creates a new profit centre grade. Service enforces tI_ProfitCentreGrade FK constraint.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ProfitCentreGradeReq request)
        {
            var dto = _mapper.Map<ProfitCentreGradeDto>(request);
            try
            {
                var created = await _profitCentreGradeService.CreateAsync(dto);
                return Ok(_mapper.Map<ProfitCentreGradeRes>(created));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing profit centre grade. Service enforces tU_ProfitCentreGrade FK constraint.
        /// </summary>
        [HttpPut("{pcGrade}")]
        public async Task<IActionResult> UpdateAsync(string pcGrade, [FromBody] ProfitCentreGradeReq request)
        {
            var dto = _mapper.Map<ProfitCentreGradeDto>(request);
            try
            {
                var updated = await _profitCentreGradeService.UpdateAsync(pcGrade, dto);
                return Ok(_mapper.Map<ProfitCentreGradeRes>(updated));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a profit centre grade by PcGrade code.
        /// </summary>
        [HttpDelete("{pcGrade}")]
        public async Task<IActionResult> DeleteAsync(string pcGrade)
        {
            var deleted = await _profitCentreGradeService.DeleteAsync(pcGrade);
            if (!deleted)
                return NotFound($"Profit centre grade '{pcGrade}' not found.");
            return Ok(new { success = true });
        }

        /// <summary>
        /// Returns all ProfitCentre codes for dropdown population.
        /// </summary>
        [HttpGet("profitcentres")]
        public async Task<IActionResult> GetProfitCentreCodesAsync()
        {
            var result = await _profitCentreGradeService.GetAllProfitCentreCodesAsync();
            return Ok(result);
        }
    }
}
