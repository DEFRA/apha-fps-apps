using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    [ApiController]
    [Route("api/timecodevalid")]
    public class TimeCodeValidController : ControllerBase
    {
        private readonly ITimeCodeValidService _service;
        private readonly IMapper _mapper;

        public TimeCodeValidController(ITimeCodeValidService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("jobcode/{jobCode}/project/{parentProject}")]
        public async Task<IActionResult> GetByJobCode(string jobCode, string parentProject)
        {
            var items = await _service.GetByJobCodeAsync(jobCode, parentProject);
            return Ok(_mapper.Map<IEnumerable<TimeCodeValidRes>>(items));
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] QueryParameters<string> query, [FromQuery] string? jobCode, [FromQuery] string? parentProject)
        {
            var pagedResult = await _service.GetPagedTimeCodesAsync(query, jobCode, parentProject);
            var mapped = _mapper.Map<IEnumerable<TimeCodeValidRes>>(pagedResult.Data);
            return Ok(new { data = mapped, pagination = pagedResult.PaginationData });
        }

        [HttpGet("{workGroup}/{timeCode}/{parentProject}")]
        public async Task<IActionResult> GetById(string workGroup, string timeCode, string parentProject)
        {
            var item = await _service.GetTimeCodeValidAsync(workGroup, timeCode, parentProject);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<TimeCodeValidRes>(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TimeCodeValidReq request)
        {
            var dto = _mapper.Map<TimeCodeValidDto>(request);
            var created = await _service.CreateTimeCodeValidAsync(dto);
            return Ok(_mapper.Map<TimeCodeValidRes>(created));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] TimeCodeValidReq request)
        {
            var dto = _mapper.Map<TimeCodeValidDto>(request);
            var updated = await _service.UpdateTimeCodeValidAsync(dto);
            return Ok(_mapper.Map<TimeCodeValidRes>(updated));
        }

        [HttpDelete("{workGroup}/{timeCode}/{parentProject}")]
        public async Task<IActionResult> Delete(string workGroup, string timeCode, string parentProject)
        {
            var deleted = await _service.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpDelete("jobcode/{jobCode}/project/{parentProject}")]
        public async Task<IActionResult> DeleteAllByJobCode(string jobCode, string parentProject)
        {
            await _service.DeleteAllByJobCodeAsync(jobCode, parentProject);
            return NoContent();
        }

        [HttpPost("copy")]
        public async Task<IActionResult> CopyWorkGroup([FromQuery] string sourceJobCode, [FromQuery] string targetJobCode, [FromQuery] string parentProject)
        {
            var items = await _service.CopyWorkGroupAsync(sourceJobCode, targetJobCode, parentProject);
            return Ok(_mapper.Map<IEnumerable<TimeCodeValidRes>>(items));
        }
    }
}
