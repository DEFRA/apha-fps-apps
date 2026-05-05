using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.Costbook.CostBookStaffApiClientTest
{
    public class CostBookStaffApiClientTests
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly CostBookStaffApiClient _client;

        public CostBookStaffApiClientTests()
        {
            _http = Substitute.For<ICostBookHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new CostBookStaffApiClient(_http, _mapper);
        }

        #region GetAllStaffAsync Tests

        [Fact]
        public async Task GetAllStaffAsync_WithSuccessResponse_ReturnsMappedStaffList()
        {
            // Arrange
            var staffResList = new List<StaffRes> { new StaffRes(), new StaffRes() };
            var apiResponse = new ApiResponse<List<StaffRes>> { Success = true, Data = staffResList };
            var mappedStaffDto = new List<StaffDto> { new StaffDto(), new StaffDto() };
            var expectedDto = ApiResponseDto<List<StaffDto>>.SuccessResponse(mappedStaffDto);

            _http.GetAsync<List<StaffRes>>("api/v1/projects/staff").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<StaffRes>>("api/v1/projects/staff");
            _mapper.Received(1).Map<ApiResponseDto<List<StaffDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllStaffAsync_WithSuccessResponseButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<StaffRes>> { Success = true, Data = null };
            var mappedResponse = new ApiResponseDto<List<StaffDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "No data", Code = "NO_DATA" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<StaffRes>>("api/v1/projects/staff").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<StaffRes>>("api/v1/projects/staff");
            _mapper.Received(1).Map<ApiResponseDto<List<StaffDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllStaffAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<StaffRes>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } }
            };
            var mappedResponse = new ApiResponseDto<List<StaffDto>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<StaffRes>>("api/v1/projects/staff").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
            await _http.Received(1).GetAsync<List<StaffRes>>("api/v1/projects/staff");
            _mapper.Received(1).Map<ApiResponseDto<List<StaffDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllStaffAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var exceptionMessage = "Network connection failed";
            _http.GetAsync<List<StaffRes>>("api/v1/projects/staff")
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _client.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve staff", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal(exceptionMessage, result.Errors[0].Details);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<StaffRes>>("api/v1/projects/staff");
        }

        [Fact]
        public async Task GetAllStaffAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<StaffRes>> { Success = true, Data = new List<StaffRes> { new StaffRes() } };
            _http.GetAsync<List<StaffRes>>("api/v1/projects/staff").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve staff", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            await _http.Received(1).GetAsync<List<StaffRes>>("api/v1/projects/staff");
        }

        [Fact]
        public async Task GetAllStaffAsync_WithEmptyDataList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<StaffRes>> { Success = true, Data = new List<StaffRes>() };
            var mappedDto = ApiResponseDto<List<StaffDto>>.SuccessResponse(new List<StaffDto>());

            _http.GetAsync<List<StaffRes>>("api/v1/projects/staff").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            await _http.Received(1).GetAsync<List<StaffRes>>("api/v1/projects/staff");
        }

        [Fact]
        public async Task GetAllStaffAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<StaffRes>> { Success = true, Data = new List<StaffRes>() };
            _http.GetAsync<List<StaffRes>>("api/v1/projects/staff").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffDto>>>(apiResponse)
                .Returns(ApiResponseDto<List<StaffDto>>.SuccessResponse(new List<StaffDto>()));

            // Act
            await _client.GetAllStaffAsync();

            // Assert
            await _http.Received(1).GetAsync<List<StaffRes>>(Arg.Is<string>(s => s == "api/v1/projects/staff"));
        }

        #endregion
    }
}
