using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.ProfitCentreControllerTest
{
    public class ProfitCentreControllerTests
    {
        private readonly IProfitCentreService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProfitCentreController _controller;

        public ProfitCentreControllerTests()
        {
            _serviceMock = Substitute.For<IProfitCentreService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProfitCentreController(_serviceMock, _mapperMock);
        }

        #region GetAllProfitCentres

        [Fact]
        public async Task GetAllProfitCentres_HappyPath_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<ProfitCentreSettingsDto>
            {
                new() { ProfitCentre = "PC001", ProfitCentreName = "Centre One" },
                new() { ProfitCentre = "PC002", ProfitCentreName = "Centre Two" }
            };
            var mapped = new List<ProfitCentreSettingsRes>
            {
                new() { ProfitCentre = "PC001", ProfitCentreName = "Centre One" },
                new() { ProfitCentre = "PC002", ProfitCentreName = "Centre Two" }
            };

            _serviceMock.GetAllProfitCentresAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<ProfitCentreSettingsRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetAllProfitCentres();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetAllProfitCentresAsync();
        }

        [Fact]
        public async Task GetAllProfitCentres_EmptyList_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            var dtos = new List<ProfitCentreSettingsDto>();
            var mapped = new List<ProfitCentreSettingsRes>();

            _serviceMock.GetAllProfitCentresAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<ProfitCentreSettingsRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetAllProfitCentres();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value as IEnumerable<ProfitCentreSettingsRes>;
            Assert.NotNull(value);
            Assert.Empty(value);
        }

        [Fact]
        public async Task GetAllProfitCentres_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAllProfitCentresAsync().ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllProfitCentres());
        }

        #endregion

        #region GetSettings

        [Fact]
        public async Task GetSettings_WithExistingProfitCentre_ReturnsOkWithMappedSettings()
        {
            // Arrange
            const string profitCentre = "PC001";
            var dto = new ProfitCentreSettingsDto
            {
                ProfitCentre    = profitCentre,
                Timesheet       = -1,
                Outputsheet     = 0,
                TimesheetLayout = 1
            };
            var mapped = new ProfitCentreSettingsRes
            {
                ProfitCentre    = profitCentre,
                Timesheet       = -1,
                Outputsheet     = 0,
                TimesheetLayout = 1
            };

            _serviceMock.GetProfitCentreSettingsAsync(profitCentre).Returns(dto);
            _mapperMock.Map<ProfitCentreSettingsRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetSettings(profitCentre);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetProfitCentreSettingsAsync(profitCentre);
        }

        [Fact]
        public async Task GetSettings_WithNonExistentProfitCentre_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.GetProfitCentreSettingsAsync("PC_MISSING").Returns((ProfitCentreSettingsDto?)null);

            // Act
            var result = await _controller.GetSettings("PC_MISSING");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetSettings_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetProfitCentreSettingsAsync(Arg.Any<string>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetSettings("PC001"));
        }

        #endregion

        #region PatchSettings

        [Fact]
        public async Task PatchSettings_WithValidRequest_ReturnsOkWithTrue()
        {
            // Arrange
            var request = new UpdateProfitCentreSettingsReq
            {
                ProfitCentre    = "PC001",
                Timesheet       = -1,
                Outputsheet     = -1,
                TimesheetLayout = 1
            };

            _serviceMock.UpdateProfitCentreSettingsAsync("PC001", -1, -1, 1).Returns(true);

            // Act
            var result = await _controller.PatchSettings(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).UpdateProfitCentreSettingsAsync("PC001", -1, -1, 1);
        }

        [Fact]
        public async Task PatchSettings_WithEmptyProfitCentre_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateProfitCentreSettingsReq
            {
                ProfitCentre    = "",
                Timesheet       = -1,
                Outputsheet     = 0,
                TimesheetLayout = 1
            };

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
        public async Task PatchSettings_ServiceReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            var request = new UpdateProfitCentreSettingsReq
            {
                ProfitCentre    = "PC001",
                Timesheet       = 0,
                Outputsheet     = 0,
                TimesheetLayout = 2
            };

            _serviceMock.UpdateProfitCentreSettingsAsync("PC001", 0, 0, 2).Returns(false);

            // Act
            var result = await _controller.PatchSettings(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        [Fact]
        public async Task PatchSettings_ServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new UpdateProfitCentreSettingsReq { ProfitCentre = "PC001" };
            _serviceMock.UpdateProfitCentreSettingsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.PatchSettings(request));
        }

        #endregion
    }
}
