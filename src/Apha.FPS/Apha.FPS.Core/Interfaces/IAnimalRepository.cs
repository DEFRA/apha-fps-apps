using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IAnimalRepository
    {  
        Task<List<Animal>> GetAnimalLookup();
        Task<PagedData<AnimalCostView>> GetAnimalCostAsync(PaginationParameters<string> query, string jobCode);
        Task<decimal> GetTotalAnimalCostAsync(string jobCode);
        Task<AnimalCostView?> GetAnimalCostViewByIdAsync(int indCounter, string jobCode);
        Task<decimal?> GetAnimalRateByIdAsync(string animalType, string jobCode);
        Task<AnimalRequest> AddAnimalCostAsync(AnimalRequest animalReq);
        Task<AnimalRequest> UpdateAnimalCostAsync(AnimalRequest animalReq);
        Task<bool> DeleteJobAnimalCostAsync(int indCounter);
    }
}
