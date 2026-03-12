using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IAnimalService
    {
        Task<List<AnimalDto>> GetAnimalLookupAsync();
        Task<PaginatedResult<AnimalCostViewDto>> GetAnimalCostAsync(QueryParameters<string> query, string jobCode);
        Task<decimal?> GetAnimalRateByIdAsync(string animalType);
        Task<AnimalRequestDto> AddAnimalCostAsync(AnimalRequestDto animalReq);
        Task<AnimalRequestDto> UpdateAnimalCostAsync(AnimalRequestDto animalReq);
        Task<bool> DeleteAnimalCostAsync(int indCounter);
    }
}
