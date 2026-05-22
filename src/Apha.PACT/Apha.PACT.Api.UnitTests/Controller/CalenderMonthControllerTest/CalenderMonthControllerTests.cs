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

        #region GetCalenderMonthsAsync

        [Fact]
        public async Task GetCalenderMonthsAsync_WithData_ReturnsOkWithMappedResult()
        {
            // Arrange
            var dtos = new List<CalenderMonthDto>
            {
                new() { MonthNumber = 1, MonthName = "January", AccntsPeriod = 1 },
                new() { MonthNumber = 2, MonthName = "February", AccntsPeriod = 2 }
            };
            var mapped = new List<CalenderMonthRes>
            {
                new() { MonthNumber = 1, MonthName = "January", AccntsPeriod = 1 },
                new() { MonthNumber = 2, MonthName = "February", AccntsPeriod = 2 }
            };

            _serviceMock.GetCalenderMonthsAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<CalenderMonthRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetCalenderMonthsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CalenderMonthRes>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
            await _serviceMock.Received(1).GetCalenderMonthsAsync();
            _mapperMock.Received(1).Map<IEnumerable<CalenderMonthRes>>(dtos);
        }

        [Fact]
        public async Task GetCalenderMonthsAsync_EmptyResult_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            var dtos = new List<CalenderMonthDto>();
            var mapped = new List<CalenderMonthRes>();

            _serviceMock.GetCalenderMonthsAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<CalenderMonthRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetCalenderMonthsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CalenderMonthRes>>(okResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task GetCalenderMonthsAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetCalenderMonthsAsync().ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetCalenderMonthsAsync());
        }

        #endregion
    }
}
