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
    /// API controller for Division maintenance operations.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [Route("api/v{version:apiVersion}/division")]
    [ApiController]
    [ApiVersion("1.0")]
    public class DivisionController : ControllerBase
    {
        private readonly IDivisionService _divisionService;
        private readonly IMapper _mapper;

        public DivisionController(
            IDivisionService divisionService,
            IMapper mapper)
        {
            _divisionService = divisionService ?? throw new ArgumentNullException(nameof(divisionService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Retrieves all divisions with their agency information.
        /// </summary>
        /// <returns>Collection of division records.</returns>
        [HttpGet]
        public async Task<ActionResult> GetAllDivisionsAsync()
        {
            var divisionDtos = await _divisionService.GetAllDivisionsAsync();
            if (divisionDtos == null)
            {
                throw new ArgumentException("Division records not found");
            }
            return Ok(_mapper.Map<List<DivisionRes>>(divisionDtos));
        }

        /// <summary>
        /// Retrieves a paginated list of divisions.
        /// </summary>
        /// <param name="query">Pagination parameters.</param>
        /// <returns>Paginated division records.</returns>
        [HttpGet("paged")]
        public async Task<ActionResult> GetAllDivisionsPagedAsync(
            [FromQuery] QueryParameters<string> query)
        {
            var divisionDtos = await _divisionService.GetAllDivisionsPagedAsync(query);
            if (divisionDtos == null)
            {
                throw new ArgumentException("Division records not found");
            }
            return Ok(_mapper.Map<PaginationRes<DivisionRes>>(divisionDtos));
        }

        /// <summary>
        /// Retrieves a single division by name.
        /// </summary>
        /// <param name="divName">Division name (case-insensitive).</param>
        /// <returns>Division record if found.</returns>
        [HttpGet("{divName}")]
        public async Task<ActionResult<DivisionRes>> GetDivisionByNameAsync(string divName)
        {
            var divisionDto = await _divisionService.GetDivisionByNameAsync(divName);
            if (divisionDto == null)
            {
                throw new ArgumentException($"Division record with name: {divName} not found");
            }
            return Ok(_mapper.Map<DivisionRes>(divisionDto));
        }

        /// <summary>
        /// Creates a new division record.
        /// </summary>
        /// <param name="request">Division data to create.</param>
        /// <returns>Created division record.</returns>
        [HttpPost]
        public async Task<ActionResult<DivisionRes>> CreateDivisionAsync(
            [FromBody] DivisionReq divisionRequest)
        {
            var mappedDivisionDto = _mapper.Map<DivisionDto>(divisionRequest);
            var createdDivision = await _divisionService.CreateDivisionAsync(mappedDivisionDto);
            return Ok(_mapper.Map<DivisionRes>(createdDivision));
        }

        /// <summary>
        /// Updates an existing division record.
        /// </summary>
        /// <param name="divName">Division name to update.</param>
        /// <param name="request">Updated division data.</param>
        /// <returns>Updated division record.</returns>
        [HttpPut("{divName}")]
        public async Task<ActionResult<DivisionRes>> UpdateDivisionAsync(
            string divName,
            [FromBody] DivisionReq divisionRequest)
        {
            var mappedDivisionDto = _mapper.Map<DivisionDto>(divisionRequest);
            var updatedDivision = await _divisionService.UpdateDivisionAsync(divName, mappedDivisionDto);
            return Ok(_mapper.Map<DivisionRes>(updatedDivision));
        }

        /// <summary>
        /// Deletes a division record by name.
        /// </summary>
        /// <param name="divName">Division name to delete.</param>
        /// <returns>Boolean indicating success.</returns>
        [HttpDelete("{divName}")]
        public async Task<IActionResult> DeleteDivisionAsync(string divName)
        {
            if (string.IsNullOrWhiteSpace(divName))
                throw new ArgumentException("Division name cannot be null or empty.", nameof(divName));

            var isDeleted = await _divisionService.DeleteDivisionAsync(divName);
            if (!isDeleted)
            {
                throw new ArgumentException($"Division record with name: {divName} not found for deletion");
            }
            return Ok(isDeleted);
        }
    }
}
