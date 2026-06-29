using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [Route("api/yearmaster")]
    [ApiController]
    public class YearMasterController : ControllerBase
    {
        private readonly IYearMasterService _yearMasterService;
        private readonly IMapper _mapper;

        public YearMasterController(
            IYearMasterService yearMasterService,
            IMapper mapper)
        {
            _yearMasterService = yearMasterService ?? throw new ArgumentNullException(nameof(yearMasterService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult> GetAllFpsYearsAsync()
        {
            var yearMasterDto = await _yearMasterService.GetAllFpsYearsAsync();
            if (yearMasterDto == null)
            {
                return NotFound("Year Master records not found");
            }
            return Ok(_mapper.Map<List<YearMasterRes>>(yearMasterDto));
        }

        [HttpGet("paged")]
        public async Task<ActionResult> GetAllFpsYearsPagedAsync(
            [FromQuery] QueryParameters<int> query)
        {
            var yearMasterDto = await _yearMasterService.GetAllFpsYearsPagedAsync(query);
            if (yearMasterDto == null)
            {
                return NotFound("Year Master records not found");
            }
            return Ok(_mapper.Map<PaginationRes<YearMasterRes>>(yearMasterDto));
        }

        [HttpGet("{fpsYear}")]
        public async Task<ActionResult<YearMasterRes>> GetFpsYearById(int fpsYear)
        {
            var yearMasterDto = await _yearMasterService.GetFpsYearByIdAsync(fpsYear);
            if (yearMasterDto == null)
            {
                return NotFound($"Year Master record with FPS Year: {fpsYear} not found");
            }
            return Ok(_mapper.Map<YearMasterRes>(yearMasterDto));
        }
    }
}
