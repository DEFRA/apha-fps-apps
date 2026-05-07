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

namespace Apha.FPS.Api.UnitTests.Controller.WorkGroupGradeControllerTest
{
    public class WorkGroupGradeControllerTests
    {
        private const string DefaultPcGrade = "G001";
        private const string DefaultWgGrade = "WG01";

        private readonly IWorkGroupGradeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly WorkGroupGradeController _controller;

        public WorkGroupGradeControllerTests()
        {
            _serviceMock = Substitute.For<IWorkGroupGradeService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new WorkGroupGradeController(_serviceMock, _mapperMock);
        }

        #region GetWorkGroupGradeAsync Tests

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var grades = new List<WorkgroupGradeDto>
            {
                new() { WgGrade = DefaultWgGrade, ProfitCentreGrade = DefaultPcGrade }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<WorkgroupGradeDto>(grades, paginationDto);
            var expectedRes   = new PaginationRes<WorkgroupGradeRes>
            {
                Data           = new List<WorkgroupGradeRes> { new() { WgGrade = DefaultWgGrade } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetWorkGroupGradeAsync(mapped, DefaultPcGrade).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkgroupGradeRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetWorkGroupGradeAsync(query, DefaultPcGrade);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetWorkGroupGradeAsync(mapped, DefaultPcGrade);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetWorkGroupGradeAsync(mapped, DefaultPcGrade)
                .ThrowsAsync(new ArgumentException("Invalid pc grade"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.GetWorkGroupGradeAsync(query, DefaultPcGrade));
        }

        #endregion

        #region DeleteWorkGroupGradeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WithValidWgGrade_ReturnsNoContent()
        {
            // Arrange
            _serviceMock.DeleteWorkGroupGradeAsync(DefaultWgGrade).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteWorkGroupGradeAsync(DefaultWgGrade);

            // Assert
            Assert.IsType<NoContentResult>(result);
            await _serviceMock.Received(1).DeleteWorkGroupGradeAsync(DefaultWgGrade);
        }

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteWorkGroupGradeAsync(DefaultWgGrade)
                .ThrowsAsync(new KeyNotFoundException("WG grade not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.DeleteWorkGroupGradeAsync(DefaultWgGrade));
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new WorkGroupGradeController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new WorkGroupGradeController(_serviceMock, null!));
        }

        #endregion
    }
}
