using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.CommentServiceTest
{
    public class CommentServiceTests
    {
        private readonly ICommentRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly CommentService _sut;

        public CommentServiceTests()
        {
            _mockRepository = Substitute.For<ICommentRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new CommentService(_mockRepository, _mockMapper);
        }

        #region GetCommentsByProjectAsync

        [Fact]
        public async Task GetCommentsByProjectAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var project = "PP001";
            int? year = 2024;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            var commentEntities = new List<Comment>
            {
                new Comment { Commentno = 1, Project = project, Year = 2024, Topic = "Topic1", Commenttext = "Text1", Madeby = "User1" },
                new Comment { Commentno = 2, Project = project, Year = 2024, Topic = "Topic2", Commenttext = "Text2", Madeby = "User2" }
            };

            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var pagedData = new PagedData<Comment>(commentEntities, paginationData);

            var expectedDtos = new List<CommentDto>
            {
                new CommentDto { Commentno = 1, Project = project, Year = 2024, Topic = "Topic1", Commenttext = "Text1", Madeby = "User1" },
                new CommentDto { Commentno = 2, Project = project, Year = 2024, Topic = "Topic2", Commenttext = "Text2", Madeby = "User2" }
            };

            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var expectedResult = new PaginatedResult<CommentDto>(expectedDtos, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetCommentsByProjectAsync(project, year, paginationParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<CommentDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetCommentsByProjectAsync(project, year, query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().Commentno.Should().Be(1);
            result.PaginationData.TotalRecords.Should().Be(2);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetCommentsByProjectAsync(project, year, paginationParams);
            _mockMapper.Received(1).Map<PaginatedResult<CommentDto>>(pagedData);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WithNullYear_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var project = "PP001";
            int? year = null;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            var emptyPagedData = new PagedData<Comment>(
                new List<Comment>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            );

            var emptyResult = new PaginatedResult<CommentDto>(
                Enumerable.Empty<CommentDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            );

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetCommentsByProjectAsync(project, year, paginationParams).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<CommentDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetCommentsByProjectAsync(project, year, query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetCommentsByProjectAsync(project, year, paginationParams);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var project = "PP001";
            int? year = 2024;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var expectedException = new Exception("Database connection failed");

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetCommentsByProjectAsync(project, year, paginationParams)
                .Returns(Task.FromException<PagedData<Comment>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetCommentsByProjectAsync(project, year, query)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetCommentsByProjectAsync(project, year, paginationParams);
            _mockMapper.DidNotReceive().Map<PaginatedResult<CommentDto>>(Arg.Any<PagedData<Comment>>());
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WithValidCommentNo_ReturnsMappedDto()
        {
            // Arrange
            var commentno = 1;

            var entity = new Comment
            {
                Commentno = commentno,
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                Commenttext = "Some comment text",
                Madeby = "User1",
                Dateentered = new DateTime(2024, 1, 15)
            };

            var expectedDto = new CommentDto
            {
                Commentno = commentno,
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                Commenttext = "Some comment text",
                Madeby = "User1",
                Dateentered = new DateTime(2024, 1, 15)
            };

            _mockRepository.GetByIdAsync(commentno)
                .Returns(Task.FromResult<Comment?>(entity));

            _mockMapper.Map<CommentDto>(entity).Returns(expectedDto);

            // Act
            var result = await _sut.GetByIdAsync(commentno);

            // Assert
            result.Should().NotBeNull();
            result!.Commentno.Should().Be(1);
            result.Project.Should().Be("PP001");
            result.Commenttext.Should().Be("Some comment text");

            await _mockRepository.Received(1).GetByIdAsync(commentno);
            _mockMapper.Received(1).Map<CommentDto>(entity);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCommentNotFound_ReturnsNull()
        {
            // Arrange
            var commentno = 999;

            _mockRepository.GetByIdAsync(commentno)
                .Returns(Task.FromResult<Comment?>(null));

            // Act
            var result = await _sut.GetByIdAsync(commentno);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetByIdAsync(commentno);
            _mockMapper.DidNotReceive().Map<CommentDto>(Arg.Any<Comment>());
        }

        [Fact]
        public async Task GetByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var commentno = 1;
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetByIdAsync(commentno)
                .Returns(Task.FromException<Comment?>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetByIdAsync(commentno)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetByIdAsync(commentno);
            _mockMapper.DidNotReceive().Map<CommentDto>(Arg.Any<Comment>());
        }

        #endregion

        #region AddAsync

        [Theory]
        [InlineData(null, "Some comment text")]
        [InlineData("", "Some comment text")]
        [InlineData("   ", "Some comment text")]
        public async Task AddAsync_WithInvalidProject_ThrowsBusinessValidationErrorException(string? project, string commentText)
        {
            // Arrange
            var dto = new CommentDto { Project = project, Commenttext = commentText };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.AddAsync(dto)
            );

            exception.Errors.Should().ContainSingle();
            exception.Errors.First().Code.Should().Be("PROJECT_REQUIRED");
            exception.Errors.First().Message.Should().Be("Project is required.");

            await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Comment>());
        }

        [Theory]
        [InlineData("PP001", null)]
        [InlineData("PP001", "")]
        [InlineData("PP001", "   ")]
        public async Task AddAsync_WithInvalidCommentText_ThrowsBusinessValidationErrorException(string project, string? commentText)
        {
            // Arrange
            var dto = new CommentDto { Project = project, Commenttext = commentText };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.AddAsync(dto)
            );

            exception.Errors.Should().ContainSingle();
            exception.Errors.First().Code.Should().Be("COMMENT_TEXT_REQUIRED");
            exception.Errors.First().Message.Should().Be("Comment text is required.");

            await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Comment>());
        }

        [Fact]
        public async Task AddAsync_WithBothProjectAndCommentTextInvalid_ThrowsWithMultipleErrors()
        {
            // Arrange
            var dto = new CommentDto { Project = null, Commenttext = null };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.AddAsync(dto)
            );

            exception.Errors.Should().HaveCount(2);
            exception.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            exception.Errors.Should().Contain(e => e.Code == "COMMENT_TEXT_REQUIRED");

            await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Comment>());
        }

        [Fact]
        public async Task AddAsync_WithValidDto_SetsDateenteredAndReturnsMappedCreatedDto()
        {
            // Arrange
            var dto = new CommentDto
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                Commenttext = "Some comment text",
                Madeby = "User1"
            };

            var entity = new Comment
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                Commenttext = "Some comment text",
                Madeby = "User1"
            };

            var createdEntity = new Comment
            {
                Commentno = 42,
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                Commenttext = "Some comment text",
                Madeby = "User1",
                Dateentered = new DateTime(2024, 6, 1)
            };

            var expectedDto = new CommentDto
            {
                Commentno = 42,
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                Commenttext = "Some comment text",
                Madeby = "User1",
                Dateentered = createdEntity.Dateentered
            };

            _mockMapper.Map<Comment>(dto).Returns(entity);
            _mockRepository.AddAsync(entity).Returns(Task.FromResult(createdEntity));
            _mockMapper.Map<CommentDto>(createdEntity).Returns(expectedDto);

            // Act
            var result = await _sut.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Commentno.Should().Be(42);
            result.Project.Should().Be("PP001");
            result.Commenttext.Should().Be("Some comment text");

            entity.Dateentered.Should().NotBeNull();
            entity.Dateentered!.Value.Kind.Should().Be(DateTimeKind.Unspecified);

            _mockMapper.Received(1).Map<Comment>(dto);
            await _mockRepository.Received(1).AddAsync(entity);
            _mockMapper.Received(1).Map<CommentDto>(createdEntity);
        }

        [Fact]
        public async Task AddAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new CommentDto
            {
                Project = "PP001",
                Commenttext = "Some comment text"
            };

            var entity = new Comment
            {
                Project = "PP001",
                Commenttext = "Some comment text"
            };

            var expectedException = new Exception("Database connection failed");

            _mockMapper.Map<Comment>(dto).Returns(entity);
            _mockRepository.AddAsync(entity)
                .Returns(Task.FromException<Comment>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.AddAsync(dto)
            );

            exception.Message.Should().Be("Database connection failed");

            _mockMapper.Received(1).Map<Comment>(dto);
            await _mockRepository.Received(1).AddAsync(entity);
            _mockMapper.DidNotReceive().Map<CommentDto>(Arg.Any<Comment>());
        }

        #endregion

        #region UpdateAsync

        [Theory]
        [InlineData(null, "Some comment text")]
        [InlineData("", "Some comment text")]
        [InlineData("   ", "Some comment text")]
        public async Task UpdateAsync_WithInvalidProject_ThrowsBusinessValidationErrorException(string? project, string commentText)
        {
            // Arrange
            var dto = new CommentDto { Project = project, Commenttext = commentText };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateAsync(dto)
            );

            exception.Errors.Should().ContainSingle();
            exception.Errors.First().Code.Should().Be("PROJECT_REQUIRED");
            exception.Errors.First().Message.Should().Be("Project is required.");

            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>());
        }

        [Theory]
        [InlineData("PP001", null)]
        [InlineData("PP001", "")]
        [InlineData("PP001", "   ")]
        public async Task UpdateAsync_WithInvalidCommentText_ThrowsBusinessValidationErrorException(string project, string? commentText)
        {
            // Arrange
            var dto = new CommentDto { Project = project, Commenttext = commentText };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateAsync(dto)
            );

            exception.Errors.Should().ContainSingle();
            exception.Errors.First().Code.Should().Be("COMMENT_TEXT_REQUIRED");
            exception.Errors.First().Message.Should().Be("Comment text is required.");

            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>());
        }

        [Fact]
        public async Task UpdateAsync_WithBothProjectAndCommentTextInvalid_ThrowsWithMultipleErrors()
        {
            // Arrange
            var dto = new CommentDto { Project = null, Commenttext = null };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateAsync(dto)
            );

            exception.Errors.Should().HaveCount(2);
            exception.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            exception.Errors.Should().Contain(e => e.Code == "COMMENT_TEXT_REQUIRED");

            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>());
        }

        [Fact]
        public async Task UpdateAsync_WithValidDto_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var dto = new CommentDto
            {
                Commentno = 1,
                Project = "PP001",
                Year = 2024,
                Topic = "Updated Topic",
                Commenttext = "Updated comment text",
                Madeby = "User1"
            };

            var entity = new Comment
            {
                Commentno = 1,
                Project = "PP001",
                Year = 2024,
                Topic = "Updated Topic",
                Commenttext = "Updated comment text",
                Madeby = "User1"
            };

            var updatedEntity = new Comment
            {
                Commentno = 1,
                Project = "PP001",
                Year = 2024,
                Topic = "Updated Topic",
                Commenttext = "Updated comment text",
                Madeby = "User1",
                Dateentered = new DateTime(2024, 1, 15)
            };

            var expectedDto = new CommentDto
            {
                Commentno = 1,
                Project = "PP001",
                Year = 2024,
                Topic = "Updated Topic",
                Commenttext = "Updated comment text",
                Madeby = "User1",
                Dateentered = new DateTime(2024, 1, 15)
            };

            _mockMapper.Map<Comment>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity).Returns(Task.FromResult(updatedEntity));
            _mockMapper.Map<CommentDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Commentno.Should().Be(1);
            result.Project.Should().Be("PP001");
            result.Commenttext.Should().Be("Updated comment text");

            _mockMapper.Received(1).Map<Comment>(dto);
            await _mockRepository.Received(1).UpdateAsync(entity);
            _mockMapper.Received(1).Map<CommentDto>(updatedEntity);
        }

        [Fact]
        public async Task UpdateAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new CommentDto
            {
                Commentno = 1,
                Project = "PP001",
                Commenttext = "Some comment text"
            };

            var entity = new Comment
            {
                Commentno = 1,
                Project = "PP001",
                Commenttext = "Some comment text"
            };

            var expectedException = new Exception("Database connection failed");

            _mockMapper.Map<Comment>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity)
                .Returns(Task.FromException<Comment>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.UpdateAsync(dto)
            );

            exception.Message.Should().Be("Database connection failed");

            _mockMapper.Received(1).Map<Comment>(dto);
            await _mockRepository.Received(1).UpdateAsync(entity);
            _mockMapper.DidNotReceive().Map<CommentDto>(Arg.Any<Comment>());
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WhenCommentExists_ReturnsTrue()
        {
            // Arrange
            var commentno = 1;

            _mockRepository.DeleteAsync(commentno).Returns(Task.FromResult(true));

            // Act
            var result = await _sut.DeleteAsync(commentno);

            // Assert
            result.Should().BeTrue();

            await _mockRepository.Received(1).DeleteAsync(commentno);
        }

        [Fact]
        public async Task DeleteAsync_WhenCommentNotFound_ReturnsFalse()
        {
            // Arrange
            var commentno = 999;

            _mockRepository.DeleteAsync(commentno).Returns(Task.FromResult(false));

            // Act
            var result = await _sut.DeleteAsync(commentno);

            // Assert
            result.Should().BeFalse();

            await _mockRepository.Received(1).DeleteAsync(commentno);
        }

        [Fact]
        public async Task DeleteAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var commentno = 1;
            var expectedException = new Exception("Database connection failed");

            _mockRepository.DeleteAsync(commentno)
                .Returns(Task.FromException<bool>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.DeleteAsync(commentno)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).DeleteAsync(commentno);
        }

        #endregion
    }
}