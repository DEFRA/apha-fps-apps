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

namespace Apha.PIMS.Api.UnitTests.Controllers.ProjectCommentControllerTest
{
    public class ProjectCommentControllerTests
    {
        private readonly ICommentService _service;
        private readonly IMapper _mapper;
        private readonly ProjectCommentController _controller;

        public ProjectCommentControllerTests()
        {
            _service = Substitute.For<ICommentService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ProjectCommentController(_service, _mapper);
        }

        #region GetCommentsByProject

        [Fact]
        public async Task GetCommentsByProject_ReturnsOkResult_WithMappedPaginatedComments()
        {
            // Arrange
            var project = "PP001";
            var year = 2024;
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var filter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginatedResult = new PaginatedResult<CommentDto>();
            var mappedResult = new PaginationRes<CommentRes>();

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetCommentsByProjectAsync(project, year, filter).Returns(paginatedResult);
            _mapper.Map<PaginationRes<CommentRes>>(paginatedResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetCommentsByProject(project, year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<QueryParameters<string>>(query);
            await _service.Received(1).GetCommentsByProjectAsync(project, year, filter);
            _mapper.Received(1).Map<PaginationRes<CommentRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetCommentsByProject_WithNullYear_ReturnsOkResult_WithMappedPaginatedComments()
        {
            // Arrange
            var project = "PP001";
            int? year = null;
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var filter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginatedResult = new PaginatedResult<CommentDto>();
            var mappedResult = new PaginationRes<CommentRes>();

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetCommentsByProjectAsync(project, year, filter).Returns(paginatedResult);
            _mapper.Map<PaginationRes<CommentRes>>(paginatedResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetCommentsByProject(project, year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<QueryParameters<string>>(query);
            await _service.Received(1).GetCommentsByProjectAsync(project, year, filter);
            _mapper.Received(1).Map<PaginationRes<CommentRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetCommentsByProject_WithEmptyServiceResult_ReturnsOkWithEmptyPagination()
        {
            // Arrange
            var project = "PP001";
            var year = 2024;
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var filter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var emptyResult = new PaginatedResult<CommentDto>();
            var emptyMapped = new PaginationRes<CommentRes>();

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetCommentsByProjectAsync(project, year, filter).Returns(emptyResult);
            _mapper.Map<PaginationRes<CommentRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetCommentsByProject(project, year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetCommentsByProjectAsync(project, year, filter);
        }

        [Fact]
        public async Task GetCommentsByProject_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var project = "PP001";
            var year = 2024;
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var filter = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetCommentsByProjectAsync(project, year, filter).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetCommentsByProject(project, year, query));

            _mapper.Received(1).Map<QueryParameters<string>>(query);
            await _service.Received(1).GetCommentsByProjectAsync(project, year, filter);
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_WithValidComment_ReturnsOkResult_WithMappedComment()
        {
            // Arrange
            var commentno = 1;
            var commentDto = new CommentDto
            {
                Commentno = commentno,
                Project = "PP001",
                Year = 2024,
                Topic = "Budget",
                Commenttext = "Review required",
                Madeby = "user1",
                Dateentered = new DateTime(2024, 1, 15)
            };
            var commentRes = new CommentRes
            {
                Commentno = commentno,
                Project = "PP001",
                Year = 2024,
                Topic = "Budget",
                Comment = "Review required",
                Madeby = "user1",
                Dateentered = new DateTime(2024, 1, 15)
            };

            _service.GetByIdAsync(commentno).Returns(commentDto);
            _mapper.Map<CommentRes>(commentDto).Returns(commentRes);

            // Act
            var result = await _controller.GetById(commentno);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(commentRes, okResult.Value);

            await _service.Received(1).GetByIdAsync(commentno);
            _mapper.Received(1).Map<CommentRes>(commentDto);
        }

        [Fact]
        public async Task GetById_WhenCommentNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var commentno = 999;
            _service.GetByIdAsync(commentno).Returns((CommentDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _controller.GetById(commentno));

            Assert.Equal($"Comment {commentno} not found.", exception.Message);

            await _service.Received(1).GetByIdAsync(commentno);
            _mapper.DidNotReceive().Map<CommentRes>(Arg.Any<CommentDto>());
        }

        [Fact]
        public async Task GetById_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var commentno = 1;
            _service.GetByIdAsync(commentno).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetById(commentno));

            await _service.Received(1).GetByIdAsync(commentno);
            _mapper.DidNotReceive().Map<CommentRes>(Arg.Any<CommentDto>());
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WithMappedComment()
        {
            // Arrange
            var request = new CommentReq
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Budget",
                Comment = "Initial review required",
                Madeby = "user1"
            };
            var dto = new CommentDto
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Budget",
                Commenttext = "Initial review required",
                Madeby = "user1"
            };
            var createdDto = new CommentDto
            {
                Commentno = 42,
                Project = "PP001",
                Year = 2024,
                Topic = "Budget",
                Commenttext = "Initial review required",
                Madeby = "user1",
                Dateentered = new DateTime(2024, 6, 1)
            };
            var createdRes = new CommentRes
            {
                Commentno = 42,
                Project = "PP001",
                Year = 2024,
                Topic = "Budget",
                Comment = "Initial review required",
                Madeby = "user1",
                Dateentered = new DateTime(2024, 6, 1)
            };

            _mapper.Map<CommentDto>(request).Returns(dto);
            _service.AddAsync(dto).Returns(createdDto);
            _mapper.Map<CommentRes>(createdDto).Returns(createdRes);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            Assert.NotNull(createdResult.RouteValues);
            Assert.Equal(42, createdResult.RouteValues["commentno"]);
            Assert.Equal(createdRes, createdResult.Value);

            _mapper.Received(1).Map<CommentDto>(request);
            await _service.Received(1).AddAsync(dto);
            _mapper.Received(1).Map<CommentRes>(createdDto);
        }

        [Fact]
        public async Task Create_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new CommentReq { Project = "PP001", Topic = "Budget", Comment = "Review required" };
            var dto = new CommentDto { Project = "PP001", Topic = "Budget", Commenttext = "Review required" };

            _mapper.Map<CommentDto>(request).Returns(dto);
            _service.AddAsync(dto).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Create(request));

            _mapper.Received(1).Map<CommentDto>(request);
            await _service.Received(1).AddAsync(dto);
            _mapper.DidNotReceive().Map<CommentRes>(Arg.Any<CommentDto>());
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_ReturnsOkResult_WithMappedComment_AndSetsCommentno()
        {
            // Arrange
            var commentno = 10;
            var request = new CommentReq
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Risk",
                Comment = "Updated risk review",
                Madeby = "user2"
            };
            var dto = new CommentDto
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Risk",
                Commenttext = "Updated risk review",
                Madeby = "user2"
            };
            var updatedDto = new CommentDto
            {
                Commentno = commentno,
                Project = "PP001",
                Year = 2024,
                Topic = "Risk",
                Commenttext = "Updated risk review",
                Madeby = "user2"
            };
            var updatedRes = new CommentRes
            {
                Commentno = commentno,
                Project = "PP001",
                Year = 2024,
                Topic = "Risk",
                Comment = "Updated risk review",
                Madeby = "user2"
            };

            _mapper.Map<CommentDto>(request).Returns(dto);
            _service.UpdateAsync(dto).Returns(updatedDto);
            _mapper.Map<CommentRes>(updatedDto).Returns(updatedRes);

            // Act
            var result = await _controller.Update(commentno, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedRes, okResult.Value);

            // Verify commentno was set on the dto before service call
            Assert.Equal(commentno, dto.Commentno);

            _mapper.Received(1).Map<CommentDto>(request);
            await _service.Received(1).UpdateAsync(dto);
            _mapper.Received(1).Map<CommentRes>(updatedDto);
        }

