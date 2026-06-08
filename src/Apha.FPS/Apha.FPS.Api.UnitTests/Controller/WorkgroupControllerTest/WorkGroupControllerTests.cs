using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
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

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkGroupController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkGroupController(_serviceMock, null!));
        }

        #endregion

        #region GetAllWorkGroupNamesAsync Tests

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_WithData_ReturnsOk()
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
        public async Task GetAllWorkGroupNamesAsync_WithEmptyResult_ReturnsOkWithEmptyList()
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
        public async Task GetAllWorkGroupNamesAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAllWorkGroupNamesAsync()
                .ThrowsAsync(new InvalidOperationException("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.GetAllWorkGroupNamesAsync());
        }

        #endregion

        #region GetWorkGroupsAsync Tests

        [Fact]
        public async Task GetWorkGroupsAsync_WithData_ReturnsOkWithMappedResults()
        {
            // Arrange
            var dtos = new List<WorkGroupViewDto>
            {
                new() { WorkGroupName = "WG01", ProfitCentre = "PC01" }
            };
            var res = new List<WorkGroupRes>
            {
                new() { WorkGroupName = "WG01", ProfitCentre = "PC01" }
            };
            _serviceMock.GetWorkGroupsAsync("PC01").Returns(dtos);
            _mapperMock.Map<List<WorkGroupRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetWorkGroupsAsync("PC01");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<WorkGroupRes>>(okResult.Value);
            Assert.Single(data);
            Assert.Equal("WG01", data[0].WorkGroupName);
        }

        [Fact]
        public async Task GetWorkGroupsAsync_WithEmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            _serviceMock.GetWorkGroupsAsync("PC01").Returns(new List<WorkGroupViewDto>());
            _mapperMock.Map<List<WorkGroupRes>>(Arg.Any<List<WorkGroupViewDto>>()).Returns(new List<WorkGroupRes>());

            // Act
            var result = await _controller.GetWorkGroupsAsync("PC01");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<WorkGroupRes>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetWorkGroupsAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetWorkGroupsAsync("PC01")
                .ThrowsAsync(new ArgumentException("Invalid profit centre"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.GetWorkGroupsAsync("PC01"));
        }

        #endregion
    }
}
