using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.TestsRequiredByYearServiceTest
{
    public class TestsRequiredByYearServiceTests
    {
        private readonly IFpsApiClient _fpsClient = Substitute.For<IFpsApiClient>();
        private readonly IFpsTestsRequiredByWgApiClient _wgClient = Substitute.For<IFpsTestsRequiredByWgApiClient>();
        private readonly IFpsTestsRequiredByRcApiClient _rcClient = Substitute.For<IFpsTestsRequiredByRcApiClient>();

        public TestsRequiredByYearServiceTests()
        {
            _fpsClient.FpsTestsRequiredByWg.Returns(_wgClient);
            _fpsClient.FpsTestsRequiredByRc.Returns(_rcClient);
        }

        [Fact]
        public async Task GetTestsRequiredByWgByYearAsync_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<List<TestsRequiredByWgDto>>.SuccessResponse(
                new List<TestsRequiredByWgDto> { new() { TestCode = "TC1" } });
            _wgClient.GetTestsRequiredByWgByYearAsync("ASU", 2025).Returns(expected);
            var sut = new TestsRequiredByWgService(_fpsClient);

            var result = await sut.GetTestsRequiredByWgByYearAsync("ASU", 2025);

            Assert.Same(expected, result);
            await _wgClient.Received(1).GetTestsRequiredByWgByYearAsync("ASU", 2025);
        }

        [Fact]
        public async Task GetTestsRequiredByWgByYearAsync_WhenApiFails_ReturnsFailure()
        {
            var failure = ApiResponseDto<List<TestsRequiredByWgDto>>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto());
            _wgClient.GetTestsRequiredByWgByYearAsync(null, 2024).Returns(failure);
            var sut = new TestsRequiredByWgService(_fpsClient);

            var result = await sut.GetTestsRequiredByWgByYearAsync(null, 2024);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetTestsRequiredByRcByYearAsync_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<List<TestsRequiredByRcDto>>.SuccessResponse(
                new List<TestsRequiredByRcDto> { new() { TestCode = "TC1" } });
            _rcClient.GetTestsRequiredByRcByYearAsync("ASU", 2025).Returns(expected);
            var sut = new TestsRequiredByRcService(_fpsClient);

            var result = await sut.GetTestsRequiredByRcByYearAsync("ASU", 2025);

            Assert.Same(expected, result);
            await _rcClient.Received(1).GetTestsRequiredByRcByYearAsync("ASU", 2025);
        }

        [Fact]
        public async Task GetTestsRequiredByRcByYearAsync_WhenApiFails_ReturnsFailure()
        {
            var failure = ApiResponseDto<List<TestsRequiredByRcDto>>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto());
            _rcClient.GetTestsRequiredByRcByYearAsync(null, 2024).Returns(failure);
            var sut = new TestsRequiredByRcService(_fpsClient);

            var result = await sut.GetTestsRequiredByRcByYearAsync(null, 2024);

            Assert.False(result.Success);
        }
    }
}
