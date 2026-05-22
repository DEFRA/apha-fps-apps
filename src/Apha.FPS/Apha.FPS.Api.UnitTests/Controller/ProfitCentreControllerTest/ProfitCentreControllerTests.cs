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

        #region GetAllProfitCentres Tests

        [Fact]
        public async Task GetAllProfitCentres_WithValidData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two" }
            };
            var mapped = new List<ProfitCentreRes>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two" }
            };

            _serviceMock.GetAllProfitCentresAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<ProfitCentreRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetAllProfitCentres();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(mapped);
            await _serviceMock.Received(1).GetAllProfitCentresAsync();
        }

        [Fact]
        public async Task GetAllProfitCentres_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos   = new List<ProfitCentreDto>();
            var mapped = new List<ProfitCentreRes>();

            _serviceMock.GetAllProfitCentresAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<ProfitCentreRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetAllProfitCentres();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().BeEquivalentTo(mapped);
        }

        [Fact]
        public async Task GetAllProfitCentres_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAllProfitCentresAsync()
                .ThrowsAsync(new InvalidOperationException("Service failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetAllProfitCentres());
        }

        #endregion

        #region GetProfitCentreById Tests

        [Fact]
        public async Task GetProfitCentreById_WithExistingId_ReturnsOkWithMappedDto()
        {
            // Arrange
            var dto    = new ProfitCentreDto { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" };
            var mapped = new ProfitCentreRes { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" };

            _serviceMock.GetProfitCentreByIdAsync("PC01").Returns(dto);
            _mapperMock.Map<ProfitCentreRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetProfitCentreById("PC01");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(mapped);
            await _serviceMock.Received(1).GetProfitCentreByIdAsync("PC01");
        }

        [Fact]
        public async Task GetProfitCentreById_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.GetProfitCentreByIdAsync("PC_MISSING").Returns((ProfitCentreDto?)null);

            // Act
            var result = await _controller.GetProfitCentreById("PC_MISSING");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetProfitCentreById_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetProfitCentreByIdAsync(Arg.Any<string>())
                .ThrowsAsync(new InvalidOperationException("Service failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetProfitCentreById("PC01"));
        }

        #endregion

        #region PatchSettings Tests

        [Fact]
        public async Task PatchSettings_WithValidRequest_ReturnsOkWithTrue()
        {
            // Arrange
            var request = new UpdateProfitCentreSettingsReq
            {
                ProfitCentre    = "PC01",
                Timesheet       = -1,
                Outputsheet     = -1,
                TimesheetLayout = 1
            };

            _serviceMock.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1).Returns(true);

            // Act
            var result = await _controller.PatchSettings(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);
        }

        [Fact]
        public async Task PatchSettings_WithNullOrEmptyProfitCentre_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateProfitCentreSettingsReq { ProfitCentre = "" };

            // Act
            var result = await _controller.PatchSettings(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _serviceMock.DidNotReceive()
                .UpdateProfitCentreSettingsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>());
        }

        [Fact]
        public async Task PatchSettings_WithWhitespaceProfitCentre_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateProfitCentreSettingsReq { ProfitCentre = "   " };

            // Act
            var result = await _controller.PatchSettings(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task PatchSettings_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new UpdateProfitCentreSettingsReq { ProfitCentre = "PC01" };
            _serviceMock.UpdateProfitCentreSettingsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>())
                .ThrowsAsync(new InvalidOperationException("Service failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.PatchSettings(request));
        }

        #endregion
    }
}
