using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Api.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.PACT.Api.UnitTests.Controller.MonthControllerTest
{
    public class MonthControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IMonthService _service;
        private readonly MonthController _controller;

        public MonthControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _service = Substitute.For<IMonthService>();
            _controller = new MonthController(_service, _mapper);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResultWithMonths()
        {
            // Arrange
            var months = new List<MonthDto>
            {
                new() { MonthNumber = 1, MonthName = "January" },
                new() { MonthNumber = 2, MonthName = "February" }
            };
            var monthsRes = new List<MonthRes>
            {
                new() { MonthNumber = 1, MonthName = "January" },
                new() { MonthNumber = 2, MonthName = "February" }
            };

            _service.GetAllMonthsAsync()
                .Returns(months);
            _mapper.Map<IEnumerable<MonthRes>>(months)
                .Returns(monthsRes);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult.Value as IEnumerable<MonthRes>;
            Assert.NotNull(returnValue);
            Assert.Equal(2, returnValue.Count());
            await _service.Received(1).GetAllMonthsAsync();
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList_WhenNoMonthsExist()
        {
            // Arrange
            var emptyList = new List<MonthDto>();
            _service.GetAllMonthsAsync()
                .Returns(emptyList);
            _mapper.Map<IEnumerable<MonthRes>>(emptyList)
                .Returns(new List<MonthRes>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult.Value as IEnumerable<MonthRes>;
            Assert.NotNull(returnValue);
            Assert.Empty(returnValue);
        }
    }
}
