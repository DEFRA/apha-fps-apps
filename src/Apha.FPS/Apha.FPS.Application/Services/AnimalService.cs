using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class AnimalService : IAnimalService
    {
        private readonly IAnimalRepository _animalRepository;
        private readonly IMapper _mapper;

        public AnimalService(IAnimalRepository animalRepository, IMapper mapper)
        {
            _animalRepository = animalRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<AnimalCostViewDto>> GetAnimalCostAsync(QueryParameters<string> query, string jobCode)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var animalCostViews = await _animalRepository.GetAnimalCostAsync(filter, jobCode);
            return _mapper.Map<PaginatedResult<AnimalCostViewDto>>(animalCostViews);
        }

        public async Task<List<AnimalDto>> GetAnimalLookupAsync()
        {
            var animalLookup = await _animalRepository.GetAnimalLookup();
            return _mapper.Map<List<AnimalDto>>(animalLookup);
        }

        public async Task<decimal?> GetAnimalRateByIdAsync(string animalType)
        {
            var animalCostViews = await _animalRepository.GetAnimalRateByIdAsync(animalType);
            return animalCostViews;
        }
        public async Task<AnimalRequestDto> AddAnimalCostAsync(AnimalRequestDto animalReq)
        {
            ArgumentNullException.ThrowIfNull(animalReq);
           
            if (animalReq.NumberOfDays < 0)
            {
                throw new ArgumentException("You have entered a negative number for Number of day.");
            }
            
            if (animalReq.NumberOfAnimals < 0)
            {
                throw new ArgumentException("You have entered a negative number Number of animal.");
            }

            var mapAnimalReq = _mapper.Map<AnimalRequest>(animalReq);
            var animalRequest  = await _animalRepository.AddAnimalCostAsync(mapAnimalReq);
            return _mapper.Map<AnimalRequestDto>(animalRequest);
        }
        public async Task<AnimalRequestDto> UpdateAnimalCostAsync(AnimalRequestDto animalReq)
        {
            if (animalReq.NumberOfDays < 0)
            {
                throw new ArgumentException("You have entered a negative number for Number of day.");
            }

            if (animalReq.NumberOfAnimals < 0)
            {
                throw new ArgumentException("You have entered a negative number Number of animal.");
            }

            var mapAnimalReq = _mapper.Map<AnimalRequest>(animalReq);
            var animalRequest = await _animalRepository.UpdateAnimalCostAsync(mapAnimalReq);
            return _mapper.Map<AnimalRequestDto>(animalRequest);
        }
        public async Task<bool> DeleteAnimalCostAsync(int indCounter)
        {
            if (indCounter < 0)
            {
                throw new ArgumentException("Not found any records.");
            }
            var isDeleted = await _animalRepository.DeleteJobAnimalCostAsync(indCounter);
            return isDeleted;
        }
        
    }
}
