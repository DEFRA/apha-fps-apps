using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.CalenderMonthControllerTest
{
    public class CalenderMonthControllerTests
    {
        private readonly ICalenderMonthService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly CalenderMonthController _controller;

        public CalenderMonthControllerTests()
        {
            _serviceMock = Substitute.For<ICalenderMonthService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new CalenderMonthController(_serviceMock, _mapperMock);
        }

        #region GetAll

        [Fact]
        public async Task GetAll_HappyPath_ReturnsOkWithMappedMonths()
        {
            // Arrange
            var dtos = new List<CalenderMonthDto>
            {
                new() { MonthNumber = 1, MonthName = "January",  AccntsPeriod = 1 },
                new() { MonthNumber = 2, MonthName = "February", AccntsPeriod = 2 }
            };
            var mapped = new List<CalenderMonthRes>
            {
                new() { MonthNumber = 1, MonthName = "January",  AccntsPeriod = 1 },
                new() { MonthNumber = 2, MonthName = "February", AccntsPeriod = 2 }
            };

            _serviceMock.GetAllCalenderMonthsAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<CalenderMonthRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetAllCalenderMonthsAsync();
        }

        [Fact]
        public async Task GetAll_EmptyList_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            var dtos = new List<CalenderMonthDto>();
            var mapped = new List<CalenderMonthRes>();

            _serviceMock.GetAllCalenderMonthsAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<CalenderMonthRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value as IEnumerable<CalenderMonthRes>;
            Assert.NotNull(value);
            Assert.Empty(value);
        }

        [Fact]
        public async Task GetAll_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAllCalenderMonthsAsync().ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAll());
        }

        #endregion
    }
}
