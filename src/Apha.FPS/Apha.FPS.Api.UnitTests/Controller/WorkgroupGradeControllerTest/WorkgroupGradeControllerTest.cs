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

namespace Apha.FPS.Api.UnitTests.Controller.WorkgroupGradeControllerTest
{
    public class WorkgroupGradeControllerTest
    {
        private const string DefaultPcGrade = "G001";
        private const string DefaultWgGrade = "WG01";

        private readonly IWorkGroupGradeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly WorkGroupGradeController _controller;

        public WorkgroupGradeControllerTest()
        {
            _serviceMock = Substitute.For<IWorkGroupGradeService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new WorkGroupGradeController(_serviceMock, _mapperMock);
        }

        #region Constructor

        [Fact]
        public void Constructor_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkGroupGradeController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkGroupGradeController(_serviceMock, null!));
        }

        #endregion

        #region GetWorkGroupGradeAsync

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var grades = new List<WorkgroupGradeDto>
            {
                new() { WgGrade = DefaultWgGrade, ProfitCentreGrade = DefaultPcGrade }
            };
            var paginationDto = new PaginationDto 
            { 
                PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 
            };
            var serviceResult = new PaginatedResult<WorkgroupGradeDto>(grades, paginationDto);
            var expectedRes = new PaginationRes<WorkgroupGradeRes>
            {
                Data = new List<WorkgroupGradeRes> { new() { WgGrade = DefaultWgGrade } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetWorkGroupGradeAsync(mapped, DefaultPcGrade).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkgroupGradeRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetWorkGroupGradeAsync(query, DefaultPcGrade);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedRes, okResult.Value);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetWorkGroupGradeAsync(mapped, DefaultPcGrade).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetWorkGroupGradeAsync(query, DefaultPcGrade));
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_EmptyResult_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkgroupGradeDto>(
                new List<WorkgroupGradeDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 }
            );
            var expectedRes = new PaginationRes<WorkgroupGradeRes>
            {
                Data = new List<WorkgroupGradeRes>(),
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetWorkGroupGradeAsync(mapped, DefaultPcGrade).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkgroupGradeRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetWorkGroupGradeAsync(query, DefaultPcGrade);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = ((PaginationRes<WorkgroupGradeRes>)okResult.Value!).Data;
            returnedData.Should().BeEmpty();
        }

        #endregion

        #region DeleteWorkGroupGradeAsync

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WithValidWgGrade_ReturnsOk()
        {
            // Arrange
            _serviceMock.DeleteWorkGroupGradeAsync(DefaultWgGrade).Returns(true);

            // Act
            var result = await _controller.DeleteWorkGroupGradeAsync(DefaultWgGrade);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_NotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.DeleteWorkGroupGradeAsync("INVALID").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteWorkGroupGradeAsync("INVALID"));
        }

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteWorkGroupGradeAsync(DefaultWgGrade).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.DeleteWorkGroupGradeAsync(DefaultWgGrade));
        }

        #endregion
    }
}
