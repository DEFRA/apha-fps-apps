using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;

namespace Apha.Costbook.Core.Interfaces;

public interface IAnimalRequirementRepository
{
    Task<PagedData<AnimalRequirementDetailView>> GetAnimalRequirementsByProjectYearAsync(string project, int year, PaginationParameters<string> query);
    Task<AnimalRequirement> AddAnimalRequirementAsync(AnimalRequirement animalRequirement);
    Task<AnimalRequirement> UpdateAnimalRequirementAsync(AnimalRequirement animalRequirement);
    Task<bool> DeleteAnimalRequirementAsync(int arIdentity);
    Task<IEnumerable<AnimalRateLookup>> GetAnimalRatesAsync(string projectId, int year, bool isDefra);
    Task<IEnumerable<FpsAnimals>> GetAllAnimalsAsync();
}


