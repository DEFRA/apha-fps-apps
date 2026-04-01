using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.SettingServiceTest
{
    public class SettingServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsSettingApiClient _fpsSettingApiClient;
        private readonly SettingService _sut;

        public SettingServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsSettingApiClient = Substitute.For<IFpsSettingApiClient>();
            _fpsClient.FpsSetting.Returns(_fpsSettingApiClient);
            _sut = new SettingService(_fpsClient);
        }

        #region GetHoursPerDayAsync

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiReturnsSuccess_ReturnsSuccessResponseWithValue()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(7.5m);
            _fpsSettingApiClient.GetHoursPerDayAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(7.5m, result.Data);
            await _fpsSettingApiClient.Received(1).GetHoursPerDayAsync();
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiReturnsDefaultValue_ReturnsSuccessResponseWithEight()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(8m);
            _fpsSettingApiClient.GetHoursPerDayAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(8m, result.Data);
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Setting not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto());
            _fpsSettingApiClient.GetHoursPerDayAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Setting not found", error.Message);
            await _fpsSettingApiClient.Received(1).GetHoursPerDayAsync();
        }

        [Fact]
        public async Task GetHoursPerDayAsync_DelegatesToFpsSettingApiClient()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(8m);
            _fpsSettingApiClient.GetHoursPerDayAsync().Returns(expectedResponse);

            // Act
            await _sut.GetHoursPerDayAsync();

            // Assert — verify delegation to the correct sub-client
            await _fpsSettingApiClient.Received(1).GetHoursPerDayAsync();
            await _fpsClient.Received(1).FpsSetting.GetHoursPerDayAsync();
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsSettingApiClient.GetHoursPerDayAsync().ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetHoursPerDayAsync());
            Assert.Equal("API unavailable", exception.Message);
            await _fpsSettingApiClient.Received(1).GetHoursPerDayAsync();
        }

        #endregion
    }
}
