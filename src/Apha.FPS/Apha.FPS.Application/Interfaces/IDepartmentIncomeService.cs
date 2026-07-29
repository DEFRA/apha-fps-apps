using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IDepartmentIncomeService
    {
        Task<List<DepartmentIncomeTimeDto>> GetTimeIncomeAsync(string? project, int? monthFrom, int? monthTo);

        Task<List<DepartmentIncomeTestDto>> GetTestIncomeAsync(string? project, int? monthFrom, int? monthTo);

        Task<List<DepartmentIncomeAnimalDto>> GetAnimalIncomeAsync(string? project, int? monthFrom, int? monthTo);

        Task<List<DepartmentIncomeAdditionalDto>> GetAdditionalIncomeAsync(string? project, int? monthFrom, int? monthTo);

        Task<List<DepartmentIncomeTotalsDto>> GetTotalsAsync(string? project, int? monthFrom, int? monthTo);

        Task<List<PeriodLookupDto>> GetPeriodsAsync();
    }
}
