using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.ProfitCentreControllerTest
{
    public class ProfitCentreControllerTests
    {
        private readonly IProfitCentreService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProfitCentreController _controller;

        public ProfitCentreControllerTests()
        {
            _serviceMock = Substitute.For<IProfitCentreService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new ProfitCentreController(_serviceMock, _mapperMock);
        }

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_WithValidData_ReturnsOk()
        {
            // Arrange
            var dtos = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two" }
            };
            var expectedRes = new List<ProfitCentreRes>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two" }
            };

            _serviceMock.GetProfitCentresAsync().Returns(dtos);
            _mapperMock.Map<List<ProfitCentreRes>>(dtos).Returns(expectedRes);

            // Act
            var result = await _controller.GetProfitCentresAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetProfitCentresAsync();
        }

        [Fact]
        public async Task GetProfitCentresAsync_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos        = new List<ProfitCentreDto>();
            var expectedRes = new List<ProfitCentreRes>();

            _serviceMock.GetProfitCentresAsync().Returns(dtos);
            _mapperMock.Map<List<ProfitCentreRes>>(dtos).Returns(expectedRes);

            // Act
            var result = await _controller.GetProfitCentresAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().BeEquivalentTo(expectedRes);
        }

        [Fact]
        public async Task GetProfitCentresAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetProfitCentresAsync()
                .ThrowsAsync(new InvalidOperationException("Service failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.GetProfitCentresAsync());
        }

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ProfitCentreController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ProfitCentreController(_serviceMock, null!));
        }

        #endregion
    }
}
