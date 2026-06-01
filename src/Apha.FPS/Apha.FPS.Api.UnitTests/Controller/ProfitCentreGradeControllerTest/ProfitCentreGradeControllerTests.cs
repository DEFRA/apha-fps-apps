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
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.ProfitCentreGradeControllerTest
{
    public class ProfitCentreGradeControllerTests
    {
        private const string DefaultProfitCentre = "PC01";

        private readonly IProfitCentreGradeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProfitCentreGradeController _controller;

        public ProfitCentreGradeControllerTests()
        {
            _serviceMock = Substitute.For<IProfitCentreGradeService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new ProfitCentreGradeController(_serviceMock, _mapperMock);
        }

        #region GetProfitCentreGradesAsync Tests

        [Fact]
        public async Task GetProfitCentreGradesAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var grades = new List<ProfitCentreGradeDto>
            {
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, ChargeRate = 100m }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<ProfitCentreGradeDto>(grades, paginationDto);
            var expectedRes   = new PaginationRes<ProfitCentreGradeRes>
            {
                Data           = new List<ProfitCentreGradeRes> { new() { PcGrade = "G001" } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetProfitCentreGradesAsync(mapped, DefaultProfitCentre).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProfitCentreGradeRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetProfitCentreGradesAsync(mapped, DefaultProfitCentre);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetProfitCentreGradesAsync(mapped, DefaultProfitCentre)
                .ThrowsAsync(new ArgumentException("Invalid profit centre"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.GetProfitCentreGradesAsync(query, DefaultProfitCentre));
        }

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ProfitCentreGradeController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ProfitCentreGradeController(_serviceMock, null!));
        }

        #endregion

        #region GetAllPcGradesAsync Tests

        [Fact]
        public async Task GetAllPcGradesAsync_WithSuccessResponse_ReturnsOk()
        {
            // Arrange
            var grades = new List<string> { "G001", "G002", "G003" };
            _serviceMock.GetAllPcGradesAsync().Returns(grades);

            // Act
            var result = await _controller.GetAllPcGradesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(grades, okResult.Value);
            await _serviceMock.Received(1).GetAllPcGradesAsync();
        }

        [Fact]
        public async Task GetAllPcGradesAsync_WithEmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            _serviceMock.GetAllPcGradesAsync().Returns(new List<string>());

            // Act
            var result = await _controller.GetAllPcGradesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<string>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetAllPcGradesAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAllPcGradesAsync()
                .ThrowsAsync(new InvalidOperationException("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.GetAllPcGradesAsync());
        }

        #endregion
    }
}
