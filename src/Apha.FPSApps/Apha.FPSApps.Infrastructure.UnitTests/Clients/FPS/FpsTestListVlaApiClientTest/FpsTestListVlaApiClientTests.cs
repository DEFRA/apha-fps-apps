/*
 * TRANSFORMENGINE MIGRATION — FpsTestListVlaApiClientTests.cs (Infrastructure)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New xUnit test class for FpsTestListVlaApiClient (Infrastructure HTTP client layer)
 *   - Verifies URL construction, IFpsHttpExecutor calls, IMapper calls, and try/catch paths
 *   - NSubstitute for IFpsHttpExecutor and IMapper; NSubstitute.ExceptionExtensions for ThrowsAsync
 *   - Covers GetAllAsync, GetAllByYearAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync
 *
 * PRESERVED:
 *   - ApiResponse<T> (Common.Contracts) used for HTTP-layer mock responses
 *   - ApiResponseDto<T> (FPSApps.Application.Dtos) used for application-layer return type
 *   - INTERNAL_ERROR code used in exception paths
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult]
 *
 * DEFERRED: none — fully automated.
 */

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
using NSubstitute.ExceptionExtensions;
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

        [Fact]
        public async Task GetAllAsync_HttpThrowsException_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _http.GetAsync<List<TestListVlaRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllAsync(query, DefaultFpsYear);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
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

        [Fact]
        public async Task GetAllByYearAsync_HttpThrowsException_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            _http.GetAsync<List<TestListVlaRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllByYearAsync(DefaultFpsYear);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
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

        [Fact]
        public async Task GetByIdAsync_HttpThrowsException_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            _http.GetAsync<TestListVlaRes>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetByIdAsync(DefaultItemCode, DefaultFpsYear);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var request = new TestListVlaReq { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var apiResponse = new ApiResponse<TestListVlaRes>
            {
                Success = true,
                Data = new TestListVlaRes { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear }
            };
            var expectedDto = ApiResponseDto<TestListVlaDto>.SuccessResponse(dto);

            _mapper.Map<TestListVlaReq>(dto).Returns(request);
            _http.PostAsync<TestListVlaReq, TestListVlaRes>(BaseUrl, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestListVlaDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<TestListVlaReq>(dto);
            _mapper.Received(1).Map<ApiResponseDto<TestListVlaDto>>(apiResponse);
        }

        [Fact]
        public async Task CreateAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var request = new TestListVlaReq { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var apiResponse = new ApiResponse<TestListVlaRes> { Success = false };
            var failureDto = ApiResponseDto<TestListVlaDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Conflict" } }, new ApiMetaDto());

            _mapper.Map<TestListVlaReq>(dto).Returns(request);
            _http.PostAsync<TestListVlaReq, TestListVlaRes>(BaseUrl, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestListVlaDto>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateAsync_HttpThrowsException_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            _mapper.Map<TestListVlaReq>(dto).Returns(new TestListVlaReq());
            _http.PostAsync<TestListVlaReq, TestListVlaRes>(Arg.Any<string>(), Arg.Any<TestListVlaReq>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var request = new TestListVlaReq { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var expectedUrl = $"{BaseUrl}/{DefaultItemCode}/{DefaultFpsYear}";
            var apiResponse = new ApiResponse<TestListVlaRes>
            {
                Success = true,
                Data = new TestListVlaRes { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear }
            };
            var expectedDto = ApiResponseDto<TestListVlaDto>.SuccessResponse(dto);

            _mapper.Map<TestListVlaReq>(dto).Returns(request);
            _http.PutAsync<TestListVlaReq, TestListVlaRes>(expectedUrl, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestListVlaDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAsync(DefaultItemCode, DefaultFpsYear, dto);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<TestListVlaReq, TestListVlaRes>(expectedUrl, request);
        }

        [Fact]
        public async Task UpdateAsync_HttpThrowsException_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            _mapper.Map<TestListVlaReq>(dto).Returns(new TestListVlaReq());
            _http.PutAsync<TestListVlaReq, TestListVlaRes>(Arg.Any<string>(), Arg.Any<TestListVlaReq>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdateAsync(DefaultItemCode, DefaultFpsYear, dto);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var expectedUrl = $"{BaseUrl}/{DefaultItemCode}/{DefaultFpsYear}";
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAsync(DefaultItemCode, DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool?>(expectedUrl);
            _mapper.Received(1).Map<ApiResponseDto<bool>>(apiResponse);
        }

        [Fact]
        public async Task DeleteAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var expectedUrl = $"{BaseUrl}/{DefaultItemCode}/{DefaultFpsYear}";
            var apiResponse = new ApiResponse<bool?> { Success = false };
            var failureDto = ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto());

            _http.DeleteAsync<bool?>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.DeleteAsync(DefaultItemCode, DefaultFpsYear);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_HttpThrowsException_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            _http.DeleteAsync<bool?>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.DeleteAsync(DefaultItemCode, DefaultFpsYear);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
        }

        [Fact]
        public async Task DeleteAsync_UrlContainsCompositeKey_CorrectRouteCalled()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);
            string capturedUrl = string.Empty;

            _http.DeleteAsync<bool?>(Arg.Do<string>(url => capturedUrl = url)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.DeleteAsync(DefaultItemCode, DefaultFpsYear);

            // Assert
            Assert.Equal($"{BaseUrl}/{DefaultItemCode}/{DefaultFpsYear}", capturedUrl);
        }

        #endregion
    }
}
