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

namespace Apha.FPS.Api.UnitTests.Controller.ResourceCentreGradeControllerTest
{
    public class ResourceCentreGradeControllerTests
    {
        private const string DefaultProfitCentre = "PC01";

        private readonly IResourceCentreGradeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ResourceCentreGradeController _controller;

        public ResourceCentreGradeControllerTests()
        {
            _serviceMock = Substitute.For<IResourceCentreGradeService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new ResourceCentreGradeController(_serviceMock, _mapperMock);
        }

        #region GetResourceCentreGradesAsync Tests

        [Fact]
        public async Task GetResourceCentreGradesAsync_WithValidRequest_ReturnsOk()
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
            _serviceMock.GetResourceCentreGradesAsync(mapped, DefaultProfitCentre).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProfitCentreGradeRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetResourceCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetResourceCentreGradesAsync(mapped, DefaultProfitCentre);
        }

        [Fact]
        public async Task GetResourceCentreGradesAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetResourceCentreGradesAsync(mapped, DefaultProfitCentre)
                .ThrowsAsync(new ArgumentException("Invalid profit centre"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.GetResourceCentreGradesAsync(query, DefaultProfitCentre));
        }

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ResourceCentreGradeController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ResourceCentreGradeController(_serviceMock, null!));
        }

        #endregion
    }
}
