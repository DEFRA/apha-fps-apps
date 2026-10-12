using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using MapsterMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsTestsRequiredByYearApiClientTest
{
    public class FpsTestsRequiredByYearApiClientTests
    {
        private readonly IFpsHttpExecutor _http = Substitute.For<IFpsHttpExecutor>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();

        private static ApiResponse<T> Success<T>(T data) => new() { Success = true, Data = data };

        private static ApiResponse<T> Failure<T>() =>
            new() { Success = false, Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } } };

        private static ApiResponseDto<T> MappedFailure<T>() => new()
        {
            Success = false,
            Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERROR" } },
            Meta = new ApiMetaDto()
        };

        [Fact]
        public async Task GetTestsRequiredByWgByYearAsync_OnSuccess_CallsYearEndpointAndReturnsMapped()
        {
            var apiResponse = Success(new List<TestsRequiredByWgRes> { new() { TestCode = "TC1" } });
            var expected = ApiResponseDto<List<TestsRequiredByWgDto>>.SuccessResponse(new List<TestsRequiredByWgDto> { new() { TestCode = "TC1" } });
            _http.GetAsync<List<TestsRequiredByWgRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestsRequiredByWgDto>>>(apiResponse).Returns(expected);
            var client = new FpsTestsRequiredByWgApiClient(_http, _mapper);

            var result = await client.GetTestsRequiredByWgByYearAsync("A S", 2025);

            Assert.Same(expected, result);
            await _http.Received(1).GetAsync<List<TestsRequiredByWgRes>>("api/v1/testsrequiredbywg/byyear?profitCentre=A%20S&fpsYear=2025");
        }

        [Fact]
        public async Task GetTestsRequiredByWgByYearAsync_WithNullProfitCentre_SendsEmptyValue()
        {
            var apiResponse = Success(new List<TestsRequiredByWgRes>());
            _http.GetAsync<List<TestsRequiredByWgRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestsRequiredByWgDto>>>(apiResponse)
                .Returns(ApiResponseDto<List<TestsRequiredByWgDto>>.SuccessResponse(new List<TestsRequiredByWgDto>()));
            var client = new FpsTestsRequiredByWgApiClient(_http, _mapper);

            await client.GetTestsRequiredByWgByYearAsync(null, 2024);

            await _http.Received(1).GetAsync<List<TestsRequiredByWgRes>>("api/v1/testsrequiredbywg/byyear?profitCentre=&fpsYear=2024");
        }

        [Fact]
        public async Task GetTestsRequiredByWgByYearAsync_OnFailure_ReturnsFailureResponse()
        {
            var apiResponse = Failure<List<TestsRequiredByWgRes>>();
            _http.GetAsync<List<TestsRequiredByWgRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestsRequiredByWgDto>>>(apiResponse).Returns(MappedFailure<List<TestsRequiredByWgDto>>());
            var client = new FpsTestsRequiredByWgApiClient(_http, _mapper);

            var result = await client.GetTestsRequiredByWgByYearAsync("ASU", 2025);

            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetTestsRequiredByRcByYearAsync_OnSuccess_CallsYearEndpointAndReturnsMapped()
        {
            var apiResponse = Success(new List<TestsRequiredByRcRes> { new() { TestCode = "TC1" } });
            var expected = ApiResponseDto<List<TestsRequiredByRcDto>>.SuccessResponse(new List<TestsRequiredByRcDto> { new() { TestCode = "TC1" } });
            _http.GetAsync<List<TestsRequiredByRcRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestsRequiredByRcDto>>>(apiResponse).Returns(expected);
            var client = new FpsTestsRequiredByRcApiClient(_http, _mapper);

            var result = await client.GetTestsRequiredByRcByYearAsync("ASU", 2025);

            Assert.Same(expected, result);
            await _http.Received(1).GetAsync<List<TestsRequiredByRcRes>>("api/v1/testsrequiredbyrc/byyear?profitCentre=ASU&fpsYear=2025");
        }

        [Fact]
        public async Task GetTestsRequiredByRcByYearAsync_WithNullProfitCentre_SendsEmptyValue()
        {
            var apiResponse = Success(new List<TestsRequiredByRcRes>());
            _http.GetAsync<List<TestsRequiredByRcRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestsRequiredByRcDto>>>(apiResponse)
                .Returns(ApiResponseDto<List<TestsRequiredByRcDto>>.SuccessResponse(new List<TestsRequiredByRcDto>()));
            var client = new FpsTestsRequiredByRcApiClient(_http, _mapper);

            await client.GetTestsRequiredByRcByYearAsync(null, 2024);

            await _http.Received(1).GetAsync<List<TestsRequiredByRcRes>>("api/v1/testsrequiredbyrc/byyear?profitCentre=&fpsYear=2024");
        }

        [Fact]
        public async Task GetTestsRequiredByRcByYearAsync_OnFailure_ReturnsFailureResponse()
        {
            var apiResponse = Failure<List<TestsRequiredByRcRes>>();
            _http.GetAsync<List<TestsRequiredByRcRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestsRequiredByRcDto>>>(apiResponse).Returns(MappedFailure<List<TestsRequiredByRcDto>>());
            var client = new FpsTestsRequiredByRcApiClient(_http, _mapper);

            var result = await client.GetTestsRequiredByRcByYearAsync("ASU", 2025);

            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }
    }
}
