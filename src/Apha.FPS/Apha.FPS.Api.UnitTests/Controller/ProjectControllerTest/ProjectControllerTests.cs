using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.ProjectControllerTest
{
    public class ProjectControllerTests
    {
        private readonly IProjectService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectController _controller;

        public ProjectControllerTests()
        {
            _serviceMock = Substitute.For<IProjectService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProjectController(
                _serviceMock,
                _mapperMock);
        }

        #region GetProjectsByProgramAsync

        [Fact]
        public async Task GetProjectsByProgramAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var projectDtos = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var serviceResult = new PaginatedResult<ProjectDto>(projectDtos, paginationDto);
            var mappedResult = new PaginationRes<ProjectRes>
            {
                Data = new List<ProjectRes>
                {
                    new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                    new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
                }
            };

            _serviceMock.GetProjectsByProgramAsync(query, programNo).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectsByProgramAsync(query, programNo);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetProjectsByProgramAsync(query, programNo);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_WhenProgramNoIsNull_ReturnsBadRequest()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.GetProjectsByProgramAsync(query, null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("programNo is required.", badRequest.Value);
            await _serviceMock.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetProjectsByProgramAsync_WhenProgramNoIsNullOrWhitespace_ReturnsBadRequest(string programNo)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.GetProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _serviceMock.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            _serviceMock.GetProjectsByProgramAsync(query, programNo)
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetProjectsByProgramAsync(query, programNo));
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_EmptyProjectList_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var emptyResult = new PaginatedResult<ProjectDto>(
                Enumerable.Empty<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            );
            var mappedResult = new PaginationRes<ProjectRes> { Data = new List<ProjectRes>() };

            _serviceMock.GetProjectsByProgramAsync(query, programNo).Returns(emptyResult);
            _mapperMock.Map<PaginationRes<ProjectRes>>(emptyResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectsByProgramAsync(query, programNo);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        #endregion

        #region GetProjectByIdAsync

        [Fact]
        public async Task GetProjectByIdAsync_HappyPath_ReturnsOk()
        {
            var dto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Alpha" };
            var mapped = new ProjectRes { ParentProject = "PP001", ProjectTitle = "Alpha" };

            _serviceMock.GetProjectByIdAsync("PP001").Returns(dto);
            _mapperMock.Map<ProjectRes>(dto).Returns(mapped);

            var result = await _controller.GetProjectByIdAsync("PP001");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetProjectByIdAsync_NotFound_ReturnsNotFound()
        {
            _serviceMock.GetProjectByIdAsync("NOPE").Returns((ProjectDto?)null);

            var result = await _controller.GetProjectByIdAsync("NOPE");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetProjectByIdAsync("PP001").Throws(new Exception("Error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetProjectByIdAsync("PP001"));
        }

        #endregion

        #region CreateProjectAsync

        [Fact]
        public async Task CreateProjectAsync_HappyPath_ReturnsOk()
        {
            var req = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Alpha" };
            var dto = new ProjectDto { ParentProject = "PP001" };
            var created = new ProjectDto { ParentProject = "PP001" };
            var mapped = new ProjectRes { ParentProject = "PP001" };

            _mapperMock.Map<ProjectDto>(req).Returns(dto);
            _serviceMock.CreateProjectAsync(dto).Returns(created);
            _mapperMock.Map<ProjectRes>(created).Returns(mapped);

            var result = await _controller.CreateProjectAsync(req);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task CreateProjectAsync_ServiceThrows_PropagatesException()
        {
            var req = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Alpha" };
            var dto = new ProjectDto { ParentProject = "PP001" };

            _mapperMock.Map<ProjectDto>(req).Returns(dto);
            _serviceMock.CreateProjectAsync(dto).Throws(new Exception("Error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.CreateProjectAsync(req));
        }

        #endregion

        #region UpdateProjectAsync

        [Fact]
        public async Task UpdateProjectAsync_HappyPath_ReturnsOk()
        {
            var req = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Updated" };
            var dto = new ProjectDto { ParentProject = "PP001" };
            var updated = new ProjectDto { ParentProject = "PP001" };
            var mapped = new ProjectRes { ParentProject = "PP001" };

            _mapperMock.Map<ProjectDto>(req).Returns(dto);
            _serviceMock.UpdateProjectAsync(dto).Returns(updated);
            _mapperMock.Map<ProjectRes>(updated).Returns(mapped);

            var result = await _controller.UpdateProjectAsync("PP001", req);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task UpdateProjectAsync_MismatchedCodes_ThrowsArgumentException()
        {
            var req = new ProjectReq { ParentProject = "PP002", ProjectTitle = "Alpha" };

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.UpdateProjectAsync("PP001", req));
        }

        #endregion

        #region DeleteProjectAndChildrenAsync

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_HappyPath_ReturnsOk()
        {
            var result = await _controller.DeleteProjectAndChildrenAsync("PP001");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).DeleteProjectAndChildrenAsync("PP001");
        }

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_EmptyId_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteProjectAndChildrenAsync(""));
        }

        #endregion

        #region ChangeProjectCodeAsync

        [Fact]
        public async Task ChangeProjectCodeAsync_HappyPath_ReturnsOk()
        {
            var req = new ChangeProjectCodeReq("OLD1", "NEW1");
            var existing = new ProjectDto { ParentProject = "OLD1" };

            _serviceMock.GetProjectByIdAsync("OLD1").Returns(existing);

            var result = await _controller.ChangeProjectCodeAsync(req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).ChangeProjectCodeAsync("OLD1", "NEW1");
        }

        [Fact]
        public async Task ChangeProjectCodeAsync_OldCodeNotFound_ReturnsNotFound()
        {
            var req = new ChangeProjectCodeReq("NOPE", "NEW1");
            _serviceMock.GetProjectByIdAsync("NOPE").Returns((ProjectDto?)null);

            var result = await _controller.ChangeProjectCodeAsync(req);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ChangeProjectCodeAsync_EmptyCodes_ThrowsArgumentException()
        {
            var req = new ChangeProjectCodeReq("", "NEW1");

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.ChangeProjectCodeAsync(req));
        }

        #endregion

        #region CheckProjectExistsAsync

        [Fact]
        public async Task CheckProjectExistsAsync_Exists_ReturnsTrue()
        {
            _serviceMock.CheckProjectExistsAsync("PP001").Returns(true);

            var result = await _controller.CheckProjectExistsAsync("PP001");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task CheckProjectExistsAsync_NotExists_ReturnsFalse()
        {
            _serviceMock.CheckProjectExistsAsync("NOPE").Returns(false);

            var result = await _controller.CheckProjectExistsAsync("NOPE");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.False((bool)okResult.Value!);
        }

        #endregion

        #region GetAllProjectsAsync

        [Fact]
        public async Task GetAllProjectsAsync_HappyPath_ReturnsOk()
        {
            var dtos = new List<ProjectDto> { new() { ParentProject = "PP001" } };
            var mapped = new List<ProjectRes> { new() { ParentProject = "PP001" } };

            _serviceMock.GetAllProjectsAsync().Returns(dtos);
            _mapperMock.Map<List<ProjectRes>>(dtos).Returns(mapped);

            var result = await _controller.GetAllProjectsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        #endregion

        #region GetPagedPactProjectsAsync

        [Fact]
        public async Task GetPagedPactProjectsAsync_HappyPath_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectDto>(
                new List<ProjectDto> { new() { ParentProject = "PP001" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });
            var mapped = new PaginationRes<ProjectRes>();

            _serviceMock.GetPagedPactProjectsAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedPactProjectsAsync(query);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        #endregion

        #region GetAllPactProjectsAsync

        [Fact]
        public async Task GetAllPactProjectsAsync_HappyPath_ReturnsOk()
        {
            var dtos = new List<ProjectDto> { new() { ParentProject = "PP001" } };
            var mapped = new List<ProjectRes> { new() { ParentProject = "PP001" } };

            _serviceMock.GetAllPactProjectsAsync().Returns(dtos);
            _mapperMock.Map<List<ProjectRes>>(dtos).Returns(mapped);

            var result = await _controller.GetAllPactProjectsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        #endregion

        #region UpdatePactProjectDetailsAsync

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_HappyPath_ReturnsOk()
        {
            var req = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Alpha" };
            var dto = new ProjectDto { ParentProject = "PP001" };
            var updated = new ProjectDto { ParentProject = "PP001" };
            var mapped = new ProjectRes { ParentProject = "PP001" };

            _mapperMock.Map<ProjectDto>(req).Returns(dto);
            _serviceMock.UpdatePactProjectDetailsAsync(dto).Returns(updated);
            _mapperMock.Map<ProjectRes>(updated).Returns(mapped);

            var result = await _controller.UpdatePactProjectDetailsAsync(req);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_NotFound_ReturnsNotFound()
        {
            var req = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Alpha" };
            var dto = new ProjectDto { ParentProject = "PP001" };

            _mapperMock.Map<ProjectDto>(req).Returns(dto);
            _serviceMock.UpdatePactProjectDetailsAsync(dto).Returns((ProjectDto?)null);

            var result = await _controller.UpdatePactProjectDetailsAsync(req);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region UpdateProjectRootAsync

        [Fact]
        public async Task UpdateProjectRootAsync_HappyPath_ReturnsOk()
        {
            var req = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Alpha" };
            var dto = new ProjectDto { ParentProject = "PP001" };
            var updated = new ProjectDto { ParentProject = "PP001" };
            var mapped = new ProjectRes { ParentProject = "PP001" };

            _mapperMock.Map<ProjectDto>(req).Returns(dto);
            _serviceMock.UpdateProjectAsync(dto).Returns(updated);
            _mapperMock.Map<ProjectRes>(updated).Returns(mapped);

            var result = await _controller.UpdateProjectRootAsync(req);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        #endregion

        #region DeleteProjectAsync

        [Fact]
        public async Task DeleteProjectAsync_HappyPath_ReturnsOk()
        {
            _serviceMock.DeleteProjectAsync("PP001").Returns(true);

            var result = await _controller.DeleteProjectAsync("PP001");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteProjectAsync_NotFound_ReturnsNotFound()
        {
            _serviceMock.DeleteProjectAsync("NOPE").Returns(false);

            var result = await _controller.DeleteProjectAsync("NOPE");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteProjectAsync_EmptyId_ReturnsBadRequest()
        {
            var result = await _controller.DeleteProjectAsync("");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion
    }
}
