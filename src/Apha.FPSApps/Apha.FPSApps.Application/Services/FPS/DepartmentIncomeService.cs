using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.DepartmentIncome;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class DepartmentIncomeService : IDepartmentIncomeService
    {
        private readonly IFpsApiClient _fpsClient;

        public DepartmentIncomeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
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

        public async Task<ApiResponseDto<List<PeriodLookupDto>>> GetPeriodsAsync()
            => await _fpsClient.FpsDepartmentIncome.GetPeriodsAsync();
    }
}
