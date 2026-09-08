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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsProgramManagerLinkApiClientTest
{
    public class PimsProgramManagerLinkApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsProgramManagerLinkApiClient _client;

        public PimsProgramManagerLinkApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsProgramManagerLinkApiClient(_http, _mapper);
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

        private static ProgramManagerLinkRes MakeRes(string program = "P1", string mgr = "domain\\user1") =>
            new ProgramManagerLinkRes { Program = program, Manager = mgr };

        private static ProgramManagerLinkDto MakeDto(string program = "P1", string mgr = "domain\\user1") =>
            new ProgramManagerLinkDto { Program = program, Manager = mgr };

        [Fact]
        public async Task GetAllProgramManagerLinksAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            var apiResp = SuccessApiResponse(new List<ProgramManagerLinkRes> { MakeRes() });
            var dto = SuccessDto(new List<ProgramManagerLinkDto> { MakeDto() });
            _http.GetAsync<List<ProgramManagerLinkRes>>(PimsApiEndpoints.GetAllProgramManagerLinks).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(apiResp).Returns(dto);

            var result = await _client.GetAllProgramManagerLinksAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAllProgramManagerLinksAsync_HttpThrowsException_PropagatesException()
        {
            _http.GetAsync<List<ProgramManagerLinkRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("network"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetAllProgramManagerLinksAsync());

            Assert.Equal("network", ex.Message);
        }

        [Fact]
        public async Task GetPagedByManagerAsync_HttpReturnsSuccess_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            const string manager = "dom\\jsmith";
            var apiResp = SuccessApiResponse(new List<ProgramManagerLinkRes> { MakeRes("PR1", manager), MakeRes("PR2", manager) });
            apiResp.Pagination = new Pagination { PageNumber = 2, PageSize = 5, TotalRecords = 12, TotalPages = 3 };
            _http.GetAsync<List<ProgramManagerLinkRes>>(Arg.Is<string>(s => s.StartsWith(PimsApiEndpoints.GetPagedProgramManagerLinks))).Returns(apiResp);
            _mapper.Map<List<ProgramManagerLinkDto>>(apiResp.Data!).Returns(new List<ProgramManagerLinkDto> { MakeDto("PR1", manager), MakeDto("PR2", manager) });

            var result = await _client.GetPagedByManagerAsync(query, manager);

            Assert.True(result.Success);
            Assert.Equal(12, result.Data!.TotalCount);
        }

        [Fact]
        public async Task GetProgramManagerLinkByIdAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            var apiResp = FailureApiResponse<ProgramManagerLinkRes>();
            var dto = FailureDto<ProgramManagerLinkDto>();
            _http.GetAsync<ProgramManagerLinkRes>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<ProgramManagerLinkDto>>(apiResp).Returns(dto);

            var result = await _client.GetProgramManagerLinkByIdAsync("PRX", "dom\\none");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetProgramManagerLinkByIdAsync_HttpThrowsException_PropagatesException()
        {
            _http.GetAsync<ProgramManagerLinkRes>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetProgramManagerLinkByIdAsync("PR1", "dom\\user"));

            Assert.Equal("timeout", ex.Message);
        }

        [Fact]
        public async Task CreateProgramManagerLinkAsync_HttpThrowsException_PropagatesException()
        {
            _mapper.Map<ProgramManagerLinkReq>(Arg.Any<ProgramManagerLinkDto>()).Returns(new ProgramManagerLinkReq());
            _http.PostAsync<ProgramManagerLinkReq, ProgramManagerLinkRes>(Arg.Any<string>(), Arg.Any<ProgramManagerLinkReq>())
                .ThrowsAsync(new Exception("post failed"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.CreateProgramManagerLinkAsync(MakeDto()));

            Assert.Equal("post failed", ex.Message);
        }

        [Fact]
        public async Task DeleteProgramManagerLinkAsync_HttpReturnsSuccess_UsesFormattedEndpoint()
        {
            const string program = "PR01";
            const string manager = "dom\\user";
            var expectedUrl = string.Format(PimsApiEndpoints.DeleteProgramManagerLink, Uri.EscapeDataString(program), Uri.EscapeDataString(manager));
            var apiResp = SuccessApiResponse(true);
            var dto = SuccessDto(true);
            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(dto);

            var result = await _client.DeleteProgramManagerLinkAsync(program, manager);

            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool>(expectedUrl);
        }
    }
}
