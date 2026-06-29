using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ProjectServiceTest
{
    public class ProjectProfitabilityVlaServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProjectApiClient _fpsProjectApiClient;
        private readonly ProjectService _sut;

        public ProjectProfitabilityVlaServiceTests()
        {
            _fpsClient           = Substitute.For<IFpsApiClient>();
            _fpsProjectApiClient = Substitute.For<IFpsProjectApiClient>();
            _fpsClient.FpsProject.Returns(_fpsProjectApiClient);
            _sut = new ProjectService(_fpsClient);
        }

        #region GetProjectProfitabilityVlaAsync

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithSuccessResponse_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var items = new List<ProjectProfitabilityVlaDto>
            {
                new() { JobCode = "PP001", StaffCosts = 1000m, Budget = 5000m, Profit = 4000m, TargetProfit = 3500m, OffTarget = 500m },
                new() { JobCode = "PP002", StaffCosts = 2000m, Budget = 6000m, Profit = 4000m, TargetProfit = 3000m, OffTarget = 1000m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(
                items,
                new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 2 });

            _fpsProjectApiClient
                .GetProjectProfitabilityVlaAsync(query, null, null, null, null)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetProjectProfitabilityVlaAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal("PP001", result.Data![0].JobCode);
            await _fpsProjectApiClient.Received(1)
                .GetProjectProfitabilityVlaAsync(query, null, null, null, null);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var expectedResponse = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(
                new List<ProjectProfitabilityVlaDto>(),
                new PaginationDto { TotalRecords = 0 });

            _fpsProjectApiClient
                .GetProjectProfitabilityVlaAsync(query, null, null, null, null)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetProjectProfitabilityVlaAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithFailureResponse_ReturnsFailure()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "VLA profitability fetch failed", Code = "PROFITABILITY_VLA_ERROR" }
            };
            var failureResponse = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProjectApiClient
                .GetProjectProfitabilityVlaAsync(query, null, null, null, null)
                .Returns(failureResponse);

            // Act
            var result = await _sut.GetProjectProfitabilityVlaAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("PROFITABILITY_VLA_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_ForwardsAllFilterParamsToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            const string status   = "Approved";
            const string programNo = "P001";
            const string manager  = "John Smith";
            const string customer = "ACME Ltd";

            var expectedResponse = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(
                new List<ProjectProfitabilityVlaDto>(), new PaginationDto());

            _fpsProjectApiClient
                .GetProjectProfitabilityVlaAsync(query, status, programNo, manager, customer)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetProjectProfitabilityVlaAsync(query, status, programNo, manager, customer);

            // Assert — verify all 5 parameters are forwarded verbatim
            Assert.True(result.Success);
            await _fpsProjectApiClient.Received(1)
                .GetProjectProfitabilityVlaAsync(query, status, programNo, manager, customer);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_DelegatesToFpsProjectApiClient_NotOtherClients()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var expectedResponse = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(
                new List<ProjectProfitabilityVlaDto>(), new PaginationDto());

            _fpsProjectApiClient
                .GetProjectProfitabilityVlaAsync(query, null, null, null, null)
                .Returns(expectedResponse);

            // Act
            await _sut.GetProjectProfitabilityVlaAsync(query);

            // Assert — delegation strictly to FpsProject sub-client only
            await _fpsProjectApiClient.Received(1)
                .GetProjectProfitabilityVlaAsync(query, null, null, null, null);
            await _fpsProjectApiClient.DidNotReceive().GetAllProjectsAsync();
        }

        [Theory]
        [InlineData("Approved",     null,   null,         null)]
        [InlineData(null,           "P001", null,         null)]
        [InlineData(null,           null,   "John Smith",  null)]
        [InlineData(null,           null,   null,         "ACME Ltd")]
        [InlineData("Completed",    "P002", "Jane Doe",    "Beta Corp")]
        public async Task GetProjectProfitabilityVlaAsync_WithVariousFilterCombinations_ForwardsToApiClient(
            string? status, string? programNo, string? manager, string? customer)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var expectedResponse = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(
                new List<ProjectProfitabilityVlaDto>(), new PaginationDto());

            _fpsProjectApiClient
                .GetProjectProfitabilityVlaAsync(query, status, programNo, manager, customer)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetProjectProfitabilityVlaAsync(query, status, programNo, manager, customer);

            // Assert
            Assert.True(result.Success);
            await _fpsProjectApiClient.Received(1)
                .GetProjectProfitabilityVlaAsync(query, status, programNo, manager, customer);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WhenApiClientThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };

            _fpsProjectApiClient
                .GetProjectProfitabilityVlaAsync(query, null, null, null, null)
                .Returns(Task.FromException<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(
                    new HttpRequestException("API unavailable")));

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => _sut.GetProjectProfitabilityVlaAsync(query));
        }

        #endregion
    }
}
