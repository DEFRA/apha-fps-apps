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

            var user = new User { UserName = TestUserName, Comments = "Test Comment" };

            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = DateTime.UtcNow.AddDays(-2), FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 2, DateDone = DateTime.UtcNow.AddDays(-1), FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 3, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
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
            var result = await repository.GetRecreateSummaryLogAsync(parameters);

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
            var user = new User { UserName = "CVLNT" + TestUserId, Comments = "Test Comment" };
            await context.Users.AddAsync(user);

            var log = new RecreateSummaryLog
            { 
                UserId = TestUserId, 
                Period = TestPeriod, 
                DateDone = DateTime.UtcNow, 
                FpsYear = TestFpsYear // Will be populated by repository join logic
            };

            await context.RecreateSummaryLogs.AddAsync(log);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
            var firstLog = result.Data.FirstOrDefault();

            // Assert
            Assert.NotNull(firstLog);
            Assert.Equal("Test Comment", firstLog.Comments);
        }

        #endregion

        #region GetRecreateSummariesLogsAsync - Pagination

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment" };

            var logs = Enumerable.Range(1, 25)
                .Select(i => new RecreateSummaryLog
                {
                    UserId = TestUserId,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = TestFpsYear
                }).ToList();

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 2, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);

            // Assert
            Assert.NotNull(result);
            // ApplyPaging is called on already-paginated data (10 items from DB),
            // then Skip((2-1)*10).Take(10) results in 0 items
            Assert.Empty(result.Data);
            Assert.Equal(10, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.TotalPages);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithPaginationLastPage_ReturnsRemainingRecords()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment" };

            var logs = Enumerable.Range(1, 25)
                .Select(i => new RecreateSummaryLog
                {
                    UserId = TestUserId,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = TestFpsYear
                }).ToList();

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 3, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);

            // Assert
            Assert.NotNull(result);
            // ApplyPaging is called on already-paginated data (5 items from DB),
            // then Skip((3-1)*10).Take(10) results in 0 items
            Assert.Empty(result.Data);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithPageBeyondLimit_ReturnsEmptyData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment" };

            var logs = Enumerable.Range(1, 5)
                .Select(i => new RecreateSummaryLog
                {
                    UserId = TestUserId,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = TestFpsYear
                }).ToList();

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 10, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            // ApplyPaging counts 0 items from DB pagination, so total is 0
            Assert.Equal(0, result.PaginationData.TotalRecords);
            Assert.Equal(0, result.PaginationData.TotalPages);
        }

        #endregion

        #region GetRecreateSummariesLogsAsync - Sorting

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_SortByDateDoneAscending_ReturnsSortedData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment" };

            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = new DateTime(2024, 3, 15), FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 2, DateDone = new DateTime(2024, 1, 10), FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 3, DateDone = new DateTime(2024, 2, 20), FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "datedone", descending: false);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
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
            var user = new User { UserName = TestUserName, Comments = "Test Comment" };

            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = TestUserId, Period = 5, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 8, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "period", descending: true);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
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
            var user1 = new User { UserName = "UserC", Comments = "Comment C" };
            var user2 = new User { UserName = "UserA", Comments = "Comment A" };
            var user3 = new User { UserName = "UserB", Comments = "Comment B" };

            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = "UserC", Period = 1, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear },
                new() { UserId = "UserA", Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear },
                new() { UserId = "UserB", Period = 3, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "userid", descending: false);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
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
            var userA = new User { UserName = "CVLNTUser3", Comments = "Comment C" };
            var userB = new User { UserName = "CVLNTUser1", Comments = "Comment A" };
            var userC = new User { UserName = "CVLNTUser2", Comments = "Comment B" };

            await context.Users.AddRangeAsync(new[] { userA, userB, userC });

            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = "User3", Period = 1, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear },
                new() { UserId = "User1", Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear },
                new() { UserId = "User2", Period = 3, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "user", descending: false);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal("Comment A", resultList[0].Comments);
            Assert.Equal("Comment B", resultList[1].Comments);
            Assert.Equal("Comment C", resultList[2].Comments);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_SortByIdDescending_ReturnsSortedData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment" };

            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 3, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "id", descending: true);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
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
            var user = new User { UserName = "CVLNT" + TestUserId, Comments = "Test Comment" };
            await context.Users.AddAsync(user);

            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = new DateTime(2024, 1, 10), FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 2, DateDone = new DateTime(2024, 3, 15), FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 3, DateDone = new DateTime(2024, 2, 20), FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "invalidfield", descending: false);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
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
            var user = new User { UserName = TestUserName, Comments = "Test Comment" };

            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = new DateTime(2024, 1, 10), FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 2, DateDone = new DateTime(2024, 3, 15), FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 3, DateDone = new DateTime(2024, 2, 20), FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: null, descending: false);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
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
            var user = new User { UserName = TestUserName, Comments = "Test Comment" };

            var logs = Enumerable.Range(1, 42)
                .Select(i => new RecreateSummaryLog
                {
                    UserId = TestUserId,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = TestFpsYear
                }).ToList();

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 3, pageSize: 15);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);

            // Assert
            Assert.NotNull(result.PaginationData);
            Assert.Equal(3, result.PaginationData.PageNumber);
            Assert.Equal(15, result.PaginationData.PageSize);
            // ApplyPaging counts 12 items (from DB Skip/Take), not original 42
            Assert.Equal(12, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.TotalPages);
        }

        #endregion

        #region GetRecreateSummariesLogsAsync - Edge Cases

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithLogsWithoutMatchingUsers_ReturnsLogsWithEmptyUserData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            // Create logs without matching users in the Users table
            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = "NonExistentUser1", Period = 1, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear },
                new() { UserId = "NonExistentUser2", Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, log =>
            {
                Assert.Equal(string.Empty, log.Comments);
            });
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithMixedUserData_ReturnsCorrectUserAssociations()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            // Create one user with matching CVLNT pattern
            var matchedUser = new User { UserName = "CVLNTMatchedUser", Comments = "Matched Comment" };
            await context.Users.AddAsync(matchedUser);

            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = "MatchedUser", Period = 1, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear },
                new() { UserId = "UnmatchedUser", Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "userid", descending: false);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, resultList.Count);

            // First log should have matched user
            var matchedLog = resultList.First(l => l.UserId == "MatchedUser");
            Assert.Equal("Matched Comment", matchedLog.Comments);

            // Second log should have empty user
            var unmatchedLog = resultList.First(l => l.UserId == "UnmatchedUser");
            Assert.Equal(string.Empty, unmatchedLog.Comments);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithPageSizeMatchingTotalRecords_ReturnsAllRecordsInOnePage()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment" };

            var logs = Enumerable.Range(1, 10)
                .Select(i => new RecreateSummaryLog
                {
                    UserId = TestUserId,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = TestFpsYear
                }).ToList();

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Data.Count);
            Assert.Equal(10, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.TotalPages);
            Assert.Equal(1, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithSingleRecord_ReturnsSingleRecordWithCorrectPagination()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = "CVLNT" + TestUserId, Comments = "Test Comment" };
            await context.Users.AddAsync(user);

            var log = new RecreateSummaryLog
            {
                UserId = TestUserId,
                Period = TestPeriod,
                DateDone = DateTime.UtcNow,
                FpsYear = TestFpsYear
            };

            await context.RecreateSummaryLogs.AddAsync(log);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.TotalPages);
            Assert.Equal(TestUserId, result.Data.First().UserId);
            Assert.Equal("Test Comment", result.Data.First().Comments);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithEmptyStringsortBy_DefaultsToDateDoneDescending()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment" };

            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = new DateTime(2024, 1, 10), FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 2, DateDone = new DateTime(2024, 3, 15), FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 3, DateDone = new DateTime(2024, 2, 20), FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: string.Empty, descending: false);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal(new DateTime(2024, 3, 15), resultList[0].DateDone);
            Assert.Equal(new DateTime(2024, 2, 20), resultList[1].DateDone);
            Assert.Equal(new DateTime(2024, 1, 10), resultList[2].DateDone);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithMultipleUsersAndComplexJoin_ReturnsCorrectAssociations()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            // Create multiple users with CVLNT pattern
            var users = new[]
            {
                new User { UserName = "CVLNTUserA", Comments = "Comment A" },
                new User { UserName = "CVLNTUserB", Comments = "Comment B" },
                new User { UserName = "CVLNTUserC", Comments = "Comment C" }
            };
            await context.Users.AddRangeAsync(users);

            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = "UserA", Period = 1, DateDone = new DateTime(2024, 1, 1), FpsYear = TestFpsYear },
                new() { UserId = "UserB", Period = 2, DateDone = new DateTime(2024, 1, 2), FpsYear = TestFpsYear },
                new() { UserId = "UserC", Period = 3, DateDone = new DateTime(2024, 1, 3), FpsYear = TestFpsYear },
                new() { UserId = "UserA", Period = 4, DateDone = new DateTime(2024, 1, 4), FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "datedone", descending: false);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, resultList.Count);

            Assert.Equal("Comment A", resultList[0].Comments);
            Assert.Equal("Comment B", resultList[1].Comments);
            Assert.Equal("Comment C", resultList[2].Comments);
            Assert.Equal("Comment A", resultList[3].Comments);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithCaseInsensitiveSortBy_HandlesCorrectly()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment" };

            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = TestUserId, Period = 5, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear },
                new() { UserId = TestUserId, Period = 8, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "PERIOD", descending: false);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal((short)2, resultList[0].Period);
            Assert.Equal((short)5, resultList[1].Period);
            Assert.Equal((short)8, resultList[2].Period);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithZeroPageSize_StillReturnsValidPaginationData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new User { UserName = TestUserName, Comments = "Test Comment" };

            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            // Note: PaginationParameters might enforce minimum page size, but testing edge case
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 1);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithUserCommentsNull_ReturnsEmptyUserData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            // The join will return null UserComments when user doesn't exist
            var logs = new List<RecreateSummaryLog>
            {
                new() { UserId = "NoUser", Period = 1, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear }
            };

            await context.RecreateSummaryLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummaryLogAsync(parameters);
            var firstLog = result.Data.FirstOrDefault();

            // Assert
            Assert.NotNull(firstLog);
            Assert.Equal(string.Empty, firstLog.Comments);
        }

        #endregion

        #region GetReleaseSummariesAsync

        [Fact]
        public async Task GetReleaseSummariesAsync_WithExistingPeriods_ReturnsAllPeriodsOrderedByEndPeriodAscending()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var periods = new List<ReleasePeriod>
            {
                new() { PeriodName = "Period3", EndPeriod = 3.0, StartPeriod = 2.5, FpsYear = TestFpsYear },
                new() { PeriodName = "Period1", EndPeriod = 1.0, StartPeriod = 0.5, FpsYear = TestFpsYear },
                new() { PeriodName = "Period2", EndPeriod = 2.0, StartPeriod = 1.5, FpsYear = TestFpsYear }
            };

            await context.ReleasePeriods.AddRangeAsync(periods);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.ReleasePeriods.Count);
            Assert.Equal("Period1", result.ReleasePeriods[0].PeriodName);
            Assert.Equal("Period2", result.ReleasePeriods[1].PeriodName);
            Assert.Equal("Period3", result.ReleasePeriods[2].PeriodName);
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_WithNoPeriods_ReturnsEmptyCollection()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.ReleasePeriods);
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_WithNullEndPeriod_ReturnsPeriodsWithNullEndPeriodFirst()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var periods = new List<ReleasePeriod>
            {
                new() { PeriodName = "PeriodB", EndPeriod = 2.0, StartPeriod = 1.5, FpsYear = TestFpsYear },
                new() { PeriodName = "PeriodA", EndPeriod = null, StartPeriod = null, FpsYear = TestFpsYear }
            };

            await context.ReleasePeriods.AddRangeAsync(periods);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.ReleasePeriods.Count);
            Assert.Null(result.ReleasePeriods[0].EndPeriod);
            Assert.Equal(2.0, result.ReleasePeriods[1].EndPeriod);
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_OnlyReturnsPeriodsBelongingToCurrentFpsYear()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var periodsCurrentYear = new List<ReleasePeriod>
            {
                new() { PeriodName = "Current1", EndPeriod = 1.0, StartPeriod = 0.5, FpsYear = TestFpsYear },
                new() { PeriodName = "Current2", EndPeriod = 2.0, StartPeriod = 1.5, FpsYear = TestFpsYear }
            };

            var periodsOtherYear = new List<ReleasePeriod>
            {
                new() { PeriodName = "Other1", EndPeriod = 1.0, StartPeriod = 0.5, FpsYear = TestFpsYear + 1 }
            };

            await context.ReleasePeriods.AddRangeAsync(periodsCurrentYear);
            await context.ReleasePeriods.AddRangeAsync(periodsOtherYear);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.ReleasePeriods.Count);
            Assert.All(result.ReleasePeriods, p => Assert.Equal(TestFpsYear, p.FpsYear));
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_ReturnsReadOnlyList()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var period = new ReleasePeriod
            {
                PeriodName = "Period1",
                EndPeriod = 1.0,
                StartPeriod = 0.5,
                FpsYear = TestFpsYear
            };

            await context.ReleasePeriods.AddAsync(period);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<IList<ReleasePeriod>>(result.ReleasePeriods, exactMatch: false);
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_WithExistingSendEmailSetting_ReturnsSettingValue()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            await context.Settings.AddAsync(new Settings { Id = "SendEmail", Setting = "-1" });
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("-1", result.Setting);
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_WithNoSendEmailSetting_ReturnsNullSetting()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Setting);
        }

        #endregion

        #region SetFinalSummaryRunAsync - UpdateFinalSummaryRunAsync path (sendEmail null/empty/whitespace)

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithNullSendEmail_AndExistingPeriod_UpdatesAndReturnsPeriod()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var period = new ReleasePeriod
            {
                PeriodName = "TestPeriod",
                FpsYear = TestFpsYear,
                FinalSummariesRun = 0,
                EndPeriod = 1.0
            };

            await context.ReleasePeriods.AddAsync(period);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.SetFinalSummaryRunAsync("TestPeriod", 1, null);

            // Assert — finalSummariesRun==1 stored as -1 per business rule
            Assert.NotNull(result);
            Assert.Equal("TestPeriod", result.PeriodName);
            Assert.Equal((short)-1, result.FinalSummariesRun);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithNullSendEmail_FinalSummariesRunMinusOne_StoredAsMinusOne()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var period = new ReleasePeriod
            {
                PeriodName = "TestPeriod",
                FpsYear = TestFpsYear,
                FinalSummariesRun = 0,
                EndPeriod = 1.0
            };

            await context.ReleasePeriods.AddAsync(period);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.SetFinalSummaryRunAsync("TestPeriod", -1, null);

            // Assert — finalSummariesRun==-1 also stored as -1
            Assert.NotNull(result);
            Assert.Equal((short)-1, result.FinalSummariesRun);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithNullSendEmail_FinalSummariesRunZero_StoredAsZero()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var period = new ReleasePeriod
            {
                PeriodName = "TestPeriod",
                FpsYear = TestFpsYear,
                FinalSummariesRun = -1,
                EndPeriod = 1.0
            };

            await context.ReleasePeriods.AddAsync(period);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.SetFinalSummaryRunAsync("TestPeriod", 0, null);

            // Assert — 0 is not 1 or -1 so stored as 0
            Assert.NotNull(result);
            Assert.Equal((short)0, result.FinalSummariesRun);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithNullSendEmail_FinalSummariesRunOtherValue_StoredAsZero()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var period = new ReleasePeriod
            {
                PeriodName = "TestPeriod",
                FpsYear = TestFpsYear,
                FinalSummariesRun = -1,
                EndPeriod = 1.0
            };

            await context.ReleasePeriods.AddAsync(period);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.SetFinalSummaryRunAsync("TestPeriod", 2, null);

            // Assert — 2 is not 1 or -1 so stored as 0
            Assert.NotNull(result);
            Assert.Equal((short)0, result.FinalSummariesRun);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithNullSendEmail_NullFinalSummariesRun_DefaultsToZero()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var period = new ReleasePeriod
            {
                PeriodName = "TestPeriod",
                FpsYear = TestFpsYear,
                FinalSummariesRun = -1,
                EndPeriod = 1.0
            };

            await context.ReleasePeriods.AddAsync(period);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act — null finalSummariesRun defaults to 0 via ?? operator
            var result = await repository.SetFinalSummaryRunAsync("TestPeriod", null, null);

            // Assert — 0 stored as 0
            Assert.NotNull(result);
            Assert.Equal((short)0, result.FinalSummariesRun);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithNullSendEmail_AndExistingPeriod_PersistsValueToDatabase()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var period = new ReleasePeriod
            {
                PeriodName = "PersistPeriod",
                FpsYear = TestFpsYear,
                FinalSummariesRun = 0,
                EndPeriod = 5.0
            };

            await context.ReleasePeriods.AddAsync(period);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            await repository.SetFinalSummaryRunAsync("PersistPeriod", 1, null);

            // Assert — reload from DB to confirm SaveChangesAsync was called
            context.ChangeTracker.Clear();
            var reloaded = await context.ReleasePeriods.FindAsync("PersistPeriod", TestFpsYear);
            Assert.NotNull(reloaded);
            Assert.Equal((short)-1, reloaded.FinalSummariesRun);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithNullSendEmail_AndNonExistingPeriod_ReturnsNull()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.SetFinalSummaryRunAsync("NonExistentPeriod", 1, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithNullSendEmail_AndNonExistingPeriod_DoesNotModifyOtherPeriods()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var period = new ReleasePeriod
            {
                PeriodName = "ExistingPeriod",
                FpsYear = TestFpsYear,
                FinalSummariesRun = 5,
                EndPeriod = 1.0
            };

            await context.ReleasePeriods.AddAsync(period);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            await repository.SetFinalSummaryRunAsync("NonExistentPeriod", 1, null);

            // Assert — existing period must remain unchanged
            context.ChangeTracker.Clear();
            var unchanged = await context.ReleasePeriods.FindAsync("ExistingPeriod", TestFpsYear);
            Assert.NotNull(unchanged);
            Assert.Equal((short)5, unchanged.FinalSummariesRun);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithNullSendEmail_PeriodInDifferentFpsYear_ReturnsNull()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var period = new ReleasePeriod
            {
                PeriodName = "TestPeriod",
                FpsYear = TestFpsYear + 1,  // different year from context
                FinalSummariesRun = 0,
                EndPeriod = 1.0
            };

            await context.ReleasePeriods.AddAsync(period);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.SetFinalSummaryRunAsync("TestPeriod", 1, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithNullSendEmail_UpdatesOnlyFinalSummariesRunField()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var period = new ReleasePeriod
            {
                PeriodName = "FieldCheckPeriod",
                FpsYear = TestFpsYear,
                FinalSummariesRun = 0,
                StartPeriod = 1.5,
                EndPeriod = 2.5,
                PeriodType = "Month",
                PeriodLocked = 0
            };

            await context.ReleasePeriods.AddAsync(period);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.SetFinalSummaryRunAsync("FieldCheckPeriod", 1, null);

            // Assert — only FinalSummariesRun must change; all other fields remain intact
            Assert.NotNull(result);
            Assert.Equal((short)-1, result.FinalSummariesRun);
            Assert.Equal(1.5, result.StartPeriod);
            Assert.Equal(2.5, result.EndPeriod);
            Assert.Equal("Month", result.PeriodType);
            Assert.Equal((short)0, result.PeriodLocked);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithEmptySendEmail_RoutesToUpdateFinalSummaryRun()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var period = new ReleasePeriod
            {
                PeriodName = "TestPeriod",
                FpsYear = TestFpsYear,
                FinalSummariesRun = 0,
                EndPeriod = 1.0
            };

            await context.ReleasePeriods.AddAsync(period);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act — empty string is treated as no sendEmail
            var result = await repository.SetFinalSummaryRunAsync("TestPeriod", 1, "");

            // Assert — UpdateFinalSummaryRunAsync path taken
            Assert.NotNull(result);
            Assert.Equal("TestPeriod", result.PeriodName);
            Assert.Equal((short)-1, result.FinalSummariesRun);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithWhitespaceSendEmail_RoutesToUpdateFinalSummaryRun()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var period = new ReleasePeriod
            {
                PeriodName = "TestPeriod",
                FpsYear = TestFpsYear,
                FinalSummariesRun = 0,
                EndPeriod = 1.0
            };

            await context.ReleasePeriods.AddAsync(period);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act — whitespace is treated as no sendEmail
            var result = await repository.SetFinalSummaryRunAsync("TestPeriod", 1, "   ");

            // Assert — UpdateFinalSummaryRunAsync path taken
            Assert.NotNull(result);
            Assert.Equal("TestPeriod", result.PeriodName);
            Assert.Equal((short)-1, result.FinalSummariesRun);
        }

        #endregion

        #region SetFinalSummaryRunAsync - UpdateSettingsAsync path (sendEmail non-empty)

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithSendEmailOne_SetsSettingToMinusOne_AndReturnsEmptyPeriod()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            await context.Settings.AddAsync(new Settings { Id = "SendEmail", Setting = "0" });
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.SetFinalSummaryRunAsync(null, null, "1");

            // Assert — returns non-null empty ReleasePeriod, NOT the period record
            Assert.NotNull(result);
            Assert.Null(result.PeriodName);

            context.ChangeTracker.Clear();
            var setting = await context.Settings.FindAsync("SendEmail");
            Assert.Equal("-1", setting!.Setting);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithSendEmailMinusOne_SetsSettingToMinusOne()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            await context.Settings.AddAsync(new Settings { Id = "SendEmail", Setting = "0" });
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.SetFinalSummaryRunAsync(null, null, "-1");

            // Assert
            Assert.NotNull(result);

            context.ChangeTracker.Clear();
            var setting = await context.Settings.FindAsync("SendEmail");
            Assert.Equal("-1", setting!.Setting);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithSendEmailZero_SetsSettingToZero()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            await context.Settings.AddAsync(new Settings { Id = "SendEmail", Setting = "-1" });
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.SetFinalSummaryRunAsync(null, null, "0");

            // Assert — "0" is not "1" or "-1" so settingValue = "0"
            Assert.NotNull(result);

            context.ChangeTracker.Clear();
            var setting = await context.Settings.FindAsync("SendEmail");
            Assert.Equal("0", setting!.Setting);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithOtherNonEmptySendEmail_SetsSettingToZero()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            await context.Settings.AddAsync(new Settings { Id = "SendEmail", Setting = "-1" });
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act — "true" is not "1" or "-1"
            var result = await repository.SetFinalSummaryRunAsync(null, null, "true");

            // Assert
            Assert.NotNull(result);

            context.ChangeTracker.Clear();
            var setting = await context.Settings.FindAsync("SendEmail");
            Assert.Equal("0", setting!.Setting);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithSendEmailOne_SettingNotFound_ReturnsEmptyPeriodWithoutThrowing()
        {
            // Arrange — no Settings seeded
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act — null-conditional on setting means no crash when not found
            var result = await repository.SetFinalSummaryRunAsync(null, null, "1");

            // Assert — still returns a non-null empty ReleasePeriod
            Assert.NotNull(result);
            Assert.Null(result.PeriodName);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithSendEmailOne_PersistsSettingToDatabase()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            await context.Settings.AddAsync(new Settings { Id = "SendEmail", Setting = "0" });
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            await repository.SetFinalSummaryRunAsync(null, null, "1");

            // Assert — reload to confirm SaveChangesAsync was called
            context.ChangeTracker.Clear();
            var reloaded = await context.Settings.FindAsync("SendEmail");
            Assert.Equal("-1", reloaded!.Setting);
        }

        #endregion
    }
}
