using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.WorkgroupControllerTest
{
    public class WorkgroupControllerTests
    {
        private readonly IWorkGroupService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly WorkGroupController _controller;

        public WorkgroupControllerTests()
        {
            _serviceMock = Substitute.For<IWorkGroupService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new WorkGroupController(_serviceMock, _mapperMock);
        }

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkGroupController(null!, _mapperMock));
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithData_ReturnsOk()
        {
            // Arrange
            var names = new List<string> { "WG01", "WG02" };
            _serviceMock.GetAllWorkGroupNamesAsync().Returns(names);

            // Act
            var result = await _controller.GetAllWorkGroupNamesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(names, okResult.Value);
            await _serviceMock.Received(1).GetAllWorkGroupNamesAsync();
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithEmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            _serviceMock.GetAllWorkGroupNamesAsync().Returns(new List<string>());

            // Act
            var result = await _controller.GetAllWorkGroupNamesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<string>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAllWorkGroupNamesAsync()
                .ThrowsAsync(new InvalidOperationException("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.GetAllWorkGroupNamesAsync());
        }
    }
}
