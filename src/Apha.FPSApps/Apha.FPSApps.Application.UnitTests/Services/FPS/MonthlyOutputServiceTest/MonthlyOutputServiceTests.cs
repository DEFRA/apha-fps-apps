using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.MonthlyOutputServiceTest
{
    public class MonthlyOutputServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsMonthlyOutputApiClient _apiClient;
        private readonly MonthlyOutputService _service;

        public MonthlyOutputServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _apiClient = Substitute.For<IFpsMonthlyOutputApiClient>();
            _fpsClient.FpsMonthlyOutput.Returns(_apiClient);
            _service = new MonthlyOutputService(_fpsClient);
        }

        private static QueryParameters<string> Q() => new() { Page = 1, PageSize = 10 };
        private static Dictionary<(string, string), decimal> EmptyLookup() => new();
        private static Dictionary<(string, string), decimal> PriceLookup(string testCode, string buyer, decimal price)
            => new() { { (testCode, buyer), price } };

        #region GetMonthlyOutputByProjectAsync

        [Fact]
        public async Task GetMonthlyOutputByProjectAsync_WithSuccessResponse_ReturnsDtoList()
        {
            var items = new List<MonthlyOutputDto> { new() { Buyer = "AH0033", TestCode = "TC01" }, new() { Buyer = "AH0033", TestCode = "TC02" } };
            var resp = ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(items, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033").Returns(resp);

            var result = await _service.GetMonthlyOutputByProjectAsync(Q(), "AH0033", EmptyLookup());

            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputByProjectAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            var resp = ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(new List<MonthlyOutputDto>());
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033").Returns(resp);

            var result = await _service.GetMonthlyOutputByProjectAsync(Q(), "AH0033", EmptyLookup());

            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetMonthlyOutputByProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "err" } };
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputDto>>.FailureResponse(errors, new ApiMetaDto()));

            var result = await _service.GetMonthlyOutputByProjectAsync(Q(), "AH0033", EmptyLookup());

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetMonthlyOutputByProjectAsync_EnrichesItemsWithPricesFromLookup()
        {
            var items = new List<MonthlyOutputDto> { new() { Buyer = "AH0033", TestCode = "TC01", Volume = 4 } };
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(items));

            var result = await _service.GetMonthlyOutputByProjectAsync(Q(), "AH0033", PriceLookup("TC01", "AH0033", 100m));

            Assert.Equal(100.0, result.Data![0].TestPrice);
            Assert.Equal(400.0, result.Data![0].Charge);
        }

        [Fact]
        public async Task GetMonthlyOutputByProjectAsync_WithNullTestCodeAndBuyer_FallsBackToEmptyStringKey()
        {
            var items = new List<MonthlyOutputDto> { new() { TestCode = null, Buyer = null, Volume = 2 } };
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(items));
            var lookup = new Dictionary<(string, string), decimal> { { (string.Empty, string.Empty), 50m } };

            var result = await _service.GetMonthlyOutputByProjectAsync(Q(), "AH0033", lookup);

            Assert.Equal(50.0,  result.Data![0].TestPrice);
            Assert.Equal(100.0, result.Data![0].Charge);
        }

        [Fact]
        public async Task GetMonthlyOutputByProjectAsync_WithNullVolume_ChargeIsZero()
        {
            var items = new List<MonthlyOutputDto> { new() { Buyer = "AH0033", TestCode = "TC01", Volume = null } };
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(items));

            var result = await _service.GetMonthlyOutputByProjectAsync(Q(), "AH0033", PriceLookup("TC01", "AH0033", 100m));

            Assert.Equal(100.0, result.Data![0].TestPrice);
            Assert.Equal(0.0,   result.Data![0].Charge);
        }

        [Fact]
        public async Task GetMonthlyOutputByProjectAsync_SetsAllDtoProperties()
        {
            var items = new List<MonthlyOutputDto>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", Volume = 3, Month = 1.0, WorkGroup = "WG1", FpsYear = 2024 }
            };
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(items));

            var result = await _service.GetMonthlyOutputByProjectAsync(Q(), "AH0033", EmptyLookup());

            var item = result.Data![0];
            Assert.Equal(1.0,   item.Month);
            Assert.Equal("WG1", item.WorkGroup);
            Assert.Equal(2024,  item.FpsYear);
        }

        [Fact]
        public async Task GetMonthlyOutputByProjectAsync_DelegatesToFpsMonthlyOutputApiClient()
        {
            var resp = ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(new List<MonthlyOutputDto>());
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033").Returns(resp);

            await _service.GetMonthlyOutputByProjectAsync(Q(), "AH0033", EmptyLookup());

            await _apiClient.Received(1).GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033");
            _ = _fpsClient.Received(1).FpsMonthlyOutput;
        }

        #endregion

        #region GetTotalActualByProjectAsync

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithSuccessResponse_ReturnsTotalCost()
        {
            var items = new List<MonthlyOutputDto> { new() { Buyer = "AH0033", TestCode = "TC01", Volume = 5 } };
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(items));

            var result = await _service.GetTotalActualByProjectAsync("AH0033", PriceLookup("TC01", "AH0033", 100m));

            Assert.True(result.Success);
            Assert.Equal(500, result.Data);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WhenApiFails_ReturnsZeroCost()
        {
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "err" } }, new ApiMetaDto()));

            var result = await _service.GetTotalActualByProjectAsync("AH0033", EmptyLookup());

            Assert.True(result.Success);
            Assert.Equal(0, result.Data);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithNoPriceMatch_TotalCostIsZero()
        {
            var items = new List<MonthlyOutputDto> { new() { Buyer = "AH0033", TestCode = "TC01", Volume = 5 } };
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(items));

            var result = await _service.GetTotalActualByProjectAsync("AH0033", EmptyLookup());

            Assert.True(result.Success);
            Assert.Equal(0, result.Data);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithNullTestCodeAndBuyer_TreatsAsEmptyStringKey()
        {
            var items = new List<MonthlyOutputDto> { new() { TestCode = null, Buyer = null, Volume = 3 } };
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(items));
            var lookup = new Dictionary<(string, string), decimal> { { (string.Empty, string.Empty), 10m } };

            var result = await _service.GetTotalActualByProjectAsync("AH0033", lookup);

            Assert.True(result.Success);
            Assert.Equal(30, result.Data);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithNullVolume_TreatsAsZero()
        {
            var items = new List<MonthlyOutputDto> { new() { Buyer = "AH0033", TestCode = "TC01", Volume = null } };
            _apiClient.GetByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(items));

            var result = await _service.GetTotalActualByProjectAsync("AH0033", PriceLookup("TC01", "AH0033", 100m));

            Assert.True(result.Success);
            Assert.Equal(0, result.Data);
        }

        #endregion

        #region DeleteMonthlyOutputAsync

        [Fact]
        public async Task DeleteMonthlyOutputAsync_WithValidKey_ReturnsTrueResponse()
        {
            _apiClient.DeleteMonthlyOutputAsync("AH0033", "TC01", 1.0, "WG1")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            var result = await _service.DeleteMonthlyOutputAsync("AH0033", "TC01", 1.0, "WG1");

            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteMonthlyOutputAsync_WhenApiFails_ReturnsFailureResponse()
        {
            _apiClient.DeleteMonthlyOutputAsync("AH0033", "TC01", 1.0, "WG1")
                .Returns(ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "err" } }, new ApiMetaDto()));

            Assert.False((await _service.DeleteMonthlyOutputAsync("AH0033", "TC01", 1.0, "WG1")).Success);
        }

        #endregion
    }
}