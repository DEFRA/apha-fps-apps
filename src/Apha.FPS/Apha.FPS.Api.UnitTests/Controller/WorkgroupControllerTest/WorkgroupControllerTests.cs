using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.WorkgroupControllerTest
{
    public class WorkgroupControllerTests
    {
        private readonly IWorkgroupService _serviceMock;
        private readonly WorkgroupController _controller;

        public WorkgroupControllerTests()
        {
            _serviceMock = Substitute.For<IWorkgroupService>();
            _controller  = new WorkgroupController(_serviceMock);
        }

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkgroupController(null!));
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithData_ReturnsOk()
        {
            // Arrange
            var names = new List<string> { "WG01", "WG02" };
            _serviceMock.GetAllWorkgroupNamesAsync(default).Returns(names);

            // Act
            var result = await _controller.GetAllWorkgroupNamesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(names, okResult.Value);
            await _serviceMock.Received(1).GetAllWorkgroupNamesAsync(default);
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithEmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            _serviceMock.GetAllWorkgroupNamesAsync(default).Returns(new List<string>());

            // Act
            var result = await _controller.GetAllWorkgroupNamesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<string>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAllWorkgroupNamesAsync(default)
                .ThrowsAsync(new InvalidOperationException("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.GetAllWorkgroupNamesAsync());
        }
    }
}
