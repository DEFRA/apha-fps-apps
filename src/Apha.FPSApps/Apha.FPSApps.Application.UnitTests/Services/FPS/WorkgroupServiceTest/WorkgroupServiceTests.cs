using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.WorkgroupServiceTest
{
    public class WorkgroupServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsWorkgroupApiClient _fpsWorkgroupApiClient;
        private readonly WorkgroupService _sut;

        public WorkgroupServiceTests()
        {
            _fpsClient            = Substitute.For<IFpsApiClient>();
            _fpsWorkgroupApiClient = Substitute.For<IFpsWorkgroupApiClient>();
            _fpsClient.FpsWorkgroup.Returns(_fpsWorkgroupApiClient);
            _sut = new WorkgroupService(_fpsClient);
        }

        [Fact]
        public void Constructor_WithNullClient_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkgroupService(null!));
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithSuccessResponse_ReturnsWorkgroupNames()
        {
            // Arrange
            var names            = new List<string> { "WG01", "WG02" };
            var expectedResponse = ApiResponseDto<List<string>>.SuccessResponse(names);

            _fpsWorkgroupApiClient.GetAllWorkgroupNamesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllWorkgroupNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsWorkgroupApiClient.Received(1).GetAllWorkgroupNamesAsync();
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<string>>.SuccessResponse(new List<string>());

            _fpsWorkgroupApiClient.GetAllWorkgroupNamesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllWorkgroupNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());

            _fpsWorkgroupApiClient.GetAllWorkgroupNamesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllWorkgroupNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }
    }
}
