using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.WorkGroupGradeServiceTest
{
    public class WorkGroupGradeServiceTests
    {
        private const string DefaultPcGrade = "G001";
        private const string DefaultWgGrade = "WG01";

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsWorkGroupGradeApiClient _fpsWgGradeApiClient;
        private readonly WorkGroupGradeService _sut;

        public WorkGroupGradeServiceTests()
        {
            _fpsClient           = Substitute.For<IFpsApiClient>();
            _fpsWgGradeApiClient = Substitute.For<IFpsWorkGroupGradeApiClient>();
            _fpsClient.FpsWorkGroupGrade.Returns(_fpsWgGradeApiClient);
            _sut = new WorkGroupGradeService(_fpsClient);
        }

        #region GetWorkGroupGradeAsync Tests

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithSuccessResponse_ReturnsGradeList()
        {
            // Arrange
            var grades = new List<WorkgroupGradeDto>
            {
                new() { WgGrade = DefaultWgGrade, ProfitCentreGrade = DefaultPcGrade }
            };
            var expectedResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(grades);

            _fpsWgGradeApiClient.GetWorkGroupGradeAsync(Arg.Any<QueryParameters<string>>(), DefaultPcGrade)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetWorkGroupGradeAsync(DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsWgGradeApiClient.Received(1)
                .GetWorkGroupGradeAsync(Arg.Any<QueryParameters<string>>(), DefaultPcGrade);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(new List<WorkgroupGradeDto>());

            _fpsWgGradeApiClient.GetWorkGroupGradeAsync(Arg.Any<QueryParameters<string>>(), DefaultPcGrade)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetWorkGroupGradeAsync(DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsWgGradeApiClient.GetWorkGroupGradeAsync(Arg.Any<QueryParameters<string>>(), DefaultPcGrade)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetWorkGroupGradeAsync(DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region DeleteWorkGroupGradeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsWgGradeApiClient.DeleteWorkGroupGradeAsync(DefaultWgGrade).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteWorkGroupGradeAsync(DefaultWgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsWgGradeApiClient.Received(1).DeleteWorkGroupGradeAsync(DefaultWgGrade);
        }

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _fpsWgGradeApiClient.DeleteWorkGroupGradeAsync(DefaultWgGrade).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteWorkGroupGradeAsync(DefaultWgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
