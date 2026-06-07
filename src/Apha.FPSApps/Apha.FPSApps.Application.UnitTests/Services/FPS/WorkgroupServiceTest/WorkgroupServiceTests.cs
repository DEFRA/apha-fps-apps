using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.WorkGroupServiceTest
{
    public class WorkGroupServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsWorkGroupApiClient _fpsWorkGroupApiClient;
        private readonly WorkGroupService _sut;

        public WorkGroupServiceTests()
        {
            _fpsClient            = Substitute.For<IFpsApiClient>();
            _fpsWorkGroupApiClient = Substitute.For<IFpsWorkGroupApiClient>();
            _fpsClient.FpsWorkGroup.Returns(_fpsWorkGroupApiClient);
            _sut = new WorkGroupService(_fpsClient);
        }

        [Fact]
        public void Constructor_WithNullClient_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkGroupService(null!));
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithSuccessResponse_ReturnsWorkgroupNames()
        {
            // Arrange
            var names            = new List<string> { "WG01", "WG02" };
            var expectedResponse = ApiResponseDto<List<string>>.SuccessResponse(names);

            _fpsWorkGroupApiClient.GetAllWorkGroupNamesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsWorkGroupApiClient.Received(1).GetAllWorkGroupNamesAsync();
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<string>>.SuccessResponse(new List<string>());

            _fpsWorkGroupApiClient.GetAllWorkGroupNamesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllWorkGroupNamesAsync();

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

            _fpsWorkGroupApiClient.GetAllWorkGroupNamesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }
    }
}
