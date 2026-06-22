using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.BosworthInterfaceServiceTest
{
    public class BosworthInterfaceServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactBosworthInterfaceApiClient _apiClient;
        private readonly BosworthInterfaceService _service;

        public BosworthInterfaceServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _apiClient = Substitute.For<IPactBosworthInterfaceApiClient>();
            _pactClient.PactBosworthInterface.Returns(_apiClient);
            _service = new BosworthInterfaceService(_pactClient);
        }

        #region GetTimePurchaseProjectAsync

        [Fact]
        public async Task GetTimePurchaseProjectAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<List<TimePurchaseProjectDto>>.SuccessResponse(
                [new TimePurchaseProjectDto { Project = "P1", SellingWg = "WG1" }]);
            _apiClient.GetTimePurchaseProjectAsync("P1").Returns(expected);

            var result = await _service.GetTimePurchaseProjectAsync("P1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTimePurchaseProjectAsync("P1");
        }

        [Fact]
        public async Task GetTimePurchaseProjectAsync_WhenApiReturnsFailure_ReturnsFailure()
        {
            var expected = ApiResponseDto<List<TimePurchaseProjectDto>>.FailureResponse(
                [new ApiErrorDto { Code = "ERR", Message = "Error" }], new ApiMetaDto());
            _apiClient.GetTimePurchaseProjectAsync("P1").Returns(expected);

            var result = await _service.GetTimePurchaseProjectAsync("P1");

            Assert.False(result.Success);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetTimePurchaseProjectAsync_WhenApiReturnsEmptyList_ReturnsEmptyList()
        {
            var expected = ApiResponseDto<List<TimePurchaseProjectDto>>.SuccessResponse([]);
            _apiClient.GetTimePurchaseProjectAsync("P1").Returns(expected);

            var result = await _service.GetTimePurchaseProjectAsync("P1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetTimeSaleProfitCentreAsync

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<List<TimeSaleProfitCentreDto>>.SuccessResponse(
                [new TimeSaleProfitCentreDto { ProfitCentre = "PC1", WorkGroup = "WG1" }]);
            _apiClient.GetTimeSaleProfitCentreAsync("PC1").Returns(expected);

            var result = await _service.GetTimeSaleProfitCentreAsync("PC1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTimeSaleProfitCentreAsync("PC1");
        }

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_WhenApiReturnsFailure_ReturnsFailure()
        {
            var expected = ApiResponseDto<List<TimeSaleProfitCentreDto>>.FailureResponse(
                [new ApiErrorDto { Code = "ERR", Message = "Error" }], new ApiMetaDto());
            _apiClient.GetTimeSaleProfitCentreAsync("PC1").Returns(expected);

            var result = await _service.GetTimeSaleProfitCentreAsync("PC1");

            Assert.False(result.Success);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_WhenApiReturnsEmptyList_ReturnsEmptyList()
        {
            var expected = ApiResponseDto<List<TimeSaleProfitCentreDto>>.SuccessResponse([]);
            _apiClient.GetTimeSaleProfitCentreAsync("PC1").Returns(expected);

            var result = await _service.GetTimeSaleProfitCentreAsync("PC1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetTestSaleSellingWorkgroupAsync

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<List<TestSaleSellingWorkgroupDto>>.SuccessResponse(
                [new TestSaleSellingWorkgroupDto { SellerWG = "WG1", TestCode = "TC1" }]);
            _apiClient.GetTestSaleSellingWorkgroupAsync("WG1").Returns(expected);

            var result = await _service.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTestSaleSellingWorkgroupAsync("WG1");
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_WhenApiReturnsFailure_ReturnsFailure()
        {
            var expected = ApiResponseDto<List<TestSaleSellingWorkgroupDto>>.FailureResponse(
                [new ApiErrorDto { Code = "ERR", Message = "Error" }], new ApiMetaDto());
            _apiClient.GetTestSaleSellingWorkgroupAsync("WG1").Returns(expected);

            var result = await _service.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.False(result.Success);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_WhenApiReturnsEmptyList_ReturnsEmptyList()
        {
            var expected = ApiResponseDto<List<TestSaleSellingWorkgroupDto>>.SuccessResponse([]);
            _apiClient.GetTestSaleSellingWorkgroupAsync("WG1").Returns(expected);

            var result = await _service.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetTestSaleBuyingProjectAsync

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<List<TestSaleBuyingProjectDto>>.SuccessResponse(
                [new TestSaleBuyingProjectDto { Buyer = "B1", TestCode = "TC1" }]);
            _apiClient.GetTestSaleBuyingProjectAsync("PP1").Returns(expected);

            var result = await _service.GetTestSaleBuyingProjectAsync("PP1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTestSaleBuyingProjectAsync("PP1");
        }

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_WhenApiReturnsFailure_ReturnsFailure()
        {
            var expected = ApiResponseDto<List<TestSaleBuyingProjectDto>>.FailureResponse(
                [new ApiErrorDto { Code = "ERR", Message = "Error" }], new ApiMetaDto());
            _apiClient.GetTestSaleBuyingProjectAsync("PP1").Returns(expected);

            var result = await _service.GetTestSaleBuyingProjectAsync("PP1");

            Assert.False(result.Success);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_WhenApiReturnsEmptyList_ReturnsEmptyList()
        {
            var expected = ApiResponseDto<List<TestSaleBuyingProjectDto>>.SuccessResponse([]);
            _apiClient.GetTestSaleBuyingProjectAsync("PP1").Returns(expected);

            var result = await _service.GetTestSaleBuyingProjectAsync("PP1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion
    }
}
