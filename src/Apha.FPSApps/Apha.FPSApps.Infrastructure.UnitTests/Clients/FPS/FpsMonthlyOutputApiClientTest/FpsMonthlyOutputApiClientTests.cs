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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsMonthlyOutputApiClientTest
{
    public class FpsMonthlyOutputApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsMonthlyOutputApiClient _client;

        public FpsMonthlyOutputApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsMonthlyOutputApiClient(_http, _mapper);
        }

        private static QueryParameters<string> Q() => new() { Page = 1, PageSize = 10 };

        [Fact]
        public async Task GetByProjectAsync_WithSuccessResponse_ReturnsMappedDtoList()
        {
            var projectCode = "AH0033";
            var resList = new List<MonthlyOutputRes> { new() { Buyer = projectCode, TestCode = "TC01" }, new() { Buyer = projectCode, TestCode = "TC02" } };
            var apiResp = new ApiResponse<List<MonthlyOutputRes>> { Success = true, Data = resList, Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 } };
            var expectedDto = ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(new List<MonthlyOutputDto> { new() { Buyer = projectCode, TestCode = "TC01" }, new() { Buyer = projectCode, TestCode = "TC02" } }, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });
            _http.GetAsync<List<MonthlyOutputRes>>(Arg.Is<string>(url => url.Contains($"projectCode={projectCode}"))).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<MonthlyOutputDto>>>(apiResp).Returns(expectedDto);
            var result = await _client.GetByProjectAsync(Q(), projectCode);
            Assert.True(result.Success); Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetByProjectAsync_UrlContainsProjectCode()
        {
            var apiResp = new ApiResponse<List<MonthlyOutputRes>> { Success = true, Data = new List<MonthlyOutputRes>() };
            var mappedDto = ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(new List<MonthlyOutputDto>());
            _http.GetAsync<List<MonthlyOutputRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<MonthlyOutputDto>>>(apiResp).Returns(mappedDto);
            await _client.GetByProjectAsync(Q(), "PROJ001");
            await _http.Received(1).GetAsync<List<MonthlyOutputRes>>(Arg.Is<string>(url => url.Contains("projectCode=PROJ001")));
        }

        [Fact]
        public async Task GetByProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResp = new ApiResponse<List<MonthlyOutputRes>> { Success = false, Errors = new List<ApiError> { new() { Message = "err", Code = "E" } } };
            var mappedResp = new ApiResponseDto<List<MonthlyOutputDto>> { Success = false, Errors = new List<ApiErrorDto> { new() { Message = "err" } }, Meta = new ApiMetaDto() };
            _http.GetAsync<List<MonthlyOutputRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<MonthlyOutputDto>>>(apiResp).Returns(mappedResp);
            Assert.False((await _client.GetByProjectAsync(Q(), "AH0033")).Success);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithSuccessResponse_ReturnsTotalVolume()
        {
            var apiResp = new ApiResponse<double> { Success = true, Data = 10.0 };
            var expectedDto = ApiResponseDto<double>.SuccessResponse(10.0);
            _http.GetAsync<double>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<double>>(apiResp).Returns(expectedDto);
            var result = await _client.GetTotalActualByProjectAsync("AH0033");
            Assert.True(result.Success); Assert.Equal(10.0, result.Data);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_UrlContainsProjectCode()
        {
            var apiResp = new ApiResponse<double> { Success = true, Data = 0.0 };
            var expectedDto = ApiResponseDto<double>.SuccessResponse(0.0);
            _http.GetAsync<double>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<double>>(apiResp).Returns(expectedDto);
            await _client.GetTotalActualByProjectAsync("AH0033");
            await _http.Received(1).GetAsync<double>(Arg.Is<string>(url => url.Contains("AH0033")));
        }

        [Fact]
        public async Task DeleteMonthlyOutputAsync_WithValidRequest_ReturnsSuccessTrue()
        {
            var apiResp = new ApiResponse<bool> { Success = true, Data = true };
            _http.DeleteAsync<MonthlyOutputReq, bool>(Apha.Common.Constants.FpsApiEndpoints.DeleteMonthlyOutput, Arg.Is<MonthlyOutputReq>(r => r.Buyer == "AH0033" && r.TestCode == "TC01")).Returns(apiResp);
            var result = await _client.DeleteMonthlyOutputAsync("AH0033", "TC01", 1.0, "WG1");
            Assert.True(result.Success); Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteMonthlyOutputAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var apiResp = new ApiResponse<bool> { Success = false, Errors = new List<ApiError> { new() { Message = "err" } } };
            var mappedResp = new ApiResponseDto<bool> { Success = false, Errors = new List<ApiErrorDto> { new() { Message = "err" } }, Meta = new ApiMetaDto() };
            _http.DeleteAsync<MonthlyOutputReq, bool>(Arg.Any<string>(), Arg.Any<MonthlyOutputReq>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(mappedResp);
            Assert.False((await _client.DeleteMonthlyOutputAsync("AH0033", "TC01", 1.0, "WG1")).Success);
        }
    }
}