using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactProjectMonthApiClientTest
{
    public class PactProjectMonthApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactProjectMonthApiClient _client;

        public PactProjectMonthApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactProjectMonthApiClient(_http, _mapper);
        }

        #region GetProjectMonthByProjectAsync Tests

        [Fact]
        public async Task GetProjectMonthByProjectAsync_WhenApiReturnsSuccess_ReturnsMappedDtoList()
        {
            // Arrange
            var project = "PRJ1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectMonthsByProject, Uri.EscapeDataString(project));
            var resList = new List<ProjectMonthRes>
            {
                new() { Project = project, MonthNo = 1, CostProfile = 100m },
                new() { Project = project, MonthNo = 2, CostProfile = 200m }
            };
            var apiResponse = new ApiResponse<List<ProjectMonthRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<ProjectMonthDto>>.SuccessResponse(new List<ProjectMonthDto>
            {
                new() { Project = project, MonthNo = 1, CostProfile = 100m },
                new() { Project = project, MonthNo = 2, CostProfile = 200m }
            });

            _http.GetAsync<List<ProjectMonthRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectMonthDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectMonthByProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectMonthRes>>(expectedUrl);
        }

        [Fact]
        public async Task GetProjectMonthByProjectAsync_WhenNoRecords_ReturnsMappedEmptyList()
        {
            // Arrange
            var project = "PRJ_NONE";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectMonthsByProject, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<List<ProjectMonthRes>> { Success = true, Data = new List<ProjectMonthRes>() };
            var expectedDto = ApiResponseDto<List<ProjectMonthDto>>.SuccessResponse(new List<ProjectMonthDto>());

            _http.GetAsync<List<ProjectMonthRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectMonthDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectMonthByProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectMonthByProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectMonthsByProject, Uri.EscapeDataString(project));
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectMonthRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectMonthDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectMonthRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectMonthDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProjectMonthByProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectMonthDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectMonthByProjectAsync_UrlEncodesProjectParameter()
        {
            // Arrange
            var project = "PRJ/1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectMonthsByProject, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<List<ProjectMonthRes>> { Success = true, Data = new List<ProjectMonthRes>() };
            var expectedDto = ApiResponseDto<List<ProjectMonthDto>>.SuccessResponse(new List<ProjectMonthDto>());

            _http.GetAsync<List<ProjectMonthRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectMonthDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectMonthByProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).GetAsync<List<ProjectMonthRes>>(expectedUrl);
        }

        #endregion

        #region GetProjectMonthAsync Tests

        [Fact]
        public async Task GetProjectMonthAsync_WhenApiReturnsSuccess_ReturnsMappedDto()
        {
            // Arrange
            var project = "PRJ1";
            var monthNo = 3;
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectMonthById, Uri.EscapeDataString(project), monthNo);
            var res = new ProjectMonthRes { Project = project, MonthNo = monthNo, CostProfile = 250m };
            var apiResponse = new ApiResponse<ProjectMonthRes> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<ProjectMonthDto>.SuccessResponse(
                new ProjectMonthDto { Project = project, MonthNo = monthNo, CostProfile = 250m });

            _http.GetAsync<ProjectMonthRes>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectMonthDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectMonthAsync(project, monthNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(project, result.Data?.Project);
            Assert.Equal(monthNo, result.Data?.MonthNo);
            await _http.Received(1).GetAsync<ProjectMonthRes>(expectedUrl);
        }

        [Fact]
        public async Task GetProjectMonthAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ_NONE";
            var monthNo = 99;
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectMonthById, Uri.EscapeDataString(project), monthNo);
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectMonthRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectMonthDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProjectMonthRes>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectMonthDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProjectMonthAsync(project, monthNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            _mapper.Received(1).Map<ApiResponseDto<ProjectMonthDto>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectMonthAsync_UrlEncodesProjectParameter()
        {
            // Arrange
            var project = "PRJ/1";
            var monthNo = 1;
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectMonthById, Uri.EscapeDataString(project), monthNo);
            var apiResponse = new ApiResponse<ProjectMonthRes> { Success = true, Data = new ProjectMonthRes { Project = project, MonthNo = monthNo } };
            var expectedDto = ApiResponseDto<ProjectMonthDto>.SuccessResponse(new ProjectMonthDto { Project = project, MonthNo = monthNo });

            _http.GetAsync<ProjectMonthRes>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectMonthDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectMonthAsync(project, monthNo);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).GetAsync<ProjectMonthRes>(expectedUrl);
        }

        #endregion

        #region CreateProjectMonthAsync Tests

        [Fact]
        public async Task CreateProjectMonthAsync_WhenApiReturnsSuccess_ReturnsMappedCreatedDto()
        {
            // Arrange
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };
            var req = new ProjectMonthReq { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };
            var res = new ProjectMonthRes { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };
            var apiResponse = new ApiResponse<ProjectMonthRes> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<ProjectMonthDto>.SuccessResponse(
                new ProjectMonthDto { Project = "PRJ1", MonthNo = 1, CostProfile = 100m });

            _mapper.Map<ProjectMonthReq>(dto).Returns(req);
            _http.PostAsync<ProjectMonthReq, ProjectMonthRes>(PactApiEndpoints.CreateProjectMonth, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectMonthDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateProjectMonthAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PRJ1", result.Data?.Project);
            Assert.Equal(1, result.Data?.MonthNo);
            _mapper.Received(1).Map<ProjectMonthReq>(dto);
            await _http.Received(1).PostAsync<ProjectMonthReq, ProjectMonthRes>(PactApiEndpoints.CreateProjectMonth, req);
        }

        [Fact]
        public async Task CreateProjectMonthAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 1 };
            var req = new ProjectMonthReq { Project = "PRJ1", MonthNo = 1 };
            var errors = new List<ApiError> { new() { Message = "Create failed", Code = "CREATE_ERROR" } };
            var apiResponse = new ApiResponse<ProjectMonthRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectMonthDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Create failed", Code = "CREATE_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectMonthReq>(dto).Returns(req);
            _http.PostAsync<ProjectMonthReq, ProjectMonthRes>(PactApiEndpoints.CreateProjectMonth, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectMonthDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateProjectMonthAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            _mapper.Received(1).Map<ApiResponseDto<ProjectMonthDto>>(apiResponse);
        }

        #endregion

        #region UpdateProjectMonthAsync

        [Fact]
        public async Task UpdateProjectMonthAsync_WhenApiReturnsSuccess_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 2, CostProfile = 500m };
            var req = new ProjectMonthReq { Project = "PRJ1", MonthNo = 2, CostProfile = 500m };
            var res = new ProjectMonthRes { Project = "PRJ1", MonthNo = 2, CostProfile = 500m };
            var apiResponse = new ApiResponse<ProjectMonthRes> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<ProjectMonthDto>.SuccessResponse(
                new ProjectMonthDto { Project = "PRJ1", MonthNo = 2, CostProfile = 500m });

            _mapper.Map<ProjectMonthReq>(dto).Returns(req);
            _http.PutAsync<ProjectMonthReq, ProjectMonthRes>(PactApiEndpoints.UpdateProjectMonth, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectMonthDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateProjectMonthAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PRJ1", result.Data?.Project);
            Assert.Equal(2, result.Data?.MonthNo);
            _mapper.Received(1).Map<ProjectMonthReq>(dto);
            await _http.Received(1).PutAsync<ProjectMonthReq, ProjectMonthRes>(PactApiEndpoints.UpdateProjectMonth, req);
        }

        [Fact]
        public async Task UpdateProjectMonthAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 2 };
            var req = new ProjectMonthReq { Project = "PRJ1", MonthNo = 2 };
            var errors = new List<ApiError> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = new ApiResponse<ProjectMonthRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectMonthDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectMonthReq>(dto).Returns(req);
            _http.PutAsync<ProjectMonthReq, ProjectMonthRes>(PactApiEndpoints.UpdateProjectMonth, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectMonthDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateProjectMonthAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            _mapper.Received(1).Map<ApiResponseDto<ProjectMonthDto>>(apiResponse);
        }

        #endregion

        #region DeleteProjectMonthAsync

        [Fact]
        public async Task DeleteProjectMonthAsync_WhenApiReturnsSuccess_ReturnsTrueResponse()
        {
            // Arrange
            var project = "PRJ1";
            var monthNo = 1;
            var expectedUrl = string.Format(PactApiEndpoints.DeleteProjectMonth, Uri.EscapeDataString(project), monthNo);
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteProjectMonthAsync(project, monthNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool>(expectedUrl);
        }

        [Fact]
        public async Task DeleteProjectMonthAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ1";
            var monthNo = 1;
            var expectedUrl = string.Format(PactApiEndpoints.DeleteProjectMonth, Uri.EscapeDataString(project), monthNo);
            var errors = new List<ApiError> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var apiResponse = new ApiResponse<bool> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteProjectMonthAsync(project, monthNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            _mapper.Received(1).Map<ApiResponseDto<bool>>(apiResponse);
        }

        [Fact]
        public async Task DeleteProjectMonthAsync_UrlEncodesProjectParameter()
        {
            // Arrange
            var project = "PRJ/1";
            var monthNo = 2;
            var expectedUrl = string.Format(PactApiEndpoints.DeleteProjectMonth, Uri.EscapeDataString(project), monthNo);
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteProjectMonthAsync(project, monthNo);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).DeleteAsync<bool>(expectedUrl);
        }

        #endregion
    }
}
