using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsProfitCentreApiClientTest
{
    public class FpsProfitCentreApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProfitCentreApiClient _client;

        public FpsProfitCentreApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProfitCentreApiClient(_http, _mapper);
        }

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var resList = new List<ProfitCentreRes>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two" }
            };
            var apiResponse = new ApiResponse<List<ProfitCentreRes>> { Success = true, Data = resList };
            var dtoList     = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two" }
            };
            var expectedDto = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(dtoList);

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProfitCentreRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProfitCentresAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ProfitCentreRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "API Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<ProfitCentreDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetProfitCentresAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ProfitCentreRes>> { Success = true, Data = new List<ProfitCentreRes>() };
            var expectedDto = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>());

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProfitCentresAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ProfitCentreRes>> { Success = true, Data = new List<ProfitCentreRes>() };
            var expectedDto = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>());

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProfitCentresAsync();

            // Assert
            await _http.Received(1).GetAsync<List<ProfitCentreRes>>(
                Arg.Is<string>(url => url.Contains("profitcentres")));
        }

        #endregion
    }
}
