using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsTestSupplierApiClientTest
{
    public class FpsTestSupplierApiClientTests
    {
        private const string DefaultTestCode = "TEST001";
        private const string DefaultBuyer = "BUYER001";

        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsTestSupplierApiClient _client;

        public FpsTestSupplierApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsTestSupplierApiClient(_http, _mapper);
        }

        #region GetPagedAsync

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_ReturnsMappedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>> { Success = true, Data = new List<TestSupplierViewRes>() };
            var expectedDto = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(new List<TestSupplierViewDto>());

            _http.GetAsync<List<TestSupplierViewRes>>(Arg.Is<string>(u => u.Contains($"testCode={DefaultTestCode}"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetPagedAsync(query, DefaultTestCode, false);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<TestSupplierViewRes>>(Arg.Is<string>(u => u.Contains($"testCode={DefaultTestCode}")));
        }

        [Fact]
        public async Task GetPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Code = "ERROR", Message = "API Error" } };
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>> { Success = false, Errors = errors };
            var failureDto = ApiResponseDto<List<TestSupplierViewDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERROR", Message = "API Error" } }, new ApiMetaDto());

            _http.GetAsync<List<TestSupplierViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(failureDto);

            var result = await _client.GetPagedAsync(query, DefaultTestCode, false);

            Assert.False(result.Success);
        }

        #endregion

        #region GetViewByIdAsync

        [Fact]
        public async Task GetViewByIdAsync_WhenMatchFound_ReturnsMatchingRecord()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = int.MaxValue };
            var viewDtos = new List<TestSupplierViewDto>
            {
                new() { TestCode = DefaultTestCode, JobCode = DefaultBuyer, TestCost = 100m }
            };
            var pagedResponse = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(viewDtos);
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>> { Success = true, Data = new List<TestSupplierViewRes>() };

            _http.GetAsync<List<TestSupplierViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(pagedResponse);

            var result = await _client.GetViewByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(DefaultBuyer, result.Data.JobCode);
        }

        [Fact]
        public async Task GetViewByIdAsync_WhenNoMatchFound_ReturnsFailureResponse()
        {
            var pagedResponse = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(new List<TestSupplierViewDto>());
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>> { Success = true, Data = new List<TestSupplierViewRes>() };

            _http.GetAsync<List<TestSupplierViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(pagedResponse);

            var result = await _client.GetViewByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetViewByIdAsync_WhenPagedApiFails_ReturnsFailureResponse()
        {
            var errors = new List<ApiError> { new() { Code = "500", Message = "Server Error" } };
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>> { Success = false, Errors = errors };
            var failureDto = ApiResponseDto<List<TestSupplierViewDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "500", Message = "Server Error" } }, new ApiMetaDto());

            _http.GetAsync<List<TestSupplierViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(failureDto);

            var result = await _client.GetViewByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.False(result.Success);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            var apiResponse = new ApiResponse<TestRequirementRes> { Success = true, Data = new TestRequirementRes() };
            var expectedDto = ApiResponseDto<FpsTestRequirementDto>.SuccessResponse(new FpsTestRequirementDto { TestCode = DefaultTestCode });

            _http.GetAsync<TestRequirementRes>(Arg.Is<string>(u => u.Contains(DefaultTestCode))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<FpsTestRequirementDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<TestRequirementRes>(Arg.Is<string>(u => u.Contains(DefaultTestCode)));
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var errors = new List<ApiError> { new() { Code = "404", Message = "Not Found" } };
            var apiResponse = new ApiResponse<TestRequirementRes> { Success = false, Errors = errors };
            var failureDto = ApiResponseDto<FpsTestRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "404", Message = "Not Found" } }, new ApiMetaDto());

            _http.GetAsync<TestRequirementRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<FpsTestRequirementDto>>(apiResponse).Returns(failureDto);

            var result = await _client.GetByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.False(result.Success);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var req = new TestRequirementReq { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var apiResponse = new ApiResponse<TestRequirementRes> { Success = true, Data = new TestRequirementRes() };
            var expectedDto = ApiResponseDto<FpsTestRequirementDto>.SuccessResponse(dto);

            _mapper.Map<TestRequirementReq>(dto).Returns(req);
            _http.PostAsync<TestRequirementReq, TestRequirementRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<FpsTestRequirementDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.CreateAsync(dto);

            Assert.True(result.Success);
            await _http.Received(1).PostAsync<TestRequirementReq, TestRequirementRes>(Arg.Any<string>(), req);
        }

        [Fact]
        public async Task CreateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var req = new TestRequirementReq();
            var errors = new List<ApiError> { new() { Code = "400", Message = "Bad Request" } };
            var apiResponse = new ApiResponse<TestRequirementRes> { Success = false, Errors = errors };
            var failureDto = ApiResponseDto<FpsTestRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "400", Message = "Bad Request" } }, new ApiMetaDto());

            _mapper.Map<TestRequirementReq>(dto).Returns(req);
            _http.PostAsync<TestRequirementReq, TestRequirementRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<FpsTestRequirementDto>>(apiResponse).Returns(failureDto);

            var result = await _client.CreateAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var req = new TestRequirementReq { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var apiResponse = new ApiResponse<TestRequirementRes> { Success = true, Data = new TestRequirementRes() };
            var expectedDto = ApiResponseDto<FpsTestRequirementDto>.SuccessResponse(dto);

            _mapper.Map<TestRequirementReq>(dto).Returns(req);
            _http.PutAsync<TestRequirementReq, TestRequirementRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<FpsTestRequirementDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.UpdateAsync(dto);

            Assert.True(result.Success);
            await _http.Received(1).PutAsync<TestRequirementReq, TestRequirementRes>(Arg.Any<string>(), req);
        }

        [Fact]
        public async Task UpdateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var req = new TestRequirementReq();
            var errors = new List<ApiError> { new() { Code = "409", Message = "Conflict" } };
            var apiResponse = new ApiResponse<TestRequirementRes> { Success = false, Errors = errors };
            var failureDto = ApiResponseDto<FpsTestRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "409", Message = "Conflict" } }, new ApiMetaDto());

            _mapper.Map<TestRequirementReq>(dto).Returns(req);
            _http.PutAsync<TestRequirementReq, TestRequirementRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<FpsTestRequirementDto>>(apiResponse).Returns(failureDto);

            var result = await _client.UpdateAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WithSuccessResponse_ReturnsSuccess()
        {
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = null };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(u => u.Contains(DefaultTestCode))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            var result = await _client.DeleteAsync(DefaultTestCode, DefaultBuyer);

            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool?>(Arg.Is<string>(u => u.Contains(DefaultTestCode)));
        }

        [Fact]
        public async Task DeleteAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var errors = new List<ApiError> { new() { Code = "404", Message = "Not Found" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var failureDto = ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "404", Message = "Not Found" } }, new ApiMetaDto());

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(failureDto);

            var result = await _client.DeleteAsync(DefaultTestCode, DefaultBuyer);

            Assert.False(result.Success);
        }

        #endregion
    }
}

