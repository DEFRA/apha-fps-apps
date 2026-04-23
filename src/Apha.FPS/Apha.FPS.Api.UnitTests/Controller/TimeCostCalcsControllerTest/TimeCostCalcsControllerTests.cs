using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.TimeCostCalcsControllerTest
{
    public class TimeCostCalcsControllerTests
    {
        private readonly ITimeCostCalcsService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly TimeCostCalcsController _controller;

        public TimeCostCalcsControllerTests()
        {
            _serviceMock = Substitute.For<ITimeCostCalcsService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new TimeCostCalcsController(_serviceMock, _mapperMock);
        }

        private static QueryParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new QueryParameters<string> { Page = page, PageSize = pageSize };

        private static PaginatedResult<TimeCostCalcsViewDto> MakeResult(int count = 2)
        {
            var items = Enumerable.Range(1, count)
                .Select(i => new TimeCostCalcsViewDto
                {
                    Project   = "AH0033",
                    StaffId   = $"S{i:D2}",
                    Name      = $"Staff{i}",
                    WorkGroup = "WG1",
                    Month     = i,
                    Time      = 8,
                    Cost      = 100
                })
                .ToList();
            return new PaginatedResult<TimeCostCalcsViewDto>(
                items,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = count });
        }

        #region GetTimeCostCalcsByProjectAsync — Happy path

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query       = DefaultQuery();
            var projectCode = "AH0033";
            var serviceResult = MakeResult(2);
            var mappedResult  = new PaginationRes<TimeCostCalcsViewRes>
            {
                Data = new List<TimeCostCalcsViewRes>
                {
                    new() { Project = "AH0033", StaffId = "S01", Name = "Staff1" },
                    new() { Project = "AH0033", StaffId = "S02", Name = "Staff2" }
                }
            };

            _serviceMock.GetTimeCostCalcsByProjectAsync(query, projectCode).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TimeCostCalcsViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetTimeCostCalcsByProjectAsync(query, projectCode);
            _mapperMock.Received(1).Map<PaginationRes<TimeCostCalcsViewRes>>(serviceResult);
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_EmptyData_ReturnsOkWithEmptyList()
        {
            // Arrange
            var query         = DefaultQuery();
            var projectCode   = "AH0033";
            var serviceResult = MakeResult(0);
            var mappedResult  = new PaginationRes<TimeCostCalcsViewRes> { Data = new List<TimeCostCalcsViewRes>() };

            _serviceMock.GetTimeCostCalcsByProjectAsync(query, projectCode).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TimeCostCalcsViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value    = Assert.IsType<PaginationRes<TimeCostCalcsViewRes>>(okResult.Value);
            Assert.Empty(value.Data);
        }

        #endregion

        #region GetTimeCostCalcsByProjectAsync — Validation

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_WhenProjectCodeIsNull_ThrowsArgumentException()
        {
            // Arrange
            var query = DefaultQuery();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetTimeCostCalcsByProjectAsync(query, null!));
            Assert.Equal("projectCode is required.", ex.Message);
            await _serviceMock.DidNotReceive().GetTimeCostCalcsByProjectAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTimeCostCalcsByProjectAsync_WhenProjectCodeIsNullOrWhitespace_ThrowsArgumentException(string projectCode)
        {
            // Arrange
            var query = DefaultQuery();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetTimeCostCalcsByProjectAsync(query, projectCode));
            await _serviceMock.DidNotReceive().GetTimeCostCalcsByProjectAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        #endregion

        #region GetTimeCostCalcsByProjectAsync — Exception propagation

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query       = DefaultQuery();
            var projectCode = "AH0033";
            _serviceMock.GetTimeCostCalcsByProjectAsync(query, projectCode)
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _controller.GetTimeCostCalcsByProjectAsync(query, projectCode));
        }

        #endregion

        #region GetTimeCostCalcsByProjectAsync — Pagination parameters

        [Theory]
        [InlineData(1, 5)]
        [InlineData(2, 10)]
        [InlineData(3, 20)]
        public async Task GetTimeCostCalcsByProjectAsync_WithPaginationParams_PassesThemToService(int page, int pageSize)
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = page, PageSize = pageSize };
            var projectCode = "AH0033";
            var result      = MakeResult(0);
            var mapped      = new PaginationRes<TimeCostCalcsViewRes>();

            _serviceMock.GetTimeCostCalcsByProjectAsync(query, projectCode).Returns(result);
            _mapperMock.Map<PaginationRes<TimeCostCalcsViewRes>>(result).Returns(mapped);

            // Act
            await _controller.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            await _serviceMock.Received(1).GetTimeCostCalcsByProjectAsync(
                Arg.Is<QueryParameters<string>>(q => q.Page == page && q.PageSize == pageSize),
                projectCode);
        }

        #endregion

        #region GetTotalActualByProjectAsync

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithValidProjectCode_ReturnsOk()
        {
            // Arrange
            var projectCode = "AH0033";
            var dto         = new TimeCostCalcsTotalsDto { TotalHours = 40.5, TotalCost = 5000.0 };
            var mapped      = new TimeCostCalcsTotalsRes { TotalHours = 40.5, TotalCost = 5000.0 };

            _serviceMock.GetTotalActualByProjectAsync(projectCode).Returns(dto);
            _mapperMock.Map<TimeCostCalcsTotalsRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetTotalActualByProjectAsync(projectCode);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, ok.Value);
            await _serviceMock.Received(1).GetTotalActualByProjectAsync(projectCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTotalActualByProjectAsync_WithNullOrWhitespaceProjectCode_ThrowsArgumentException(string? projectCode)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetTotalActualByProjectAsync(projectCode!));
            await _serviceMock.DidNotReceive().GetTotalActualByProjectAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var projectCode = "AH0033";
            _serviceMock.GetTotalActualByProjectAsync(projectCode)
                .Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _controller.GetTotalActualByProjectAsync(projectCode));
        }

        #endregion

        #region DeleteTimeCostCalcsAsync

        [Fact]
        public async Task DeleteTimeCostCalcsAsync_WithValidParams_ReturnsOk()
        {
            // Arrange
            _serviceMock.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01")
                .Returns(true);

            // Act
            var result = await _controller.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01");

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteTimeCostCalcsAsync_WhenRecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01")
                .Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _controller.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01"));
        }

        [Theory]
        [InlineData("",    "JOB1", "AH0033", "S01")]
        [InlineData("WG1", "",     "AH0033", "S01")]
        [InlineData("WG1", "JOB1", "",       "S01")]
        [InlineData("WG1", "JOB1", "AH0033", ""   )]
        public async Task DeleteTimeCostCalcsAsync_WithMissingRequiredParam_ThrowsArgumentException(
            string workgroup, string jobCode, string project, string staffId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.DeleteTimeCostCalcsAsync(workgroup, jobCode, project, 1, staffId));
            await _serviceMock.DidNotReceive().DeleteTimeCostCalcsAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<double>(), Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteTimeCostCalcsAsync_PassesAllParamsToService()
        {
            // Arrange
            _serviceMock.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 3.5, "S01")
                .Returns(true);

            // Act
            await _controller.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 3.5, "S01");

            // Assert
            await _serviceMock.Received(1)
                .DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 3.5, "S01");
        }

        #endregion
    }
}
