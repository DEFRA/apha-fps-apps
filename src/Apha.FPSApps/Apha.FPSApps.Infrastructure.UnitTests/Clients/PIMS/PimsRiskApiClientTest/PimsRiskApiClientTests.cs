using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using MapsterMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsRiskApiClientTest
{
    public class PimsRiskApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsRiskApiClient _client;

        public PimsRiskApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsRiskApiClient(_http, _mapper);
        }

        private static ApiResponse<T> SuccessApiResponse<T>(T data) =>
            new ApiResponse<T> { Success = true, Data = data };

        private static ApiResponse<T> FailureApiResponse<T>() =>
            new ApiResponse<T>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Code = "ERR", Message = "API error" } }
            };

        private static ApiResponseDto<T> SuccessDto<T>(T data) => ApiResponseDto<T>.SuccessResponse(data);

        private static ApiResponseDto<T> FailureDto<T>() =>
            ApiResponseDto<T>.FailureResponse(
                new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "Error" } },
                new ApiMetaDto());

        private static RiskRes MakeRes(int id = 1, string rating = "Low") =>
            new RiskRes { Riskid = id, Riskrating = rating };

        private static RiskDto MakeDto(int id = 1, string rating = "Low") =>
            new RiskDto { Riskid = id, Riskrating = rating };

        [Fact]
        public async Task GetAllRiskRatingsAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            var apiResp = SuccessApiResponse(new List<RiskRes> { MakeRes() });
            var dto = SuccessDto(new List<RiskDto> { MakeDto() });
            _http.GetAsync<List<RiskRes>>(PimsApiEndpoints.GetAllRiskRatings).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<RiskDto>>>(apiResp).Returns(dto);

            var result = await _client.GetAllRiskRatingsAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAllRiskRatingsAsync_HttpThrowsException_PropagatesException()
        {
            _http.GetAsync<List<RiskRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("network"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetAllRiskRatingsAsync());

            Assert.Equal("network", ex.Message);
        }

        [Fact]
        public async Task GetPagedRiskRatingsAsync_HttpReturnsSuccess_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var apiResp = SuccessApiResponse(new List<RiskRes> { MakeRes(1, "Low"), MakeRes(2, "High") });
            apiResp.Pagination = new Pagination { PageNumber = 2, PageSize = 5, TotalRecords = 12, TotalPages = 3 };
            _http.GetAsync<List<RiskRes>>(Arg.Is<string>(s => s.StartsWith(PimsApiEndpoints.GetPagedRiskRatings))).Returns(apiResp);
            _mapper.Map<List<RiskDto>>(apiResp.Data!).Returns(new List<RiskDto> { MakeDto(1, "Low"), MakeDto(2, "High") });

            var result = await _client.GetPagedRiskRatingsAsync(query);

            Assert.True(result.Success);
            Assert.Equal(12, result.Data!.TotalCount);
        }

        [Fact]
        public async Task GetRiskRatingByIdAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            var apiResp = FailureApiResponse<RiskRes>();
            var dto = FailureDto<RiskDto>();
            _http.GetAsync<RiskRes>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<RiskDto>>(apiResp).Returns(dto);

            var result = await _client.GetRiskRatingByIdAsync(99);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetRiskRatingByIdAsync_HttpThrowsException_PropagatesException()
        {
            _http.GetAsync<RiskRes>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetRiskRatingByIdAsync(1));

            Assert.Equal("timeout", ex.Message);
        }

        [Fact]
        public async Task CreateRiskRatingAsync_HttpThrowsException_PropagatesException()
        {
            _mapper.Map<RiskReq>(Arg.Any<RiskDto>()).Returns(new RiskReq());
            _http.PostAsync<RiskReq, RiskRes>(Arg.Any<string>(), Arg.Any<RiskReq>())
                .ThrowsAsync(new Exception("post failed"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.CreateRiskRatingAsync(MakeDto()));

            Assert.Equal("post failed", ex.Message);
        }

        [Fact]
        public async Task DeleteRiskRatingAsync_HttpReturnsSuccess_UsesFormattedEndpoint()
        {
            const int riskId = 6;
            var expectedUrl = string.Format(PimsApiEndpoints.DeleteRiskRating, riskId);
            var apiResp = SuccessApiResponse(true);
            var dto = SuccessDto(true);
            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(dto);

            var result = await _client.DeleteRiskRatingAsync(riskId);

            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool>(expectedUrl);
        }
    }
}
