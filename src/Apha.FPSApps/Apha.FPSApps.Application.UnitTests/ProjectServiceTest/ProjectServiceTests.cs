using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.ProjectServiceTest
{
    public class ProjectServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProjectApiClient _fpsProjectApiClient;
        private readonly ProjectService _projectService;

        public ProjectServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsProjectApiClient = Substitute.For<IFpsProjectApiClient>();
            _fpsClient.FpsProject.Returns(_fpsProjectApiClient);
            _projectService = new ProjectService(_fpsClient);
        }

        #region GetProjectsByProgramAsync Tests

        [Fact]
        public async Task GetProjectsByProgramAsync_WithSuccessResponse_ReturnsProjectList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project",  Program = "P001" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                projects,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _fpsProjectApiClient.GetProjectsByProgramAsync(query, programNo).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProjectApiClient.Received(1).GetProjectsByProgramAsync(query, programNo);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            );

            _fpsProjectApiClient.GetProjectsByProgramAsync(query, programNo).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProjectApiClient.GetProjectsByProgramAsync(query, programNo).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_CallsFpsProjectApiClient_WithCorrectArguments()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "parentproject" };
            var programNo = "P002";
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _fpsProjectApiClient.GetProjectsByProgramAsync(query, programNo).Returns(expectedResponse);

            // Act
            await _projectService.GetProjectsByProgramAsync(query, programNo);

            // Assert
            await _fpsProjectApiClient.Received(1).GetProjectsByProgramAsync(query, programNo);
        }

        #endregion
    }
}
