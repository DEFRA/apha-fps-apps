using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PIMS;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PIMS.ProjectListServiceTest
{
    public class ProjectListServiceTests
    {
        private readonly IPimsApiClient _pimsApiClient;
        private readonly IPimsProjectListApiClient _pimsProjectListApiClient;
        private readonly ProjectListService _projectListService;

        public ProjectListServiceTests()
        {
            _pimsApiClient = Substitute.For<IPimsApiClient>();
            _pimsProjectListApiClient = Substitute.For<IPimsProjectListApiClient>();
            _pimsApiClient.PimsProjectList.Returns(_pimsProjectListApiClient);
            _projectListService = new ProjectListService(_pimsApiClient);
        }

        #region GetAllProjectsAsync Tests

        [Fact]
        public async Task GetAllProjectsAsync_WithSuccessResponse_ReturnsProjectList()
        {
            // Arrange
            var queryParameters = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Search = "Test"
            };
            var projects = new List<ProjectListViewDto>
            {
                new ProjectListViewDto { Parentproject = "PP001", Program = "Program A", Customer = "Customer A", OnFps = "Yes" },
                new ProjectListViewDto { Parentproject = "PP002", Program = "Program B", Customer = "Customer B", OnFps = "No" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(
                projects,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _pimsProjectListApiClient.GetAllProjectsAsync(queryParameters).Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetAllProjectsAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsProjectListApiClient.Received(1).GetAllProjectsAsync(queryParameters);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(new List<ProjectListViewDto>());

            _pimsProjectListApiClient.GetAllProjectsAsync(queryParameters).Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetAllProjectsAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectListViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectListApiClient.GetAllProjectsAsync(queryParameters).Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetAllProjectsAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetAllProjectsAsync_PassesCorrectQueryParameters()
        {
            // Arrange
            var queryParameters = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 20,
                Search = "Project",
                SortBy = "Parentproject",
                Descending = false
            };
            var expectedResponse = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(new List<ProjectListViewDto>());

            _pimsProjectListApiClient.GetAllProjectsAsync(queryParameters).Returns(expectedResponse);

            // Act
            await _projectListService.GetAllProjectsAsync(queryParameters);

            // Assert
            await _pimsProjectListApiClient.Received(1).GetAllProjectsAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 2 &&
                    q.PageSize == 20 &&
                    q.Search == "Project" &&
                    q.SortBy == "Parentproject" &&
                    q.Descending == false
                )
            );
        }

        #endregion

        #region GetFpsProjectByIdAsync Tests

        [Fact]
        public async Task GetFpsProjectByIdAsync_WithValidParentProject_ReturnsProject()
        {
            // Arrange
            var parentproject = "PP001";
            var project = new ProjectDto
            {
                Parentproject = parentproject,
                Projecttitle = "Test FPS Project",
                Projectstatus = "Active"
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);

            _pimsProjectListApiClient.GetFpsProjectByIdAsync(parentproject).Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetFpsProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(parentproject, result.Data.Parentproject);
            await _pimsProjectListApiClient.Received(1).GetFpsProjectByIdAsync(parentproject);
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_WhenProjectNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Project not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectListApiClient.GetFpsProjectByIdAsync(parentproject).Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetFpsProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_PassesCorrectParentProject()
        {
            // Arrange
            var parentproject = "PP123";
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto { Parentproject = parentproject });

            _pimsProjectListApiClient.GetFpsProjectByIdAsync(parentproject).Returns(expectedResponse);

            // Act
            await _projectListService.GetFpsProjectByIdAsync(parentproject);

            // Assert
            await _pimsProjectListApiClient.Received(1).GetFpsProjectByIdAsync(parentproject);
        }

        #endregion

        #region GetProposedProjectByIdAsync Tests

        [Fact]
        public async Task GetProposedProjectByIdAsync_WithValidParentProject_ReturnsProposedProject()
        {
            // Arrange
            var parentproject = "PP001";
            var proposedProject = new ProposedProjectDto
            {
                Id = 1,
                Parentproject = parentproject,
                Projecttitle = "Test Proposed Project",
                Projectstatus = "Proposed"
            };
            var expectedResponse = ApiResponseDto<ProposedProjectDto>.SuccessResponse(proposedProject);

            _pimsProjectListApiClient.GetProposedProjectByIdAsync(parentproject).Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetProposedProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(parentproject, result.Data.Parentproject);
            await _pimsProjectListApiClient.Received(1).GetProposedProjectByIdAsync(parentproject);
        }

        [Fact]
        public async Task GetProposedProjectByIdAsync_WhenProjectNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Proposed project not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<ProposedProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectListApiClient.GetProposedProjectByIdAsync(parentproject).Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetProposedProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetProposedProjectByIdAsync_PassesCorrectParentProject()
        {
            // Arrange
            var parentproject = "PP123";
            var expectedResponse = ApiResponseDto<ProposedProjectDto>.SuccessResponse(new ProposedProjectDto { Parentproject = parentproject });

            _pimsProjectListApiClient.GetProposedProjectByIdAsync(parentproject).Returns(expectedResponse);

            // Act
            await _projectListService.GetProposedProjectByIdAsync(parentproject);

            // Assert
            await _pimsProjectListApiClient.Received(1).GetProposedProjectByIdAsync(parentproject);
        }

        #endregion

        #region GetYearlyDetailsByProjectAsync Tests

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_WithValidParentProject_ReturnsYearlyDetails()
        {
            // Arrange
            var parentproject = "PP001";
            var yearlyDetails = new List<ProjectsDto>
            {
                new ProjectsDto { Year = 2023, Parentproject = parentproject, Program = "Program A", Customer = "Customer A", Manager = "Manager A" },
                new ProjectsDto { Year = 2024, Parentproject = parentproject, Program = "Program B", Customer = "Customer B", Manager = "Manager B" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectsDto>>.SuccessResponse(yearlyDetails);

            _pimsProjectListApiClient.GetYearlyDetailsByProjectAsync(parentproject).Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetYearlyDetailsByProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsProjectListApiClient.Received(1).GetYearlyDetailsByProjectAsync(parentproject);
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var parentproject = "PP001";
            var expectedResponse = ApiResponseDto<List<ProjectsDto>>.SuccessResponse(new List<ProjectsDto>());

            _pimsProjectListApiClient.GetYearlyDetailsByProjectAsync(parentproject).Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetYearlyDetailsByProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Yearly details not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectsDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectListApiClient.GetYearlyDetailsByProjectAsync(parentproject).Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetYearlyDetailsByProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_PassesCorrectParentProject()
        {
            // Arrange
            var parentproject = "PP123";
            var expectedResponse = ApiResponseDto<List<ProjectsDto>>.SuccessResponse(new List<ProjectsDto>());

            _pimsProjectListApiClient.GetYearlyDetailsByProjectAsync(parentproject).Returns(expectedResponse);

            // Act
            await _projectListService.GetYearlyDetailsByProjectAsync(parentproject);

            // Assert
            await _pimsProjectListApiClient.Received(1).GetYearlyDetailsByProjectAsync(parentproject);
        }

        #endregion

        #region GetAllProjectsListAsync Tests

        [Fact]
        public async Task GetAllProjectsListAsync_WithSuccessResponse_ReturnsProjectList()
        {
            // Arrange
            var projects = new List<ProjectListViewDto>
            {
                new() { Parentproject = "PP001", Program = "Program A", Customer = "Customer A", OnFps = "Yes" },
                new() { Parentproject = "PP002", Program = "Program B", Customer = "Customer B", OnFps = "No" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(projects);

            _pimsProjectListApiClient.GetAllProjectsListAsync().Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetAllProjectsListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsProjectListApiClient.Received(1).GetAllProjectsListAsync();
        }

        [Fact]
        public async Task GetAllProjectsListAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(new List<ProjectListViewDto>());

            _pimsProjectListApiClient.GetAllProjectsListAsync().Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetAllProjectsListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _pimsProjectListApiClient.Received(1).GetAllProjectsListAsync();
        }

        [Fact]
        public async Task GetAllProjectsListAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectListViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectListApiClient.GetAllProjectsListAsync().Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetAllProjectsListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            await _pimsProjectListApiClient.Received(1).GetAllProjectsListAsync();
        }

        #endregion

        #region GetAllProjectsForMilestoneAsync Tests

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_WithSuccessResponse_ReturnsMilestoneList()
        {
            // Arrange
            var milestones = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001", Program = "Program A", Customer = "Customer A", ProjectGroup = "GRP1" },
                new() { Parentproject = "PP002", Program = "Program B", Customer = "Customer B", ProjectGroup = "GRP2" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectListMilestoneDto>>.SuccessResponse(milestones);

            _pimsProjectListApiClient.GetAllProjectsForMilestoneAsync().Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetAllProjectsForMilestoneAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal("GRP1", result.Data![0].ProjectGroup);
            await _pimsProjectListApiClient.Received(1).GetAllProjectsForMilestoneAsync();
        }

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ProjectListMilestoneDto>>.SuccessResponse(new List<ProjectListMilestoneDto>());

            _pimsProjectListApiClient.GetAllProjectsForMilestoneAsync().Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetAllProjectsForMilestoneAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _pimsProjectListApiClient.Received(1).GetAllProjectsForMilestoneAsync();
        }

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectListMilestoneDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectListApiClient.GetAllProjectsForMilestoneAsync().Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetAllProjectsForMilestoneAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            await _pimsProjectListApiClient.Received(1).GetAllProjectsForMilestoneAsync();
        }

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_MapsProjectGroupCorrectly()
        {
            // Arrange
            var milestones = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001", Program = "Program A", Customer = "Customer A", ProjectGroup = null }
            };
            var expectedResponse = ApiResponseDto<List<ProjectListMilestoneDto>>.SuccessResponse(milestones);

            _pimsProjectListApiClient.GetAllProjectsForMilestoneAsync().Returns(expectedResponse);

            // Act
            var result = await _projectListService.GetAllProjectsForMilestoneAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Null(result.Data![0].ProjectGroup);
            await _pimsProjectListApiClient.Received(1).GetAllProjectsForMilestoneAsync();
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidClient_InitializesService()
        {
            // Arrange & Act
            var service = new ProjectListService(_pimsApiClient);

            // Assert
            Assert.NotNull(service);
        }

        #endregion
    }
}