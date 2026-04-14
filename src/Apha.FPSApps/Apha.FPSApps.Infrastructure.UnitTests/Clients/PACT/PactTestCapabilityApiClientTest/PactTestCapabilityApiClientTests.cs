using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactTestCapabilityApiClientTest
{
    public class PactTestCapabilityApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactWorkGroupTestCapabilityApiClient _client;

        public PactTestCapabilityApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactWorkGroupTestCapabilityApiClient(_http, _mapper);
        }

        #region GetPagedByWorkGroupAsync

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WithValidParams_ReturnsMappedList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestCapabilityRes>> { Success = true, Data = [new TestCapabilityRes { TestCode = "TC1", WorkGroup = "WG1" }] };
            var expectedDto = ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([new TestCapabilityDto { TestCode = "TC1" }]);

            _http.GetAsync<List<TestCapabilityRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/workgroup") && url.Contains("WG1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetPagedByWorkGroupAsync(query, "WG1");

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<TestCapabilityRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/workgroup")));
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<TestCapabilityRes>> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<List<TestCapabilityDto>> { Success = false, Errors = [new ApiErrorDto { Code = "NOT_FOUND" }], Meta = new ApiMetaDto() };

            _http.GetAsync<List<TestCapabilityRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetPagedByWorkGroupAsync(query, "WG1");

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<TestCapabilityRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            var result = await _client.GetPagedByWorkGroupAsync(query, "WG1");

            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve test capabilities by work group", error.Message);
        }

        #endregion

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithValidParams_ReturnsMappedList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestCapabilityRes>> { Success = true, Data = [new TestCapabilityRes { TestCode = "TC1" }] };
            var expectedDto = ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([new TestCapabilityDto { TestCode = "TC1" }]);

            _http.GetAsync<List<TestCapabilityRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/testcode") && url.Contains("TC1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetPagedByTestCodeAsync(query, "TC1");

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<TestCapabilityRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/testcode")));
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<TestCapabilityRes>> { Success = false, Errors = [new ApiError { Code = "ERR" }] };
            var mappedDto = new ApiResponseDto<List<TestCapabilityDto>> { Success = false, Errors = [new ApiErrorDto { Code = "ERR" }], Meta = new ApiMetaDto() };

            _http.GetAsync<List<TestCapabilityRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetPagedByTestCodeAsync(query, "TC1");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            var query = new QueryParameters<string>();
            _http.GetAsync<List<TestCapabilityRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("error"));

            var result = await _client.GetPagedByTestCodeAsync(query, "TC1");

            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve test capabilities by test code", error.Message);
        }

        #endregion

        #region GetTestCapabilityByIdAsync

        [Fact]
        public async Task GetTestCapabilityByIdAsync_WithValidKeys_ReturnsMappedDto()
        {
            var apiResponse = new ApiResponse<TestCapabilityRes> { Success = true, Data = new TestCapabilityRes { TestCode = "TC1", WorkGroup = "WG1" } };
            var expectedDto = ApiResponseDto<TestCapabilityDto>.SuccessResponse(new TestCapabilityDto { TestCode = "TC1" });

            _http.GetAsync<TestCapabilityRes>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/testcapability/") &&
                url.Contains(Uri.EscapeDataString("TC1")) && url.Contains(Uri.EscapeDataString("WG1"))))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestCapabilityDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTestCapabilityByIdAsync("TC1", "WG1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetTestCapabilityByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<TestCapabilityRes> { Success = false, Errors = [new ApiError { Code = "NOT_FOUND" }] };
            var mappedDto = new ApiResponseDto<TestCapabilityDto> { Success = false, Errors = [new ApiErrorDto { Code = "NOT_FOUND" }], Meta = new ApiMetaDto() };

            _http.GetAsync<TestCapabilityRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestCapabilityDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetTestCapabilityByIdAsync("TC1", "WG1");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetTestCapabilityByIdAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            _http.GetAsync<TestCapabilityRes>(Arg.Any<string>()).ThrowsAsync(new Exception("error"));

            var result = await _client.GetTestCapabilityByIdAsync("TC1", "WG1");

            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve test capability", error.Message);
        }

        #endregion

        #region CreateTestCapabilityAsync

        [Fact]
        public async Task CreateTestCapabilityAsync_WithValidDto_PostsAndReturnsMappedDto()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var req = new TestCapabilityReq { TestCode = "TC1", WorkGroup = "WG1" };
            var apiResponse = new ApiResponse<TestCapabilityRes> { Success = true, Data = new TestCapabilityRes { TestCode = "TC1" } };
            var expectedDto = ApiResponseDto<TestCapabilityDto>.SuccessResponse(new TestCapabilityDto { TestCode = "TC1" });

            _mapper.Map<TestCapabilityReq>(dto).Returns(req);
            _http.PostAsync<TestCapabilityReq, TestCapabilityRes>(
                Arg.Is<string>(url => url.Contains("api/v1/testcapability/testcapability")), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestCapabilityDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.CreateTestCapabilityAsync(dto);

            Assert.True(result.Success);
            await _http.Received(1).PostAsync<TestCapabilityReq, TestCapabilityRes>(
                Arg.Is<string>(url => url.Contains("api/v1/testcapability/testcapability")), req);
        }

        [Fact]
        public async Task CreateTestCapabilityAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var req = new TestCapabilityReq();
            var apiResponse = new ApiResponse<TestCapabilityRes> { Success = false, Errors = [new ApiError { Code = "CONFLICT" }] };
            var mappedDto = new ApiResponseDto<TestCapabilityDto> { Success = false, Errors = [new ApiErrorDto { Code = "CONFLICT" }], Meta = new ApiMetaDto() };

            _mapper.Map<TestCapabilityReq>(dto).Returns(req);
            _http.PostAsync<TestCapabilityReq, TestCapabilityRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestCapabilityDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.CreateTestCapabilityAsync(dto);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateTestCapabilityAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            _mapper.Map<TestCapabilityReq>(dto).Returns(new TestCapabilityReq());
            _http.PostAsync<TestCapabilityReq, TestCapabilityRes>(Arg.Any<string>(), Arg.Any<TestCapabilityReq>())
                .ThrowsAsync(new Exception("error"));

            var result = await _client.CreateTestCapabilityAsync(dto);

            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to create test capability", error.Message);
        }

        #endregion

        #region UpdateTestCapabilityAsync

        [Fact]
        public async Task UpdateTestCapabilityAsync_WithValidDto_PutsAndReturnsMappedDto()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var req = new TestCapabilityReq { TestCode = "TC1", WorkGroup = "WG1" };
            var apiResponse = new ApiResponse<TestCapabilityRes> { Success = true, Data = new TestCapabilityRes { TestCode = "TC1" } };
            var expectedDto = ApiResponseDto<TestCapabilityDto>.SuccessResponse(new TestCapabilityDto { TestCode = "TC1" });

            _mapper.Map<TestCapabilityReq>(dto).Returns(req);
            _http.PutAsync<TestCapabilityReq, TestCapabilityRes>(
                Arg.Is<string>(url => url.Contains("api/v1/testcapability/testcapability")), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestCapabilityDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.UpdateTestCapabilityAsync(dto);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateTestCapabilityAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var req = new TestCapabilityReq();
            var apiResponse = new ApiResponse<TestCapabilityRes> { Success = false, Errors = [new ApiError { Code = "ERR" }] };
            var mappedDto = new ApiResponseDto<TestCapabilityDto> { Success = false, Errors = [new ApiErrorDto { Code = "ERR" }], Meta = new ApiMetaDto() };

            _mapper.Map<TestCapabilityReq>(dto).Returns(req);
            _http.PutAsync<TestCapabilityReq, TestCapabilityRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestCapabilityDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.UpdateTestCapabilityAsync(dto);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateTestCapabilityAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            _mapper.Map<TestCapabilityReq>(dto).Returns(new TestCapabilityReq());
            _http.PutAsync<TestCapabilityReq, TestCapabilityRes>(Arg.Any<string>(), Arg.Any<TestCapabilityReq>())
                .ThrowsAsync(new Exception("error"));

            var result = await _client.UpdateTestCapabilityAsync(dto);

            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to update test capability", error.Message);
        }

        #endregion

        #region DeleteTestCapabilityAsync

        [Fact]
        public async Task DeleteTestCapabilityAsync_WithValidKeys_DeletesAndReturnsTrue()
        {
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/testcapability/") &&
                url.Contains(Uri.EscapeDataString("TC1")) && url.Contains(Uri.EscapeDataString("WG1"))))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            var result = await _client.DeleteTestCapabilityAsync("TC1", "WG1");

            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteTestCapabilityAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<bool> { Success = false, Errors = [new ApiError { Code = "NOT_FOUND" }] };
            var mappedDto = new ApiResponseDto<bool> { Success = false, Errors = [new ApiErrorDto { Code = "NOT_FOUND" }], Meta = new ApiMetaDto() };

            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedDto);

            var result = await _client.DeleteTestCapabilityAsync("TC1", "WG1");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteTestCapabilityAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            _http.DeleteAsync<bool>(Arg.Any<string>()).ThrowsAsync(new Exception("error"));

            var result = await _client.DeleteTestCapabilityAsync("TC1", "WG1");

            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to delete test capability", error.Message);
        }

        #endregion              

        #region GetAllTestorProductsAsync

        [Fact]
        public async Task GetAllTestorProductsAsync_WithData_ReturnsMappedList()
        {
            var apiResponse = new ApiResponse<List<TestorProductRes>>
            {
                Success = true,
                Data = [new TestorProductRes { ItemCode = "BLOOD" }, new TestorProductRes { ItemCode = "URINE" }]
            };
            var expectedDto = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
            [
                new TestorProductDto { ItemCode = "BLOOD" },
                new TestorProductDto { ItemCode = "URINE" }
            ]);

            _http.GetAsync<List<TestorProductRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/testcapability/testorproducts")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestorProductDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetAllTestorProductsAsync();

            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetAllTestorProductsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<List<TestorProductRes>> { Success = false, Errors = [new ApiError { Code = "ERR" }] };
            var mappedDto = new ApiResponseDto<List<TestorProductDto>> { Success = false, Errors = [new ApiErrorDto { Code = "ERR" }], Meta = new ApiMetaDto() };

            _http.GetAsync<List<TestorProductRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestorProductDto>>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetAllTestorProductsAsync();

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllTestorProductsAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            _http.GetAsync<List<TestorProductRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("error"));

            var result = await _client.GetAllTestorProductsAsync();

            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve test or products", error.Message);
        }

        #endregion
    }
}
