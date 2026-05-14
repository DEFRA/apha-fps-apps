using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ProfitCentreGradeServiceTest
{
    public class ProfitCentreGradeServiceTests
    {
        private const string DefaultProfitCentre = "PC01";

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProfitCentreGradeApiClient _fpsRcGradeApiClient;
        private readonly ProfitCentreGradeService _sut;

        public ProfitCentreGradeServiceTests()
        {
            _fpsClient           = Substitute.For<IFpsApiClient>();
            _fpsRcGradeApiClient = Substitute.For<IFpsProfitCentreGradeApiClient>();
            _fpsClient.FpsProfitCentreGrade.Returns(_fpsRcGradeApiClient);
            _sut = new ProfitCentreGradeService(_fpsClient);
        }

        #region GetProfitCentreGradesAsync Tests

        [Fact]
        public async Task GetProfitCentreGradesAsync_WithSuccessResponse_ReturnsGradeList()
        {
            // Arrange
            var grades = new List<ProfitCentreGradeDto>
            {
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, ChargeRate = 100m },
                new() { PcGrade = "G002", ProfitCentre = DefaultProfitCentre, ChargeRate = 200m }
            };
            var expectedResponse = ApiResponseDto<List<ProfitCentreGradeDto>>.SuccessResponse(grades);

            _fpsRcGradeApiClient.GetProfitCentreGradesAsync(Arg.Any<QueryParameters<string>>(), DefaultProfitCentre)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentreGradesAsync(DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsRcGradeApiClient.Received(1)
                .GetProfitCentreGradesAsync(Arg.Any<QueryParameters<string>>(), DefaultProfitCentre);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ProfitCentreGradeDto>>.SuccessResponse(new List<ProfitCentreGradeDto>());

            _fpsRcGradeApiClient.GetProfitCentreGradesAsync(Arg.Any<QueryParameters<string>>(), DefaultProfitCentre)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentreGradesAsync(DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProfitCentreGradeDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsRcGradeApiClient.GetProfitCentreGradesAsync(Arg.Any<QueryParameters<string>>(), DefaultProfitCentre)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentreGradesAsync(DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion
    }
}
