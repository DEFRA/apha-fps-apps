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

namespace Apha.FPS.Api.UnitTests.Controller.ProjectStaffPlanControllerTest
{
    public class ProjectStaffPlanControllerTests
    {
        private readonly IProjectStaffPlanService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectStaffPlanController _controller;

        public ProjectStaffPlanControllerTests()
        {
            _serviceMock = Substitute.For<IProjectStaffPlanService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProjectStaffPlanController(_serviceMock, _mapperMock);
        }

        private static QueryParameters<string> DefaultQuery() => new() { Page = 1, PageSize = 10 };

        private static PaginatedResult<ProjectStaffPlanViewDto> MakeResult(int count) =>
            new(
                Enumerable.Range(1, count)
                    .Select(i => new ProjectStaffPlanViewDto
                    {
                        ParentProject = $"P{i:D3}",
                        Name = $"Staff {i}",
                        StaffId = $"S{i:D3}"
                    })
                    .ToList(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = count });

        #region GetPaged Tests

        [Fact]
        public async Task GetPaged_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = DefaultQuery();
            var serviceResult = MakeResult(2);
            var mappedResult = new PaginationRes<ProjectStaffPlanViewRes>
            {
                Data = new List<ProjectStaffPlanViewRes>
                {
                    new() { ParentProject = "P001", Name = "Staff 1", StaffId = "S001" },
                    new() { ParentProject = "P002", Name = "Staff 2", StaffId = "S002" }
                }
            };

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectStaffPlanViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            await _serviceMock.Received(1).GetPagedAsync(query);
            _mapperMock.Received(1).Map<PaginationRes<ProjectStaffPlanViewRes>>(serviceResult);
        }

        [Fact]
        public async Task GetPaged_EmptyData_ReturnsOkWithEmptyList()
        {
            // Arrange
            var query = DefaultQuery();
            var serviceResult = MakeResult(0);
            var mappedResult = new PaginationRes<ProjectStaffPlanViewRes> { Data = new List<ProjectStaffPlanViewRes>() };

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectStaffPlanViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaginationRes<ProjectStaffPlanViewRes>>(okResult.Value);
            Assert.Empty(value.Data);
        }

        [Fact]
        public async Task GetPaged_WithFilter_ReturnsFilteredResults()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = "P001" };
            var serviceResult = MakeResult(1);
            var mappedResult = new PaginationRes<ProjectStaffPlanViewRes>
            {
                Data = new List<ProjectStaffPlanViewRes>
                {
                    new() { ParentProject = "P001" }
                }
            };

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectStaffPlanViewRes>>(serviceResult).Returns(mappedResult);

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
            var serviceResult = new PaginatedResult<ProjectStaffPlanViewDto>(
                new List<ProjectStaffPlanViewDto>
                {
                    new() { ParentProject = "P006" },
                    new() { ParentProject = "P007" }
                },
                new PaginationDto { PageNumber = 2, PageSize = 5, TotalPages = 3, TotalRecords = 12 });

            var mappedResult = new PaginationRes<ProjectStaffPlanViewRes>();

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectStaffPlanViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            Assert.IsType<OkObjectResult>(result);
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
            _mapperMock.Map<PaginationRes<ProjectStaffPlanViewRes>>(serviceResult)
                .Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPaged(query));
        }

        [Fact]
        public async Task GetPaged_ServiceCalledOnce()
        {
            // Arrange
            var query = DefaultQuery();
            var serviceResult = MakeResult(1);
            var mappedResult = new PaginationRes<ProjectStaffPlanViewRes>();

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectStaffPlanViewRes>>(serviceResult).Returns(mappedResult);

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
            var mappedResult = new PaginationRes<ProjectStaffPlanViewRes>();

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectStaffPlanViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            await _controller.GetPaged(query);

            // Assert
            _mapperMock.Received(1).Map<PaginationRes<ProjectStaffPlanViewRes>>(serviceResult);
        }

        #endregion
    }
}
