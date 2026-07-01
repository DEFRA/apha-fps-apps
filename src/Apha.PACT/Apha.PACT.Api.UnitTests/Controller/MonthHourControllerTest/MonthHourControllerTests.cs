using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.PACT.Api.UnitTests.Controller.MonthHourControllerTest
{
    public class MonthHourControllerTests
    {
        private readonly IMonthHourService _service;
        private readonly IMapper _mapper;
        private readonly MonthHourController _controller;

        public MonthHourControllerTests()
        {
            _service = Substitute.For<IMonthHourService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new MonthHourController(_service, _mapper);
        }

        [Fact]
        public async Task GetAll_WithData_ReturnsOkWithMappedPaginationResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var serviceResult = new PaginatedResult<MonthHourDto>
            {
                Data = [new MonthHourDto { Year = 2025, Month = 1, CvlHours = 160 }],
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var mappedResult = new PaginationRes<MonthHourRes>
            {
                Data = [new MonthHourRes { Year = 2025, Month = 1, CvlHours = 160 }],
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _service.GetAllAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<MonthHourRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAll(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _service.Received(1).GetAllAsync(query);
        }

        [Fact]
        public async Task GetAll_WithEmptyData_ReturnsOkWithEmptyPaginationResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var serviceResult = new PaginatedResult<MonthHourDto> { Data = [] };
            var mappedResult = new PaginationRes<MonthHourRes> { Data = [] };

            _service.GetAllAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<MonthHourRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAll(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaginationRes<MonthHourRes>>(okResult.Value);
            Assert.Empty(value.Data);
        }

        [Fact]
        public async Task GetByYear_WithValidYear_ReturnsOkWithMappedItems()
        {
            // Arrange
            const short year = 2025;
            var items = new List<MonthHourDto>
            {
                new() { Year = year, Month = 1, CvlHours = 160 }
            };
            var mapped = new List<MonthHourRes>
            {
                new() { Year = year, Month = 1, CvlHours = 160 }
            };

            _service.GetByYearAsync(year).Returns(items);
            _mapper.Map<IEnumerable<MonthHourRes>>(items).Returns(mapped);

            // Act
            var result = await _controller.GetByYear(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _service.Received(1).GetByYearAsync(year);
        }

        [Fact]
        public async Task GetByYear_WithNoItems_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            const short year = 1900;
            var items = new List<MonthHourDto>();
            var mapped = new List<MonthHourRes>();

            _service.GetByYearAsync(year).Returns(items);
            _mapper.Map<IEnumerable<MonthHourRes>>(items).Returns(mapped);

            // Act
            var result = await _controller.GetByYear(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsAssignableFrom<IEnumerable<MonthHourRes>>(okResult.Value);
            Assert.Empty(value);
        }

        [Fact]
        public async Task GetDistinctYears_WithData_ReturnsOkWithYears()
        {
            // Arrange
            var years = new List<short> { 2023, 2024, 2025 };
            _service.GetDistinctYearsAsync().Returns(years);

            // Act
            var result = await _controller.GetDistinctYears();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(years, okResult.Value);
            await _service.Received(1).GetDistinctYearsAsync();
        }

        [Fact]
        public async Task GetDistinctYears_WithEmptyData_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            _service.GetDistinctYearsAsync().Returns(new List<short>());

            // Act
            var result = await _controller.GetDistinctYears();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsAssignableFrom<IEnumerable<short>>(okResult.Value);
            Assert.Empty(value);
        }
    }
}
