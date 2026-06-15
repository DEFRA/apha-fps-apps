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
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [Route("api/v{version:apiVersion}/DivisionGrade")]
    [ApiController]
    [ApiVersion("1.0")]
    public class DivisionGradeController : ControllerBase
    {
        private readonly IDivisionGradeService _maintDGService;
        private readonly IMapper _mapper;

        public DivisionGradeController(IDivisionGradeService maintDGService, IMapper mapper)
        {
            _maintDGService = maintDGService ?? throw new ArgumentNullException(nameof(maintDGService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("paged")]
        public async Task<ActionResult> GetAllPagedAsync([FromQuery] QueryParameters<string> query)
        {
            var result = await _maintDGService.GetAllPagedAsync(query);
            if (result == null)
            {
                throw new ArgumentException("Division grade records not found");
            }
            return Ok(_mapper.Map<PaginationRes<DivisionGradeRes>>(result));
        }

        [HttpGet("{divisionGradeCode}")]
        public async Task<ActionResult<DivisionGradeRes>> GetByIdAsync(string divisionGradeCode)
        {
            var dto = await _maintDGService.GetByIdAsync(divisionGradeCode);
            if (dto == null)
            {
                throw new ArgumentException($"Division grade record '{divisionGradeCode}' not found");
            }
            return Ok(_mapper.Map<DivisionGradeRes>(dto));
        }

        [HttpPost]
        public async Task<ActionResult<DivisionGradeRes>> CreateAsync([FromBody] DivisionGradeReq request)
        {
            var dto = _mapper.Map<DivisionGradeDto>(request);
            var created = await _maintDGService.CreateAsync(dto);
            return Ok(_mapper.Map<DivisionGradeRes>(created));
        }

        [HttpPut("{divisionGradeCode}")]
        public async Task<ActionResult<DivisionGradeRes>> UpdateAsync(
            string divisionGradeCode,
            [FromBody] DivisionGradeReq request)
        {
            var dto = _mapper.Map<DivisionGradeDto>(request);
            var updated = await _maintDGService.UpdateAsync(divisionGradeCode, dto);
            return Ok(_mapper.Map<DivisionGradeRes>(updated));
        }

        [HttpDelete("{divisionGradeCode}")]
        public async Task<IActionResult> DeleteAsync(string divisionGradeCode)
        {
            if (string.IsNullOrWhiteSpace(divisionGradeCode))
            {
                throw new ArgumentException("Division grade code cannot be null or empty.", nameof(divisionGradeCode));
            }

            var deleted = await _maintDGService.DeleteAsync(divisionGradeCode);
            if (!deleted)
            {
                throw new ArgumentException($"Division grade record '{divisionGradeCode}' not found");
            }
            return Ok(true);
        }

        [HttpGet("grades")]
        public async Task<ActionResult<List<string>>> GetAllGradeCodesAsync()
        {
            var gradeCodes = await _maintDGService.GetAllGradeCodesAsync();
            return Ok(gradeCodes);
        }

        [HttpGet("divisiongrades")]
        public async Task<ActionResult<List<string>>> GetAllDivisionGradeCodesAsync()
        {
            var codes = await _maintDGService.GetAllDivisionGradeCodesAsync();
            return Ok(codes);
        }
    }
}
