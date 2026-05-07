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
    /// API controller for Project Sub-Contract operations.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectsubcontract")]
    public class ProjectSubContractController : ControllerBase
    {
        private readonly IProjectSubContractService _service;
        private readonly IMapper _mapper;

        public ProjectSubContractController(IProjectSubContractService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Retrieves a paginated list of Project Sub-Contract records.</summary>
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] QueryParameters<string> query, [FromQuery] string? project)
        {
            PaginatedResult<ProjectSubContractDto> pagedResult = await _service.GetPagedProjectSubContractsAsync(query, project);
            return Ok(_mapper.Map<PaginationRes<ProjectSubContractRes>>(pagedResult));
        }

        /// <summary>Retrieves the YTD total Amount for project sub-contracts.</summary>
        [HttpGet("total")]
        public async Task<IActionResult> GetTotal([FromQuery] string? project)
        {
            decimal total = await _service.GetTotalAmountAsync(project);
            return Ok(total);
        }

        /// <summary>Retrieves a paginated list of animal-category sub-contract records (AcctCode IN LargeAnimals/SmallAnimals/Mice).</summary>
        [HttpGet("animals")]
        public async Task<IActionResult> GetFpsProjectSubContracts([FromQuery] QueryParameters<string> query, [FromQuery] string? project)
        {
            PaginatedResult<ProjectSubContractDto> pagedResult = await _service.GetFpsProjectSubContractsAsync(query, project);
            return Ok(_mapper.Map<PaginationRes<ProjectSubContractRes>>(pagedResult));
        }

        /// <summary>Retrieves the total Amount for animal-category sub-contracts.</summary>
        [HttpGet("animals/total")]
        public async Task<IActionResult> GetFpsProjectTotal([FromQuery] string? project)
        {
            decimal total = await _service.GetFpsProjectSubContractTotalAmountAsync(project);
            return Ok(total);
        }

        /// <summary>Retrieves a Project Sub-Contract record by sub-contract counter.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            ProjectSubContractDto? item = await _service.GetByIdAsync(id);
            if (item is null)
                throw new KeyNotFoundException($"Project Sub-Contract with ID {id} not found.");
            return Ok(_mapper.Map<ProjectSubContractRes>(item));
        }

        /// <summary>Creates a new Project Sub-Contract record.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProjectSubContractReq request)
        {
            ProjectSubContractDto dto = _mapper.Map<ProjectSubContractDto>(request);
            ProjectSubContractDto created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.SubContCounter }, _mapper.Map<ProjectSubContractRes>(created));
        }

        /// <summary>Updates an existing Project Sub-Contract record.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProjectSubContractReq request)
        {
            ProjectSubContractDto dto = _mapper.Map<ProjectSubContractDto>(request);
            dto.SubContCounter = id;
            ProjectSubContractDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<ProjectSubContractRes>(updated));
        }

        /// <summary>Deletes a Project Sub-Contract record.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _service.DeleteAsync(id);
            return Ok(deleted);
        }

        [HttpGet("monthly-summary")]
        public async Task<IActionResult> GetMonthlySubContractsSummary([FromQuery] QueryParameters<string> query)
        {
            MonthlySubContractsPivotDto result = await _service.GetMonthlySubContractsSummaryAsync(query);
            return Ok(_mapper.Map<MonthlySubContractsPivotRes>(result));
        }
    }
}
