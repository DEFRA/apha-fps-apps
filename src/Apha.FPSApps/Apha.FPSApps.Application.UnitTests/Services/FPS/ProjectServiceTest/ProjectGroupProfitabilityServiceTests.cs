using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ProjectServiceTest
{
    public class ProjectGroupProfitabilityServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProjectApiClient _fpsProjectApiClient;
        private readonly ProjectService _sut;

        public ProjectGroupProfitabilityServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsProjectApiClient = Substitute.For<IFpsProjectApiClient>();
            _fpsClient.FpsProject.Returns(_fpsProjectApiClient);
            _sut = new ProjectService(_fpsClient);
        }

        // ── GetProjectGroupProfitabilityAsync ─────────────────────────────────

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WithSuccessResponse_ReturnsProfitabilityList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var workTypeFilter = "all";

            var items = new List<ProjectProfitabilityDto>
            {
                new() { JobCode = "PP001", JcTotalStaffCosts = 1000m, BudgetCvl = 5000m, JcProfit = 4000m, TargetProfit = 3500m, OffTarget = 500m },
                new() { JobCode = "PP002", JcTotalStaffCosts = 2000m, BudgetCvl = 6000m, JcProfit = 4000m, TargetProfit = 3000m, OffTarget = 1000m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectProfitabilityDto>>.SuccessResponse(
                items,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _fpsProjectApiClient.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal("PP001", result.Data![0].JobCode);
            await _fpsProjectApiClient.Received(1).GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var workTypeFilter = "all";
            var expectedResponse = ApiResponseDto<List<ProjectProfitabilityDto>>.SuccessResponse(
                new List<ProjectProfitabilityDto>(),
                new PaginationDto { TotalRecords = 0 });

            _fpsProjectApiClient.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WithFailureResponse_ReturnsFailure()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var workTypeFilter = "all";
            var errors = new List<ApiErrorDto> { new() { Message = "Group profitability fetch failed", Code = "PROFITABILITY_ERROR" } };
            var failureResponse = ApiResponseDto<List<ProjectProfitabilityDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProjectApiClient.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter)
                .Returns(failureResponse);

            // Act
            var result = await _sut.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("PROFITABILITY_ERROR", result.Errors![0].Code);
        }

        [Theory]
        [InlineData("approved")]
        [InlineData("not-approved")]
        [InlineData("all")]
        public async Task GetProjectGroupProfitabilityAsync_WithDifferentWorkTypeFilters_ForwardsFilterToApiClient(string workTypeFilter)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var expectedResponse = ApiResponseDto<List<ProjectProfitabilityDto>>.SuccessResponse(
                new List<ProjectProfitabilityDto>(), new PaginationDto());

            _fpsProjectApiClient.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);

            // Assert
            Assert.True(result.Success);
            await _fpsProjectApiClient.Received(1).GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WhenApiClientThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            _fpsProjectApiClient.GetProjectGroupProfitabilityAsync(query, projectGroup, "all")
                .Returns(Task.FromException<ApiResponseDto<List<ProjectProfitabilityDto>>>(
                    new HttpRequestException("API unavailable")));

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => _sut.GetProjectGroupProfitabilityAsync(query, projectGroup, "all"));
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_DelegatesToFpsProjectApiClient_NotOtherClients()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 5 };
            var projectGroup = "Group2";
            var workTypeFilter = "approved";
            var expectedResponse = ApiResponseDto<List<ProjectProfitabilityDto>>.SuccessResponse(
                new List<ProjectProfitabilityDto>(), new PaginationDto());

            _fpsProjectApiClient.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter)
                .Returns(expectedResponse);

            // Act
            await _sut.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);

            // Assert — verify strict delegation; programme profitability must not be called
            await _fpsProjectApiClient.Received(1).GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);
            await _fpsProjectApiClient.DidNotReceive().GetProjectProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
