using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ProjectTestPlanActualServiceTest
{
    public class ProjectTestPlanActualServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsMonthlyOutputCalcsApiClient _apiClient;
        private readonly ITestRequirementService _testRequirementService;
        private readonly ProjectTestPlanActualService _service;

        public ProjectTestPlanActualServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _apiClient = Substitute.For<IFpsMonthlyOutputCalcsApiClient>();
            _fpsClient.FpsMonthlyOutputCalcs.Returns(_apiClient);
            _testRequirementService = Substitute.For<ITestRequirementService>();
            _service = new ProjectTestPlanActualService(_fpsClient, _testRequirementService);
        }

        private static QueryParameters<string> Q() => new() { Page = 1, PageSize = 10 };

        private void SetupEmptyPactPrices(string projectCode)
            => _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(new List<TestRequirementDto>()));

        #region GetMonthlyOutputCalcsByProjectAsync

        [Fact]
        public async Task GetMonthlyOutputCalcsByProjectAsync_WithSuccessResponse_ReturnsDtoList()
        {
            var items = new List<MonthlyOutputCalcsViewDto> { new() { Buyer = "AH0033", TestCode = "TC01" }, new() { Buyer = "AH0033", TestCode = "TC02" } };
            var resp = ApiResponseDto<List<MonthlyOutputCalcsViewDto>>.SuccessResponse(items, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033").Returns(resp);
            SetupEmptyPactPrices("AH0033");

            var result = await _service.GetMonthlyOutputCalcsByProjectAsync(Q(), "AH0033");

            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputCalcsByProjectAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            var resp = ApiResponseDto<List<MonthlyOutputCalcsViewDto>>.SuccessResponse(new List<MonthlyOutputCalcsViewDto>());
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033").Returns(resp);
            SetupEmptyPactPrices("AH0033");

            var result = await _service.GetMonthlyOutputCalcsByProjectAsync(Q(), "AH0033");

            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetMonthlyOutputCalcsByProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "err" } };
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputCalcsViewDto>>.FailureResponse(errors, new ApiMetaDto()));

            var result = await _service.GetMonthlyOutputCalcsByProjectAsync(Q(), "AH0033");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetMonthlyOutputCalcsByProjectAsync_EnrichesItemsWithPricesFromPact()
        {
            var items = new List<MonthlyOutputCalcsViewDto> { new() { Buyer = "AH0033", TestCode = "TC01", Volume = 4 } };
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputCalcsViewDto>>.SuccessResponse(items));

            var prices = new List<TestRequirementDto> { new() { TestCode = "TC01", Buyer = "AH0033", UnitPrice = 100m } };
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(prices));

            var result = await _service.GetMonthlyOutputCalcsByProjectAsync(Q(), "AH0033");

            Assert.Equal(100.0, result.Data![0].TestPrice);
            Assert.Equal(400.0, result.Data![0].Charge);
        }

        [Fact]
        public async Task GetMonthlyOutputCalcsByProjectAsync_DelegatesToFpsMonthlyOutputCalcsApiClient()
        {
            var resp = ApiResponseDto<List<MonthlyOutputCalcsViewDto>>.SuccessResponse(new List<MonthlyOutputCalcsViewDto>());
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033").Returns(resp);
            SetupEmptyPactPrices("AH0033");

            await _service.GetMonthlyOutputCalcsByProjectAsync(Q(), "AH0033");

            await _apiClient.Received(1).GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033");
            _ = _fpsClient.Received(1).FpsMonthlyOutputCalcs;
        }

        #endregion

        #region GetTotalActualByProjectAsync

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithSuccessResponse_ReturnsEnrichedTotals()
        {
            var items = new List<MonthlyOutputCalcsViewDto> { new() { Buyer = "AH0033", TestCode = "TC01", Volume = 5 } };
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputCalcsViewDto>>.SuccessResponse(items));

            var prices = new List<TestRequirementDto> { new() { TestCode = "TC01", Buyer = "AH0033", UnitPrice = 100m } };
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(prices));

            var result = await _service.GetTotalActualByProjectAsync("AH0033");

            Assert.True(result.Success);
            Assert.Equal(5,   result.Data?.TotalVolume);
            Assert.Equal(500, result.Data?.TotalCost);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputCalcsViewDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "err" } }, new ApiMetaDto()));

            Assert.False((await _service.GetTotalActualByProjectAsync("AH0033")).Success);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithNoPriceMatch_TotalCostIsZero()
        {
            var items = new List<MonthlyOutputCalcsViewDto> { new() { Buyer = "AH0033", TestCode = "TC01", Volume = 5 } };
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputCalcsViewDto>>.SuccessResponse(items));
            SetupEmptyPactPrices("AH0033");

            var result = await _service.GetTotalActualByProjectAsync("AH0033");

            Assert.True(result.Success);
            Assert.Equal(5, result.Data?.TotalVolume);
            Assert.Equal(0, result.Data?.TotalCost);
        }

        #endregion

        #region GetTotalPlannedCostAsync

        [Fact]
        public async Task GetTotalPlannedCostAsync_WithTestRequirements_ReturnsSumOfUnitPriceTimesNoRequired()
        {
            var reqs = new List<TestRequirementDto>
            {
                new() { TestCode = "TC01", Buyer = "AH0033", UnitPrice = 100m, NoRequired = 2 },
                new() { TestCode = "TC02", Buyer = "AH0033", UnitPrice = 50m,  NoRequired = 4 }
            };
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(reqs));

            var result = await _service.GetTotalPlannedCostAsync("AH0033");

            Assert.True(result.Success);
            Assert.Equal(400m, result.Data);
        }

        [Fact]
        public async Task GetTotalPlannedCostAsync_WithEmptyRequirements_ReturnsZero()
        {
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(new List<TestRequirementDto>()));

            var result = await _service.GetTotalPlannedCostAsync("AH0033");

            Assert.True(result.Success);
            Assert.Equal(0m, result.Data);
        }

        [Fact]
        public async Task GetTotalPlannedCostAsync_WhenPactApiFails_ReturnsFailureWithZero()
        {
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "err" } }, new ApiMetaDto()));

            var result = await _service.GetTotalPlannedCostAsync("AH0033");

            Assert.False(result.Success);
            Assert.Equal(0m, result.Data);
        }

        [Fact]
        public async Task GetTotalPlannedCostAsync_WithNullUnitPrice_TreatsAsZero()
        {
            var reqs = new List<TestRequirementDto>
            {
                new() { TestCode = "TC01", Buyer = "AH0033", UnitPrice = null, NoRequired = 5 }
            };
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(reqs));

            var result = await _service.GetTotalPlannedCostAsync("AH0033");

            Assert.True(result.Success);
            Assert.Equal(0m, result.Data);
        }

        #endregion

        #region DeleteMonthlyOutputCalcsAsync

        [Fact]
        public async Task DeleteMonthlyOutputCalcsAsync_WithValidKey_ReturnsTrueResponse()
        {
            _apiClient.DeleteMonthlyOutputCalcsAsync("AH0033", "TC01", 1.0, "WG1")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            var result = await _service.DeleteMonthlyOutputCalcsAsync("AH0033", "TC01", 1.0, "WG1");

            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteMonthlyOutputCalcsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            _apiClient.DeleteMonthlyOutputCalcsAsync("AH0033", "TC01", 1.0, "WG1")
                .Returns(ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "err" } }, new ApiMetaDto()));

            Assert.False((await _service.DeleteMonthlyOutputCalcsAsync("AH0033", "TC01", 1.0, "WG1")).Success);
        }

        #endregion
    }
}