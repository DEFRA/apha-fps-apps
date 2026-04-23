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
    }
}
