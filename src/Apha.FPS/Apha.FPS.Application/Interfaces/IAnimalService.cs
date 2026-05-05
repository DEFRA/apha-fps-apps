using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IAnimalService
    {
        Task<List<AnimalDto>> GetAnimalLookupAsync();
        Task<PaginatedResult<AnimalCostViewDto>> GetAnimalCostAsync(QueryParameters<string> query, string jobCode);
        Task<decimal> GetTotalAnimalCostAsync(string jobCode);
        Task<AnimalCostViewDto?> GetAnimalCostViewByIdAsync(int indCounter, string jobCode);
        Task<decimal?> GetAnimalRateByIdAsync(string animalType, string jobCode);
        Task<AnimalRequestDto> AddAnimalCostAsync(AnimalRequestDto animalReq);
        Task<AnimalRequestDto> UpdateAnimalCostAsync(AnimalRequestDto animalReq);
        Task<bool> DeleteAnimalCostAsync(int indCounter);
    }
}
