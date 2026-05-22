using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.MonthlyOutputControllerTest
{
    public class MonthlyOutputControllerTests
    {
        private readonly IMonthlyOutputService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly MonthlyOutputController _controller;

        public MonthlyOutputControllerTests()
        {
            _serviceMock = Substitute.For<IMonthlyOutputService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new MonthlyOutputController(_serviceMock, _mapperMock);
        }

        #region SearchAsync

        [Fact]
        public async Task SearchAsync_HappyPath_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyOutputLogDto>
            {
                Data = new List<MonthlyOutputLogDto> { new() { TestCode = "TC1", Buyer = "BuyerA" } },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10 }
            };
            var mappedResult = new PaginationRes<MonthlyOutputLogRes>
            {
                Data = new List<MonthlyOutputLogRes> { new() { TestCode = "TC1", Buyer = "BuyerA" } }
            };

            _serviceMock.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null)
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.SearchAsync(query, null, null, null, null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task SearchAsync_WithAllFilters_PassesFiltersToService()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var dateImported = new DateTime(2024, 1, 15);
            var paginatedResult = new PaginatedResult<MonthlyOutputLogDto> { Data = new List<MonthlyOutputLogDto>() };
            var mappedResult = new PaginationRes<MonthlyOutputLogRes>();

            _serviceMock.GetMonthlyOutputLogAsync(query, "WG1", "TC1", "BuyerA", dateImported, 1.0, "user1", "I")
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.SearchAsync(query, "WG1", "TC1", "BuyerA", dateImported, 1.0, "user1", "I");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetMonthlyOutputLogAsync(query, "WG1", "TC1", "BuyerA", dateImported, 1.0, "user1", "I");
        }

        [Fact]
        public async Task SearchAsync_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyOutputLogDto>
            {
                Data = new List<MonthlyOutputLogDto>(),
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10 }
            };
            var mappedResult = new PaginationRes<MonthlyOutputLogRes>
            {
                Data = new List<MonthlyOutputLogRes>()
            };

            _serviceMock.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null)
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.SearchAsync(query, null, null, null, null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<MonthlyOutputLogRes>>(okResult.Value);
            Assert.Empty(returnValue.Data);
        }

        [Fact]
        public async Task SearchAsync_MapsServiceResultToResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyOutputLogDto>();
            var mappedResult = new PaginationRes<MonthlyOutputLogRes>();

            _serviceMock.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null)
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            await _controller.SearchAsync(query, null, null, null, null, null, null, null);

            // Assert
            _mapperMock.Received(1).Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult);
        }

        [Fact]
        public async Task SearchAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();

            _serviceMock.GetMonthlyOutputLogAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                    Arg.Any<DateTime?>(), Arg.Any<double?>(), Arg.Any<string?>(), Arg.Any<string?>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.SearchAsync(query, null, null, null, null, null, null, null));
        }

        #endregion
    }
}
