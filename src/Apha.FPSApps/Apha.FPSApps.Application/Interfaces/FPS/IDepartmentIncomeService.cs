using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

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

        Task<ApiResponseDto<List<PeriodLookupDto>>> GetPeriodsAsync(double? accntsPeriod = null);

        Task<ApiResponseDto<List<PeriodSnapshotDto>>> GetSnapshotPeriodsAsync();

        Task<ApiResponseDto<bool>> UpdatePeriodLockedAsync(string periodName, bool periodLocked);

        // Current (old style) variants
        Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeIncomeCurrentAsync(string? project = null, int? monthFrom = null, int? monthTo = null);
        Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestIncomeCurrentAsync(string? project = null, int? monthFrom = null, int? monthTo = null);
        Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalIncomeCurrentAsync(string? project = null, int? monthFrom = null, int? monthTo = null);
        Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetAdditionalIncomeCurrentAsync(string? project = null, int? monthFrom = null, int? monthTo = null);
        Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsCurrentAsync(string? project = null, int? monthFrom = null, int? monthTo = null);
    }
}
