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
    public class ProjectProfitabilityControllerTests
    {
        private readonly IProjectService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectController _controller;

        public ProjectProfitabilityControllerTests()
        {
            _serviceMock = Substitute.For<IProjectService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProjectController(_serviceMock, _mapperMock);
        }

        #region GetProjectProfitabilityAsync

        [Fact]
        public async Task GetProjectProfitabilityAsync_HappyPath_ReturnsOkWithMappedData()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
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
            _serviceMock.GetProjectProfitabilityAsync(mappedQuery, programNo, workTypeFilter)
                .Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectProfitabilityRes>>(serviceResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectProfitabilityAsync(query, programNo, workTypeFilter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_WhenProgramNoIsNull_ReturnsBadRequest()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.GetProjectProfitabilityAsync(query, null!, "all");

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("programNo is required.", badRequest.Value);
            await _serviceMock.DidNotReceive().GetProjectProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetProjectProfitabilityAsync_WhenProgramNoIsWhitespace_ReturnsBadRequest(string programNo)
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.GetProjectProfitabilityAsync(query, programNo, "all");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _serviceMock.DidNotReceive().GetProjectProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("approved")]
        [InlineData("not-approved")]
        [InlineData("all")]
        public async Task GetProjectProfitabilityAsync_WithDifferentWorkTypeFilters_CallsServiceWithCorrectFilter(string workTypeFilter)
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var mappedQuery = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(),
                new PaginationDto());
            var mappedResult = new PaginationRes<ProjectProfitabilityRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetProjectProfitabilityAsync(mappedQuery, programNo, workTypeFilter)
                .Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectProfitabilityRes>>(serviceResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectProfitabilityAsync(query, programNo, workTypeFilter);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetProjectProfitabilityAsync(mappedQuery, programNo, workTypeFilter);
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_WithEmptyResults_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var mappedQuery = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });
            var mappedResult = new PaginationRes<ProjectProfitabilityRes> { Data = new List<ProjectProfitabilityRes>() };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetProjectProfitabilityAsync(mappedQuery, programNo, "all").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectProfitabilityRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectProfitabilityAsync(query, programNo, "all");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaginationRes<ProjectProfitabilityRes>>(okResult.Value);
            Assert.Empty(value.Data!);
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var mappedQuery = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetProjectProfitabilityAsync(mappedQuery, programNo, "all")
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _controller.GetProjectProfitabilityAsync(query, programNo, "all"));
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_DefaultWorkTypeFilter_IsAll()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var mappedQuery = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(), new PaginationDto());
            var mappedResult = new PaginationRes<ProjectProfitabilityRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetProjectProfitabilityAsync(mappedQuery, programNo, "all").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectProfitabilityRes>>(serviceResult).Returns(mappedResult);

            // Act — omit workTypeFilter to use default
            var result = await _controller.GetProjectProfitabilityAsync(query, programNo);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetProjectProfitabilityAsync(mappedQuery, programNo, "all");
        }

        #endregion
    }
}
