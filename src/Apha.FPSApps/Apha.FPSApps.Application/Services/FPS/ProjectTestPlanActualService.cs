using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProjectTestPlanActualService : IProjectTestPlanActualService
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly ITestRequirementService _testRequirementService;

        public ProjectTestPlanActualService(IFpsApiClient fpsClient, ITestRequirementService testRequirementService)
        {
            _fpsClient = fpsClient;
            _testRequirementService = testRequirementService;
        }

        public async Task<ApiResponseDto<decimal>> GetTotalPlannedCostAsync(string projectCode)
        {
            var allQuery = new QueryParameters<string> { Page = 1, PageSize = 9999 };
            var result = await _testRequirementService.GetPagedTestReqmtbyProjectAsync(allQuery, projectCode);
            if (!result.Success || result.Data == null)
                return new ApiResponseDto<decimal> { Success = false, Data = 0m };

            var total = result.Data.Sum(t => (t.UnitPrice ?? 0m) * (decimal)(t.NoRequired ?? 0.0));
            return new ApiResponseDto<decimal> { Success = true, Data = total };
        }

        public async Task<ApiResponseDto<List<MonthlyOutputDto>>> GetMonthlyOutputByProjectAsync(QueryParameters<string> query, string projectCode)
        {
            var result = await _fpsClient.FpsMonthlyOutput.GetByProjectAsync(query, projectCode);
            if (!result.Success || result.Data == null)
                return result;

            await EnrichWithPricesAsync(result.Data, projectCode);
            return result;
        }

        public async Task<ApiResponseDto<MonthlyOutputTotalsDto>> GetTotalActualByProjectAsync(string projectCode)
        {
            var allQuery = new QueryParameters<string> { Page = 1, PageSize = 9999 };
            var dataResult = await _fpsClient.FpsMonthlyOutput.GetByProjectAsync(allQuery, projectCode);

            if (!dataResult.Success || dataResult.Data == null)
                return new ApiResponseDto<MonthlyOutputTotalsDto> { Success = false };

            await EnrichWithPricesAsync(dataResult.Data, projectCode);

            return new ApiResponseDto<MonthlyOutputTotalsDto>
            {
                Success = true,
                Data = new MonthlyOutputTotalsDto
                {
                    TotalVolume = dataResult.Data.Sum(x => x.Volume   ?? 0),
                    TotalCost   = dataResult.Data.Sum(x => x.Charge   ?? 0)
                }
            };
        }

        public async Task<ApiResponseDto<bool>> DeleteMonthlyOutputAsync(string buyer, string testCode, double month, string workGroup)
            => await _fpsClient.FpsMonthlyOutput.DeleteMonthlyOutputAsync(buyer, testCode, month, workGroup);

        private async Task EnrichWithPricesAsync(List<MonthlyOutputDto> items, string projectCode)
        {
            var pactResult = await _testRequirementService.GetPagedTestReqmtbyProjectAsync(
                new QueryParameters<string> { Page = 1, PageSize = 9999 }, projectCode);

            var priceLookup = pactResult.Data?
                .ToDictionary(t => (t.TestCode, t.Buyer), t => t.UnitPrice ?? 0m)
                ?? new Dictionary<(string, string), decimal>();

            foreach (var item in items)
            {
                var key = (item.TestCode ?? string.Empty, item.Buyer ?? string.Empty);
                if (priceLookup.TryGetValue(key, out var unitPrice))
                {
                    item.TestPrice = (double)unitPrice;
                    item.Charge    = (item.Volume ?? 0) * (double)unitPrice;
                }
            }
        }
    }
}
