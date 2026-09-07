using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsProfitCentreManagerLinkApiClientTest
{
    public class PimsProfitCentreManagerLinkApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsProfitCentreManagerLinkApiClient _client;

        public PimsProfitCentreManagerLinkApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsProfitCentreManagerLinkApiClient(_http, _mapper);
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

        private static ProfitCentreManagerLinkRes MakeRes(string pc = "PC1", string mgr = "domain\\user1") =>
            new ProfitCentreManagerLinkRes { ProfitCentre = pc, Manager = mgr };

        private static ProfitCentreManagerLinkDto MakeDto(string pc = "PC1", string mgr = "domain\\user1") =>
            new ProfitCentreManagerLinkDto { ProfitCentre = pc, Manager = mgr };

        [Fact]
        public async Task GetAllProfitCentreManagerLinksAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            var apiResp = SuccessApiResponse(new List<ProfitCentreManagerLinkRes> { MakeRes() });
            var dto = SuccessDto(new List<ProfitCentreManagerLinkDto> { MakeDto() });
            _http.GetAsync<List<ProfitCentreManagerLinkRes>>(PimsApiEndpoints.GetAllProfitCentreManagerLinks).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(apiResp).Returns(dto);

            var result = await _client.GetAllProfitCentreManagerLinksAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAllProfitCentreManagerLinksAsync_HttpThrowsException_PropagatesException()
        {
            _http.GetAsync<List<ProfitCentreManagerLinkRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("network"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetAllProfitCentreManagerLinksAsync());

            Assert.Equal("network", ex.Message);
        }

        [Fact]
        public async Task GetPagedByManagerAsync_HttpReturnsSuccess_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            const string manager = "dom\\jsmith";
            var apiResp = SuccessApiResponse(new List<ProfitCentreManagerLinkRes> { MakeRes("PC1", manager), MakeRes("PC2", manager) });
            apiResp.Pagination = new Pagination { PageNumber = 2, PageSize = 5, TotalRecords = 12, TotalPages = 3 };
            _http.GetAsync<List<ProfitCentreManagerLinkRes>>(Arg.Is<string>(s => s.StartsWith(PimsApiEndpoints.GetPagedProfitCentreManagerLinks))).Returns(apiResp);
            _mapper.Map<List<ProfitCentreManagerLinkDto>>(apiResp.Data!).Returns(new List<ProfitCentreManagerLinkDto> { MakeDto("PC1", manager), MakeDto("PC2", manager) });

            var result = await _client.GetPagedByManagerAsync(query, manager);

            Assert.True(result.Success);
            Assert.Equal(12, result.Data!.TotalCount);
        }

        [Fact]
        public async Task GetProfitCentreManagerLinkByIdAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            var pc = "PCX";
            var manager = "dom\\none";
            var apiResp = FailureApiResponse<ProfitCentreManagerLinkRes>();
            var dto = FailureDto<ProfitCentreManagerLinkDto>();
            _http.GetAsync<ProfitCentreManagerLinkRes>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<ProfitCentreManagerLinkDto>>(apiResp).Returns(dto);

            var result = await _client.GetProfitCentreManagerLinkByIdAsync(pc, manager);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetProfitCentreManagerLinkByIdAsync_HttpThrowsException_PropagatesException()
        {
            _http.GetAsync<ProfitCentreManagerLinkRes>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetProfitCentreManagerLinkByIdAsync("PC1", "dom\\user"));

            Assert.Equal("timeout", ex.Message);
        }

        [Fact]
        public async Task CreateProfitCentreManagerLinkAsync_HttpThrowsException_PropagatesException()
        {
            _mapper.Map<ProfitCentreManagerLinkReq>(Arg.Any<ProfitCentreManagerLinkDto>()).Returns(new ProfitCentreManagerLinkReq());
            _http.PostAsync<ProfitCentreManagerLinkReq, ProfitCentreManagerLinkRes>(Arg.Any<string>(), Arg.Any<ProfitCentreManagerLinkReq>())
                .ThrowsAsync(new Exception("post failed"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.CreateProfitCentreManagerLinkAsync(MakeDto()));

            Assert.Equal("post failed", ex.Message);
        }

        [Fact]
        public async Task DeleteProfitCentreManagerLinkAsync_HttpReturnsSuccess_UsesFormattedEndpoint()
        {
            const string pc = "PC01";
            const string manager = "dom\\user";
            var expectedUrl = string.Format(PimsApiEndpoints.DeleteProfitCentreManagerLink, Uri.EscapeDataString(pc), Uri.EscapeDataString(manager));
            var apiResp = SuccessApiResponse(true);
            var dto = SuccessDto(true);
            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(dto);

            var result = await _client.DeleteProfitCentreManagerLinkAsync(pc, manager);

            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool>(expectedUrl);
        }
    }
}
