using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ProfitCentreServiceTest
{
    public class ProfitCentreServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProfitCentreApiClient _fpsProfitCentreApiClient;
        private readonly ProfitCentreService _sut;

        public ProfitCentreServiceTests()
        {
            _fpsClient               = Substitute.For<IFpsApiClient>();
            _fpsProfitCentreApiClient = Substitute.For<IFpsProfitCentreApiClient>();
            _fpsClient.FpsProfitCentre.Returns(_fpsProfitCentreApiClient);
            _sut = new ProfitCentreService(_fpsClient);
        }

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_WithSuccessResponse_ReturnsProfitCentreList()
        {
            // Arrange
            var profitCentres = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two" }
            };
            var expectedResponse = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(profitCentres);

            _fpsProfitCentreApiClient.GetProfitCentresAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProfitCentreApiClient.Received(1).GetProfitCentresAsync();
        }

        [Fact]
        public async Task GetProfitCentresAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>());

            _fpsProfitCentreApiClient.GetProfitCentresAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProfitCentresAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.GetProfitCentresAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion
    }
}
