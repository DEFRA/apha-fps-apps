using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.FpsProjectApiClientTest
{
    public class FpsProjectApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProjectApiClient _client;

        public FpsProjectApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProjectApiClient(_http, _mapper);
        }

        #region GetProjectsByProgramAsync Tests

        [Fact]
        public async Task GetProjectsByProgramAsync_WithSuccessResponse_ReturnsMappedProjectList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var projectList = new List<ProjectRes>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
            };
            var apiResponse = new ApiResponse<List<ProjectRes>>
            {
                Success = true,
                Data = projectList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>
                {
                    new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                    new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<ProjectRes>>(Arg.Is<string>(url =>
                    url.Contains("api/project/paged") && url.Contains("programNo=P001")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectRes>>(
                Arg.Is<string>(url => url.Contains("api/project/paged") && url.Contains("programNo=P001")));
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<ProjectRes>>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            var error = Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve projects", error.Message);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_ConstructsUrlWithEscapedProgramNo()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = new List<ProjectRes>() };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectsByProgramAsync(query, programNo);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectRes>>(
                Arg.Is<string>(url => url.Contains($"programNo={Uri.EscapeDataString(programNo)}")));
        }

        #endregion

        #region GetAllProjectsAsync Tests

        [Fact]
        public async Task GetAllProjectsAsync_WithSuccessResponse_ReturnsMappedProjectList()
        {
            // Arrange
            var projectList = new List<ProjectRes>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
            };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = projectList };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>
                {
                    new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                    new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
                }
            );

            _http.GetAsync<List<ProjectRes>>("api/project").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectRes>>("api/project");
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve projects", error.Message);
        }

        #endregion

        #region GetPagedProjectsAsync Tests

        [Fact]
        public async Task GetPagedProjectsAsync_WithSuccessResponse_ReturnsMappedPagedProjects()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectList = new List<ProjectRes> { new() { ParentProject = "PP001", ProjectTitle = "Alpha" } };
            var apiResponse = new ApiResponse<List<ProjectRes>>
            {
                Success = true,
                Data = projectList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto> { new() { ParentProject = "PP001" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );

            _http.GetAsync<List<ProjectRes>>(Arg.Is<string>(url => url.Contains("api/project/paged"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<ProjectRes>>(Arg.Is<string>(url => url.Contains("api/project/paged")));
        }

        [Fact]
        public async Task GetPagedProjectsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetPagedProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve paged projects", error.Message);
        }

        #endregion

        #region GetPagedPactProjectsAsync Tests

        [Fact]
        public async Task GetPagedPactProjectsAsync_WithSuccessResponse_ReturnsMappedPactProjects()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectList = new List<ProjectRes> { new() { ParentProject = "PP001", ProjectTitle = "PACT Project" } };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = projectList };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto> { new() { ParentProject = "PP001" } }
            );

            _http.GetAsync<List<ProjectRes>>(Arg.Is<string>(url => url.Contains("api/project/pactview"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<ProjectRes>>(Arg.Is<string>(url => url.Contains("api/project/pactview")));
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve paged projects", error.Message);
        }

        #endregion

        #region GetProjectByIdAsync Tests

        [Fact]
        public async Task GetProjectByIdAsync_WithValidId_ReturnsMappedProject()
        {
            // Arrange
            var parentProject = "PP001";
            var projectRes = new ProjectRes { ParentProject = parentProject, ProjectTitle = "Alpha Project" };
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = projectRes };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(
                new ProjectDto { ParentProject = parentProject, ProjectTitle = "Alpha Project" }
            );

            _http.GetAsync<ProjectRes>(Arg.Is<string>(url => url.Contains(Uri.EscapeDataString(parentProject)))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectByIdAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(parentProject, result.Data?.ParentProject);
            await _http.Received(1).GetAsync<ProjectRes>(Arg.Is<string>(url => url.Contains($"api/project/{Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task GetProjectByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var parentProject = "NONEXISTENT";
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProjectRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProjectByIdAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<ProjectRes>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetProjectByIdAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve project", error.Message);
        }

        #endregion

        #region CreateProjectAsync Tests

        [Fact]
        public async Task CreateProjectAsync_WithValidProject_ReturnsMappedCreatedProject()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "New Project", Program = "P001" };
            var projectReq = new ProjectReq { ParentProject = "PP001", ProjectTitle = "New Project" };
            var projectRes = new ProjectRes { ParentProject = "PP001", ProjectTitle = "New Project" };
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = projectRes };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(projectDto);

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PostAsync<ProjectReq, ProjectRes>("api/project", projectReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PP001", result.Data?.ParentProject);
            await _http.Received(1).PostAsync<ProjectReq, ProjectRes>("api/project", projectReq);
        }

        [Fact]
        public async Task CreateProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "PP001" };
            var projectReq = new ProjectReq { ParentProject = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<ProjectRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PostAsync<ProjectReq, ProjectRes>(Arg.Any<string>(), Arg.Any<ProjectReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task CreateProjectAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "PP001" };
            _mapper.Map<ProjectReq>(projectDto).Returns(new ProjectReq());
            _http.PostAsync<ProjectReq, ProjectRes>(Arg.Any<string>(), Arg.Any<ProjectReq>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.CreateProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to create project", error.Message);
        }

        #endregion

        #region UpdateProjectAsync Tests

        [Fact]
        public async Task UpdateProjectAsync_WithValidProject_ReturnsMappedUpdatedProject()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Updated Project" };
            var projectReq = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Updated Project" };
            var projectRes = new ProjectRes { ParentProject = "PP001", ProjectTitle = "Updated Project" };
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = projectRes };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(projectDto);

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PutAsync<ProjectReq, ProjectRes>("api/project", projectReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Updated Project", result.Data?.ProjectTitle);
            await _http.Received(1).PutAsync<ProjectReq, ProjectRes>("api/project", projectReq);
        }

        [Fact]
        public async Task UpdateProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "NONEXISTENT" };
            var projectReq = new ProjectReq { ParentProject = "NONEXISTENT" };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PutAsync<ProjectReq, ProjectRes>(Arg.Any<string>(), Arg.Any<ProjectReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdateProjectAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "PP001" };
            _mapper.Map<ProjectReq>(projectDto).Returns(new ProjectReq());
            _http.PutAsync<ProjectReq, ProjectRes>(Arg.Any<string>(), Arg.Any<ProjectReq>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdateProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to update project", error.Message);
        }

        #endregion

        #region UpdatePactProjectAsync Tests

        [Fact]
        public async Task UpdatePactProjectAsync_WithValidProject_ReturnsMappedUpdatedProject()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "PACT Updated" };
            var projectReq = new ProjectReq { ParentProject = "PP001", ProjectTitle = "PACT Updated" };
            var projectRes = new ProjectRes { ParentProject = "PP001", ProjectTitle = "PACT Updated" };
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = projectRes };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(projectDto);

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PatchAsync<ProjectReq, ProjectRes>("api/project/external/pact", projectReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdatePactProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PP001", result.Data?.ParentProject);
            await _http.Received(1).PatchAsync<ProjectReq, ProjectRes>("api/project/external/pact", projectReq);
        }

        [Fact]
        public async Task UpdatePactProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "PP001" };
            var projectReq = new ProjectReq { ParentProject = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = new ApiResponse<ProjectRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PatchAsync<ProjectReq, ProjectRes>(Arg.Any<string>(), Arg.Any<ProjectReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdatePactProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdatePactProjectAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "PP001" };
            _mapper.Map<ProjectReq>(projectDto).Returns(new ProjectReq());
            _http.PatchAsync<ProjectReq, ProjectRes>(Arg.Any<string>(), Arg.Any<ProjectReq>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdatePactProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to update project", error.Message);
        }

        #endregion

        #region DeleteProjectAsync Tests

        [Fact]
        public async Task DeleteProjectAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var parentProject = "PP001";
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(Arg.Is<string>(url => url.Contains(Uri.EscapeDataString(parentProject)))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool>(Arg.Is<string>(url => url.Contains($"api/project/{Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task DeleteProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var parentProject = "NONEXISTENT";
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<bool> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task DeleteProjectAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.DeleteAsync<bool>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.DeleteProjectAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to delete project", error.Message);
        }

        #endregion
    }
}
