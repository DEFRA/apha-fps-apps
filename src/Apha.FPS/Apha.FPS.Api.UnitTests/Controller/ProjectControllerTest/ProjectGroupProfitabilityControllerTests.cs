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

namespace Apha.FPS.Api.UnitTests.Controller.ProjectControllerTest
{
    public class ProjectGroupProfitabilityControllerTests
    {
        private readonly IProjectService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectController _controller;

        public ProjectGroupProfitabilityControllerTests()
        {
            _serviceMock = Substitute.For<IProjectService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProjectController(_serviceMock, _mapperMock);
        }

        // ── GetProjectGroupProfitabilityAsync ─────────────────────────────────

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_HappyPath_ReturnsOkWithMappedData()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var workTypeFilter = "all";

            var dtos = new List<ProjectProfitabilityDto>
            {
                new() { JobCode = "PP001", JcTotalStaffCosts = 1000m, BudgetCvl = 5000m, JcProfit = 4000m, TargetProfit = 3500m, OffTarget = 500m },
                new() { JobCode = "PP002", JcTotalStaffCosts = 2000m, BudgetCvl = 6000m, JcProfit = 4000m, TargetProfit = 3000m, OffTarget = 1000m }
            };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 };
            var serviceResult = new PaginatedResult<ProjectProfitabilityDto>(dtos, pagination);

            var mappedResult = new PaginationRes<ProjectProfitabilityRes>
            {
                Data = new List<ProjectProfitabilityRes>
                {
                    new() { JobCode = "PP001" },
                    new() { JobCode = "PP002" }
                }
            };

            var mappedQuery = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetProjectGroupProfitabilityAsync(mappedQuery, projectGroup, workTypeFilter)
                .Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectProfitabilityRes>>(serviceResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WhenProjectGroupIsNull_ThrowsArgumentException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetProjectGroupProfitabilityAsync(query, null!, "all"));
            await _serviceMock.DidNotReceive().GetProjectGroupProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetProjectGroupProfitabilityAsync_WhenProjectGroupIsWhitespace_ThrowsArgumentException(string projectGroup)
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetProjectGroupProfitabilityAsync(query, projectGroup, "all"));
            await _serviceMock.DidNotReceive().GetProjectGroupProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("approved")]
        [InlineData("not-approved")]
        [InlineData("all")]
        public async Task GetProjectGroupProfitabilityAsync_WithDifferentWorkTypeFilters_CallsServiceWithCorrectFilter(string workTypeFilter)
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var mappedQuery = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(), new PaginationDto());
            var mappedResult = new PaginationRes<ProjectProfitabilityRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetProjectGroupProfitabilityAsync(mappedQuery, projectGroup, workTypeFilter)
                .Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectProfitabilityRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetProjectGroupProfitabilityAsync(mappedQuery, projectGroup, workTypeFilter);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WithEmptyResults_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var mappedQuery = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });
            var mappedResult = new PaginationRes<ProjectProfitabilityRes> { Data = new List<ProjectProfitabilityRes>() };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetProjectGroupProfitabilityAsync(mappedQuery, projectGroup, "all").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectProfitabilityRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectGroupProfitabilityAsync(query, projectGroup, "all");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaginationRes<ProjectProfitabilityRes>>(okResult.Value);
            Assert.Empty(value.Data!);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var mappedQuery = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetProjectGroupProfitabilityAsync(mappedQuery, projectGroup, "all")
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _controller.GetProjectGroupProfitabilityAsync(query, projectGroup, "all"));
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_DefaultWorkTypeFilter_IsAll()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var mappedQuery = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(), new PaginationDto());
            var mappedResult = new PaginationRes<ProjectProfitabilityRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetProjectGroupProfitabilityAsync(mappedQuery, projectGroup, "all").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectProfitabilityRes>>(serviceResult).Returns(mappedResult);

            // Act — omit workTypeFilter to use default
            var result = await _controller.GetProjectGroupProfitabilityAsync(query, projectGroup);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetProjectGroupProfitabilityAsync(mappedQuery, projectGroup, "all");
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_DoesNotCallGetProjectProfitabilityAsync()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var mappedQuery = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(), new PaginationDto());
            var mappedResult = new PaginationRes<ProjectProfitabilityRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetProjectGroupProfitabilityAsync(mappedQuery, projectGroup, "all").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectProfitabilityRes>>(serviceResult).Returns(mappedResult);

            // Act
            await _controller.GetProjectGroupProfitabilityAsync(query, projectGroup, "all");

            // Assert — group path must not invoke programme profitability
            await _serviceMock.DidNotReceive().GetProjectProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
