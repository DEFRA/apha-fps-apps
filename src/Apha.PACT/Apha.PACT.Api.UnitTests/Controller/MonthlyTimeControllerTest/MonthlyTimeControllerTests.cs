using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.MonthlyTimeControllerTest
{
    public class MonthlyTimeControllerTests
    {
        private readonly IMonthlyTimeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ICurrentUserContext _currentUserContextMock;
        private readonly MonthlyTimeController _controller;

        public MonthlyTimeControllerTests()
        {
            _serviceMock = Substitute.For<IMonthlyTimeService>();
            _mapperMock = Substitute.For<IMapper>();
            _currentUserContextMock = Substitute.For<ICurrentUserContext>();
            _currentUserContextMock.UserId.Returns("test-user-id");
            _controller = new MonthlyTimeController(_serviceMock, _mapperMock, _currentUserContextMock);
        }

        #region SearchAsync

        [Fact]
        public async Task SearchAsync_HappyPath_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyTimeLogDto>
            {
                Data = new List<MonthlyTimeLogDto> { new() { TimeCode = "TC1", PactStaffId = "S001" } },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10 }
            };
            var mappedResult = new PaginationRes<MonthlyTimeLogRes>
            {
                Data = new List<MonthlyTimeLogRes> { new() { TimeCode = "TC1", PactStaffId = "S001" } }
            };

            _serviceMock.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyTimeLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.SearchAsync(query, null, null, null, null, null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>());
        }

        [Fact]
        public async Task SearchAsync_WithAllFilters_PassesFiltersToService()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var dateImported = new DateTime(2024, 6, 1);
            var paginatedResult = new PaginatedResult<MonthlyTimeLogDto> { Data = new List<MonthlyTimeLogDto>() };
            var mappedResult = new PaginationRes<MonthlyTimeLogRes>();

            _serviceMock.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyTimeLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.SearchAsync(query, "WG1", "TC1", "S001", "PP1", dateImported, 6.0, "USER1", "I");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).SearchAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<MonthlyTimeLogFilterDto>(f =>
                    f.WorkGroup == "WG1" && f.TimeCode == "TC1" && f.PactStaffId == "S001" &&
                    f.ParentProject == "PP1" && f.DateImported == dateImported &&
                    f.Month == 6.0 && f.UserId == "USER1" && f.InsertDelete == "I"));
        }

        [Fact]
        public async Task SearchAsync_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyTimeLogDto>
            {
                Data = new List<MonthlyTimeLogDto>(),
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10 }
            };
            var mappedResult = new PaginationRes<MonthlyTimeLogRes>
            {
                Data = new List<MonthlyTimeLogRes>()
            };

            _serviceMock.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyTimeLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.SearchAsync(query, null, null, null, null, null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<MonthlyTimeLogRes>>(okResult.Value);
            Assert.Empty(returnValue.Data);
        }

        [Fact]
        public async Task SearchAsync_MapsServiceResultToResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyTimeLogDto>();
            var mappedResult = new PaginationRes<MonthlyTimeLogRes>();

            _serviceMock.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyTimeLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            await _controller.SearchAsync(query, null, null, null, null, null, null, null, null);

            // Assert
            _mapperMock.Received(1).Map<PaginationRes<MonthlyTimeLogRes>>(paginatedResult);
        }

        [Fact]
        public async Task SearchAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();

            _serviceMock.SearchAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<MonthlyTimeLogFilterDto>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.SearchAsync(query, null, null, null, null, null, null, null, null));
        }

        #endregion
    }
}
