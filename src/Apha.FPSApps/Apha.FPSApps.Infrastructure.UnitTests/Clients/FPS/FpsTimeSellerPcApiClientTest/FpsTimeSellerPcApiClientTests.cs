using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsTimeSellerPcApiClientTest
{
    public class FpsTimeSellerPcApiClientTests
    {
        private readonly IFpsHttpExecutor         _http;
        private readonly IMapper                  _mapper;
        private readonly FpsTimeSellerPcApiClient _client;

        public FpsTimeSellerPcApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsTimeSellerPcApiClient(_http, _mapper);
        }

        private static ApiResponse<List<TimeSellerPcRowRes>> MakeRowsApiResponse(bool success = true)
            => new()
            {
                Success = success,
                Data    = success ? new List<TimeSellerPcRowRes>
                {
                    new() { WorkGroup = "WG1", WgGrade = "G1", Fec = 500m },
                    new() { WorkGroup = "WG2", WgGrade = "G2", Fec = 750m }
                } : null,
                Errors = success ? null : new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };

        private static ApiResponse<TimeSellerPcTotalsRes> MakeTotalsApiResponse(bool success = true)
            => new()
            {
                Success = success,
                Data    = success ? new TimeSellerPcTotalsRes { SellingPc = "ENV", TotalFec = 1250m } : null,
                Errors  = success ? null : new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };

        #region GetRowsAsync

        [Fact]
        public async Task GetRowsAsync_WhenApiSucceeds_ReturnsMappedDtos()
        {
            // Arrange
            var sellingPc   = "ENV";
            var apiResponse = MakeRowsApiResponse();
            var expectedDto = ApiResponseDto<List<TimeSellerPcRowDto>>.SuccessResponse(
                new List<TimeSellerPcRowDto>
                {
                    new() { WorkGroup = "WG1" },
                    new() { WorkGroup = "WG2" }
                });

            _http.GetAsync<List<TimeSellerPcRowRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeSellerPcRowDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetRowsAsync(sellingPc);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<TimeSellerPcRowRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<TimeSellerPcRowDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetRowsAsync_UsesCorrectUrl()
        {
            // Arrange
            var sellingPc   = "ENV";
            var apiResponse = MakeRowsApiResponse();
            var dto         = ApiResponseDto<List<TimeSellerPcRowDto>>.SuccessResponse(new List<TimeSellerPcRowDto>());

            _http.GetAsync<List<TimeSellerPcRowRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeSellerPcRowDto>>>(Arg.Any<ApiResponse<List<TimeSellerPcRowRes>>>()).Returns(dto);

            // Act
            await _client.GetRowsAsync(sellingPc);

            // Assert
            await _http.Received(1).GetAsync<List<TimeSellerPcRowRes>>(
                Arg.Is<string>(url => url.Contains("timeseller")
                                   && url.Contains(sellingPc)
                                   && url.Contains("rows")));
        }

        [Fact]
        public async Task GetRowsAsync_WhenApiReturnsFail_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse    = MakeRowsApiResponse(success: false);
            var mappedResponse = new ApiResponseDto<List<TimeSellerPcRowDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<TimeSellerPcRowRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeSellerPcRowDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetRowsAsync("ENV");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetTotalsAsync

        [Fact]
        public async Task GetTotalsAsync_WhenApiSucceeds_ReturnsMappedDto()
        {
            // Arrange
            var sellingPc   = "ENV";
            var apiResponse = MakeTotalsApiResponse();
            var expectedDto = ApiResponseDto<TimeSellerPcTotalsDto>.SuccessResponse(
                new TimeSellerPcTotalsDto { SellingPc = sellingPc, TotalFec = 1250m });

            _http.GetAsync<TimeSellerPcTotalsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeSellerPcTotalsDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalsAsync(sellingPc);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(sellingPc, result.Data?.SellingPc);
            await _http.Received(1).GetAsync<TimeSellerPcTotalsRes>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<TimeSellerPcTotalsDto>>(apiResponse);
        }

        [Fact]
        public async Task GetTotalsAsync_UsesCorrectUrl()
        {
            // Arrange
            var sellingPc   = "ENV";
            var apiResponse = MakeTotalsApiResponse();
            var dto         = ApiResponseDto<TimeSellerPcTotalsDto>.SuccessResponse(
                new TimeSellerPcTotalsDto { SellingPc = sellingPc });

            _http.GetAsync<TimeSellerPcTotalsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeSellerPcTotalsDto>>(Arg.Any<ApiResponse<TimeSellerPcTotalsRes>>()).Returns(dto);

            // Act
            await _client.GetTotalsAsync(sellingPc);

            // Assert
            await _http.Received(1).GetAsync<TimeSellerPcTotalsRes>(
                Arg.Is<string>(url => url.Contains("timeseller")
                                   && url.Contains(sellingPc)
                                   && url.Contains("totals")));
        }

        [Fact]
        public async Task GetTotalsAsync_WhenApiReturnsFail_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse    = MakeTotalsApiResponse(success: false);
            var mappedResponse = new ApiResponseDto<TimeSellerPcTotalsDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<TimeSellerPcTotalsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeSellerPcTotalsDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetTotalsAsync("ENV");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
