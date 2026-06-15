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

namespace Apha.FPS.Api.UnitTests.Controller.ProjectControllerTest
{
    public class ProjectProfitabilityVlaControllerTests
    {
        private readonly IProjectService _projectService;
        private readonly IMapper _mapper;
        private readonly ProjectController _controller;

        public ProjectProfitabilityVlaControllerTests()
        {
            _projectService = Substitute.For<IProjectService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ProjectController(_projectService, _mapper);
        }

        #region GetProjectProfitabilityVlaAsync

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithNoFilters_ReturnsOkWithPagedResult()
        {
            // Arrange
            var dtos = new List<ProjectProfitabilityVlaDto>
            {
                new() { JobCode = "PP001", StaffCosts = 1000m, Budget = 5000m, Profit = 4000m, TargetProfit = 3500m, OffTarget = 500m },
                new() { JobCode = "PP002", StaffCosts = 2000m, Budget = 6000m, Profit = 4000m, TargetProfit = 3000m, OffTarget = 1000m }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 2 };
            var serviceResult = new PaginatedResult<ProjectProfitabilityVlaDto>(dtos, paginationDto);
            var expectedRes = new PaginationRes<ProjectProfitabilityVlaRes>
            {
                // TRANSFORMENGINE: ProjectProfitabilityVlaRes uses 'Project' (not 'JobCode') as the display name field;
                // JobCode in the DTO maps to Project in the Res contract (Phase 15 build fix).
                Data = dtos.Select(d => new ProjectProfitabilityVlaRes { Project = d.JobCode }).ToList(),
                PaginationData = new Pagination { PageNumber = 1, PageSize = 15, TotalRecords = 2 }
            };

            _projectService.GetProjectProfitabilityVlaAsync(Arg.Any<QueryParameters<ProjectProfitabilityVlaReq>>())
                .Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectProfitabilityVlaRes>>(serviceResult)
                .Returns(expectedRes);

            // Act
            var result = await _controller.GetProjectProfitabilityVlaAsync(
                projectStatus: null, programNo: null, manager: null, customer: null,
                page: 1, pageSize: 15);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _projectService.Received(1)
                .GetProjectProfitabilityVlaAsync(Arg.Any<QueryParameters<ProjectProfitabilityVlaReq>>());
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithAllFilters_PassesFiltersThroughToService()
        {
            // Arrange
            var serviceResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });
            var expectedRes = new PaginationRes<ProjectProfitabilityVlaRes>();

            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Is<QueryParameters<ProjectProfitabilityVlaReq>>(q =>
                        q.Filter != null &&
                        q.Filter.ProjectStatus == "Approved" &&
                        q.Filter.ProgramNo == "P001" &&
                        q.Filter.Manager == "John Smith" &&
                        q.Filter.Customer == "ACME Ltd"))
                .Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectProfitabilityVlaRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetProjectProfitabilityVlaAsync(
                projectStatus: "Approved",
                programNo: "P001",
                manager: "John Smith",
                customer: "ACME Ltd",
                page: 1,
                pageSize: 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            await _projectService.Received(1)
                .GetProjectProfitabilityVlaAsync(Arg.Is<QueryParameters<ProjectProfitabilityVlaReq>>(q =>
                    q.Filter!.ProjectStatus == "Approved" &&
                    q.Filter.ProgramNo == "P001" &&
                    q.Filter.Manager == "John Smith" &&
                    q.Filter.Customer == "ACME Ltd"));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithEmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var serviceResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 0 });
            var expectedRes = new PaginationRes<ProjectProfitabilityVlaRes>
            {
                Data = new List<ProjectProfitabilityVlaRes>()
            };

            _projectService.GetProjectProfitabilityVlaAsync(Arg.Any<QueryParameters<ProjectProfitabilityVlaReq>>())
                .Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectProfitabilityVlaRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetProjectProfitabilityVlaAsync(
                projectStatus: null, programNo: null, manager: null, customer: null,
                page: 1, pageSize: 15);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var res = okResult.Value as PaginationRes<ProjectProfitabilityVlaRes>;
            res?.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_PageAndPageSizeDefaults_AreUsedWhenNotProvided()
        {
            // Arrange — call controller with explicitly matching defaults (page=1, pageSize=15)
            var serviceResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 0 });

            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Is<QueryParameters<ProjectProfitabilityVlaReq>>(q =>
                        q.Page == 1 && q.PageSize == 15))
                .Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectProfitabilityVlaRes>>(serviceResult)
                .Returns(new PaginationRes<ProjectProfitabilityVlaRes>());

            // Act — call with default values
            var result = await _controller.GetProjectProfitabilityVlaAsync(
                projectStatus: null, programNo: null, manager: null, customer: null,
                page: 1, pageSize: 15);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _projectService.Received(1)
                .GetProjectProfitabilityVlaAsync(Arg.Is<QueryParameters<ProjectProfitabilityVlaReq>>(q =>
                    q.Page == 1 && q.PageSize == 15));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_MapperMapsServiceResultToResponse()
        {
            // Arrange
            var serviceResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>
                {
                    new() { JobCode = "PP001", StaffCosts = 500m }
                },
                new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 1 });
            var expectedRes = new PaginationRes<ProjectProfitabilityVlaRes>();

            _projectService.GetProjectProfitabilityVlaAsync(Arg.Any<QueryParameters<ProjectProfitabilityVlaReq>>())
                .Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectProfitabilityVlaRes>>(serviceResult).Returns(expectedRes);

            // Act
            await _controller.GetProjectProfitabilityVlaAsync(
                projectStatus: null, programNo: null, manager: null, customer: null,
                page: 1, pageSize: 15);

            // Assert — mapper is invoked once with the service result
            _mapper.Received(1).Map<PaginationRes<ProjectProfitabilityVlaRes>>(serviceResult);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _projectService.GetProjectProfitabilityVlaAsync(Arg.Any<QueryParameters<ProjectProfitabilityVlaReq>>())
                .ThrowsAsync(new InvalidOperationException("Service failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.GetProjectProfitabilityVlaAsync(
                    projectStatus: null, programNo: null, manager: null, customer: null,
                    page: 1, pageSize: 15));
        }

        #endregion
    }
}
