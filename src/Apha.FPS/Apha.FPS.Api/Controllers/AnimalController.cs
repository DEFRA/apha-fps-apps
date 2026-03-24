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
    /// Controller for managing animal-related operations.
    /// </summary>    
    [ApiController]
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [Route("api/animal")]
    public class AnimalController : ControllerBase
    {
        private readonly IAnimalService _animalService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimalController"/> class.
        /// </summary>
        /// <param name="animalService">The animal service.</param>
        /// <param name="mapper">The AutoMapper instance.</param>
        public AnimalController(
                        IAnimalService animalService,
                        IMapper mapper)
        {
            _animalService = animalService;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets a paginated list of animal costs for a specific job code.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="jobCode">The job code to filter animal costs.</param>
        /// <returns>A paginated list of animal cost view results.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAnimalCostAsync([FromQuery] PaginationReq<string> query, string jobCode)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _animalService.GetAnimalCostAsync(filter, jobCode);
            return Ok(_mapper.Map<PaginationRes<AnimalCostViewRes>>(result));
        }

        /// <summary>
        /// Gets a lookup list of all animals.
        /// </summary>
        /// <returns>A list of animal resources.</returns>
        [HttpGet("lookup")]
        public async Task<IActionResult> GetAnimalLookupAsync()
        {
            var result = await _animalService.GetAnimalLookupAsync();
            return Ok(_mapper.Map<List<AnimalRes>>(result));
        }

        /// <summary>
        /// Gets the rate for a specific animal type.
        /// </summary>
        /// <param name="animalType">The animal type identifier.</param>
        /// <returns>The rate for the specified animal type, or NotFound if not found.</returns>
        [HttpGet("rate")]
        public async Task<IActionResult> GetAnimalRateByIdAsync(string animalType)
        {
            var result = await _animalService.GetAnimalRateByIdAsync(animalType);
            if (result.HasValue)
            {
                return Ok(result.Value);
            }
            return NotFound();
        }

        /// <summary>
        /// Adds a new animal cost entry.
        /// </summary>
        /// <param name="animalReq">The animal request data.</param>
        /// <returns>The created animal request resource.</returns>
        [HttpPost]
        public async Task<IActionResult> AddAnimalCostAsync(AnimalRequestReq animalReq)
        {
            var mapAnimalReq = _mapper.Map<AnimalRequestDto>(animalReq);
            var result = await _animalService.AddAnimalCostAsync(mapAnimalReq);
            return Ok(_mapper.Map<AnimalRequestRes>(result));
        }

        /// <summary>
        /// Updates an existing animal cost entry.
        /// </summary>
        /// <param name="animalReq">The animal request data to update.</param>
        /// <returns>The updated animal request resource.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateAnimalCostAsync(AnimalRequestReq animalReq)
        {
            var mapAnimalReq = _mapper.Map<AnimalRequestDto>(animalReq);
            var result = await _animalService.UpdateAnimalCostAsync(mapAnimalReq);
            return Ok(_mapper.Map<AnimalRequestRes>(result));
        }

        /// <summary>
        /// Deletes an animal cost entry by its index counter.
        /// </summary>
        /// <param name="indCounter">The index counter of the animal cost entry to delete.</param>
        /// <returns>True if deleted; otherwise, throws if not found.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteAnimalCostAsync(int indCounter)
        {
            var isDeleted = await _animalService.DeleteAnimalCostAsync(indCounter);
            if (!isDeleted)
            {
                throw new KeyNotFoundException("Data not found.");
            }
            return Ok(isDeleted);
        }

    }
}
