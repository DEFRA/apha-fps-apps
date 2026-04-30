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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsMonthlyOutputCalcsApiClientTest
{
    public class FpsMonthlyOutputCalcsApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsMonthlyOutputCalcsApiClient _client;

        public FpsMonthlyOutputCalcsApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsMonthlyOutputCalcsApiClient(_http, _mapper);
        }

        private static QueryParameters<string> Q() => new() { Page = 1, PageSize = 10 };

        [Fact]
        public async Task GetByProjectAsync_WithSuccessResponse_ReturnsMappedDtoList()
        {
            var projectCode = "AH0033";
            var resList = new List<MonthlyOutputCalcsViewRes> { new() { Buyer = projectCode, TestCode = "TC01" }, new() { Buyer = projectCode, TestCode = "TC02" } };
            var apiResp = new ApiResponse<List<MonthlyOutputCalcsViewRes>> { Success = true, Data = resList, Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 } };
            var expectedDto = ApiResponseDto<List<MonthlyOutputCalcsViewDto>>.SuccessResponse(new List<MonthlyOutputCalcsViewDto> { new() { Buyer = projectCode, TestCode = "TC01" }, new() { Buyer = projectCode, TestCode = "TC02" } }, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });
            _http.GetAsync<List<MonthlyOutputCalcsViewRes>>(Arg.Is<string>(url => url.Contains($"projectCode={projectCode}"))).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<MonthlyOutputCalcsViewDto>>>(apiResp).Returns(expectedDto);
            var result = await _client.GetByProjectAsync(Q(), projectCode);
            Assert.True(result.Success); Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetByProjectAsync_UrlContainsProjectCode()
        {
            var apiResp = new ApiResponse<List<MonthlyOutputCalcsViewRes>> { Success = true, Data = new List<MonthlyOutputCalcsViewRes>() };
            var mappedDto = ApiResponseDto<List<MonthlyOutputCalcsViewDto>>.SuccessResponse(new List<MonthlyOutputCalcsViewDto>());
            _http.GetAsync<List<MonthlyOutputCalcsViewRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<MonthlyOutputCalcsViewDto>>>(apiResp).Returns(mappedDto);
            await _client.GetByProjectAsync(Q(), "PROJ001");
            await _http.Received(1).GetAsync<List<MonthlyOutputCalcsViewRes>>(Arg.Is<string>(url => url.Contains("projectCode=PROJ001")));
        }

        [Fact]
        public async Task GetByProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResp = new ApiResponse<List<MonthlyOutputCalcsViewRes>> { Success = false, Errors = new List<ApiError> { new() { Message = "err", Code = "E" } } };
            var mappedResp = new ApiResponseDto<List<MonthlyOutputCalcsViewDto>> { Success = false, Errors = new List<ApiErrorDto> { new() { Message = "err" } }, Meta = new ApiMetaDto() };
            _http.GetAsync<List<MonthlyOutputCalcsViewRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<MonthlyOutputCalcsViewDto>>>(apiResp).Returns(mappedResp);
            Assert.False((await _client.GetByProjectAsync(Q(), "AH0033")).Success);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithSuccessResponse_ReturnsMappedTotalsDto()
        {
            var apiResp = new ApiResponse<MonthlyOutputCalcsTotalsRes> { Success = true, Data = new MonthlyOutputCalcsTotalsRes { TotalVolume = 10, TotalCost = 1200 } };
            var expectedDto = ApiResponseDto<MonthlyOutputCalcsTotalsDto>.SuccessResponse(new MonthlyOutputCalcsTotalsDto { TotalVolume = 10, TotalCost = 1200 });
            _http.GetAsync<MonthlyOutputCalcsTotalsRes>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<MonthlyOutputCalcsTotalsDto>>(apiResp).Returns(expectedDto);
            var result = await _client.GetTotalActualByProjectAsync("AH0033");
            Assert.True(result.Success); Assert.Equal(10, result.Data?.TotalVolume); Assert.Equal(1200, result.Data?.TotalCost);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_UrlContainsProjectCode()
        {
            var apiResp = new ApiResponse<MonthlyOutputCalcsTotalsRes> { Success = true, Data = new MonthlyOutputCalcsTotalsRes() };
            var expectedDto = ApiResponseDto<MonthlyOutputCalcsTotalsDto>.SuccessResponse(new MonthlyOutputCalcsTotalsDto());
            _http.GetAsync<MonthlyOutputCalcsTotalsRes>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<MonthlyOutputCalcsTotalsDto>>(apiResp).Returns(expectedDto);
            await _client.GetTotalActualByProjectAsync("AH0033");
            await _http.Received(1).GetAsync<MonthlyOutputCalcsTotalsRes>(Arg.Is<string>(url => url.Contains("AH0033")));
        }

        [Fact]
        public async Task DeleteMonthlyOutputCalcsAsync_WithValidRequest_ReturnsSuccessTrue()
        {
            var apiResp = new ApiResponse<bool> { Success = true, Data = true };
            _http.DeleteAsync<MonthlyOutputCalcsReq, bool>(Apha.Common.Constants.FpsApiEndpoints.DeleteMonthlyOutputCalcs, Arg.Is<MonthlyOutputCalcsReq>(r => r.Buyer == "AH0033" && r.TestCode == "TC01")).Returns(apiResp);
            var result = await _client.DeleteMonthlyOutputCalcsAsync("AH0033", "TC01", 1.0, "WG1");
            Assert.True(result.Success); Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteMonthlyOutputCalcsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var apiResp = new ApiResponse<bool> { Success = false, Errors = new List<ApiError> { new() { Message = "err" } } };
            var mappedResp = new ApiResponseDto<bool> { Success = false, Errors = new List<ApiErrorDto> { new() { Message = "err" } }, Meta = new ApiMetaDto() };
            _http.DeleteAsync<MonthlyOutputCalcsReq, bool>(Arg.Any<string>(), Arg.Any<MonthlyOutputCalcsReq>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(mappedResp);
            Assert.False((await _client.DeleteMonthlyOutputCalcsAsync("AH0033", "TC01", 1.0, "WG1")).Success);
        }
    }
}