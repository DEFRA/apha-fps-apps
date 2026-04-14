using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ProjectServiceTest
{
    public class ProjectServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProjectApiClient _fpsProjectApiClient;
        private readonly IFpsLookupApiClient _fpsLookupApiClient;
        private readonly ProjectService _projectService;

        public ProjectServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsProjectApiClient = Substitute.For<IFpsProjectApiClient>();
            _fpsLookupApiClient = Substitute.For<IFpsLookupApiClient>();
            _fpsClient.FpsProject.Returns(_fpsProjectApiClient);
            _fpsClient.FpsLookup.Returns(_fpsLookupApiClient);
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

        #region GetAllPactProjectsAsync Tests

        [Fact]
        public async Task GetAllPactProjectsAsync_WithSuccessResponse_ReturnsProjectList()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "PACT Project One" },
                new() { ParentProject = "PP002", ProjectTitle = "PACT Project Two" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects);
            _fpsProjectApiClient.GetAllPactProjectsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllPactProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProjectApiClient.Received(1).GetAllPactProjectsAsync();
        }

        [Fact]
        public async Task GetAllPactProjectsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());
            _fpsProjectApiClient.GetAllPactProjectsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllPactProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllPactProjectsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetAllPactProjectsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllPactProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetPagedProjectsAsync Tests

        [Fact]
        public async Task GetPagedProjectsAsync_WithValidQuery_ReturnsPaginatedProjects()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "Test" };
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                projects,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );
            _fpsProjectApiClient.GetPagedProjectsAsync(query).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetPagedProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProjectApiClient.Received(1).GetPagedProjectsAsync(query);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetPagedProjectsAsync(query).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetPagedProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedPactProjectsAsync Tests

        [Fact]
        public async Task GetPagedPactProjectsAsync_WithValidQuery_ReturnsPaginatedPactProjects()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "PACT Project" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                projects,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );
            _fpsProjectApiClient.GetPagedPactProjectsAsync(query).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsProjectApiClient.Received(1).GetPagedPactProjectsAsync(query);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetPagedPactProjectsAsync(query).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetProjectByIdAsync Tests

        [Fact]
        public async Task GetProjectByIdAsync_WithValidId_ReturnsProject()
        {
            // Arrange
            var parentProject = "PP001";
            var project = new ProjectDto { ParentProject = parentProject, ProjectTitle = "Test Project", Program = "P001" };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);
            _fpsProjectApiClient.GetProjectByIdAsync(parentProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetProjectByIdAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(parentProject, result.Data?.ParentProject);
            await _fpsProjectApiClient.Received(1).GetProjectByIdAsync(parentProject);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var parentProject = "NONEXISTENT";
            var errors = new List<ApiErrorDto> { new() { Message = "Project not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetProjectByIdAsync(parentProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetProjectByIdAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateProjectAsync Tests

        [Fact]
        public async Task CreateProjectAsync_WithValidProject_ReturnsSuccessResponse()
        {
            // Arrange
            var newProject = new ProjectDto { ParentProject = "PP001", ProjectTitle = "New Project", Program = "P001" };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(newProject);
            _fpsProjectApiClient.CreateProjectAsync(newProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.CreateProjectAsync(newProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(newProject.ParentProject, result.Data?.ParentProject);
            await _fpsProjectApiClient.Received(1).CreateProjectAsync(newProject);
        }

        [Fact]
        public async Task CreateProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var newProject = new ProjectDto { ParentProject = "PP001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Duplicate project", Code = "DUPLICATE" } };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.CreateProjectAsync(newProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.CreateProjectAsync(newProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateProjectAsync Tests

        [Fact]
        public async Task UpdateProjectAsync_WithValidProject_ReturnsSuccessResponse()
        {
            // Arrange
            var updatedProject = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Updated Project", Program = "P002" };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(updatedProject);
            _fpsProjectApiClient.UpdateProjectAsync(updatedProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.UpdateProjectAsync(updatedProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Updated Project", result.Data?.ProjectTitle);
            await _fpsProjectApiClient.Received(1).UpdateProjectAsync(updatedProject);
        }

        [Fact]
        public async Task UpdateProjectAsync_WithNonExistentProject_ReturnsFailureResponse()
        {
            // Arrange
            var project = new ProjectDto { ParentProject = "NONEXISTENT" };
            var errors = new List<ApiErrorDto> { new() { Message = "Project not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.UpdateProjectAsync(project).Returns(expectedResponse);

            // Act
            var result = await _projectService.UpdateProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdatePactProjectAsync Tests

        [Fact]
        public async Task UpdatePactProjectAsync_WithValidProject_ReturnsSuccessResponse()
        {
            // Arrange
            var project = new ProjectDto { ParentProject = "PP001", ProjectTitle = "PACT Updated" };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);
            _fpsProjectApiClient.UpdatePactProjectAsync(project).Returns(expectedResponse);

            // Act
            var result = await _projectService.UpdatePactProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(project.ParentProject, result.Data?.ParentProject);
            await _fpsProjectApiClient.Received(1).UpdatePactProjectAsync(project);
        }

        [Fact]
        public async Task UpdatePactProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = new ProjectDto { ParentProject = "PP001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.UpdatePactProjectAsync(project).Returns(expectedResponse);

            // Act
            var result = await _projectService.UpdatePactProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteProjectAsync Tests

        [Fact]
        public async Task DeleteProjectAsync_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var parentProject = "PP001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsProjectApiClient.DeleteProjectAsync(parentProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.DeleteProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsProjectApiClient.Received(1).DeleteProjectAsync(parentProject);
        }

        [Fact]
        public async Task DeleteProjectAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var parentProject = "NONEXISTENT";
            var errors = new List<ApiErrorDto> { new() { Message = "Project not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.DeleteProjectAsync(parentProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.DeleteProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllStatusesAsync Tests

        [Fact]
        public async Task GetAllStatusesAsync_WithSuccessResponse_ReturnsStatusList()
        {
            // Arrange
            var statuses = new List<StatusDto> { new() { Status = "Active" }, new() { Status = "Inactive" } };
            var expectedResponse = ApiResponseDto<List<StatusDto>>.SuccessResponse(statuses);
            _fpsLookupApiClient.GetAllStatusesAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllStatusesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsLookupApiClient.Received(1).GetAllStatusesAsync();
        }

        [Fact]
        public async Task GetAllStatusesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<StatusDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsLookupApiClient.GetAllStatusesAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllStatusesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllDiseasesAsync Tests

        [Fact]
        public async Task GetAllDiseasesAsync_WithSuccessResponse_ReturnsDiseaseList()
        {
            // Arrange
            var diseases = new List<DiseaseDto> { new() { Disease = "Foot and Mouth" }, new() { Disease = "Avian Flu" } };
            var expectedResponse = ApiResponseDto<List<DiseaseDto>>.SuccessResponse(diseases);
            _fpsLookupApiClient.GetAllDiseasesAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsLookupApiClient.Received(1).GetAllDiseasesAsync();
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<DiseaseDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsLookupApiClient.GetAllDiseasesAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllCustomersAsync Tests

        [Fact]
        public async Task GetAllCustomersAsync_WithSuccessResponse_ReturnsCustomerList()
        {
            // Arrange
            var customers = new List<CustomerDto> { new() { Customer = "DEFRA" }, new() { Customer = "APHA" } };
            var expectedResponse = ApiResponseDto<List<CustomerDto>>.SuccessResponse(customers);
            _fpsLookupApiClient.GetAllCustomersAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsLookupApiClient.Received(1).GetAllCustomersAsync();
        }

        [Fact]
        public async Task GetAllCustomersAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<CustomerDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsLookupApiClient.GetAllCustomersAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllContractsAsync Tests

        [Fact]
        public async Task GetAllContractsAsync_WithSuccessResponse_ReturnsContractList()
        {
            // Arrange
            var contracts = new List<ContractDto> { new() { ContractNo = "C001" }, new() { ContractNo = "C002" } };
            var expectedResponse = ApiResponseDto<List<ContractDto>>.SuccessResponse(contracts);
            _fpsLookupApiClient.GetAllContractsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsLookupApiClient.Received(1).GetAllContractsAsync();
        }

        [Fact]
        public async Task GetAllContractsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ContractDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsLookupApiClient.GetAllContractsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion
    }
}
