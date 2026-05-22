using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.CommentRepositoryTest
{
    public class CommentRepositoryTests
    {
        /// <summary>
        /// Creates a CommentRepository with in-memory data.
        /// The parameter is optional — omitted set is initialised as empty.
        /// </summary>
        private static CommentRepository CreateRepository(IEnumerable<Comment>? comments = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var commentsMockSet = RepositoryTestHelper.CreateMockDbSet(comments ?? Enumerable.Empty<Comment>());

            RepositoryTestHelper.SetupDbSetOperations(commentsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Comments).Returns(commentsMockSet.Object);

            return new CommentRepository(mockContext.Object);
        }

        /// <summary>
        /// Returns the repository alongside its mocked DbSet and DbContext
        /// for tests that need to verify Add / Update / Remove / SaveChanges calls.
        /// </summary>
        private static (
            CommentRepository Repo,
            Mock<DbSet<Comment>> CommentsDbSet,
            Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<Comment>? comments = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var commentsMockSet = RepositoryTestHelper.CreateMockDbSet(comments ?? Enumerable.Empty<Comment>());

            RepositoryTestHelper.SetupDbSetOperations(commentsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Comments).Returns(commentsMockSet.Object);

            var repo = new CommentRepository(mockContext.Object);
            return (repo, commentsMockSet, mockContext);
        }

        #region GetCommentsByProjectAsync — project filter

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsAllMatchingComments_WhenProjectExists()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Topic1" },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Topic2" },
                new() { CommentNo = 3, Project = "PP002", Year = 2024, Topic = "Topic3" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, c => Assert.Equal("PP001", c.Project));
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsEmpty_WhenNoCommentsMatchProject()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP002", Year = 2024, Topic = "Topic1" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsEmpty_WhenCommentsDbSetIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(comments: new List<Comment>());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_DoesNotReturnOtherProjects()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "A" },
                new() { CommentNo = 2, Project = "PP002", Year = 2024, Topic = "B" },
                new() { CommentNo = 3, Project = "PP003", Year = 2024, Topic = "C" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("PP001", result.Data.First().Project);
        }

        #endregion

        #region GetCommentsByProjectAsync — year filter

        [Fact]
        public async Task GetCommentsByProjectAsync_FiltersByYear_WhenYearIsProvided()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2022, Topic = "Topic1" },
                new() { CommentNo = 2, Project = "PP001", Year = 2023, Topic = "Topic2" },
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Topic3" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", 2023, query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal((short)2023, result.Data.First().Year);
            Assert.Equal(2, result.Data.First().CommentNo);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsAllYears_WhenYearIsNull()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2022, Topic = "Topic1" },
                new() { CommentNo = 2, Project = "PP001", Year = 2023, Topic = "Topic2" },
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Topic3" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsEmpty_WhenYearMatchesNoComments()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2022, Topic = "Topic1" },
                new() { CommentNo = 2, Project = "PP001", Year = 2023, Topic = "Topic2" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", 2099, query);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsMultipleComments_WhenMultipleMatchProjectAndYear()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic1" },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Topic2" },
                new() { CommentNo = 3, Project = "PP001", Year = 2023, Topic = "Topic3" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", 2024, query);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, c => Assert.Equal((short)2024, c.Year));
        }

        #endregion

        #region GetCommentsByProjectAsync — ApplySorting

        [Theory]
        [InlineData("CommentNo", false)]
        [InlineData("CommentNo", true)]
        [InlineData("topic", false)]
        [InlineData("topic", true)]
        [InlineData("year", false)]
        [InlineData("year", true)]
        [InlineData("MadeBy", false)]
        [InlineData("MadeBy", true)]
        [InlineData("project", false)]
        [InlineData("project", true)]
        public async Task GetCommentsByProjectAsync_WithSorting_ReturnsSortedResults(
            string sortBy, bool descending)
        {
            // Arrange — source list is intentionally unsorted to prove sorting takes effect
            var comments = new List<Comment>
            {
                new() { CommentNo = 2, Project = "PP001", Year = 2023, Topic = "Beta",  MadeBy = "Bob"     },
                new() { CommentNo = 1, Project = "PP001", Year = 2022, Topic = "Alpha", MadeBy = "Alice"   },
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Gamma", MadeBy = "Charlie" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(3, result.Data.Count);
            var first = result.Data.First();

            switch (sortBy.ToLower())
            {
                case "CommentNo":
                    Assert.Equal(descending ? 3 : 1, first.CommentNo);
                    break;
                case "topic":
                    Assert.Equal(descending ? "Gamma" : "Alpha", first.Topic);
                    break;
                case "year":
                    Assert.Equal(descending ? (short)2024 : (short)2022, first.Year);
                    break;
                case "MadeBy":
                    Assert.Equal(descending ? "Charlie" : "Alice", first.MadeBy);
                    break;
                case "project":
                    // All results share the same project value; sorting by project is a no-op —
                    // verify the code path runs without error and all records are returned.
                    Assert.Equal(3, result.Data.Count);
                    break;
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetCommentsByProjectAsync_SortByDateEntered_ReturnsSortedResults(bool descending)
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 2, Project = "PP001", Year = 2023, Topic = "Beta",  DateEntered = new DateTime(2023, 6,  1) },
                new() { CommentNo = 1, Project = "PP001", Year = 2022, Topic = "Alpha", DateEntered = new DateTime(2022, 1,  1) },
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Gamma", DateEntered = new DateTime(2024, 12, 1) }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "DateEntered",
                Descending = descending
            };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(3, result.Data.Count);
            if (descending)
                Assert.Equal(new DateTime(2024, 12, 1), result.Data.First().DateEntered);
            else
                Assert.Equal(new DateTime(2022, 1, 1), result.Data.First().DateEntered);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WithNullSortBy_ReturnsResultsInDefaultOrder()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Gamma" },
                new() { CommentNo = 1, Project = "PP001", Year = 2022, Topic = "Alpha" },
                new() { CommentNo = 2, Project = "PP001", Year = 2023, Topic = "Beta"  }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = null };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WithEmptySortBy_ReturnsResultsInDefaultOrder()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Alpha" },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Beta"  }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = string.Empty };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WithInvalidSortBy_ReturnsResultsInDefaultOrder()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Alpha" },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Beta"  }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "invalid_field" };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region GetCommentsByProjectAsync — ApplyPaging

        [Fact]
        public async Task GetCommentsByProjectAsync_WithPaging_ReturnsCorrectPage()
        {
            // Arrange
            var comments = Enumerable.Range(1, 5)
                .Select(i => new Comment { CommentNo = i, Project = "PP001", Year = (short)(2020 + i), Topic = $"Topic{i}" })
                .ToList();
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(2, result.PaginationData.PageSize);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsCorrectPaginationMetadata()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Topic1" },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Topic2" },
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Topic3" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
            Assert.Equal(1, result.PaginationData.TotalPages);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsComment_WhenCommentNoExists()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Topic A", CommentText = "Text1", MadeBy = "User1", DateEntered = new DateTime(2023, 6, 1) },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Topic B", CommentText = "Text2", MadeBy = "User2", DateEntered = new DateTime(2024, 1, 1) }
            };
            var repo = CreateRepository(comments: comments);

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CommentNo);
            Assert.Equal("PP001", result.Project);
            Assert.Equal((short)2023, result.Year);
            Assert.Equal("Topic A", result.Topic);
            Assert.Equal("Text1", result.CommentText);
            Assert.Equal("User1", result.MadeBy);
            Assert.Equal(new DateTime(2023, 6, 1), result.DateEntered);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCommentNoDoesNotExist()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Topic A" }
            };
            var repo = CreateRepository(comments: comments);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCommentsDbSetIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(comments: new List<Comment>());

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(999)]
        public async Task GetByIdAsync_ReturnsNull_WhenIdDoesNotMatch(int CommentNo)
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" }
            };
            var repo = CreateRepository(comments: comments);

            // Act
            var result = await repo.GetByIdAsync(CommentNo);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_AddsEntityAndReturnsIt()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var entity = new Comment
            {
                CommentNo = 1,
                Project = "PP001",
                Year = 2024,
                Topic = "New Topic",
                CommentText = "Comment text",
                MadeBy = "User1",
                DateEntered = new DateTime(2024, 1, 1)
            };

            // Act
            var result = await repo.AddAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
            Assert.Equal(1, result.CommentNo);
            Assert.Equal("PP001", result.Project);
            Assert.Equal("New Topic", result.Topic);
        }

        [Fact]
        public async Task AddAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, commentsDbSet, _) = CreateRepositoryWithMocks();
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };

            // Act
            await repo.AddAsync(entity);

            // Assert
            commentsDbSet.Verify(x => x.Add(entity), Times.Once);
        }

        [Fact]
        public async Task AddAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };

            // Act
            await repo.AddAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ReturnsEntity()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var entity = new Comment
            {
                CommentNo = 1,
                Project = "PP001",
                Year = 2024,
                Topic = "Updated Topic",
                CommentText = "Updated text",
                MadeBy = "User1"
            };

            // Act
            var result = await repo.UpdateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
            Assert.Equal("Updated Topic", result.Topic);
        }

        [Fact]
        public async Task UpdateAsync_CallsDbSetUpdate()
        {
            // Arrange
            var (repo, commentsDbSet, _) = CreateRepositoryWithMocks();
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };

            // Act
            await repo.UpdateAsync(entity);

            // Assert
            commentsDbSet.Verify(x => x.Update(entity), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };

            // Act
            await repo.UpdateAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenEntityFound()
        {
            // Arrange
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };
            var (repo, commentsDbSet, _) = CreateRepositoryWithMocks();
            commentsDbSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<Comment?>(entity));

            // Act
            var result = await repo.DeleteAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_CallsDbSetRemove_WhenEntityFound()
        {
            // Arrange
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };
            var (repo, commentsDbSet, _) = CreateRepositoryWithMocks();
            commentsDbSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<Comment?>(entity));

            // Act
            await repo.DeleteAsync(1);

            // Assert
            commentsDbSet.Verify(x => x.Remove(entity), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_CallsSaveChangesAsync_WhenEntityFound()
        {
            // Arrange
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };
            var (repo, commentsDbSet, mockContext) = CreateRepositoryWithMocks();
            commentsDbSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<Comment?>(entity));

            // Act
            await repo.DeleteAsync(1);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenEntityNotFound()
        {
            // Arrange
            var (repo, commentsDbSet, _) = CreateRepositoryWithMocks();
            commentsDbSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<Comment?>((Comment?)null));

            // Act
            var result = await repo.DeleteAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_DoesNotCallRemove_WhenEntityNotFound()
        {
            // Arrange
            var (repo, commentsDbSet, _) = CreateRepositoryWithMocks();
            commentsDbSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<Comment?>((Comment?)null));

            // Act
            await repo.DeleteAsync(999);

            // Assert
            commentsDbSet.Verify(x => x.Remove(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_DoesNotCallSaveChangesAsync_WhenEntityNotFound()
        {
            // Arrange
            var (repo, commentsDbSet, mockContext) = CreateRepositoryWithMocks();
            commentsDbSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<Comment?>((Comment?)null));

            // Act
            await repo.DeleteAsync(999);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 0);
        }

        #endregion
    }
}