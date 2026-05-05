using Apha.Costbook.Core.Entities;

namespace Apha.Costbook.Core.Interfaces;

public interface IAnimalRequirementRepository
{
    Task<IEnumerable<AnimalRequirementDetailView>> GetAnimalRequirementsByProjectYearAsync(string project, int year);
    Task<AnimalRequirement> AddAnimalRequirementAsync(AnimalRequirement animalRequirement);
    Task<AnimalRequirement> UpdateAnimalRequirementAsync(AnimalRequirement animalRequirement);
    Task<bool> DeleteAnimalRequirementAsync(int arIdentity);
    Task<IEnumerable<AnimalRateLookup>> GetAnimalRatesAsync(bool isDefra);
    Task<IEnumerable<FpsAnimals>> GetAllAnimalsAsync();
}

public record AnimalRateLookup(string AnimalType, double? DailyRate);
