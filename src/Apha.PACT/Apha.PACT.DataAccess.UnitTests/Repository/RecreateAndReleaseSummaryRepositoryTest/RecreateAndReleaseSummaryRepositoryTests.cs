using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.RecreateAndReleaseSummaryRepositoryTest
{
    public class RecreateAndReleaseSummaryRepositoryTests
    {
        private const string TestUserId = "TestUser1";
        private const string TestUserName = "Test User Name";
        private const short TestPeriod = 1;
        private const int TestFpsYear = 2024;

        private static FpsDbContext CreateTestContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            var mockFpsRequestContext = Substitute.For<IFpsRequestContext>();
            mockFpsRequestContext.FpsYear.Returns(TestFpsYear);

            return new FpsDbContext(options, mockFpsRequestContext);
        }

        #region GetRecreateSummariesLogsAsync - Basic Retrieval

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithExistingLogs_ReturnsAllLogsOrderedByDateDoneDescending()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var user = new User { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummaryLogs>() };

            var logs = new List<RecreateSummaryLogs>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = DateTime.UtcNow.AddDays(-2), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 2, DateDone = DateTime.UtcNow.AddDays(-1), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 3, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.True(resultList[0].DateDone >= resultList[1].DateDone);
            Assert.True(resultList[1].DateDone >= resultList[2].DateDone);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithNoLogs_ReturnsEmptyCollection()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
            Assert.Equal(0, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_IncludesUserNavigation_ReturnsLogsWithUserData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            // Create user with UserName that matches CVLNT + UserId pattern
            var user = new User { UserName = "CVLNT" + TestUserId, Comments = "Test Comment", Logs = new List<RecreateSummaryLogs>() };
            await context.Users.AddAsync(user);

            var log = new RecreateSummaryLogs
            { 
                UserId = TestUserId, 
                Period = TestPeriod, 
                DateDone = DateTime.UtcNow, 
                FpsYear = TestFpsYear,
                User = null! // Will be populated by repository join logic
            };

            await context.RecreateSummaryLogs.AddAsync(log);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);
            var firstLog = result.Data.FirstOrDefault();

            // Assert
            Assert.NotNull(firstLog);
            Assert.NotNull(firstLog.User);
            Assert.Equal("CVLNT" + TestUserId, firstLog.User.UserName);
        }

        #endregion

        #region GetRecreateSummariesLogsAsync - Pagination

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummaryLogs>() };

            var logs = Enumerable.Range(1, 25)
                .Select(i => new RecreateSummaryLogs
                {
                    UserId = TestUserId,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = TestFpsYear,
                    User = user
                }).ToList();

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 2, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Data.Count);
            Assert.Equal(25, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.PaginationData.TotalPages);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithPaginationLastPage_ReturnsRemainingRecords()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummaryLogs>() };

            var logs = Enumerable.Range(1, 25)
                .Select(i => new RecreateSummaryLogs
                {
                    UserId = TestUserId,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = TestFpsYear,
                    User = user
                }).ToList();

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 3, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Data.Count);
            Assert.Equal(25, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithPageBeyondLimit_ReturnsEmptyData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummaryLogs>() };

            var logs = Enumerable.Range(1, 5)
                .Select(i => new RecreateSummaryLogs
                {
                    UserId = TestUserId,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = TestFpsYear,
                    User = user
                }).ToList();

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 10, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.TotalPages);
        }

        #endregion

        #region GetRecreateSummariesLogsAsync - Sorting

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_SortByDateDoneAscending_ReturnsSortedData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummaryLogs>() };

            var logs = new List<RecreateSummaryLogs>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = new DateTime(2024, 3, 15), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 2, DateDone = new DateTime(2024, 1, 10), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 3, DateDone = new DateTime(2024, 2, 20), FpsYear = TestFpsYear, User = user }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "datedone", descending: false);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal(new DateTime(2024, 1, 10), resultList[0].DateDone);
            Assert.Equal(new DateTime(2024, 2, 20), resultList[1].DateDone);
            Assert.Equal(new DateTime(2024, 3, 15), resultList[2].DateDone);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_SortByPeriodDescending_ReturnsSortedData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummaryLogs>() };

            var logs = new List<RecreateSummaryLogs>
            {
                new() { UserId = TestUserId, Period = 5, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 8, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "period", descending: true);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal((short)8, resultList[0].Period);
            Assert.Equal((short)5, resultList[1].Period);
            Assert.Equal((short)2, resultList[2].Period);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_SortByUserIdAscending_ReturnsSortedData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            // UserId is mapped to TblUser.UserName, so UserName must match UserId
            var user1 = new User { UserName = "UserC", Comments = "Comment C", Logs = new List<RecreateSummaryLogs>() };
            var user2 = new User { UserName = "UserA", Comments = "Comment A", Logs = new List<RecreateSummaryLogs>() };
            var user3 = new User { UserName = "UserB", Comments = "Comment B", Logs = new List<RecreateSummaryLogs>() };

            var logs = new List<RecreateSummaryLogs>
            {
                new() { UserId = "UserC", Period = 1, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user1 },
                new() { UserId = "UserA", Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user2 },
                new() { UserId = "UserB", Period = 3, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user3 }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "userid", descending: false);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal("UserA", resultList[0].UserId);
            Assert.Equal("UserB", resultList[1].UserId);
            Assert.Equal("UserC", resultList[2].UserId);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_SortByUserNameAscending_ReturnsSortedData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            // Create users with UserName matching CVLNT + UserId pattern
            var userA = new User { UserName = "CVLNTUser3", Comments = "Comment C", Logs = new List<RecreateSummaryLogs>() };
            var userB = new User { UserName = "CVLNTUser1", Comments = "Comment A", Logs = new List<RecreateSummaryLogs>() };
            var userC = new User { UserName = "CVLNTUser2", Comments = "Comment B", Logs = new List<RecreateSummaryLogs>() };

            await context.Users.AddRangeAsync(new[] { userA, userB, userC });

            var logs = new List<RecreateSummaryLogs>
            {
                new() { UserId = "User3", Period = 1, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = null! },
                new() { UserId = "User1", Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = null! },
                new() { UserId = "User2", Period = 3, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = null! }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "user", descending: false);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal("Comment A", resultList[0].User.Comments);
            Assert.Equal("Comment B", resultList[1].User.Comments);
            Assert.Equal("Comment C", resultList[2].User.Comments);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_SortByIdDescending_ReturnsSortedData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummaryLogs>() };

            var logs = new List<RecreateSummaryLogs>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 3, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "id", descending: true);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.True(resultList[0].Id > resultList[1].Id);
            Assert.True(resultList[1].Id > resultList[2].Id);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithInvalidSortBy_DefaultsToDateDoneDescending()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = "CVLNT" + TestUserId, Comments = "Test Comment", Logs = new List<RecreateSummaryLogs>() };
            await context.Users.AddAsync(user);

            var logs = new List<RecreateSummaryLogs>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = new DateTime(2024, 1, 10), FpsYear = TestFpsYear, User = null! },
                new() { UserId = TestUserId, Period = 2, DateDone = new DateTime(2024, 3, 15), FpsYear = TestFpsYear, User = null! },
                new() { UserId = TestUserId, Period = 3, DateDone = new DateTime(2024, 2, 20), FpsYear = TestFpsYear, User = null! }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "invalidfield", descending: false);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal(new DateTime(2024, 3, 15), resultList[0].DateDone);
            Assert.Equal(new DateTime(2024, 2, 20), resultList[1].DateDone);
            Assert.Equal(new DateTime(2024, 1, 10), resultList[2].DateDone);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithNullSortBy_DefaultsToDateDoneDescending()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummaryLogs>() };

            var logs = new List<RecreateSummaryLogs>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = new DateTime(2024, 1, 10), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 2, DateDone = new DateTime(2024, 3, 15), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 3, DateDone = new DateTime(2024, 2, 20), FpsYear = TestFpsYear, User = user }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: null, descending: false);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal(new DateTime(2024, 3, 15), resultList[0].DateDone);
            Assert.Equal(new DateTime(2024, 2, 20), resultList[1].DateDone);
            Assert.Equal(new DateTime(2024, 1, 10), resultList[2].DateDone);
        }

        #endregion

        #region GetRecreateSummariesLogsAsync - Pagination Metadata

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_ReturnsPaginationMetadata_WithCorrectValues()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummaryLogs>() };

            var logs = Enumerable.Range(1, 42)
                .Select(i => new RecreateSummaryLogs
                {
                    UserId = TestUserId,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = TestFpsYear,
                    User = user
                }).ToList();

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 3, pageSize: 15);

            // Act
            var result = await repository.GetRecreateSummariesLogsAsync(parameters);

            // Assert
            Assert.NotNull(result.PaginationData);
            Assert.Equal(3, result.PaginationData.PageNumber);
            Assert.Equal(15, result.PaginationData.PageSize);
            Assert.Equal(42, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        #endregion
    }
}
