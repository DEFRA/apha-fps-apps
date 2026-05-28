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
    /// Controller for managing Animal Master (tblAnimals_MAP) CRUD operations.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [Route("api/v{version:apiVersion}/animalmaster")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AnimalMasterController : ControllerBase
    {
        private readonly IAnimalService _animalService;
        private readonly IMapper _mapper;

        public AnimalMasterController(IAnimalService animalService, IMapper mapper)
        {
            _animalService = animalService ?? throw new ArgumentNullException(nameof(animalService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>Gets all animals.</summary>
        [HttpGet]
        public async Task<ActionResult> GetAllAnimalsAsync()
        {
            var dtos = await _animalService.GetAllAnimalsAsync();
            return Ok(_mapper.Map<List<AnimalRes>>(dtos));
        }

        /// <summary>Gets a paged list of animals.</summary>
        [HttpGet("paged")]
        public async Task<ActionResult> GetAllAnimalsPagedAsync([FromQuery] QueryParameters<string> query)
        {
            var paged = await _animalService.GetAllAnimalsAsync(query);
            return Ok(_mapper.Map<PaginationRes<AnimalRes>>(paged));
        }

        /// <summary>Gets an animal by its type key.</summary>
        [HttpGet("{animalType}")]
        public async Task<ActionResult<AnimalRes>> GetAnimalByIdAsync(string animalType)
        {
            var dto = await _animalService.GetAnimalByIdAsync(animalType);
            if (dto == null)
                throw new ArgumentException($"Animal '{animalType}' not found.");
            return Ok(_mapper.Map<AnimalRes>(dto));
        }

        /// <summary>Creates a new animal master record.</summary>
        [HttpPost]
        public async Task<ActionResult<AnimalRes>> CreateAnimal([FromBody] AnimalMasterReq req)
        {
            var dto = _mapper.Map<AnimalDto>(req);
            var added = await _animalService.AddAnimalAsync(dto);
            return Ok(_mapper.Map<AnimalRes>(added));
        }

        /// <summary>Updates an existing animal master record.</summary>
        [HttpPut]
        public async Task<ActionResult<AnimalRes>> UpdateAnimal([FromBody] AnimalMasterReq req)
        {
            var dto = _mapper.Map<AnimalDto>(req);
            var updated = await _animalService.UpdateAnimalAsync(dto);
            return Ok(_mapper.Map<AnimalRes>(updated));
        }

        /// <summary>Deletes an animal master record.</summary>
        [HttpDelete("{animalType}")]
        public async Task<IActionResult> DeleteAnimal(string animalType)
        {
            if (string.IsNullOrWhiteSpace(animalType))
                throw new ArgumentException("Animal type cannot be null or empty.", nameof(animalType));

            var deleted = await _animalService.DeleteAnimalAsync(animalType);
            if (!deleted)
                throw new ArgumentException($"Animal '{animalType}' not found.");
            return Ok(deleted);
        }
    }
}
