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
    /// API controller for WorkgroupGrade maintenance operations.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [Route("api/v{version:apiVersion}/workgroupgrade")]
    [ApiController]
    [ApiVersion("1.0")]
    public class WorkgroupGradeController : ControllerBase
    {
        private readonly IWorkgroupGradeService _service;
        private readonly IMapper _mapper;

        public WorkgroupGradeController(IWorkgroupGradeService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>Retrieves a paginated list of WorkgroupGrade records.</summary>
        [HttpGet("paged")]
        public async Task<ActionResult> GetAllWorkgroupGradesPagedAsync([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetAllWorkgroupGradesPagedAsync(query);
            return Ok(_mapper.Map<PaginationRes<WorkgroupGradeRes>>(result));
        }

        /// <summary>Retrieves a single WorkgroupGrade by WgGrade code.</summary>
        [HttpGet("{wgGrade}")]
        public async Task<ActionResult<WorkgroupGradeRes>> GetByWgGradeAsync(string wgGrade)
        {
            var dto = await _service.GetByWgGradeAsync(wgGrade);
            if (dto is null)
                throw new KeyNotFoundException($"WorkgroupGrade '{wgGrade}' not found.");
            return Ok(_mapper.Map<WorkgroupGradeRes>(dto));
        }

        /// <summary>Creates a new WorkgroupGrade record.</summary>
        [HttpPost]
        public async Task<ActionResult<WorkgroupGradeRes>> CreateAsync([FromBody] WorkgroupGradeReq request)
        {
            var dto = _mapper.Map<WorkgroupGradeDto>(request);
            var created = await _service.CreateAsync(dto);
            return Ok(_mapper.Map<WorkgroupGradeRes>(created));
        }

        /// <summary>Updates an existing WorkgroupGrade record.</summary>
        [HttpPut("{wgGrade}")]
        public async Task<ActionResult<WorkgroupGradeRes>> UpdateAsync(string wgGrade, [FromBody] WorkgroupGradeReq request)
        {
            var dto = _mapper.Map<WorkgroupGradeDto>(request);
            dto.WgGrade = wgGrade;
            var updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<WorkgroupGradeRes>(updated));
        }

        /// <summary>Deletes a WorkgroupGrade record by WgGrade code.</summary>
        [HttpDelete("{wgGrade}")]
        public async Task<ActionResult<bool>> DeleteAsync(string wgGrade)
        {
            var deleted = await _service.DeleteAsync(wgGrade);
            return Ok(deleted);
        }

        /// <summary>Returns all Profit Centre Grade codes for dropdown population.</summary>
        [HttpGet("pcgrades")]
        public async Task<ActionResult<List<string>>> GetAllPcGradesAsync()
        {
            var result = await _service.GetAllPcGradesAsync();
            return Ok(result);
        }

        /// <summary>Returns all Grade codes for dropdown population.</summary>
        [HttpGet("gradecodes")]
        public async Task<ActionResult<List<string>>> GetAllGradeCodesAsync()
        {
            var result = await _service.GetAllGradeCodesAsync();
            return Ok(result);
        }

        /// <summary>Returns all Workgroup names for dropdown population.</summary>
        [HttpGet("workgroups")]
        public async Task<ActionResult<List<string>>> GetAllWorkgroupNamesAsync()
        {
            var result = await _service.GetAllWorkgroupNamesAsync();
            return Ok(result);
        }
    }
}
