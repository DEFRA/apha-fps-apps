using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.Costbook.CostBookProjectApiClientTest
{
    public class CostBookProjectApiClientTests
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly CostBookProjectApiClient _client;

        public CostBookProjectApiClientTests()
        {
            _http = Substitute.For<ICostBookHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new CostBookProjectApiClient(_http, _mapper);
        }

        #region GetFilteredProjectsAsync Tests

        [Fact]
        public async Task GetFilteredProjectsAsync_WithSuccessResponse_ReturnsMappedProjectList()
        {
            // Arrange
            var criteria = new QueryParameters<string>();
            var projectResList = new List<ProjectRes> { new ProjectRes(), new ProjectRes() };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = projectResList };
            var mappedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto> { new ProjectDto(), new ProjectDto() });

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetFilteredProjectsAsync(criteria);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<ProjectRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetFilteredProjectsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var criteria = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<ProjectRes>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } }
            };
            var mappedResponse = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetFilteredProjectsAsync(criteria);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
        }

        #endregion

        #region GetProjectByIdAsync Tests

        [Fact]
        public async Task GetProjectByIdAsync_WithValidId_ReturnsMappedProject()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = new ProjectRes() };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto());

            _http.GetAsync<ProjectRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectByIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).GetAsync<ProjectRes>(Arg.Is<string>(s => s.Contains("PROJECT")));
        }

        [Fact]
        public async Task GetProjectByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "INVALID-ID";
            var apiResponse = new ApiResponse<ProjectRes>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProjectRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProjectByIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Not Found", result.Errors[0].Message);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WithSpecialCharactersInId_EncodesUrlCorrectly()
        {
            // Arrange
            var projectId = "PROJECT/001";
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = new ProjectRes() };
            _http.GetAsync<ProjectRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto()));

            // Act
            await _client.GetProjectByIdAsync(projectId);

            // Assert - URL should be encoded, not contain raw slash
            await _http.Received(1).GetAsync<ProjectRes>(Arg.Is<string>(s => !s.Contains("PROJECT/001")));
        }

        #endregion

        #region AddProjectAsync Tests

        [Fact]
        public async Task AddProjectAsync_WithValidProject_ReturnsMappedProject()
        {
            // Arrange
            var projectDto = new ProjectDto();
            var projectReq = new ProjectReq();
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = new ProjectRes() };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto());

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PostAsync<ProjectReq, ProjectRes>("api/v1/projects", projectReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.AddProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).PostAsync<ProjectReq, ProjectRes>("api/v1/projects", projectReq);
        }

        [Fact]
        public async Task AddProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectDto = new ProjectDto();
            var projectReq = new ProjectReq();
            var apiResponse = new ApiResponse<ProjectRes>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "Add failed", Code = "ADD_FAILED" } }
            };
            var mappedResponse = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Add failed", Code = "ADD_FAILED" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PostAsync<ProjectReq, ProjectRes>("api/v1/projects", projectReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.AddProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Add failed", result.Errors[0].Message);
        }

        [Fact]
        public async Task AddProjectAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var projectDto = new ProjectDto();
            var projectReq = new ProjectReq();
            var exceptionMessage = "Connection refused";

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PostAsync<ProjectReq, ProjectRes>("api/v1/projects", projectReq)
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _client.AddProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to add project", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal(exceptionMessage, result.Errors[0].Details);
        }

        #endregion

        #region UpdateProjectAsync Tests

        [Fact]
        public async Task UpdateProjectAsync_WithValidIdAndProject_ReturnsMappedProject()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var projectDto = new ProjectDto();
            var projectReq = new ProjectReq();
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = new ProjectRes() };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto());

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PutAsync<ProjectReq, ProjectRes>(Arg.Any<string>(), projectReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateProjectAsync(projectId, projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).PutAsync<ProjectReq, ProjectRes>(Arg.Any<string>(), projectReq);
        }

        [Fact]
        public async Task UpdateProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var projectDto = new ProjectDto();
            var projectReq = new ProjectReq();
            var apiResponse = new ApiResponse<ProjectRes>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "Update failed", Code = "UPDATE_FAILED" } }
            };
            var mappedResponse = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Update failed", Code = "UPDATE_FAILED" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PutAsync<ProjectReq, ProjectRes>(Arg.Any<string>(), projectReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateProjectAsync(projectId, projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Equal("Update failed", result.Errors[0].Message);
        }

        #endregion

        #region DeleteProjectAsync Tests

        [Fact]
        public async Task DeleteProjectAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.DeleteProjectAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteProjectAsync_WithSuccessButNullData_ReturnsSuccessTrue()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = null };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.DeleteProjectAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "Delete failed", Code = "DELETE_FAILED" } }
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Delete failed", Code = "DELETE_FAILED" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteProjectAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("Delete failed", result.Errors[0].Message);
        }

        #endregion

        #region CopyProjectAsync Tests

        [Fact]
        public async Task CopyProjectAsync_WithValidIds_ReturnsMappedProject()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var newId = "PROJECT-002";
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = new ProjectRes() };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto());

            _http.PostAsync<string, ProjectRes>(Arg.Any<string>(), newId).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyProjectAsync(projectId, newId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).PostAsync<string, ProjectRes>(Arg.Any<string>(), newId);
        }

        [Fact]
        public async Task CopyProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var newId = "PROJECT-002";
            var apiResponse = new ApiResponse<ProjectRes>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "Copy failed", Code = "COPY_FAILED" } }
            };
            var mappedResponse = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Copy failed", Code = "COPY_FAILED" } },
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<string, ProjectRes>(Arg.Any<string>(), newId).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CopyProjectAsync(projectId, newId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("Copy failed", result.Errors![0].Message);
        }

        #endregion

        #region RecostProjectAsync Tests

        [Fact]
        public async Task RecostProjectAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };

            _http.PostAsync<object, bool>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);

            // Act
            var result = await _client.RecostProjectAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).PostAsync<object, bool>(Arg.Any<string>(), Arg.Any<object>());
        }

        [Fact]
        public async Task RecostProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Errors = new List<ApiError> { new ApiError { Message = "Recost failed", Code = "RECOST_FAILED" } }
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Recost failed", Code = "RECOST_FAILED" } },
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<object, bool>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.RecostProjectAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("Recost failed", result.Errors[0].Message);
        }

        #endregion

        #region GetNextProjectNumberAsync Tests

        [Fact]
        public async Task GetNextProjectNumberAsync_WithBaseNumber_ReturnsNextProjectNumber()
        {
            // Arrange
            var baseNumber = "PRJ-001";
            var apiResponse = new ApiResponse<string> { Success = true, Data = "PRJ-002" };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PRJ-002", result.Data);
            await _http.Received(1).GetAsync<string>(Arg.Is<string>(s => s.Contains("baseNumber")));
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_WithNullBaseNumber_CallsEndpointWithoutQuery()
        {
            // Arrange
            var apiResponse = new ApiResponse<string> { Success = true, Data = "PRJ-001" };
            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetNextProjectNumberAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<string>(Arg.Is<string>(s => !s.Contains("baseNumber")));
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<string>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "Number generation failed", Code = "NUMBER_FAILED" } }
            };
            var mappedResponse = new ApiResponseDto<string>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Number generation failed", Code = "NUMBER_FAILED" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<string>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetNextProjectNumberAsync("PRJ-001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("Number generation failed", result.Errors[0].Message);
        }

        #endregion
    }
}
