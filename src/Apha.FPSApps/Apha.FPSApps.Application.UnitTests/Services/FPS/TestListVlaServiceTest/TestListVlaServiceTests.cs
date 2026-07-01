/*
 * TRANSFORMENGINE MIGRATION — TestListVlaServiceTests.cs (FPSApps frontend Application)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New xUnit test class for the frontend TestListVlaService (thin delegate)
 *   - Verifies each of the 6 methods delegates exactly to IFpsApiClient.FpsTestListVla
 *   - NSubstitute used for IFpsApiClient and IFpsTestListVlaApiClient mocks
 *   - Covers happy path (success response), failure path, and Received(1) delegation verification
 *
 * PRESERVED:
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult]
 *   - Thin-delegate pattern: no business logic in service, only forwarding verified
 *
 * DEFERRED: none — fully automated.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.TestListVlaServiceTest
{
    public class TestListVlaServiceTests
    {
        private const string DefaultItemCode = "TEST001";
        private const int DefaultFpsYear = 2025;

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsTestListVlaApiClient _testListVlaClient;
        private readonly TestListVlaService _service;

        public TestListVlaServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _testListVlaClient = Substitute.For<IFpsTestListVlaApiClient>();
            // TRANSFORMENGINE: wire sub-client into aggregate client mock
            _fpsClient.FpsTestListVla.Returns(_testListVlaClient);
            _service = new TestListVlaService(_fpsClient);
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var expected = ApiResponseDto<List<TestListVlaDto>>.SuccessResponse(
                new List<TestListVlaDto> { new() { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear } },
                new PaginationDto { TotalRecords = 1 });
            _testListVlaClient.GetAllAsync(query, DefaultFpsYear).Returns(expected);

            // Act
            var result = await _service.GetAllAsync(query, DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _testListVlaClient.Received(1).GetAllAsync(query, DefaultFpsYear);
        }

        [Fact]
        public async Task GetAllAsync_ApiClientReturnsEmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var expected = ApiResponseDto<List<TestListVlaDto>>.SuccessResponse(
                new List<TestListVlaDto>(), new PaginationDto { TotalRecords = 0 });
            _testListVlaClient.GetAllAsync(query, DefaultFpsYear).Returns(expected);

            // Act
            var result = await _service.GetAllAsync(query, DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var expected = ApiResponseDto<List<TestListVlaDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "SERVER_ERROR", Message = "Server error" } },
                new ApiMetaDto());
            _testListVlaClient.GetAllAsync(query, DefaultFpsYear).Returns(expected);

            // Act
            var result = await _service.GetAllAsync(query, DefaultFpsYear);

            // Assert
            Assert.False(result.Success);
            await _testListVlaClient.Received(1).GetAllAsync(query, DefaultFpsYear);
        }

        #endregion

        #region GetAllByYearAsync

        [Fact]
        public async Task GetAllByYearAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = ApiResponseDto<List<TestListVlaDto>>.SuccessResponse(
                new List<TestListVlaDto> { new() { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear } });
            _testListVlaClient.GetAllByYearAsync(DefaultFpsYear).Returns(expected);

            // Act
            var result = await _service.GetAllByYearAsync(DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
            await _testListVlaClient.Received(1).GetAllByYearAsync(DefaultFpsYear);
        }

        [Fact]
        public async Task GetAllByYearAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = ApiResponseDto<List<TestListVlaDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto());
            _testListVlaClient.GetAllByYearAsync(DefaultFpsYear).Returns(expected);

            // Act
            var result = await _service.GetAllByYearAsync(DefaultFpsYear);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var expected = ApiResponseDto<TestListVlaDto>.SuccessResponse(dto);
            _testListVlaClient.GetByIdAsync(DefaultItemCode, DefaultFpsYear).Returns(expected);

            // Act
            var result = await _service.GetByIdAsync(DefaultItemCode, DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(DefaultItemCode, result.Data!.ItemCode);
            await _testListVlaClient.Received(1).GetByIdAsync(DefaultItemCode, DefaultFpsYear);
        }

        [Fact]
        public async Task GetByIdAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = ApiResponseDto<TestListVlaDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto());
            _testListVlaClient.GetByIdAsync("NOTEXIST", DefaultFpsYear).Returns(expected);

            // Act
            var result = await _service.GetByIdAsync("NOTEXIST", DefaultFpsYear);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var expected = ApiResponseDto<TestListVlaDto>.SuccessResponse(dto);
            _testListVlaClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _testListVlaClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var expected = ApiResponseDto<TestListVlaDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "DUPLICATE", Message = "Duplicate key" } },
                new ApiMetaDto());
            _testListVlaClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var expected = ApiResponseDto<TestListVlaDto>.SuccessResponse(dto);
            _testListVlaClient.UpdateAsync(DefaultItemCode, DefaultFpsYear, dto).Returns(expected);

            // Act
            var result = await _service.UpdateAsync(DefaultItemCode, DefaultFpsYear, dto);

            // Assert
            Assert.True(result.Success);
            await _testListVlaClient.Received(1).UpdateAsync(DefaultItemCode, DefaultFpsYear, dto);
        }

        [Fact]
        public async Task UpdateAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var expected = ApiResponseDto<TestListVlaDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto());
            _testListVlaClient.UpdateAsync(DefaultItemCode, DefaultFpsYear, dto).Returns(expected);

            // Act
            var result = await _service.UpdateAsync(DefaultItemCode, DefaultFpsYear, dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _testListVlaClient.DeleteAsync(DefaultItemCode, DefaultFpsYear).Returns(expected);

            // Act
            var result = await _service.DeleteAsync(DefaultItemCode, DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _testListVlaClient.Received(1).DeleteAsync(DefaultItemCode, DefaultFpsYear);
        }

        [Fact]
        public async Task DeleteAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto());
            _testListVlaClient.DeleteAsync("NOTEXIST", DefaultFpsYear).Returns(expected);

            // Act
            var result = await _service.DeleteAsync("NOTEXIST", DefaultFpsYear);

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