        [Fact]
        public async Task Update_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var commentno = 10;
            var request = new CommentReq { Project = "PP001", Topic = "Risk", Comment = "Updated risk review" };
            var dto = new CommentDto { Project = "PP001", Topic = "Risk", Commenttext = "Updated risk review" };

            _mapper.Map<CommentDto>(request).Returns(dto);
            _service.UpdateAsync(dto).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Update(commentno, request));

            _mapper.Received(1).Map<CommentDto>(request);
            await _service.Received(1).UpdateAsync(dto);
            _mapper.DidNotReceive().Map<CommentRes>(Arg.Any<CommentDto>());
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_WhenCommentExists_ReturnsOkWithTrue()
        {
            // Arrange
            var commentno = 1;
            _service.DeleteAsync(commentno).Returns(true);

            // Act
            var result = await _controller.Delete(commentno);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, okResult.Value);

            await _service.Received(1).DeleteAsync(commentno);
        }

        [Fact]
        public async Task Delete_WhenCommentDoesNotExist_ReturnsOkWithFalse()
        {
            // Arrange
            var commentno = 999;
            _service.DeleteAsync(commentno).Returns(false);

            // Act
            var result = await _controller.Delete(commentno);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(false, okResult.Value);

            await _service.Received(1).DeleteAsync(commentno);
        }

        [Fact]
        public async Task Delete_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var commentno = 1;
            _service.DeleteAsync(commentno).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Delete(commentno));

            await _service.Received(1).DeleteAsync(commentno);
        }

        #endregion
    }
}