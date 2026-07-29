using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IDepartmentIncomeRepository
    {
        Task<List<DepartmentIncomeTime>> GetTimeIncomeAsync(string? project, int monthFrom, int monthTo);

        Task<List<DepartmentIncomeTest>> GetTestIncomeAsync(string? project, int monthFrom, int monthTo);

        // AcctCode IN ("LargeAnimals","SmallAnimals","Mice") filter enforced in implementation
        Task<List<DepartmentIncomeAnimal>> GetAnimalIncomeAsync(string? project, int monthFrom, int monthTo);

        // AcctCode NOT IN ("LargeAnimals","SmallAnimals","Mice") filter enforced in implementation
        Task<List<DepartmentIncomeAdditional>> GetAdditionalIncomeAsync(string? project, int monthFrom, int monthTo);

        Task<List<DepartmentIncomeTotals>> GetTotalsAsync(string? project, int monthFrom, int monthTo);

        Task<List<PeriodLookup>> GetPeriodsAsync();
    }
}
