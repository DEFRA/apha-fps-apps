using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    /// <summary>Project Year Costs — Additional Cost vs Actual data.</summary>
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectyearcosts")]
    public class ProjectYearCostsController : ControllerBase
    {
        private readonly IProjectYearCostsService _service;
        private readonly IMapper _mapper;

        public ProjectYearCostsController(IProjectYearCostsService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Returns paginated Additional Cost actuals for a given project and year.</summary>
        [HttpGet("{project}/{year}/additionalactuals")]
        public async Task<IActionResult> GetAdditionalActuals(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<AdditionalCostDto> result = await _service.GetAdditionalActualsAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<AdditionalCostRes>>(result));
        }

        /// <summary>Returns paginated Additional Cost plans for a given project and year.</summary>
        [HttpGet("{project}/{year}/additionalplans")]
        public async Task<IActionResult> GetAdditionalPlans(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<AdditionalCostDto> result = await _service.GetAdditionalPlansAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<AdditionalCostRes>>(result));
        }

        /// <summary>Returns paginated Animal Cost actuals for a given project and year.</summary>
        [HttpGet("{project}/{year}/animalactuals")]
        public async Task<IActionResult> GetAnimalActuals(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<AnimalCostDto> result = await _service.GetAnimalActualsAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<AnimalCostRes>>(result));
        }

        /// <summary>Returns paginated Animal Cost plans for a given project and year.</summary>
        [HttpGet("{project}/{year}/animalplans")]
        public async Task<IActionResult> GetAnimalPlans(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<AnimalCostDto> result = await _service.GetAnimalPlansAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<AnimalCostRes>>(result));
        }

        /// <summary>Returns paginated Test Cost plans for a given project and year.</summary>
        [HttpGet("{project}/{year}/testplans")]
        public async Task<IActionResult> GetTestPlans(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<TestCostDto> result = await _service.GetTestPlansAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<TestCostRes>>(result));
        }

        /// <summary>Returns paginated Test Cost actuals for a given project and year.</summary>
        [HttpGet("{project}/{year}/testactuals")]
        public async Task<IActionResult> GetTestActuals(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<TestCostDto> result = await _service.GetTestActualsAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<TestCostRes>>(result));
        }

        /// <summary>Returns paginated Staff Cost plans for a given project and year.</summary>
        [HttpGet("{project}/{year}/staffplans")]
        public async Task<IActionResult> GetStaffPlans(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<StaffCostDto> result = await _service.GetStaffPlansAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<StaffCostRes>>(result));
        }

        /// <summary>Returns paginated Staff Cost actuals for a given project and year.</summary>
        [HttpGet("{project}/{year}/staffactuals")]
        public async Task<IActionResult> GetStaffActuals(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<StaffCostDto> result = await _service.GetStaffActualsAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<StaffCostRes>>(result));
        }

        /// <summary>Returns project year details for a given project and year.</summary>
        [HttpGet("{project}/{year}/projectyeardetails")]
        public async Task<IActionResult> GetProjectYearDetails(string project, short year)
        {
            ProjectYearDetailsDto result = await _service.GetProjectYearDetailsAsync(project, year);
            return Ok(_mapper.Map<ProjectYearDetailsRes>(result));
        }
    }
}
