using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.Costbook;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Costbook.CostBookProjectServiceTest
{
    public class CostBookProjectServiceTests
    {
        private readonly ICostBookApiClient _costBookClient;
        private readonly ICostBookProjectApiClient _costBookProjectApiClient;
        private readonly CostBookProjectService _costBookProjectService;

        public CostBookProjectServiceTests()
        {
            _costBookClient = Substitute.For<ICostBookApiClient>();
            _costBookProjectApiClient = Substitute.For<ICostBookProjectApiClient>();
            _costBookClient.Projects.Returns(_costBookProjectApiClient);
            _costBookProjectService = new CostBookProjectService(_costBookClient);
        }

        #region GetFilteredProjectsAsync Tests

        [Fact]
        public async Task GetFilteredProjectsAsync_WithSuccessResponse_ReturnsProjectList()
        {
            // Arrange
            var queryParameters = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Search = "Test"
            };
            var projects = new List<ProjectDto>
            {
                new ProjectDto { ProjectId = "P001", Projecttitle = "Test Project 1", Status = "Active" },
                new ProjectDto { ProjectId = "P002", Projecttitle = "Test Project 2", Status = "Active" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                projects,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _costBookProjectApiClient.GetFilteredProjectsAsync(queryParameters).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.GetFilteredProjectsAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _costBookProjectApiClient.Received(1).GetFilteredProjectsAsync(queryParameters);
        }

        [Fact]
        public async Task GetFilteredProjectsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _costBookProjectApiClient.GetFilteredProjectsAsync(queryParameters).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.GetFilteredProjectsAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetFilteredProjectsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());

            _costBookProjectApiClient.GetFilteredProjectsAsync(queryParameters).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.GetFilteredProjectsAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetFilteredProjectsAsync_PassesCorrectQueryParameters()
        {
            // Arrange
            var queryParameters = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 20,
                Search = "Project",
                SortBy = "ProjectId",
                Descending = false
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _costBookProjectApiClient.GetFilteredProjectsAsync(queryParameters).Returns(expectedResponse);

            // Act
            await _costBookProjectService.GetFilteredProjectsAsync(queryParameters);

            // Assert
            await _costBookProjectApiClient.Received(1).GetFilteredProjectsAsync(
                Arg.Is<QueryParameters<string>>(q => 
                    q.Page == 2 && 
                    q.PageSize == 20 && 
                    q.Search == "Project" &&
                    q.SortBy == "ProjectId" &&
                    q.Descending == false
                )
            );
        }

        #endregion

        #region GetProjectByIdAsync Tests

        [Fact]
        public async Task GetProjectByIdAsync_WithValidId_ReturnsProject()
        {
            // Arrange
            var projectId = "P001";
            var project = new ProjectDto 
            { 
                ProjectId = projectId, 
                Projecttitle = "Test Project",
                Status = "Active"
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);

            _costBookProjectApiClient.GetProjectByIdAsync(projectId).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.GetProjectByIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(projectId, result.Data.ProjectId);
            await _costBookProjectApiClient.Received(1).GetProjectByIdAsync(projectId);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WhenProjectNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Project not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _costBookProjectApiClient.GetProjectByIdAsync(projectId).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.GetProjectByIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetProjectByIdAsync_PassesCorrectId()
        {
            // Arrange
            var projectId = "P123";
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto());

            _costBookProjectApiClient.GetProjectByIdAsync(projectId).Returns(expectedResponse);

            // Act
            await _costBookProjectService.GetProjectByIdAsync(projectId);

            // Assert
            await _costBookProjectApiClient.Received(1).GetProjectByIdAsync(projectId);
        }

        #endregion

        #region AddProjectAsync Tests

        [Fact]
        public async Task AddProjectAsync_SetsCreatedDateToUtcNow()
        {
            // Arrange
            var beforeTest = DateTime.UtcNow;
            var project = new ProjectDto 
            { 
                ProjectId = "P001",
                Projecttitle = "New Project"
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);

            _costBookProjectApiClient.AddProjectAsync(Arg.Any<ProjectDto>()).Returns(expectedResponse);

            // Act
            await _costBookProjectService.AddProjectAsync(project);
            var afterTest = DateTime.UtcNow;

            // Assert
            Assert.InRange(project.CreatedDate, beforeTest, afterTest);
        }

        [Fact]
        public async Task AddProjectAsync_SetsStatusToActive()
        {
            // Arrange
            var project = new ProjectDto 
            { 
                ProjectId = "P001",
                Projecttitle = "New Project",
                Status = "Pending" // Initial status
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);

            _costBookProjectApiClient.AddProjectAsync(Arg.Any<ProjectDto>()).Returns(expectedResponse);

            // Act
            await _costBookProjectService.AddProjectAsync(project);

            // Assert
            Assert.Equal("Active", project.Status);
        }

        [Fact]
        public async Task AddProjectAsync_CallsApiClientWithModifiedProject()
        {
            // Arrange
            var project = new ProjectDto 
            { 
                ProjectId = "P001",
                Projecttitle = "New Project"
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);

            _costBookProjectApiClient.AddProjectAsync(Arg.Any<ProjectDto>()).Returns(expectedResponse);

            // Act
            await _costBookProjectService.AddProjectAsync(project);

            // Assert
            await _costBookProjectApiClient.Received(1).AddProjectAsync(
                Arg.Is<ProjectDto>(p => 
                    p.ProjectId == "P001" &&
                    p.Status == "Active" &&
                    p.CreatedDate != null
                )
            );
        }

        [Fact]
        public async Task AddProjectAsync_WithSuccessResponse_ReturnsProject()
        {
            // Arrange
            var project = new ProjectDto 
            { 
                ProjectId = "P001",
                Projecttitle = "New Project"
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);

            _costBookProjectApiClient.AddProjectAsync(Arg.Any<ProjectDto>()).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.AddProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task AddProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = new ProjectDto 
            { 
                ProjectId = "P001",
                Projecttitle = "New Project"
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Validation error", Code = "VALIDATION_ERROR" }
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _costBookProjectApiClient.AddProjectAsync(Arg.Any<ProjectDto>()).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.AddProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateProjectAsync Tests

        [Fact]
        public async Task UpdateProjectAsync_SetsModifiedDateToUtcNow()
        {
            // Arrange
            var beforeTest = DateTime.UtcNow;
            var projectId = "P001";
            var project = new ProjectDto 
            { 
                ProjectId = projectId,
                Projecttitle = "Updated Project"
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);

            _costBookProjectApiClient.UpdateProjectAsync(projectId, Arg.Any<ProjectDto>()).Returns(expectedResponse);

            // Act
            await _costBookProjectService.UpdateProjectAsync(projectId, project);
            var afterTest = DateTime.UtcNow;

            // Assert
            Assert.NotNull(project.ModifiedDate);
            Assert.InRange(project.ModifiedDate.Value, beforeTest, afterTest);
        }

        [Fact]
        public async Task UpdateProjectAsync_CallsApiClientWithModifiedProject()
        {
            // Arrange
            var projectId = "P001";
            var project = new ProjectDto 
            { 
                ProjectId = projectId,
                Projecttitle = "Updated Project"
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);

            _costBookProjectApiClient.UpdateProjectAsync(projectId, Arg.Any<ProjectDto>()).Returns(expectedResponse);

            // Act
            await _costBookProjectService.UpdateProjectAsync(projectId, project);

            // Assert
            await _costBookProjectApiClient.Received(1).UpdateProjectAsync(
                projectId,
                Arg.Is<ProjectDto>(p => 
                    p.ProjectId == projectId &&
                    p.ModifiedDate != null
                )
            );
        }

        [Fact]
        public async Task UpdateProjectAsync_WithSuccessResponse_ReturnsUpdatedProject()
        {
            // Arrange
            var projectId = "P001";
            var project = new ProjectDto 
            { 
                ProjectId = projectId,
                Projecttitle = "Updated Project"
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);

            _costBookProjectApiClient.UpdateProjectAsync(projectId, Arg.Any<ProjectDto>()).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.UpdateProjectAsync(projectId, project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task UpdateProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "P001";
            var project = new ProjectDto 
            { 
                ProjectId = projectId,
                Projecttitle = "Updated Project"
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Update failed", Code = "UPDATE_ERROR" }
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _costBookProjectApiClient.UpdateProjectAsync(projectId, Arg.Any<ProjectDto>()).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.UpdateProjectAsync(projectId, project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdateProjectAsync_PassesCorrectId()
        {
            // Arrange
            var projectId = "P123";
            var project = new ProjectDto { ProjectId = projectId };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);

            _costBookProjectApiClient.UpdateProjectAsync(projectId, Arg.Any<ProjectDto>()).Returns(expectedResponse);

            // Act
            await _costBookProjectService.UpdateProjectAsync(projectId, project);

            // Assert
            await _costBookProjectApiClient.Received(1).UpdateProjectAsync(
                projectId,
                Arg.Any<ProjectDto>()
            );
        }

        #endregion

        #region DeleteProjectAsync Tests

        [Fact]
        public async Task DeleteProjectAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            var projectId = "P001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _costBookProjectApiClient.DeleteProjectAsync(projectId).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.DeleteProjectAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _costBookProjectApiClient.Received(1).DeleteProjectAsync(projectId);
        }

        [Fact]
        public async Task DeleteProjectAsync_WhenProjectNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Project not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _costBookProjectApiClient.DeleteProjectAsync(projectId).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.DeleteProjectAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task DeleteProjectAsync_PassesCorrectId()
        {
            // Arrange
            var projectId = "P123";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _costBookProjectApiClient.DeleteProjectAsync(projectId).Returns(expectedResponse);

            // Act
            await _costBookProjectService.DeleteProjectAsync(projectId);

            // Assert
            await _costBookProjectApiClient.Received(1).DeleteProjectAsync(projectId);
        }

        #endregion

        #region CopyProjectAsync Tests

        [Fact]
        public async Task CopyProjectAsync_WithValidIds_ReturnsCopiedProject()
        {
            // Arrange
            var sourceId = "P001";
            var newId = "P002";
            var copiedProject = new ProjectDto 
            { 
                ProjectId = newId,
                Projecttitle = "Copied Project"
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(copiedProject);

            _costBookProjectApiClient.CopyProjectAsync(sourceId, newId).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.CopyProjectAsync(sourceId, newId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(newId, result.Data.ProjectId);
            await _costBookProjectApiClient.Received(1).CopyProjectAsync(sourceId, newId);
        }

        [Fact]
        public async Task CopyProjectAsync_WhenSourceProjectNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var sourceId = "INVALID";
            var newId = "P002";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Source project not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _costBookProjectApiClient.CopyProjectAsync(sourceId, newId).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.CopyProjectAsync(sourceId, newId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task CopyProjectAsync_PassesCorrectIds()
        {
            // Arrange
            var sourceId = "P123";
            var newId = "P456";
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto());

            _costBookProjectApiClient.CopyProjectAsync(sourceId, newId).Returns(expectedResponse);

            // Act
            await _costBookProjectService.CopyProjectAsync(sourceId, newId);

            // Assert
            await _costBookProjectApiClient.Received(1).CopyProjectAsync(sourceId, newId);
        }

        #endregion

        #region RecostProjectAsync Tests

        [Fact]
        public async Task RecostProjectAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            var projectId = "P001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _costBookProjectApiClient.RecostProjectAsync(projectId).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.RecostProjectAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _costBookProjectApiClient.Received(1).RecostProjectAsync(projectId);
        }

        [Fact]
        public async Task RecostProjectAsync_WhenProjectNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Project not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _costBookProjectApiClient.RecostProjectAsync(projectId).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.RecostProjectAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task RecostProjectAsync_PassesCorrectId()
        {
            // Arrange
            var projectId = "P123";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _costBookProjectApiClient.RecostProjectAsync(projectId).Returns(expectedResponse);

            // Act
            await _costBookProjectService.RecostProjectAsync(projectId);

            // Assert
            await _costBookProjectApiClient.Received(1).RecostProjectAsync(projectId);
        }

        #endregion

        #region GetNextProjectNumberAsync Tests

        [Fact]
        public async Task GetNextProjectNumberAsync_WithBaseNumber_ReturnsNextNumber()
        {
            // Arrange
            var baseNumber = "P001";
            var nextNumber = "P002";
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(nextNumber);

            _costBookProjectApiClient.GetNextProjectNumberAsync(baseNumber).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(nextNumber, result.Data);
            await _costBookProjectApiClient.Received(1).GetNextProjectNumberAsync(baseNumber);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_WithNullBaseNumber_ReturnsNextNumber()
        {
            // Arrange
            string? baseNumber = null;
            var nextNumber = "P001";
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(nextNumber);

            _costBookProjectApiClient.GetNextProjectNumberAsync(baseNumber).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(nextNumber, result.Data);
            await _costBookProjectApiClient.Received(1).GetNextProjectNumberAsync(null);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var baseNumber = "P001";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Failed to generate number", Code = "GENERATION_ERROR" }
            };
            var expectedResponse = ApiResponseDto<string>.FailureResponse(errors, new ApiMetaDto());

            _costBookProjectApiClient.GetNextProjectNumberAsync(baseNumber).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectService.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_PassesCorrectBaseNumber()
        {
            // Arrange
            var baseNumber = "PROJECT-123";
            var expectedResponse = ApiResponseDto<string>.SuccessResponse("PROJECT-124");

            _costBookProjectApiClient.GetNextProjectNumberAsync(baseNumber).Returns(expectedResponse);

            // Act
            await _costBookProjectService.GetNextProjectNumberAsync(baseNumber);

            // Assert
            await _costBookProjectApiClient.Received(1).GetNextProjectNumberAsync(baseNumber);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidClient_InitializesService()
        {
            // Arrange & Act
            var service = new CostBookProjectService(_costBookClient);

            // Assert
            Assert.NotNull(service);
        }        

        #endregion
    }
}
