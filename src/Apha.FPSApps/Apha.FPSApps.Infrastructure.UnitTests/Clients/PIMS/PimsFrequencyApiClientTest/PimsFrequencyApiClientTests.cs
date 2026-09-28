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
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsFrequencyApiClientTest
{
    public class PimsFrequencyApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsFrequencyApiClient _client;

        public PimsFrequencyApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsFrequencyApiClient(_http, _mapper);
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

        private static FrequencyRes MakeRes(int id = 1, string value = "Monthly") =>
            new FrequencyRes { Frequencyid = id, FrequencyValue = value };

        private static FrequencyDto MakeDto(int id = 1, string value = "Monthly") =>
            new FrequencyDto { Frequencyid = id, FrequencyValue = value };

        [Fact]
        public async Task GetAllFrequenciesAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            var apiResp = SuccessApiResponse(new List<FrequencyRes> { MakeRes() });
            var dto = SuccessDto(new List<FrequencyDto> { MakeDto() });
            _http.GetAsync<List<FrequencyRes>>(PimsApiEndpoints.GetAllFrequencies).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<FrequencyDto>>>(apiResp).Returns(dto);

            var result = await _client.GetAllFrequenciesAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAllFrequenciesAsync_HttpThrowsException_PropagatesException()
        {
            _http.GetAsync<List<FrequencyRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetAllFrequenciesAsync());

            Assert.Equal("Network error", ex.Message);
        }

        [Fact]
        public async Task GetPagedFrequenciesAsync_HttpReturnsSuccess_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var apiResp = SuccessApiResponse(new List<FrequencyRes> { MakeRes(1), MakeRes(2) });
            apiResp.Pagination = new Pagination { PageNumber = 2, PageSize = 5, TotalRecords = 20, TotalPages = 4 };
            _http.GetAsync<List<FrequencyRes>>(Arg.Is<string>(s => s.StartsWith(PimsApiEndpoints.GetPagedFrequencies))).Returns(apiResp);
            _mapper.Map<List<FrequencyDto>>(apiResp.Data!).Returns(new List<FrequencyDto> { MakeDto(1), MakeDto(2) });

            var result = await _client.GetPagedFrequenciesAsync(query);

            Assert.True(result.Success);
            Assert.Equal(20, result.Data!.TotalCount);
        }

        [Fact]
        public async Task GetPagedFrequenciesAsync_HttpThrowsException_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<FrequencyRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetPagedFrequenciesAsync(query));

            Assert.Equal("timeout", ex.Message);
        }

        [Fact]
        public async Task GetFrequencyByIdAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            var apiResp = FailureApiResponse<FrequencyRes>();
            var dto = FailureDto<FrequencyDto>();
            _http.GetAsync<FrequencyRes>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<FrequencyDto>>(apiResp).Returns(dto);

            var result = await _client.GetFrequencyByIdAsync(99);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetFrequencyByIdAsync_HttpThrowsException_PropagatesException()
        {
            _http.GetAsync<FrequencyRes>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetFrequencyByIdAsync(1));

            Assert.Equal("timeout", ex.Message);
        }

        [Fact]
        public async Task CreateFrequencyAsync_HttpThrowsException_PropagatesException()
        {
            _mapper.Map<FrequencyReq>(Arg.Any<FrequencyDto>()).Returns(new FrequencyReq());
            _http.PostAsync<FrequencyReq, FrequencyRes>(Arg.Any<string>(), Arg.Any<FrequencyReq>())
                .ThrowsAsync(new Exception("POST failed"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.CreateFrequencyAsync(MakeDto()));

            Assert.Equal("POST failed", ex.Message);
        }

        [Fact]
        public async Task UpdateFrequencyAsync_HttpThrowsException_PropagatesException()
        {
            _mapper.Map<FrequencyReq>(Arg.Any<FrequencyDto>()).Returns(new FrequencyReq());
            _http.PutAsync<FrequencyReq, FrequencyRes>(Arg.Any<string>(), Arg.Any<FrequencyReq>())
                .ThrowsAsync(new Exception("PUT failed"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.UpdateFrequencyAsync(1, MakeDto()));

            Assert.Equal("PUT failed", ex.Message);
        }

        [Fact]
        public async Task DeleteFrequencyAsync_HttpThrowsException_PropagatesException()
        {
            _http.DeleteAsync<bool>(Arg.Any<string>()).ThrowsAsync(new Exception("DELETE failed"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.DeleteFrequencyAsync(1));

            Assert.Equal("DELETE failed", ex.Message);
        }
    }
}
