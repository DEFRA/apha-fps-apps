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

        #region GetAllProfitCentresAsync Tests

        [Fact]
        public async Task GetAllProfitCentresAsync_WithSuccessResponse_ReturnsEnumerable()
        {
            // Arrange
            var dtos = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two" }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<ProfitCentreDto>>.SuccessResponse(dtos);

            _fpsProfitCentreApiClient.GetAllProfitCentresAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllProfitCentresAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            await _fpsProfitCentreApiClient.Received(1).GetAllProfitCentresAsync();
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors           = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<IEnumerable<ProfitCentreDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.GetAllProfitCentresAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllProfitCentresAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetProfitCentreByIdAsync Tests

        [Fact]
        public async Task GetProfitCentreByIdAsync_WithSuccessResponse_ReturnsDto()
        {
            // Arrange
            var dto              = new ProfitCentreDto { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" };
            var expectedResponse = ApiResponseDto<ProfitCentreDto>.SuccessResponse(dto);

            _fpsProfitCentreApiClient.GetProfitCentreByIdAsync("PC01").Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentreByIdAsync("PC01");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("PC01", result.Data?.ProfitCentreId);
            await _fpsProfitCentreApiClient.Received(1).GetProfitCentreByIdAsync("PC01");
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors           = new List<ApiErrorDto> { new() { Message = "Not found", Code = "404" } };
            var expectedResponse = ApiResponseDto<ProfitCentreDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.GetProfitCentreByIdAsync("PC_MISSING").Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentreByIdAsync("PC_MISSING");

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region UpdateProfitCentreSettingsAsync Tests

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsProfitCentreApiClient.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsProfitCentreApiClient.Received(1).UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);
        }

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors           = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.UpdateProfitCentreSettingsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>())
                .Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion
    }
}
