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

namespace Apha.FPS.Api.UnitTests.Controller.ProjectGroupStaffPlanControllerTest
{
    public class ProjectGroupStaffPlanControllerTests
    {
        private readonly IProjectGroupStaffPlanService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectGroupStaffPlanController _controller;

        public ProjectGroupStaffPlanControllerTests()
        {
            _serviceMock = Substitute.For<IProjectGroupStaffPlanService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProjectGroupStaffPlanController(_serviceMock, _mapperMock);
        }

        private static QueryParameters<string> DefaultQuery() => new() { Page = 1, PageSize = 10 };

        private static PaginatedResult<ProjectGroupStaffPlanViewDto> MakeResult(int count) =>
            new(
                Enumerable.Range(1, count)
                    .Select(i => new ProjectGroupStaffPlanViewDto
                    {
                        ProjectGroup  = $"GROUP_{i}",
                        Manager       = $"Manager {i}",
                        ResourceCentre = $"RC{i}",
                        WorkGroup     = $"WG{i}",
                        GradeCode     = $"G{i}",
                        Name          = $"Staff {i}",
                        JobCode       = $"JC{i}",
                        ProjectStatus = "Active",
                        Hrs           = i * 10.0,
                        ChargeRate    = i * 100m,
                        Fee           = i * 50m
                    })
                    .ToList(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = count });

        #region GetPaged

        [Fact]
        public async Task GetPaged_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = DefaultQuery();
            var serviceResult = MakeResult(2);
            var mappedResult = new PaginationRes<ProjectGroupStaffPlanViewRes>
            {
                Data = new List<ProjectGroupStaffPlanViewRes>
                {
                    new() { ProjectGroup = "GROUP_1", Manager = "Manager 1" },
                    new() { ProjectGroup = "GROUP_2", Manager = "Manager 2" }
                }
            };

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectGroupStaffPlanViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetPaged_EmptyData_ReturnsOkWithEmptyList()
        {
            // Arrange
            var query = DefaultQuery();
            var serviceResult = MakeResult(0);
            var mappedResult = new PaginationRes<ProjectGroupStaffPlanViewRes>
            {
                Data = new List<ProjectGroupStaffPlanViewRes>()
            };

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectGroupStaffPlanViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaginationRes<ProjectGroupStaffPlanViewRes>>(okResult.Value);
            Assert.Empty(value.Data);
        }

        [Fact]
        public async Task GetPaged_WithFilter_ReturnsFilteredResults()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = "GROUP_1" };
            var serviceResult = MakeResult(1);
            var mappedResult = new PaginationRes<ProjectGroupStaffPlanViewRes>
            {
                Data = new List<ProjectGroupStaffPlanViewRes>
                {
                    new() { ProjectGroup = "GROUP_1" }
                }
            };

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectGroupStaffPlanViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetPaged_WithMultiplePages_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var serviceResult = new PaginatedResult<ProjectGroupStaffPlanViewDto>(
                new List<ProjectGroupStaffPlanViewDto>
                {
                    new() { ProjectGroup = "GROUP_6" },
                    new() { ProjectGroup = "GROUP_7" }
                },
                new PaginationDto { PageNumber = 2, PageSize = 5, TotalPages = 3, TotalRecords = 12 });

            var mappedResult = new PaginationRes<ProjectGroupStaffPlanViewRes>();

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectGroupStaffPlanViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetPaged_ServiceCalledOnce()
        {
            // Arrange
            var query = DefaultQuery();
            var serviceResult = MakeResult(1);
            var mappedResult = new PaginationRes<ProjectGroupStaffPlanViewRes>();

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectGroupStaffPlanViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            await _controller.GetPaged(query);

            // Assert
            await _serviceMock.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPaged_MapperCalledOnce()
        {
            // Arrange
            var query = DefaultQuery();
            var serviceResult = MakeResult(1);
            var mappedResult = new PaginationRes<ProjectGroupStaffPlanViewRes>();

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectGroupStaffPlanViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            await _controller.GetPaged(query);

            // Assert
            _mapperMock.Received(1).Map<PaginationRes<ProjectGroupStaffPlanViewRes>>(serviceResult);
        }

        [Fact]
        public async Task GetPaged_Error_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = DefaultQuery();
            _serviceMock.GetPagedAsync(query).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPaged(query));
        }

        [Fact]
        public async Task GetPaged_Error_MapperThrows_PropagatesException()
        {
            // Arrange
            var query = DefaultQuery();
            var serviceResult = MakeResult(1);

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectGroupStaffPlanViewRes>>(serviceResult)
                .Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPaged(query));
        }

        [Fact]
        public async Task GetPaged_PaginationData_IsPreserved()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectGroupStaffPlanViewDto>(
                new List<ProjectGroupStaffPlanViewDto> { new() { ProjectGroup = "GROUP_1" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 });

            var mappedResult = new PaginationRes<ProjectGroupStaffPlanViewRes>
            {
                Data = new List<ProjectGroupStaffPlanViewRes> { new() { ProjectGroup = "GROUP_1" } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 }
            };

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectGroupStaffPlanViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaginationRes<ProjectGroupStaffPlanViewRes>>(okResult.Value);
            Assert.Equal(1, value.PaginationData.PageNumber);
            Assert.Equal(10, value.PaginationData.PageSize);
            Assert.Equal(1, value.PaginationData.TotalRecords);
        }

        #endregion
    }
}
