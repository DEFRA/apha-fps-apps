using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.ProjectMonthControllerTest
{
    public class ProjectMonthControllerTests
    {
        private readonly IProjectMonthService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectMonthController _controller;

        public ProjectMonthControllerTests()
        {
            _serviceMock = Substitute.For<IProjectMonthService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProjectMonthController(_serviceMock, _mapperMock);
        }

        #region GetMonths

        [Fact]
        public async Task GetMonths_WithData_ReturnsOkWithMappedList()
        {
            var dtos = new List<MonthDto>
            {
                new() { MonthNumber = 1, MonthName = "January" },
                new() { MonthNumber = 2, MonthName = "February" }
            };
            var response = new List<MonthRes>
            {
                new() { MonthNumber = 1, MonthName = "January" },
                new() { MonthNumber = 2, MonthName = "February" }
            };

            _serviceMock.GetMonthsAsync().Returns(dtos);
            _mapperMock.Map<IList<MonthRes>>(dtos).Returns(response);

            var result = await _controller.GetMonths();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            await _serviceMock.Received(1).GetMonthsAsync();
            _mapperMock.Received(1).Map<IList<MonthRes>>(dtos);
        }

        [Fact]
        public async Task GetMonths_EmptyList_ReturnsOkWithEmptyList()
        {
            var dtos = new List<MonthDto>();
            var response = new List<MonthRes>();

            _serviceMock.GetMonthsAsync().Returns(dtos);
            _mapperMock.Map<IList<MonthRes>>(dtos).Returns(response);

            var result = await _controller.GetMonths();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetMonths_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetMonthsAsync().ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetMonths());
        }

        #endregion

        #region GetProjectMonthByProject

        [Fact]
        public async Task GetProjectMonthByProject_WithData_ReturnsOkWithMappedList()
        {
            var dtos = new List<ProjectMonthDto>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 100m },
                new() { Project = "PRJ1", MonthNo = 2, CostProfile = 200m }
            };
            var response = new List<ProjectMonthRes>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 100m },
                new() { Project = "PRJ1", MonthNo = 2, CostProfile = 200m }
            };

            _serviceMock.GetProjectMonthByProjectAsync("PRJ1").Returns(dtos);
            _mapperMock.Map<IList<ProjectMonthRes>>(dtos).Returns(response);

            var result = await _controller.GetProjectMonthByProject("PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            await _serviceMock.Received(1).GetProjectMonthByProjectAsync("PRJ1");
        }

        [Fact]
        public async Task GetProjectMonthByProject_EmptyList_ReturnsOkWithEmptyList()
        {
            var dtos = new List<ProjectMonthDto>();
            var response = new List<ProjectMonthRes>();

            _serviceMock.GetProjectMonthByProjectAsync("PRJ_NONE").Returns(dtos);
            _mapperMock.Map<IList<ProjectMonthRes>>(dtos).Returns(response);

            var result = await _controller.GetProjectMonthByProject("PRJ_NONE");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetProjectMonthByProject_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetProjectMonthByProjectAsync("PRJ1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetProjectMonthByProject("PRJ1"));
        }

        #endregion

        #region GetProjectMonth

        [Fact]
        public async Task GetProjectMonth_ExistingRecord_ReturnsOkWithMappedDto()
        {
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 3, CostProfile = 250m };
            var response = new ProjectMonthRes { Project = "PRJ1", MonthNo = 3, CostProfile = 250m };

            _serviceMock.GetProjectMonthAsync("PRJ1", 3).Returns(dto);
            _mapperMock.Map<ProjectMonthRes>(dto).Returns(response);

            var result = await _controller.GetProjectMonth("PRJ1", 3);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            await _serviceMock.Received(1).GetProjectMonthAsync("PRJ1", 3);
            _mapperMock.Received(1).Map<ProjectMonthRes>(dto);
        }

        [Fact]
        public async Task GetProjectMonth_NotFound_ThrowsKeyNotFoundException()
        {
            _serviceMock.GetProjectMonthAsync("PRJ_NONE", 99).Returns((ProjectMonthDto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetProjectMonth("PRJ_NONE", 99));
            _mapperMock.DidNotReceive().Map<ProjectMonthRes>(Arg.Any<ProjectMonthDto>());
        }

        [Fact]
        public async Task GetProjectMonth_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetProjectMonthAsync("PRJ1", 1).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetProjectMonth("PRJ1", 1));
        }

        #endregion

        #region CreateProjectMonth

        [Fact]
        public async Task CreateProjectMonth_ValidRequest_ReturnsCreatedAtAction()
        {
            var request = new ProjectMonthReq { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };
            var createdDto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };
            var response = new ProjectMonthRes { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };

            _mapperMock.Map<ProjectMonthDto>(request).Returns(dto);
            _serviceMock.CreateProjectMonthAsync(dto).Returns(createdDto);
            _mapperMock.Map<ProjectMonthRes>(createdDto).Returns(response);

            var result = await _controller.CreateProjectMonth(request);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(response, createdResult.Value);
            Assert.Equal("PRJ1", createdResult.RouteValues!["project"]);
            Assert.Equal(1, createdResult.RouteValues["monthNo"]);
            _mapperMock.Received(1).Map<ProjectMonthDto>(request);
            await _serviceMock.Received(1).CreateProjectMonthAsync(dto);
            _mapperMock.Received(1).Map<ProjectMonthRes>(createdDto);
        }

        [Fact]
        public async Task CreateProjectMonth_ServiceThrows_PropagatesException()
        {
            var request = new ProjectMonthReq { Project = "PRJ1", MonthNo = 1 };
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 1 };

            _mapperMock.Map<ProjectMonthDto>(request).Returns(dto);
            _serviceMock.CreateProjectMonthAsync(dto).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.CreateProjectMonth(request));
        }

        #endregion

        #region UpdateProjectMonth

        [Fact]
        public async Task UpdateProjectMonth_ValidRequest_ReturnsOkWithMappedDto()
        {
            var request = new ProjectMonthReq { Project = "PRJ1", MonthNo = 2, CostProfile = 500m };
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 2, CostProfile = 500m };
            var updatedDto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 2, CostProfile = 500m };
            var response = new ProjectMonthRes { Project = "PRJ1", MonthNo = 2, CostProfile = 500m };

            _mapperMock.Map<ProjectMonthDto>(request).Returns(dto);
            _serviceMock.UpdateProjectMonthAsync(dto).Returns(updatedDto);
            _mapperMock.Map<ProjectMonthRes>(updatedDto).Returns(response);

            var result = await _controller.UpdateProjectMonth(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            _mapperMock.Received(1).Map<ProjectMonthDto>(request);
            await _serviceMock.Received(1).UpdateProjectMonthAsync(dto);
            _mapperMock.Received(1).Map<ProjectMonthRes>(updatedDto);
        }

        [Fact]
        public async Task UpdateProjectMonth_ServiceThrows_PropagatesException()
        {
            var request = new ProjectMonthReq { Project = "PRJ1", MonthNo = 2 };
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 2 };

            _mapperMock.Map<ProjectMonthDto>(request).Returns(dto);
            _serviceMock.UpdateProjectMonthAsync(dto).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateProjectMonth(request));
        }

        #endregion

        #region DeleteProjectMonth

        [Fact]
        public async Task DeleteProjectMonth_ExistingRecord_ReturnsNoContent()
        {
            _serviceMock.DeleteProjectMonthAsync("PRJ1", 1).Returns(true);

            var result = await _controller.DeleteProjectMonth("PRJ1", 1);

            Assert.IsType<NoContentResult>(result);
            await _serviceMock.Received(1).DeleteProjectMonthAsync("PRJ1", 1);
        }

        [Fact]
        public async Task DeleteProjectMonth_NotFound_ReturnsNotFound()
        {
            _serviceMock.DeleteProjectMonthAsync("PRJ_NONE", 99).Returns(false);

            var result = await _controller.DeleteProjectMonth("PRJ_NONE", 99);

            Assert.IsType<NotFoundResult>(result);
            await _serviceMock.Received(1).DeleteProjectMonthAsync("PRJ_NONE", 99);
        }

        [Fact]
        public async Task DeleteProjectMonth_ServiceThrows_PropagatesException()
        {
            _serviceMock.DeleteProjectMonthAsync("PRJ1", 1).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.DeleteProjectMonth("PRJ1", 1));
        }

        #endregion
    }
}
