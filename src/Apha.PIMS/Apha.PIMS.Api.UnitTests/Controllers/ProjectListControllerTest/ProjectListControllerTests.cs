using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.ProjectListControllerTest
{
    public class ProjectListControllerTests
    {
        private readonly IProjectListService _service;
        private readonly IMapper _mapper;
        private readonly ProjectListController _controller;

        public ProjectListControllerTests()
        {
            _service = Substitute.For<IProjectListService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ProjectListController(_service, _mapper);
        }

        #region GetAllProjectsAsync

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsOkResult_WithMappedPagination()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var filter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginatedResult = new PaginatedResult<ProjectListViewDto>();
            var mappedResult = new PaginationRes<ProjectListRes>();
            var showWhichProjects = 2;

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetAllProjectsAsync(filter, showWhichProjects).Returns(paginatedResult);
            _mapper.Map<PaginationRes<ProjectListRes>>(paginatedResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllProjectsAsync(query, showWhichProjects);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<QueryParameters<string>>(query);
            await _service.Received(1).GetAllProjectsAsync(filter, showWhichProjects);
            _mapper.Received(1).Map<PaginationRes<ProjectListRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithEmptyServiceResult_ReturnsOkWithEmptyPagination()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var filter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var emptyResult = new PaginatedResult<ProjectListViewDto>();
            var emptyMapped = new PaginationRes<ProjectListRes>();
            var showWhichProjects = 2;

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetAllProjectsAsync(filter, showWhichProjects).Returns(emptyResult);
            _mapper.Map<PaginationRes<ProjectListRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetAllProjectsAsync(query, showWhichProjects);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetAllProjectsAsync(filter, showWhichProjects);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1 };
            var filter = new QueryParameters<string> { Page = 1 };
            var showWhichProjects = 2;

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetAllProjectsAsync(filter, showWhichProjects).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllProjectsAsync(query, showWhichProjects));

            _mapper.Received(1).Map<QueryParameters<string>>(query);
            await _service.Received(1).GetAllProjectsAsync(filter, showWhichProjects);
        }

        #endregion

        #region GetAllProjectsForDropDownAsync

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_ReturnsOkResult_WithMappedList()
        {
            // Arrange
            var dtoList = new List<ProjectListViewDto>
            {
                new ProjectListViewDto { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", OnFps = "Yes" },
                new ProjectListViewDto { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2", OnFps = "No" }
            };
            var resList = new List<ProjectListRes>
            {
                new ProjectListRes { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", OnFps = "Yes" },
                new ProjectListRes { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2", OnFps = "No" }
            };

            _service.GetAllProjectsForDropDownAsync().Returns(dtoList);
            _mapper.Map<List<ProjectListRes>>(dtoList).Returns(resList);

            // Act
            var result = await _controller.GetAllProjectsForDropDownAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resList, okResult.Value);

            await _service.Received(1).GetAllProjectsForDropDownAsync();
            _mapper.Received(1).Map<List<ProjectListRes>>(dtoList);
        }

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyDtoList = new List<ProjectListViewDto>();
            var emptyResList = new List<ProjectListRes>();

            _service.GetAllProjectsForDropDownAsync().Returns(emptyDtoList);
            _mapper.Map<List<ProjectListRes>>(emptyDtoList).Returns(emptyResList);

            // Act
            var result = await _controller.GetAllProjectsForDropDownAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<ProjectListRes>>(okResult.Value);
            Assert.Empty(value);

            await _service.Received(1).GetAllProjectsForDropDownAsync();
            _mapper.Received(1).Map<List<ProjectListRes>>(emptyDtoList);
        }

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetAllProjectsForDropDownAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllProjectsForDropDownAsync());

            await _service.Received(1).GetAllProjectsForDropDownAsync();
            _mapper.DidNotReceive().Map<List<ProjectListRes>>(Arg.Any<List<ProjectListViewDto>>());
        }

        #endregion

        #region GetAllProjectsForMilestoneAsync

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_ReturnsOkResult_WithMappedList()
        {
            // Arrange
            var dtoList = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", ProjectGroup = "GRP1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2", ProjectGroup = "GRP2" }
            };
            var resList = new List<ProjectListMilestoneRes>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", ProjectGroup = "GRP1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2", ProjectGroup = "GRP2" }
            };

            _service.GetAllProjectsForMilestoneAsync().Returns(dtoList);
            _mapper.Map<List<ProjectListMilestoneRes>>(dtoList).Returns(resList);

            // Act
            var result = await _controller.GetAllProjectsForMilestoneAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resList, okResult.Value);

            await _service.Received(1).GetAllProjectsForMilestoneAsync();
            _mapper.Received(1).Map<List<ProjectListMilestoneRes>>(dtoList);
        }

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyDtoList = new List<ProjectListMilestoneDto>();
            var emptyResList = new List<ProjectListMilestoneRes>();

            _service.GetAllProjectsForMilestoneAsync().Returns(emptyDtoList);
            _mapper.Map<List<ProjectListMilestoneRes>>(emptyDtoList).Returns(emptyResList);

            // Act
            var result = await _controller.GetAllProjectsForMilestoneAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<ProjectListMilestoneRes>>(okResult.Value);
            Assert.Empty(value);

            await _service.Received(1).GetAllProjectsForMilestoneAsync();
            _mapper.Received(1).Map<List<ProjectListMilestoneRes>>(emptyDtoList);
        }

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetAllProjectsForMilestoneAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllProjectsForMilestoneAsync());

            await _service.Received(1).GetAllProjectsForMilestoneAsync();
            _mapper.DidNotReceive().Map<List<ProjectListMilestoneRes>>(Arg.Any<List<ProjectListMilestoneDto>>());
        }

        #endregion

        #region GetYearlyDetailsByProject

        [Fact]
        public async Task GetYearlyDetailsByProject_ReturnsOkResult_WithMappedList()
        {
            // Arrange
            var parentproject = "PP001";
            var dtoList = new List<ProjectsDto>
            {
                new ProjectsDto { Year = 2023, Parentproject = parentproject, Program = "PROG1", Customer = "CUST1", Manager = "MGR1" },
                new ProjectsDto { Year = 2024, Parentproject = parentproject, Program = "PROG1", Customer = "CUST1", Manager = "MGR1" }
            };
            var resList = new List<ProjectsRes>
            {
                new ProjectsRes { Year = 2023, Parentproject = parentproject, Program = "PROG1" },
                new ProjectsRes { Year = 2024, Parentproject = parentproject, Program = "PROG1" }
            };

            _service.GetYearlyDetailsByProjectAsync(parentproject).Returns(dtoList);
            _mapper.Map<List<ProjectsRes>>(dtoList).Returns(resList);

            // Act
            var result = await _controller.GetYearlyDetailsByProject(parentproject);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resList, okResult.Value);

            await _service.Received(1).GetYearlyDetailsByProjectAsync(parentproject);
            _mapper.Received(1).Map<List<ProjectsRes>>(dtoList);
        }

        [Fact]
        public async Task GetYearlyDetailsByProject_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var parentproject = "PP001";
            var emptyDtoList = new List<ProjectsDto>();
            var emptyResList = new List<ProjectsRes>();

            _service.GetYearlyDetailsByProjectAsync(parentproject).Returns(emptyDtoList);
            _mapper.Map<List<ProjectsRes>>(emptyDtoList).Returns(emptyResList);

            // Act
            var result = await _controller.GetYearlyDetailsByProject(parentproject);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<ProjectsRes>>(okResult.Value);
            Assert.Empty(value);

            await _service.Received(1).GetYearlyDetailsByProjectAsync(parentproject);
            _mapper.Received(1).Map<List<ProjectsRes>>(emptyDtoList);
        }

        [Fact]
        public async Task GetYearlyDetailsByProject_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parentproject = "PP001";
            _service.GetYearlyDetailsByProjectAsync(parentproject).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetYearlyDetailsByProject(parentproject));

            await _service.Received(1).GetYearlyDetailsByProjectAsync(parentproject);
            _mapper.DidNotReceive().Map<List<ProjectsRes>>(Arg.Any<List<ProjectsDto>>());
        }

        #endregion
    }
}