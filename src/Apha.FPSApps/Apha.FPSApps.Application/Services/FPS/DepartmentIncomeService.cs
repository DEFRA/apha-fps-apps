using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class DepartmentIncomeService : IDepartmentIncomeService
    {
        private readonly IFpsApiClient _fpsClient;

        public DepartmentIncomeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetTimeIncomeAsync(project, monthFrom, monthTo);

        public async Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetTestIncomeAsync(project, monthFrom, monthTo);

        public async Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetAnimalIncomeAsync(project, monthFrom, monthTo);

        public async Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetAdditionalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetAdditionalIncomeAsync(project, monthFrom, monthTo);

        public async Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetTotalsAsync(project, monthFrom, monthTo);

        public async Task<ApiResponseDto<List<PeriodLookupDto>>> GetPeriodsAsync(double? accntsPeriod = null)
            => await _fpsClient.FpsDepartmentIncome.GetPeriodsAsync(accntsPeriod);

        public async Task<ApiResponseDto<List<PeriodSnapshotDto>>> GetSnapshotPeriodsAsync()
            => await _fpsClient.FpsDepartmentIncome.GetSnapshotPeriodsAsync();

        public async Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeIncomeCurrentAsync(
            string? project = null, int? monthFrom = null, int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetTimeIncomeCurrentAsync(project, monthFrom, monthTo);

        public async Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestIncomeCurrentAsync(
            string? project = null, int? monthFrom = null, int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetTestIncomeCurrentAsync(project, monthFrom, monthTo);

        public async Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalIncomeCurrentAsync(
            string? project = null, int? monthFrom = null, int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetAnimalIncomeCurrentAsync(project, monthFrom, monthTo);

        public async Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetAdditionalIncomeCurrentAsync(
            string? project = null, int? monthFrom = null, int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetAdditionalIncomeCurrentAsync(project, monthFrom, monthTo);

        public async Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsCurrentAsync(
            string? project = null, int? monthFrom = null, int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetTotalsCurrentAsync(project, monthFrom, monthTo);

        public async Task<ApiResponseDto<bool>> UpdatePeriodLockedAsync(string periodName, bool periodLocked)
            => await _fpsClient.FpsDepartmentIncome.UpdatePeriodLockedAsync(periodName, periodLocked);
    }
}
