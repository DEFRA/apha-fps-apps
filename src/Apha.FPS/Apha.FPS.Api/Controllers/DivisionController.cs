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
    /// <summary>
    /// API controller for Division maintenance operations.
    /// </summary>
   // [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [AllowAnonymous]
    [Route("api/division")]
    [ApiController]
    public class DivisionController : ControllerBase
    {
        private readonly IDivisionService _divisionService;
        private readonly IMapper _mapper;
        private readonly ILogger<DivisionController> _logger;

        public DivisionController(
            IDivisionService divisionService,
            IMapper mapper,
            ILogger<DivisionController> logger)
        {
            _divisionService = divisionService ?? throw new ArgumentNullException(nameof(divisionService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all divisions with their agency information.
        /// </summary>
        /// <returns>Collection of division records.</returns>
        [HttpGet]
        public async Task<ActionResult<List<DivisionRes>>> GetAllDivisionsAsync()
        {
            var divisionDtos = await _divisionService.GetAllDivisionsAsync();
            if (divisionDtos == null || !divisionDtos.Any())
            {
                return NotFound("No division records found");
            }
            return Ok(_mapper.Map<List<DivisionRes>>(divisionDtos));
        }

        /// <summary>
        /// Retrieves a paginated list of divisions.
        /// </summary>
        /// <param name="query">Pagination parameters.</param>
        /// <returns>Paginated division records.</returns>
        [HttpGet("paged")]
        public async Task<ActionResult<PaginationRes<DivisionRes>>> GetAllDivisionsPagedAsync(
            [FromQuery] QueryParameters<string> query)
        {
            var divisionDtos = await _divisionService.GetAllDivisionsPagedAsync(query);
            if (divisionDtos == null || !divisionDtos.Data.Any())
            {
                return NotFound("No division records found");
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
                return NotFound($"Division with name '{divName}' not found");
            }
            return Ok(_mapper.Map<DivisionRes>(divisionDto));
        }

        /// <summary>
        /// Creates a new division record.
        /// </summary>
        /// <param name="request">Division data to create.</param>
        /// <returns>Created division record.</returns>
        [HttpPost]
        // [Authorize(Roles = "API-FPSAdmin")] // Commented out for testing
        public async Task<ActionResult<DivisionRes>> CreateDivisionAsync([FromBody] DivisionReq request)
        {
            try
            {
                _logger.LogInformation("[CreateDivision] Received request: {@Request}", request);

                if (request == null)
                {
                    _logger.LogWarning("[CreateDivision] Request body is null");
                    return BadRequest(new { message = "Request body cannot be null" });
                }

                _logger.LogInformation("[CreateDivision] Mapping request to DTO");
                var divisionDto = _mapper.Map<DivisionDto>(request);

                _logger.LogInformation("[CreateDivision] Calling service to create division: {DivName}", divisionDto.DivName);
                var createdDivision = await _divisionService.CreateDivisionAsync(divisionDto);

                _logger.LogInformation("[CreateDivision] Successfully created division: {DivName}", createdDivision.DivName);
                var response = _mapper.Map<DivisionRes>(createdDivision);

                return CreatedAtAction(
                    nameof(GetDivisionByNameAsync),
                    new { divName = response.DivName },
                    response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "[CreateDivision] Business logic error: {Message}", ex.Message);
                return BadRequest(new 
                { 
                    success = false,
                    message = ex.Message,
                    errors = new[] { new { code = "BUSINESS_LOGIC_ERROR", message = ex.Message } }
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "[CreateDivision] Validation error: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CreateDivision] Unexpected error occurred");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the division", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing division record.
        /// </summary>
        /// <param name="divName">Division name to update.</param>
        /// <param name="request">Updated division data.</param>
        /// <returns>Updated division record.</returns>
        [HttpPut("{divName}")]
        // [Authorize(Roles = "API-FPSAdmin")] // Commented out for testing
        public async Task<ActionResult<DivisionRes>> UpdateDivisionAsync(
            string divName,
            [FromBody] DivisionReq request)
        {
            try
            {
                _logger.LogInformation("[UpdateDivision] Received request for division: {DivName} with new data: {@Request}", divName, request);

                // Use divName from URL to identify the record to update
                // The request body contains the new values (including potentially a new DivName)
                var divisionDto = _mapper.Map<DivisionDto>(request);

                // Pass the original divName from URL to identify which record to update
                var updatedDivision = await _divisionService.UpdateDivisionAsync(divName, divisionDto);

                _logger.LogInformation("[UpdateDivision] Successfully updated division: {DivName}", divName);
                return Ok(_mapper.Map<DivisionRes>(updatedDivision));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "[UpdateDivision] Business logic error: {Message}", ex.Message);
                return BadRequest(new 
                { 
                    success = false,
                    message = ex.Message,
                    errors = new[] { new { code = "BUSINESS_LOGIC_ERROR", message = ex.Message } }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateDivision] Error updating division: {DivName}", divName);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the division", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a division record by name.
        /// </summary>
        /// <param name="divName">Division name to delete.</param>
        /// <returns>Boolean indicating success.</returns>
        [HttpDelete("{divName}")]
        // [Authorize(Roles = "API-FPSAdmin")] // Commented out for testing
        public async Task<ActionResult<bool>> DeleteDivisionAsync(string divName)
        {
            try
            {
                _logger.LogInformation("[DeleteDivision] Deleting division: {DivName}", divName);

                var deleted = await _divisionService.DeleteDivisionAsync(divName);
                if (!deleted)
                {
                    _logger.LogWarning("[DeleteDivision] Division not found: {DivName}", divName);
                    return NotFound($"Division with name '{divName}' not found");
                }

                _logger.LogInformation("[DeleteDivision] Successfully deleted division: {DivName}", divName);
                return Ok(true);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "[DeleteDivision] Business logic error: {Message}", ex.Message);
                return BadRequest(new 
                { 
                    success = false,
                    message = ex.Message,
                    errors = new[] { new { code = "BUSINESS_LOGIC_ERROR", message = ex.Message } }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteDivision] Error deleting division: {DivName}", divName);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the division", details = ex.Message });
            }
        }
    }
}
