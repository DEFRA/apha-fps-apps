using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.FpsSettingControllerTest
{
    public class FpsSettingControllerTest
    {
        private readonly IFpsSettingService _fpsSettingService;
        private readonly IMapper _mapper;
        private readonly FpsSettingController _sut;

        public FpsSettingControllerTest()
        {
            _fpsSettingService = Substitute.For<IFpsSettingService>();
            _mapper = Substitute.For<IMapper>();
            _sut = new FpsSettingController(_fpsSettingService, _mapper);
        }

        [Fact]
        public async Task GetAsync_WhenSettingsExist_ReturnsOkWithMappedSettings()
        {
            // Arrange
            var serviceResult = new List<FpsSettingDto>
            {
                new FpsSettingDto { Id = "1", Setting = "Setting1", Notes = "Value1" },
                new FpsSettingDto { Id = "2", Setting = "Setting2", Notes = "Value2" }
            };

            var expectedMappedResult = new List<FpsSettingRes>
            {
                new FpsSettingRes { },
                new FpsSettingRes { }
            };

            _fpsSettingService.GetAllSettingsAsync()
                .Returns(Task.FromResult(serviceResult));

            _mapper.Map<List<FpsSettingRes>>(serviceResult)
                .Returns(expectedMappedResult);

            // Act
            var result = await _sut.GetAsync();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.StatusCode.Should().Be(200);
            okResult?.Value.Should().BeEquivalentTo(expectedMappedResult);

            await _fpsSettingService.Received(1).GetAllSettingsAsync();
            _mapper.Received(1).Map<List<FpsSettingRes>>(serviceResult);
        }

        [Fact]
        public async Task GetAsync_WhenNoSettingsExist_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyServiceResult = new List<FpsSettingDto>();
            var emptyMappedResult = new List<FpsSettingRes>();

            _fpsSettingService.GetAllSettingsAsync()
                .Returns(Task.FromResult(emptyServiceResult));

            _mapper.Map<List<FpsSettingRes>>(emptyServiceResult)
                .Returns(emptyMappedResult);

            // Act
            var result = await _sut.GetAsync();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.StatusCode.Should().Be(200);
            okResult?.Value.Should().BeEquivalentTo(emptyMappedResult);
            (okResult?.Value as List<FpsSettingRes>).Should().BeEmpty();

            await _fpsSettingService.Received(1).GetAllSettingsAsync();
            _mapper.Received(1).Map<List<FpsSettingRes>>(emptyServiceResult);
        }
               

        [Fact]
        public async Task GetAsync_WhenServiceThrowsException_ThrowsException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _fpsSettingService.GetAllSettingsAsync()
            .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAsync());

            exception.Message.Should().Be("Database connection failed");
            await _fpsSettingService.Received(1).GetAllSettingsAsync();
            _mapper.DidNotReceive().Map<List<FpsSettingRes>>(Arg.Any<object>());
        }

        #region GetHoursPerDayAsync

        [Fact]
        public async Task GetHoursPerDayAsync_WhenServiceReturnsValue_ReturnsOkWithDecimal()
        {
            // Arrange
            _fpsSettingService.GetHoursPerDayAsync().Returns(Task.FromResult(7.5m));

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(7.5m);
            await _fpsSettingService.Received(1).GetHoursPerDayAsync();
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenServiceReturnsDefaultValue_ReturnsOkWithEight()
        {
            // Arrange
            _fpsSettingService.GetHoursPerDayAsync().Returns(Task.FromResult(8m));

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(8m);
            await _fpsSettingService.Received(1).GetHoursPerDayAsync();
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _fpsSettingService.GetHoursPerDayAsync().Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetHoursPerDayAsync());
            exception.Message.Should().Be("Database connection failed");
            await _fpsSettingService.Received(1).GetHoursPerDayAsync();
        }

        #endregion
    }
}
