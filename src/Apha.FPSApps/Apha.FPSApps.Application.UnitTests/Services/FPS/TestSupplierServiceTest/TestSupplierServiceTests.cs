using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.TestSupplierServiceTest
{
    public class TestSupplierServiceTests
    {
        private const string DefaultTestCode = "TST001";
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 10;

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

        private static QueryParameters<string> DefaultQuery() =>
            new() { Page = DefaultPageNumber, PageSize = DefaultPageSize };

        private static ApiResponseDto<List<TestSupplierViewDto>> BuildSuccessResponse(int count = 2) =>
            ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(
                Enumerable.Range(1, count).Select(i => new TestSupplierViewDto
                {
                    TestCode = DefaultTestCode,
                    Buyer = $"B{i:D3}"
                }).ToList(),
                new PaginationDto { PageNumber = 1, PageSize = DefaultPageSize, TotalRecords = count });

        #region GetPagedTestSupplierAsync Tests

        [Fact]
        public async Task GetPagedTestSupplierAsync_WithSuccessResponse_ReturnsDtoList()
        {
            // Arrange
            var query = DefaultQuery();
            var expected = BuildSuccessResponse();

            _fpsTestSupplierApiClient.GetPagedTestSupplierAsync(query, DefaultTestCode, false)
                .Returns(expected);

            // Act
            var result = await _sut.GetPagedTestSupplierAsync(query, DefaultTestCode, showRejected: false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsTestSupplierApiClient.Received(1)
                .GetPagedTestSupplierAsync(query, DefaultTestCode, false);
        }

        [Fact]
        public async Task GetPagedTestSupplierAsync_ShowRejectedTrue_PassesFlagToClient()
        {
            // Arrange
            var query = DefaultQuery();
            var expected = BuildSuccessResponse(1);

            _fpsTestSupplierApiClient.GetPagedTestSupplierAsync(query, DefaultTestCode, true)
                .Returns(expected);

            // Act
            var result = await _sut.GetPagedTestSupplierAsync(query, DefaultTestCode, showRejected: true);

            // Assert
            Assert.NotNull(result);
            await _fpsTestSupplierApiClient.Received(1)
                .GetPagedTestSupplierAsync(query, DefaultTestCode, true);
        }

        [Fact]
        public async Task GetPagedTestSupplierAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = DefaultQuery();
            var failureResponse = new ApiResponseDto<List<TestSupplierViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "API error" } }
            };

            _fpsTestSupplierApiClient.GetPagedTestSupplierAsync(query, DefaultTestCode, false)
                .Returns(failureResponse);

            // Act
            var result = await _sut.GetPagedTestSupplierAsync(query, DefaultTestCode, false);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPagedTestSupplierAsync_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = DefaultQuery();
            var emptyResponse = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(
                new List<TestSupplierViewDto>(),
                new PaginationDto { PageNumber = 1, PageSize = DefaultPageSize, TotalRecords = 0 });

            _fpsTestSupplierApiClient.GetPagedTestSupplierAsync(query, DefaultTestCode, false)
                .Returns(emptyResponse);

            // Act
            var result = await _sut.GetPagedTestSupplierAsync(query, DefaultTestCode, false);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedTestSupplierAsync_ClientThrows_PropagatesException()
        {
            // Arrange
            var query = DefaultQuery();

            _fpsTestSupplierApiClient.GetPagedTestSupplierAsync(query, DefaultTestCode, false)
                .Throws(new Exception("Client error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _sut.GetPagedTestSupplierAsync(query, DefaultTestCode, false));
        }

        [Fact]
        public async Task GetPagedTestSupplierAsync_DelegatesToFpsTestSupplierApiClient()
        {
            // Arrange
            var query = DefaultQuery();
            var expected = BuildSuccessResponse();

            _fpsTestSupplierApiClient.GetPagedTestSupplierAsync(query, DefaultTestCode, false)
                .Returns(expected);

            // Act
            await _sut.GetPagedTestSupplierAsync(query, DefaultTestCode, false);

            // Assert
            _ = _fpsClient.Received(1).FpsTestSupplier;
        }

        #endregion
    }
}
