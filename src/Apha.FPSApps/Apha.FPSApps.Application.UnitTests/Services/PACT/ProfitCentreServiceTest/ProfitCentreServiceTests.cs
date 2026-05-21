using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.ProfitCentreServiceTest
{
    public class ProfitCentreServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactProfitCentreApiClient _pactProfitCentreApiClient;
        private readonly ProfitCentreService _service;

        public ProfitCentreServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactProfitCentreApiClient = Substitute.For<IPactProfitCentreApiClient>();
            _pactClient.PactProfitCentre.Returns(_pactProfitCentreApiClient);
            _service = new ProfitCentreService(_pactClient);
        }

        #region GetAllProfitCentresAsync Tests

        [Fact]
        public async Task GetAllProfitCentresAsync_WithSuccessResponse_ReturnsProfitCentreList()
        {
            // Arrange
            var profitCentres = new List<ProfitCentreSettingsDto>
            {
                new() { ProfitCentre = "PC001", ProfitCentreName = "Centre One" },
                new() { ProfitCentre = "PC002", ProfitCentreName = "Centre Two" }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>.SuccessResponse(profitCentres);
            _pactProfitCentreApiClient.GetAllProfitCentresAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            await _pactProfitCentreApiClient.Received(1).GetAllProfitCentresAsync();
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>.SuccessResponse(
                Enumerable.Empty<ProfitCentreSettingsDto>());
            _pactProfitCentreApiClient.GetAllProfitCentresAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProfitCentreApiClient.GetAllProfitCentresAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetProfitCentreSettingsAsync Tests

        [Fact]
        public async Task GetProfitCentreSettingsAsync_WithValidProfitCentre_ReturnsSettings()
        {
            // Arrange
            const string profitCentre = "PC001";
            var settings = new ProfitCentreSettingsDto
            {
                ProfitCentre = profitCentre,
                Timesheet = -1,
                Outputsheet = 0,
                TimesheetLayout = 1
            };
            var expectedResponse = ApiResponseDto<ProfitCentreSettingsDto>.SuccessResponse(settings);
            _pactProfitCentreApiClient.GetProfitCentreSettingsAsync(profitCentre).Returns(expectedResponse);

            // Act
            var result = await _service.GetProfitCentreSettingsAsync(profitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(profitCentre, result.Data?.ProfitCentre);
            Assert.Equal(-1, result.Data?.Timesheet);
            await _pactProfitCentreApiClient.Received(1).GetProfitCentreSettingsAsync(profitCentre);
        }

        [Fact]
        public async Task GetProfitCentreSettingsAsync_WhenNotFound_ReturnsFailureResponse()
        {
            // Arrange
            const string profitCentre = "PC_MISSING";
            var errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProfitCentreSettingsDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProfitCentreApiClient.GetProfitCentreSettingsAsync(profitCentre).Returns(expectedResponse);

            // Act
            var result = await _service.GetProfitCentreSettingsAsync(profitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region UpdateProfitCentreSettingsAsync Tests

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WithValidInput_ReturnsSuccessTrue()
        {
            // Arrange
            const string profitCentre = "PC001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactProfitCentreApiClient
                .UpdateProfitCentreSettingsAsync(profitCentre, -1, 0, 1)
                .Returns(expectedResponse);

            // Act
            var result = await _service.UpdateProfitCentreSettingsAsync(profitCentre, -1, 0, 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactProfitCentreApiClient.Received(1)
                .UpdateProfitCentreSettingsAsync(profitCentre, -1, 0, 1);
        }

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string profitCentre = "PC001";
            var errors = new List<ApiErrorDto> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactProfitCentreApiClient
                .UpdateProfitCentreSettingsAsync(profitCentre, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>())
                .Returns(expectedResponse);

            // Act
            var result = await _service.UpdateProfitCentreSettingsAsync(profitCentre, 0, 0, 2);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_ForwardsAllParametersToApiClient()
        {
            // Arrange
            const string profitCentre = "PC002";
            const int timesheet = -1;
            const int outputsheet = -1;
            const short timesheetLayout = 2;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactProfitCentreApiClient
                .UpdateProfitCentreSettingsAsync(profitCentre, timesheet, outputsheet, timesheetLayout)
                .Returns(expectedResponse);

            // Act
            await _service.UpdateProfitCentreSettingsAsync(profitCentre, timesheet, outputsheet, timesheetLayout);

            // Assert
            await _pactProfitCentreApiClient.Received(1)
                .UpdateProfitCentreSettingsAsync(profitCentre, timesheet, outputsheet, timesheetLayout);
        }

        #endregion
    }
}
