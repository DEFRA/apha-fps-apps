using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.Costbook.CostBookProjectSummaryApiClientTest
{
    public class CostBookProjectSummaryApiClientTests
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly CostBookProjectSummaryApiClient _client;

        public CostBookProjectSummaryApiClientTests()
        {
            _http = Substitute.For<ICostBookHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new CostBookProjectSummaryApiClient(_http, _mapper);
        }

        #region GetProfitIncludedTotalAsync Tests

        [Fact]
        public async Task GetProfitIncludedTotalAsync_WithSuccessResponse_ReturnsSuccessWithData()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var year = 2024;
            var apiResponse = new ApiResponse<double> { Success = true, Data = 12345.67 };

            _http.GetAsync<double>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetProfitIncludedTotalAsync(projectId, year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(12345.67, result.Data);
            await _http.Received(1).GetAsync<double>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProfitIncludedTotalAsync_WithSpecialCharactersInId_EncodesUrlCorrectly()
        {
            // Arrange
            var projectId = "PROJECT/001";
            var year = 2024;
            var apiResponse = new ApiResponse<double> { Success = true, Data = 0.0 };

            _http.GetAsync<double>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            await _client.GetProfitIncludedTotalAsync(projectId, year);

            // Assert - URL should be encoded, not contain raw slash
            await _http.Received(1).GetAsync<double>(Arg.Is<string>(s => !s.Contains("PROJECT/001")));
        }

        [Fact]
        public async Task GetProfitIncludedTotalAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var year = 2024;
            var apiResponse = new ApiResponse<double>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } };
            var mappedResponse = new ApiResponseDto<double>
            {
                Success = false,
                Errors = errors,
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<double>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<double>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProfitIncludedTotalAsync(projectId, year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Not Found", result.Errors[0].Message);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        #endregion

        #region GetStaffYearsPivotAsync Tests

        [Fact]
        public async Task GetStaffYearsPivotAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<StaffYearsPivotRes> { Success = true, Data = new StaffYearsPivotRes() };
            var expectedDto = ApiResponseDto<StaffYearsPivotDto>.SuccessResponse(new StaffYearsPivotDto());

            _http.GetAsync<StaffYearsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffYearsPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetStaffYearsPivotAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).GetAsync<StaffYearsPivotRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_WithQueryParameters_AppendsQueryString()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<StaffYearsPivotRes> { Success = true, Data = new StaffYearsPivotRes() };
            var expectedDto = ApiResponseDto<StaffYearsPivotDto>.SuccessResponse(new StaffYearsPivotDto());

            _http.GetAsync<StaffYearsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffYearsPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetStaffYearsPivotAsync(projectId, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<StaffYearsPivotRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<StaffYearsPivotRes>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } }
            };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } };
            var mappedResponse = new ApiResponseDto<StaffYearsPivotDto>
            {
                Success = false,
                Errors = errors,
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<StaffYearsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffYearsPivotDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetStaffYearsPivotAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_WhenApiSuccessButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<StaffYearsPivotRes> { Success = true, Data = null };
            var mappedResponse = new ApiResponseDto<StaffYearsPivotDto>
            {
                Success = false,
                Errors = [],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<StaffYearsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffYearsPivotDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetStaffYearsPivotAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetStaffEffortAsync Tests

        [Fact]
        public async Task GetStaffEffortAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<StaffEffortPivotRes> { Success = true, Data = new StaffEffortPivotRes() };
            var expectedDto = ApiResponseDto<StaffEffortPivotDto>.SuccessResponse(new StaffEffortPivotDto());

            _http.GetAsync<StaffEffortPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffEffortPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetStaffEffortAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).GetAsync<StaffEffortPivotRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetStaffEffortAsync_WithQueryParameters_AppendsQueryString()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<StaffEffortPivotRes> { Success = true, Data = new StaffEffortPivotRes() };
            var expectedDto = ApiResponseDto<StaffEffortPivotDto>.SuccessResponse(new StaffEffortPivotDto());

            _http.GetAsync<StaffEffortPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffEffortPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetStaffEffortAsync(projectId, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<StaffEffortPivotRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetStaffEffortAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<StaffEffortPivotRes>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } }
            };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } };
            var mappedResponse = new ApiResponseDto<StaffEffortPivotDto>
            {
                Success = false,
                Errors = errors,
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<StaffEffortPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffEffortPivotDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetStaffEffortAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetStaffEffortAsync_WhenApiSuccessButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<StaffEffortPivotRes> { Success = true, Data = null };
            var mappedResponse = new ApiResponseDto<StaffEffortPivotDto>
            {
                Success = false,
                Errors = [],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<StaffEffortPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffEffortPivotDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetStaffEffortAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetProjectCostsPivotAsync Tests

        [Fact]
        public async Task GetProjectCostsPivotAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<ProjectCostsPivotRes> { Success = true, Data = new ProjectCostsPivotRes() };
            var expectedDto = ApiResponseDto<ProjectCostsPivotDto>.SuccessResponse(new ProjectCostsPivotDto());

            _http.GetAsync<ProjectCostsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectCostsPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectCostsPivotAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).GetAsync<ProjectCostsPivotRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_WithQueryParameters_AppendsQueryString()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<ProjectCostsPivotRes> { Success = true, Data = new ProjectCostsPivotRes() };
            var expectedDto = ApiResponseDto<ProjectCostsPivotDto>.SuccessResponse(new ProjectCostsPivotDto());

            _http.GetAsync<ProjectCostsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectCostsPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectCostsPivotAsync(projectId, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<ProjectCostsPivotRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<ProjectCostsPivotRes>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } }
            };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } };
            var mappedResponse = new ApiResponseDto<ProjectCostsPivotDto>
            {
                Success = false,
                Errors = errors,
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProjectCostsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectCostsPivotDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProjectCostsPivotAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_WhenApiSuccessButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "PROJECT-001";
            var apiResponse = new ApiResponse<ProjectCostsPivotRes> { Success = true, Data = null };
            var mappedResponse = new ApiResponseDto<ProjectCostsPivotDto>
            {
                Success = false,
                Errors = [],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProjectCostsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectCostsPivotDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProjectCostsPivotAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}