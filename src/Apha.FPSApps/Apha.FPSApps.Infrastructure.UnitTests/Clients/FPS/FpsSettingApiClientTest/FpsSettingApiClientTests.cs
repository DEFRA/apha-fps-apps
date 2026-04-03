using Apha.Common.Contracts;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsSettingApiClientTest
{
    public class FpsSettingApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsSettingApiClient _client;

        public FpsSettingApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsSettingApiClient(_http, _mapper);
        }

        #region GetHoursPerDayAsync

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiReturnsSuccess_ReturnsMappedDecimal()
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal> { Success = true, Data = 7.5m };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(7.5m);

            _http.GetAsync<decimal>("api/v1/setting/hoursperday").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetHoursPerDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(7.5m, result.Data);
            await _http.Received(1).GetAsync<decimal>("api/v1/setting/hoursperday");
            _mapper.Received(1).Map<ApiResponseDto<decimal>>(apiResponse);
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiReturnsDefaultValue_ReturnsMappedEight()
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal> { Success = true, Data = 8m };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(8m);

            _http.GetAsync<decimal>("api/v1/setting/hoursperday").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetHoursPerDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(8m, result.Data);
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<decimal> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<decimal>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<decimal>("api/v1/setting/hoursperday").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetHoursPerDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            var error = Assert.Single(result.Errors);
            Assert.Equal("NOT_FOUND", error.Code);
            await _http.Received(1).GetAsync<decimal>("api/v1/setting/hoursperday");
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<decimal>("api/v1/setting/hoursperday")
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetHoursPerDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            var error = Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve hours per day setting", error.Message);
        }

        [Fact]
        public async Task GetHoursPerDayAsync_CallsCorrectEndpointUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal> { Success = true, Data = 8m };
            _http.GetAsync<decimal>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(Arg.Any<object>())
                   .Returns(ApiResponseDto<decimal>.SuccessResponse(8m));

            // Act
            await _client.GetHoursPerDayAsync();

            // Assert
            await _http.Received(1).GetAsync<decimal>("api/v1/setting/hoursperday");
        }

        #endregion
    }
}
