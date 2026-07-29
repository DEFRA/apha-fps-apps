using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.DepartmentIncome;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IDepartmentIncomeService
    {
        Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetAdditionalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        Task<ApiResponseDto<List<PeriodLookupDto>>> GetPeriodsAsync();
    }
}
