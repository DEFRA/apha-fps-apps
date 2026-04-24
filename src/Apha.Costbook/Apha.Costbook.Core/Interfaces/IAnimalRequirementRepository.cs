using Apha.Costbook.Core.Entities;

namespace Apha.Costbook.Core.Interfaces;

public interface IAnimalRequirementRepository
{
    Task<IEnumerable<AnimalRequirement>> GetByProjectYearAsync(string project, int year);
    Task<AnimalRequirement> AddAsync(AnimalRequirement animalRequirement);
    Task<AnimalRequirement> UpdateAsync(AnimalRequirement animalRequirement);
    Task<bool> DeleteAsync(int arIdentity);
    Task<IEnumerable<AnimalRateLookup>> GetAnimalRatesAsync(bool isDefra);
}

public record AnimalRateLookup(string AnimalType, double? DailyRate);
