using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsTestListVlaApiClientTest
{
    public class FpsTestListVlaApiClientTests
    {
        private const string DefaultItemCode = "TEST001";
        private const int DefaultFpsYear = 2025;
        private const string BaseUrl = "api/v1/testlistvla";

        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsTestListVlaApiClient _client;

        public FpsTestListVlaApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsTestListVlaApiClient(_http, _mapper);
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var resList = new List<TestListVlaRes> { new() { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear } };
            var apiResponse = new ApiResponse<List<TestListVlaRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<TestListVlaDto>>.SuccessResponse(
                new List<TestListVlaDto> { new() { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear } });

            _http.GetAsync<List<TestListVlaRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllAsync(query, DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<TestListVlaDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllAsync_UrlContainsFpsYear_CorrectQueryStringAppended()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<TestListVlaRes>> { Success = true, Data = new List<TestListVlaRes>() };
            var expectedDto = ApiResponseDto<List<TestListVlaDto>>.SuccessResponse(new List<TestListVlaDto>());
            string capturedUrl = string.Empty;

            _http.GetAsync<List<TestListVlaRes>>(Arg.Do<string>(url => capturedUrl = url)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetAllAsync(query, DefaultFpsYear);

            // Assert
            Assert.Contains($"fpsYear={DefaultFpsYear}", capturedUrl);
        }

        [Fact]
        public async Task GetAllAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<TestListVlaRes>> { Success = false };
            var failureDto = ApiResponseDto<List<TestListVlaDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Server error" } }, new ApiMetaDto());

            _http.GetAsync<List<TestListVlaRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.GetAllAsync(query, DefaultFpsYear);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetAllByYearAsync

        [Fact]
        public async Task GetAllByYearAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var resList = new List<TestListVlaRes> { new() { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear } };
            var apiResponse = new ApiResponse<List<TestListVlaRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<TestListVlaDto>>.SuccessResponse(
                new List<TestListVlaDto> { new() { ItemCode = DefaultItemCode } });

            _http.GetAsync<List<TestListVlaRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllByYearAsync(DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<TestListVlaDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllByYearAsync_UrlContainsLookupPath_CorrectEndpointCalled()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<TestListVlaRes>> { Success = true, Data = new List<TestListVlaRes>() };
            var expectedDto = ApiResponseDto<List<TestListVlaDto>>.SuccessResponse(new List<TestListVlaDto>());
            string capturedUrl = string.Empty;

            _http.GetAsync<List<TestListVlaRes>>(Arg.Do<string>(url => capturedUrl = url)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetAllByYearAsync(DefaultFpsYear);

            // Assert
            Assert.Contains($"{BaseUrl}/lookup", capturedUrl);
            Assert.Contains($"fpsYear={DefaultFpsYear}", capturedUrl);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var res = new TestListVlaRes { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var apiResponse = new ApiResponse<TestListVlaRes> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<TestListVlaDto>.SuccessResponse(
                new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear });

            _http.GetAsync<TestListVlaRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestListVlaDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByIdAsync(DefaultItemCode, DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<TestListVlaDto>>(apiResponse);
        }

        [Fact]
        public async Task GetByIdAsync_UrlContainsCompositeKey_CorrectRouteCalled()
        {
            // Arrange
            var apiResponse = new ApiResponse<TestListVlaRes> { Success = true, Data = new TestListVlaRes() };
            var expectedDto = ApiResponseDto<TestListVlaDto>.SuccessResponse(new TestListVlaDto());
            string capturedUrl = string.Empty;

            _http.GetAsync<TestListVlaRes>(Arg.Do<string>(url => capturedUrl = url)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestListVlaDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetByIdAsync(DefaultItemCode, DefaultFpsYear);

            // Assert
            Assert.Equal($"{BaseUrl}/{DefaultItemCode}/{DefaultFpsYear}", capturedUrl);
        }

        #endregion
    }
}