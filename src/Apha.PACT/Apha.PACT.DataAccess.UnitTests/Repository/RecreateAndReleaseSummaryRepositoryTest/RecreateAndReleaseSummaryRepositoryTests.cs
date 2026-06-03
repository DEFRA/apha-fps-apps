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

        #region GetRecreateSummariesAllLogsAsync - Basic Retrieval

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_WithExistingLogs_ReturnsAllLogsOrderedByDateDoneDescending()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var user = new TblUser { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummariesLog>() };

            var logs = new List<RecreateSummariesLog>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = DateTime.UtcNow.AddDays(-2), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 2, DateDone = DateTime.UtcNow.AddDays(-1), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 3, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user }
            };

            await context.RecreateSummariesLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.True(resultList[0].DateDone >= resultList[1].DateDone);
            Assert.True(resultList[1].DateDone >= resultList[2].DateDone);
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_WithNoLogs_ReturnsEmptyCollection()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
            Assert.Equal(0, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_IncludesUserNavigation_ReturnsLogsWithUserData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var user = new TblUser { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummariesLog>() };

            var log = new RecreateSummariesLog
            { 
                UserId = TestUserId, 
                Period = TestPeriod, 
                DateDone = DateTime.UtcNow, 
                FpsYear = TestFpsYear,
                User = user
            };

            await context.RecreateSummariesLogs.AddAsync(log);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);
            var firstLog = result.Data.FirstOrDefault();

            // Assert
            Assert.NotNull(firstLog);
            Assert.NotNull(firstLog.User);
            Assert.Equal(TestUserName, firstLog.User.UserName);
        }

        #endregion

        #region GetRecreateSummariesAllLogsAsync - Pagination

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new TblUser { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummariesLog>() };

            var logs = Enumerable.Range(1, 25)
                .Select(i => new RecreateSummariesLog
                {
                    UserId = TestUserId,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = TestFpsYear,
                    User = user
                }).ToList();

            await context.RecreateSummariesLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 2, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Data.Count);
            Assert.Equal(25, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.PaginationData.TotalPages);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_WithPaginationLastPage_ReturnsRemainingRecords()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new TblUser { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummariesLog>() };

            var logs = Enumerable.Range(1, 25)
                .Select(i => new RecreateSummariesLog
                {
                    UserId = TestUserId,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = TestFpsYear,
                    User = user
                }).ToList();

            await context.RecreateSummariesLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 3, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Data.Count);
            Assert.Equal(25, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_WithPageBeyondLimit_ReturnsEmptyData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new TblUser { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummariesLog>() };

            var logs = Enumerable.Range(1, 5)
                .Select(i => new RecreateSummariesLog
                {
                    UserId = TestUserId,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = TestFpsYear,
                    User = user
                }).ToList();

            await context.RecreateSummariesLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 10, pageSize: 10);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.TotalPages);
        }

        #endregion

        #region GetRecreateSummariesAllLogsAsync - Sorting

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_SortByDateDoneAscending_ReturnsSortedData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new TblUser { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummariesLog>() };

            var logs = new List<RecreateSummariesLog>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = new DateTime(2024, 3, 15), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 2, DateDone = new DateTime(2024, 1, 10), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 3, DateDone = new DateTime(2024, 2, 20), FpsYear = TestFpsYear, User = user }
            };

            await context.RecreateSummariesLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "datedone", descending: false);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal(new DateTime(2024, 1, 10), resultList[0].DateDone);
            Assert.Equal(new DateTime(2024, 2, 20), resultList[1].DateDone);
            Assert.Equal(new DateTime(2024, 3, 15), resultList[2].DateDone);
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_SortByPeriodDescending_ReturnsSortedData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new TblUser { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummariesLog>() };

            var logs = new List<RecreateSummariesLog>
            {
                new() { UserId = TestUserId, Period = 5, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 8, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user }
            };

            await context.RecreateSummariesLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "period", descending: true);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal((short)8, resultList[0].Period);
            Assert.Equal((short)5, resultList[1].Period);
            Assert.Equal((short)2, resultList[2].Period);
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_SortByUserIdAscending_ReturnsSortedData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            // UserId is mapped to TblUser.UserName, so UserName must match UserId
            var user1 = new TblUser { UserName = "UserC", Comments = "Comment C", Logs = new List<RecreateSummariesLog>() };
            var user2 = new TblUser { UserName = "UserA", Comments = "Comment A", Logs = new List<RecreateSummariesLog>() };
            var user3 = new TblUser { UserName = "UserB", Comments = "Comment B", Logs = new List<RecreateSummariesLog>() };

            var logs = new List<RecreateSummariesLog>
            {
                new() { UserId = "UserC", Period = 1, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user1 },
                new() { UserId = "UserA", Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user2 },
                new() { UserId = "UserB", Period = 3, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user3 }
            };

            await context.RecreateSummariesLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "userid", descending: false);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal("UserA", resultList[0].UserId);
            Assert.Equal("UserB", resultList[1].UserId);
            Assert.Equal("UserC", resultList[2].UserId);
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_SortByUserNameAscending_ReturnsSortedData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var userA = new TblUser { UserName = "Charlie", Comments = "Comment C", Logs = new List<RecreateSummariesLog>() };
            var userB = new TblUser { UserName = "Alice", Comments = "Comment A", Logs = new List<RecreateSummariesLog>() };
            var userC = new TblUser { UserName = "Bob", Comments = "Comment B", Logs = new List<RecreateSummariesLog>() };

            var logs = new List<RecreateSummariesLog>
            {
                new() { UserId = "User3", Period = 1, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = userA },
                new() { UserId = "User1", Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = userB },
                new() { UserId = "User2", Period = 3, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = userC }
            };

            await context.RecreateSummariesLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "user", descending: false);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal("Alice", resultList[0].User.UserName);
            Assert.Equal("Bob", resultList[1].User.UserName);
            Assert.Equal("Charlie", resultList[2].User.UserName);
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_SortByIdDescending_ReturnsSortedData()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new TblUser { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummariesLog>() };

            var logs = new List<RecreateSummariesLog>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 2, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 3, DateDone = DateTime.UtcNow, FpsYear = TestFpsYear, User = user }
            };

            await context.RecreateSummariesLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "id", descending: true);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.True(resultList[0].Id > resultList[1].Id);
            Assert.True(resultList[1].Id > resultList[2].Id);
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_WithInvalidSortBy_DefaultsToDateDoneDescending()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new TblUser { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummariesLog>() };

            var logs = new List<RecreateSummariesLog>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = new DateTime(2024, 1, 10), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 2, DateDone = new DateTime(2024, 3, 15), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 3, DateDone = new DateTime(2024, 2, 20), FpsYear = TestFpsYear, User = user }
            };

            await context.RecreateSummariesLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "invalidfield", descending: false);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal(new DateTime(2024, 3, 15), resultList[0].DateDone);
            Assert.Equal(new DateTime(2024, 2, 20), resultList[1].DateDone);
            Assert.Equal(new DateTime(2024, 1, 10), resultList[2].DateDone);
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_WithNullSortBy_DefaultsToDateDoneDescending()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new TblUser { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummariesLog>() };

            var logs = new List<RecreateSummariesLog>
            {
                new() { UserId = TestUserId, Period = 1, DateDone = new DateTime(2024, 1, 10), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 2, DateDone = new DateTime(2024, 3, 15), FpsYear = TestFpsYear, User = user },
                new() { UserId = TestUserId, Period = 3, DateDone = new DateTime(2024, 2, 20), FpsYear = TestFpsYear, User = user }
            };

            await context.RecreateSummariesLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: null, descending: false);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);
            var resultList = result.Data.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal(new DateTime(2024, 3, 15), resultList[0].DateDone);
            Assert.Equal(new DateTime(2024, 2, 20), resultList[1].DateDone);
            Assert.Equal(new DateTime(2024, 1, 10), resultList[2].DateDone);
        }

        #endregion

        #region GetRecreateSummariesAllLogsAsync - Pagination Metadata

        [Fact]
        public async Task GetRecreateSummariesAllLogsAsync_ReturnsPaginationMetadata_WithCorrectValues()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var user = new TblUser { UserName = TestUserName, Comments = "Test Comment", Logs = new List<RecreateSummariesLog>() };

            var logs = Enumerable.Range(1, 42)
                .Select(i => new RecreateSummariesLog
                {
                    UserId = TestUserId,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = TestFpsYear,
                    User = user
                }).ToList();

            await context.RecreateSummariesLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);
            var parameters = new PaginationParameters<string>(page: 3, pageSize: 15);

            // Act
            var result = await repository.GetRecreateSummariesAllLogsAsync(parameters);

            // Assert
            Assert.NotNull(result.PaginationData);
            Assert.Equal(3, result.PaginationData.PageNumber);
            Assert.Equal(15, result.PaginationData.PageSize);
            Assert.Equal(42, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.PaginationData.TotalPages);
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
            Assert.Equal(3, result.Count);
            Assert.Equal("Period1", result[0].PeriodName);
            Assert.Equal("Period2", result[1].PeriodName);
            Assert.Equal("Period3", result[2].PeriodName);
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
            Assert.Empty(result);
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
            Assert.Equal(2, result.Count);
            Assert.Null(result[0].EndPeriod);
            Assert.Equal(2.0, result[1].EndPeriod);
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
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(TestFpsYear, p.FpsYear));
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
            Assert.IsAssignableFrom<IReadOnlyList<ReleasePeriod>>(result);
        }

        #endregion

        #region SetFinalSummaryRunAsync

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithExistingPeriod_UpdatesFinalSummariesRunAndReturnsPeriod()
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
            var result = await repository.SetFinalSummaryRunAsync("TestPeriod", 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestPeriod", result.PeriodName);
            Assert.Equal((short)1, result.FinalSummariesRun);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithExistingPeriod_PersistsUpdatedValueToDatabase()
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
            const short newFinalSummariesRun = 3;

            // Act
            await repository.SetFinalSummaryRunAsync("PersistPeriod", newFinalSummariesRun);

            // Assert — clear tracker and reload to confirm SaveChangesAsync was called
            context.ChangeTracker.Clear();
            var reloaded = await context.ReleasePeriods.FindAsync("PersistPeriod", TestFpsYear);
            Assert.NotNull(reloaded);
            Assert.Equal(newFinalSummariesRun, reloaded.FinalSummariesRun);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithNonExistingPeriod_ReturnsNull()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());
            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act
            var result = await repository.SetFinalSummaryRunAsync("NonExistentPeriod", 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithNonExistingPeriod_DoesNotSaveChangesToDatabase()
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
            var result = await repository.SetFinalSummaryRunAsync("NonExistentPeriod", 99);

            // Assert — existing period must remain unchanged
            Assert.Null(result);

            context.ChangeTracker.Clear();
            var unchanged = await context.ReleasePeriods.FindAsync("ExistingPeriod", TestFpsYear);
            Assert.NotNull(unchanged);
            Assert.Equal((short)5, unchanged.FinalSummariesRun);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithPeriodFromDifferentFpsYear_ReturnsNull()
        {
            // Arrange
            await using var context = CreateTestContext(Guid.NewGuid().ToString());

            var period = new ReleasePeriod
            {
                PeriodName = "TestPeriod",
                FpsYear = TestFpsYear + 1,
                FinalSummariesRun = 0,
                EndPeriod = 1.0
            };

            await context.ReleasePeriods.AddAsync(period);
            await context.SaveChangesAsync();

            var repository = new RecreateAndReleaseSummaryRepository(context);

            // Act — context is configured for TestFpsYear, so FindAsync(periodName, TestFpsYear) will not find a period saved under TestFpsYear + 1
            var result = await repository.SetFinalSummaryRunAsync("TestPeriod", 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithExistingPeriod_UpdatesOnlyFinalSummariesRunField()
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
            var result = await repository.SetFinalSummaryRunAsync("FieldCheckPeriod", 2);

            // Assert — only FinalSummariesRun must change; all other fields remain intact
            Assert.NotNull(result);
            Assert.Equal((short)2, result.FinalSummariesRun);
            Assert.Equal(1.5, result.StartPeriod);
            Assert.Equal(2.5, result.EndPeriod);
            Assert.Equal("Month", result.PeriodType);
            Assert.Equal((short)0, result.PeriodLocked);
        }

        #endregion
    }
}
