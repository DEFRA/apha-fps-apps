using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.TestSupplierServiceTest
{
    public class TestSupplierServiceTests
    {
        private const string DefaultTestCode = "TEST001";
        private const string DefaultBuyer = "BUYER001";

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsTestSupplierApiClient _fpsTestSupplierApiClient;
        private readonly TestSupplierService _sut;

        public TestSupplierServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsTestSupplierApiClient = Substitute.For<IFpsTestSupplierApiClient>();
            _fpsClient.FpsTestSupplier.Returns(_fpsTestSupplierApiClient);
            _sut = new TestSupplierService(_fpsClient);
        }

        #region GetPagedAsync

        [Fact]
        public async Task GetPagedAsync_WithValidParameters_DelegatesToApiClient()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(new List<TestSupplierViewDto>());

            _fpsTestSupplierApiClient.GetPagedAsync(query, DefaultTestCode, false).Returns(expected);

            var result = await _sut.GetPagedAsync(query, DefaultTestCode, false);

            Assert.True(result.Success);
            await _fpsTestSupplierApiClient.Received(1).GetPagedAsync(query, DefaultTestCode, false);
        }

        [Fact]
        public async Task GetPagedAsync_WhenApiClientReturnsFailed_ReturnsFailure()
        {
            var query = new QueryParameters<string>();
            var failureResponse = ApiResponseDto<List<TestSupplierViewDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "500", Message = "Error" } }, new ApiMetaDto());

            _fpsTestSupplierApiClient.GetPagedAsync(query, DefaultTestCode, false).Returns(failureResponse);

            var result = await _sut.GetPagedAsync(query, DefaultTestCode, false);

            Assert.False(result.Success);
        }

        #endregion

        #region GetViewByIdAsync

        [Fact]
        public async Task GetViewByIdAsync_WithExistingRecord_ReturnsSuccessResponse()
        {
            var expected = ApiResponseDto<TestSupplierViewDto>.SuccessResponse(
                new TestSupplierViewDto { TestCode = DefaultTestCode, JobCode = DefaultBuyer });

            _fpsTestSupplierApiClient.GetViewByIdAsync(DefaultTestCode, DefaultBuyer).Returns(expected);

            var result = await _sut.GetViewByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.True(result.Success);
            Assert.Equal(DefaultBuyer, result.Data!.JobCode);
            await _fpsTestSupplierApiClient.Received(1).GetViewByIdAsync(DefaultTestCode, DefaultBuyer);
        }

        [Fact]
        public async Task GetViewByIdAsync_WhenNotFound_ReturnsFailureResponse()
        {
            var failureResponse = ApiResponseDto<TestSupplierViewDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "404", Message = "Not found" } }, new ApiMetaDto());

            _fpsTestSupplierApiClient.GetViewByIdAsync(DefaultTestCode, DefaultBuyer).Returns(failureResponse);

            var result = await _sut.GetViewByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.False(result.Success);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WithExistingRecord_ReturnsSuccessResponse()
        {
            var expected = ApiResponseDto<FpsTestRequirementDto>.SuccessResponse(
                new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer });

            _fpsTestSupplierApiClient.GetByIdAsync(DefaultTestCode, DefaultBuyer).Returns(expected);

            var result = await _sut.GetByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.True(result.Success);
            await _fpsTestSupplierApiClient.Received(1).GetByIdAsync(DefaultTestCode, DefaultBuyer);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsFailureResponse()
        {
            var failureResponse = ApiResponseDto<FpsTestRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "404", Message = "Not found" } }, new ApiMetaDto());

            _fpsTestSupplierApiClient.GetByIdAsync(DefaultTestCode, DefaultBuyer).Returns(failureResponse);

            var result = await _sut.GetByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.False(result.Success);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_WithValidDto_DelegatesToApiClient()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var expected = ApiResponseDto<FpsTestRequirementDto>.SuccessResponse(dto);

            _fpsTestSupplierApiClient.CreateAsync(dto).Returns(expected);

            var result = await _sut.CreateAsync(dto);

            Assert.True(result.Success);
            await _fpsTestSupplierApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_WhenApiClientReturnsFailed_ReturnsFailure()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var failureResponse = ApiResponseDto<FpsTestRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "400", Message = "Validation failed" } }, new ApiMetaDto());

            _fpsTestSupplierApiClient.CreateAsync(dto).Returns(failureResponse);

            var result = await _sut.CreateAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_WithValidDto_DelegatesToApiClient()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var expected = ApiResponseDto<FpsTestRequirementDto>.SuccessResponse(dto);

            _fpsTestSupplierApiClient.UpdateAsync(dto).Returns(expected);

            var result = await _sut.UpdateAsync(dto);

            Assert.True(result.Success);
            await _fpsTestSupplierApiClient.Received(1).UpdateAsync(dto);
        }

        [Fact]
        public async Task UpdateAsync_WhenApiClientReturnsFailed_ReturnsFailure()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var failureResponse = ApiResponseDto<FpsTestRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "404", Message = "Not found" } }, new ApiMetaDto());

            _fpsTestSupplierApiClient.UpdateAsync(dto).Returns(failureResponse);

            var result = await _sut.UpdateAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WhenSuccessful_ReturnsSuccessResponse()
        {
            var expected = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsTestSupplierApiClient.DeleteAsync(DefaultTestCode, DefaultBuyer).Returns(expected);

            var result = await _sut.DeleteAsync(DefaultTestCode, DefaultBuyer);

            Assert.True(result.Success);
            await _fpsTestSupplierApiClient.Received(1).DeleteAsync(DefaultTestCode, DefaultBuyer);
        }

        [Fact]
        public async Task DeleteAsync_WhenApiClientReturnsFailed_ReturnsFailure()
        {
            var failureResponse = ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "404", Message = "Not found" } }, new ApiMetaDto());

            _fpsTestSupplierApiClient.DeleteAsync(DefaultTestCode, DefaultBuyer).Returns(failureResponse);

            var result = await _sut.DeleteAsync(DefaultTestCode, DefaultBuyer);

            Assert.False(result.Success);
        }

        #endregion
    }
}

