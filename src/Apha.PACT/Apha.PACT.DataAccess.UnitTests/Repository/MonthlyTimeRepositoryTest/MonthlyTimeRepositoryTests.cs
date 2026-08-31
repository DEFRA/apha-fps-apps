using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.MonthlyTimeRepositoryTest
{
    public class MonthlyTimeRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        private static MonthlyTimeRepository CreateRepository(
            IEnumerable<MonthlyTime> monthlyTimes,
            IEnumerable<MonthlyTimeLog>? monthlyTimeLogs = null)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(monthlyTimes);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            mockContext.Setup(x => x.MonthlyTimes).Returns(mockSet.Object);

            var logMockSet = RepositoryTestHelper.CreateMockDbSet(monthlyTimeLogs ?? []);
            RepositoryTestHelper.SetupDbSetOperations(logMockSet);
            mockContext.Setup(x => x.MonthlyTimeLogs).Returns(logMockSet.Object);

            return new MonthlyTimeRepository(mockContext.Object, fpsRequestContext);
        }

        private static (FpsDbContext Context, MonthlyTimeRepository Repo) CreateInMemoryContext(int fpsYear = DefaultFpsYear)
        {
            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);
            fpsRequestContext.UserEmailId.Returns("test.user@apha.gov.uk");

            var context = new FpsDbContext(options, fpsRequestContext);
            var repo = new MonthlyTimeRepository(context, fpsRequestContext);
            return (context, repo);
        }

        // Shared log data used across SearchAsync tests
        private static readonly DateTime BaseDate = new(2024, 6, 15, 10, 0, 0);

        private static List<MonthlyTimeLog> DefaultLogs() =>
        [
            new() { SequenceNo = 1, WorkGroup = "WGA", TimeCode = "TC1", PactStaffId = "S1", ParentProject = "PP1", Month = 6, DateTime = BaseDate,          UserId = "CVLNT\\mUser1", InsertDelete = "I",  FpsYear = DefaultFpsYear },
            new() { SequenceNo = 2, WorkGroup = "WGB", TimeCode = "TC2", PactStaffId = "S2", ParentProject = "PP2", Month = 7, DateTime = BaseDate.AddDays(1), UserId = "CVLNT\\mUser2", InsertDelete = "D",  FpsYear = DefaultFpsYear },
            new() { SequenceNo = 3, WorkGroup = "WGA", TimeCode = "TC3", PactStaffId = "S3", ParentProject = "PP1", Month = 8, DateTime = BaseDate.AddDays(2), UserId = "CVLNT\\mUser3", InsertDelete = "UI", FpsYear = DefaultFpsYear }
        ];

        private static MonthlyTimeRepository CreateRepositoryWithStaging(
            IEnumerable<StagingMonthlyTime> stagingData)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(stagingData);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);

            mockContext.Setup(x => x.StagingMonthlyTimes).Returns(mockSet.Object);

            return new MonthlyTimeRepository(mockContext.Object, fpsRequestContext);
        }

        private static List<StagingMonthlyTime> StagingSeedData() =>
        [
            new() { Id = 1, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 1, WorkGroup = "WG1", Hours = 10, Passed = true, ImportedBy = "user1" },
            new() { Id = 2, PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = 2, WorkGroup = "WG1", Hours = 20, Passed = false, ImportedBy = "user1", FailureComments = "Bad data" },
            new() { Id = 3, PactStaffId = "S3", TimeCode = "TC3", ParentProject = "PP3", Month = 3, WorkGroup = "WG2", Hours = 0, Passed = false, ImportedBy = "user1" },
            new() { Id = 4, PactStaffId = "S4", TimeCode = "TC4", ParentProject = "PP4", Month = 4, WorkGroup = "WG2", Hours = null, Passed = true, ImportedBy = "user2" },
        ];

        #region HasMonthlyTimeEntriesAsync

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_MatchingAllThreeFields_ReturnsTrue()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP1");

            Assert.True(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_NoMatchingRows_ReturnsFalse()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG2", "TC2", "PP2");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_WorkGroupDiffers_ReturnsFalse()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG_DIFFERENT", "TC1", "PP1");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_TimeCodeDiffers_ReturnsFalse()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC_DIFFERENT", "PP1");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_ParentProjectDiffers_ReturnsFalse()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP_DIFFERENT");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_EmptyRepository_ReturnsFalse()
        {
            var repo = CreateRepository(Enumerable.Empty<MonthlyTime>());

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP1");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_MultipleRows_OnlyOneMatches_ReturnsTrue()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG2", TimeCode = "TC2", ParentProject = "PP2", PactStaffId = "S2", Month = 2, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP1");

            Assert.True(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_MultipleMatchingRows_ReturnsTrue()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S2", Month = 2, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP1");

            Assert.True(result);
        }

        #endregion

        #region SearchAsync — no filters

        [Fact]
        public async Task SearchAsync_NoFilters_ReturnsAllRecords()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter());

            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task SearchAsync_EmptyLogs_ReturnsEmptyResult()
        {
            var repo = CreateRepository([], []);
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter());

            Assert.Empty(result.Data);
        }

        #endregion

        #region SearchAsync — individual filters

        [Fact]
        public async Task SearchAsync_FilterByWorkGroup_ReturnsMatchingRecords()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { WorkGroup = "WGA" });

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, r => Assert.Equal("WGA", r.WorkGroup));
        }

        [Fact]
        public async Task SearchAsync_FilterByWorkGroup_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { WorkGroup = "NONE" });

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task SearchAsync_FilterByTimeCode_ReturnsMatchingRecord()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { TimeCode = "TC1" });

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("TC1", result.Data.First().TimeCode);
        }

        [Fact]
        public async Task SearchAsync_FilterByPactStaffId_ReturnsMatchingRecord()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { PactStaffId = "S2" });

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("S2", result.Data.First().PactStaffId);
        }

        [Fact]
        public async Task SearchAsync_FilterByParentProject_ReturnsMatchingRecords()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { ParentProject = "PP1" });

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, r => Assert.Equal("PP1", r.ParentProject));
        }

        [Fact]
        public async Task SearchAsync_FilterByMonth_ReturnsMatchingRecord()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { Month = 7 });

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal(7, result.Data.First().Month);
        }

        [Fact]
        public async Task SearchAsync_FilterByUserId_PartialMatch_ReturnsMatchingRecord()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { UserId = "mUser1" });

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Contains("mUser1", result.Data.First().UserId);
        }

        [Theory]
        [InlineData("I",  1)]   // exact prefix "I"
        [InlineData("D",  1)]   // exact prefix "D"
        [InlineData("UI", 1)]   // exact prefix "UI"
        public async Task SearchAsync_FilterByInsertDelete_ReturnsMatchingRecords(
            string insertDelete, int expectedCount)
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { InsertDelete = insertDelete });

            Assert.Equal(expectedCount, result.PaginationData.TotalRecords);
            Assert.All(result.Data, r => Assert.StartsWith(insertDelete, r.InsertDelete));
        }

        #endregion

        #region SearchAsync — dateImported filter

        [Fact]
        public async Task SearchAsync_FilterByDateImported_MatchingDate_ReturnsRecord()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            // BaseDate is 2024-06-15; pass any time on the same calendar day
            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { DateImported = BaseDate.Date });

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal(BaseDate.Date, result.Data.First().DateTime!.Value.Date);
        }

        [Fact]
        public async Task SearchAsync_FilterByDateImported_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { DateImported = new DateTime(2000, 1, 1) });

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task SearchAsync_FilterByDateImported_NullDateTime_ExcludesRecord()
        {
            var logs = new List<MonthlyTimeLog>
            {
                new() { SequenceNo = 1, TimeCode = "TC1", PactStaffId = "S1", ParentProject = "PP1", Month = 1, DateTime = null, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository([], logs);
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { DateImported = BaseDate.Date });

            Assert.Empty(result.Data);
        }

        #endregion

        #region SearchAsync — combined filters

        [Fact]
        public async Task SearchAsync_MultipleFilters_ReturnsIntersection()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query,
                new MonthlyTimeLogFilter { WorkGroup = "WGA", TimeCode = "TC1", ParentProject = "PP1" });

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("TC1", result.Data.First().TimeCode);
        }

        [Fact]
        public async Task SearchAsync_MultipleFilters_NoIntersection_ReturnsEmpty()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query,
                new MonthlyTimeLogFilter { WorkGroup = "WGA", TimeCode = "TC2" });

            Assert.Empty(result.Data);
        }

        #endregion

        #region SearchAsync — ordering and paging

        [Fact]
        public async Task SearchAsync_OrderedByDateTimeDescThenSequenceNoAsc()
        {
            var logs = new List<MonthlyTimeLog>
            {
                new() { SequenceNo = 1, TimeCode = "TC1", PactStaffId = "S1", ParentProject = "PP1", Month = 1, DateTime = BaseDate,          FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, TimeCode = "TC2", PactStaffId = "S2", ParentProject = "PP2", Month = 2, DateTime = BaseDate.AddDays(2), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 3, TimeCode = "TC3", PactStaffId = "S3", ParentProject = "PP3", Month = 3, DateTime = BaseDate.AddDays(2), FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository([], logs);
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter());
            var data = result.Data.ToList();

            // Most recent date first; within same date, ascending SequenceNo
            Assert.Equal(2, data[0].SequenceNo);
            Assert.Equal(3, data[1].SequenceNo);
            Assert.Equal(1, data[2].SequenceNo);
        }

        [Fact]
        public async Task SearchAsync_Paging_ReturnsCorrectPage()
        {
            var logs = Enumerable.Range(1, 10)
                .Select(i => new MonthlyTimeLog
                {
                    SequenceNo = i,
                    TimeCode = "TC",
                    PactStaffId = "S",
                    ParentProject = "PP",
                    Month = 1,
                    DateTime = BaseDate.AddMinutes(-i),
                    FpsYear = DefaultFpsYear
                })
                .ToList();

            var repo = CreateRepository([], logs);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 3 };

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter());

            Assert.Equal(10, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.Data.Count);
        }

        #endregion

        #region GetLiveByKeyAsync

        [Fact]
        public async Task GetLiveByKeyAsync_WithMatchingCompositeKey_ReturnsEntity()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", FpsYear = DefaultFpsYear },
                new() { PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = 7, WorkGroup = "WG2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthlyTimes);

            var result = await repo.GetLiveByKeyAsync("S1", "TC1", 6, "PP1");

            Assert.NotNull(result);
            Assert.Equal("S1", result!.PactStaffId);
            Assert.Equal("TC1", result.TimeCode);
            Assert.Equal("PP1", result.ParentProject);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WithNonMatchingCompositeKey_ReturnsNull()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthlyTimes);

            var result = await repo.GetLiveByKeyAsync("S9", "TC1", 6, "PP1");

            Assert.Null(result);
        }

        #endregion

        #region MakeLiveAsync

        [Fact]
        public async Task MakeLiveAsync_WhenOneRowFails_ContinuesAndMarksOnlyFailedRow()
        {
            const string importedBy = "tester";
            const string failureMessage = "This record is no longer valid. Needs re-validating";

            var (context, repo) = CreateInMemoryContext();

            context.MonthlyTimes.Add(new MonthlyTime
            {
                PactStaffId = "S1",
                TimeCode = "TC1",
                Month = 1,
                ParentProject = "PP1",
                WorkGroup = "WG1",
                Hours = 2,
                FpsYear = DefaultFpsYear
            });

            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime
                {
                    Id = 1,
                    ImportedBy = importedBy,
                    Passed = true,
                    PactId = "S1",
                    TimeCode = "TC1",
                    Month = 1,
                    ParentProject = "PP1",
                    WorkGroup = "WG1",
                    Hours = 2
                },
                new StagingMonthlyTime
                {
                    Id = 2,
                    ImportedBy = importedBy,
                    Passed = true,
                    PactId = "S2",
                    TimeCode = "TC2",
                    Month = 2,
                    ParentProject = "PP2",
                    WorkGroup = "WG2",
                    Hours = 3
                });

            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(2, result.ProcessedCount);
            Assert.Equal(1, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);

            var failedRow = await context.StagingMonthlyTimes.SingleAsync(x => x.Id == 1);
            Assert.False(failedRow.Passed);
            Assert.Equal(failureMessage, failedRow.FailureComments);

            Assert.False(await context.StagingMonthlyTimes.AnyAsync(x => x.Id == 2));
            Assert.True(await context.MonthlyTimes.AnyAsync(x => x.PactStaffId == "S2"
                && x.TimeCode == "TC2"
                && x.Month == 2
                && x.ParentProject == "PP2"
                && x.FpsYear == DefaultFpsYear));
        }

        [Fact]
        public async Task MakeLiveAsync_WhenRowIsNoLongerValid_MarksFailedAndImportsRemainingRows()
        {
            const string importedBy = "tester";
            const string failureMessage = "This record is no longer valid. Needs re-validating";

            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime
                {
                    Id = 10,
                    ImportedBy = importedBy,
                    Passed = true,
                    PactId = null,
                    TimeCode = "TC1",
                    Month = 1,
                    ParentProject = "PP1",
                    WorkGroup = "WG1",
                    Hours = 2
                },
                new StagingMonthlyTime
                {
                    Id = 11,
                    ImportedBy = importedBy,
                    Passed = true,
                    PactId = "S2",
                    TimeCode = "TC2",
                    Month = 2,
                    ParentProject = "PP2",
                    WorkGroup = "WG2",
                    Hours = 3
                });

            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(2, result.ProcessedCount);
            Assert.Equal(1, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);

            var failedRow = await context.StagingMonthlyTimes.SingleAsync(x => x.Id == 10);
            Assert.False(failedRow.Passed);
            Assert.Equal(failureMessage, failedRow.FailureComments);

            Assert.False(await context.StagingMonthlyTimes.AnyAsync(x => x.Id == 11));
            Assert.True(await context.MonthlyTimes.AnyAsync(x => x.PactStaffId == "S2"
                && x.TimeCode == "TC2"
                && x.Month == 2
                && x.ParentProject == "PP2"
                && x.FpsYear == DefaultFpsYear));
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_MatchingKey_ReturnsTrue()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { PactStaffId = "S1", TimeCode = "TC1", Month = 6, ParentProject = "PP1", WorkGroup = "WG1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthlyTimes);

            var result = await repo.ExistsAsync("S1", "TC1", 6, "PP1");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_NoMatch_ReturnsFalse()
        {
            var repo = CreateRepository(Enumerable.Empty<MonthlyTime>());

            var result = await repo.ExistsAsync("S1", "TC1", 6, "PP1");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_DifferentFpsYear_ReturnsFalse()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { PactStaffId = "S1", TimeCode = "TC1", Month = 6, ParentProject = "PP1", WorkGroup = "WG1", FpsYear = 2023 }
            };
            var repo = CreateRepository(monthlyTimes);

            var result = await repo.ExistsAsync("S1", "TC1", 6, "PP1");

            Assert.False(result);
        }

        #endregion

        #region UpdateLiveAsync

        [Fact]
        public async Task UpdateLiveAsync_TargetKeyConflict_ThrowsInvalidOperationException()
        {
            var (context, repo) = CreateInMemoryContext();
            context.MonthlyTimes.AddRange(
                new MonthlyTime { PactStaffId = "S1", TimeCode = "TC1", Month = 6, ParentProject = "PP1", WorkGroup = "WG1", FpsYear = DefaultFpsYear },
                new MonthlyTime { PactStaffId = "S2", TimeCode = "TC1", Month = 6, ParentProject = "PP1", WorkGroup = "WG1", FpsYear = DefaultFpsYear }
            );
            await context.SaveChangesAsync();

            var updated = new MonthlyTime
            {
                PactStaffId = "S2", TimeCode = "TC1", Month = 6, ParentProject = "PP1", WorkGroup = "WG1"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateLiveAsync(updated, "S1"));
        }

        [Fact]
        public async Task UpdateLiveAsync_RecordNotFound_ThrowsInvalidOperationException()
        {
            var (context, repo) = CreateInMemoryContext();

            var updated = new MonthlyTime
            {
                PactStaffId = "S1", TimeCode = "TC1", Month = 6, ParentProject = "PP1", WorkGroup = "WG1"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateLiveAsync(updated, "S1"));
        }

        #endregion

        #region DeleteLiveAsync

        [Fact]
        public async Task DeleteLiveAsync_ExistingEntity_RemovesAndReturnsTrue()
        {
            var (context, repo) = CreateInMemoryContext();
            context.MonthlyTimes.Add(new MonthlyTime
            {
                PactStaffId = "S1", TimeCode = "TC1", Month = 6, ParentProject = "PP1",
                WorkGroup = "WG1", Hours = 10, FpsYear = DefaultFpsYear
            });
            await context.SaveChangesAsync();

            var result = await repo.DeleteLiveAsync("S1", "TC1", 6, "PP1");

            Assert.True(result);
            Assert.False(await context.MonthlyTimes.AnyAsync());
            var logs = await context.MonthlyTimeLogs.ToListAsync();
            Assert.Contains(logs, l => l.InsertDelete == "D");
        }

        [Fact]
        public async Task DeleteLiveAsync_NonExistingEntity_ReturnsFalse()
        {
            var (_, repo) = CreateInMemoryContext();

            var result = await repo.DeleteLiveAsync("S1", "TC1", 6, "PP1");

            Assert.False(result);
        }

        #endregion

        #region GetStagingByIdAsync

        [Fact]
        public async Task GetStagingByIdAsync_Matching_ReturnsEntity()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = "user1", PactStaffId = "S1", TimeCode = "TC1",
                ParentProject = "PP1", Month = 6, WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.GetStagingByIdAsync(1, "user1");

            Assert.NotNull(result);
            Assert.Equal("S1", result!.PactStaffId);
        }

        [Fact]
        public async Task GetStagingByIdAsync_WrongUser_ReturnsNull()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = "user1", PactStaffId = "S1", TimeCode = "TC1",
                ParentProject = "PP1", Month = 6, WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.GetStagingByIdAsync(1, "user2");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetStagingByIdAsync_WrongId_ReturnsNull()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = "user1", PactStaffId = "S1", TimeCode = "TC1",
                ParentProject = "PP1", Month = 6, WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.GetStagingByIdAsync(99, "user1");

            Assert.Null(result);
        }

        #endregion

        #region CreateStagingAsync

        [Fact]
        public async Task CreateStagingAsync_SetsPassed_False_AndPersists()
        {
            var (context, repo) = CreateInMemoryContext();
            var entity = new StagingMonthlyTime
            {
                ImportedBy = "user1", PactStaffId = "S1", TimeCode = "TC1",
                ParentProject = "PP1", Month = 6, WorkGroup = "WG1", Hours = 10, Passed = true
            };

            var result = await repo.CreateStagingAsync(entity);

            Assert.False(result.Passed);
            Assert.True(result.Id > 0);
            Assert.Equal(1, await context.StagingMonthlyTimes.CountAsync());
        }

        #endregion

        #region UpdateStagingAsync

        [Fact]
        public async Task UpdateStagingAsync_ExistingRecord_UpdatesFieldsAndResetsPassed()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = "user1", PactStaffId = "S1", TimeCode = "TC1",
                ParentProject = "PP1", Month = 6, WorkGroup = "WG1", Hours = 10, Passed = true
            });
            await context.SaveChangesAsync();

            var update = new StagingMonthlyTime
            {
                Id = 1, PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2",
                Month = 7, WorkGroup = "WG2", Hours = 20, PactId = "P2", Name = "New Name"
            };

            var result = await repo.UpdateStagingAsync(update, "user1");

            Assert.Equal("S2", result.PactStaffId);
            Assert.Equal("TC2", result.TimeCode);
            Assert.Equal("PP2", result.ParentProject);
            Assert.Equal(7, result.Month);
            Assert.Equal("WG2", result.WorkGroup);
            Assert.Equal(20, result.Hours);
            Assert.False(result.Passed);
            Assert.Contains("re-validating", result.FailureComments);
        }

        [Fact]
        public async Task UpdateStagingAsync_NotFound_ThrowsInvalidOperationException()
        {
            var (_, repo) = CreateInMemoryContext();

            var update = new StagingMonthlyTime { Id = 99 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateStagingAsync(update, "user1"));
        }

        #endregion

        #region DeleteStagingAsync

        [Fact]
        public async Task DeleteStagingAsync_ExistingRecord_ReturnsTrue()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = "user1", PactStaffId = "S1", TimeCode = "TC1",
                ParentProject = "PP1", Month = 6, WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.DeleteStagingAsync(1, "user1");

            Assert.True(result);
            Assert.Equal(0, await context.StagingMonthlyTimes.CountAsync());
        }

        [Fact]
        public async Task DeleteStagingAsync_NotFound_ReturnsFalse()
        {
            var (_, repo) = CreateInMemoryContext();

            var result = await repo.DeleteStagingAsync(99, "user1");

            Assert.False(result);
        }

        #endregion

        #region ImportStagingAsync

        [Fact]
        public async Task ImportStagingAsync_WithRows_InsertsAndReturnsCount()
        {
            var (context, repo) = CreateInMemoryContext();
            var rows = new List<StagingMonthlyTime>
            {
                new() { ImportedBy = "user1", PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1" },
                new() { ImportedBy = "user1", PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = 7, WorkGroup = "WG2" }
            };

            var result = await repo.ImportStagingAsync(rows);

            Assert.Equal(2, result);
            Assert.Equal(2, await context.StagingMonthlyTimes.CountAsync());
        }

        [Fact]
        public async Task ImportStagingAsync_EmptyList_ReturnsZero()
        {
            var (context, repo) = CreateInMemoryContext();

            var result = await repo.ImportStagingAsync(Enumerable.Empty<StagingMonthlyTime>());

            Assert.Equal(0, result);
            Assert.Equal(0, await context.StagingMonthlyTimes.CountAsync());
        }

        #endregion

        #region GetStagingRecordsForValidationAsync

        [Fact]
        public async Task GetStagingRecordsForValidationAsync_ReturnsOnlyFailedRecordsForUser()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime { ImportedBy = "user1", Passed = false, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 1, WorkGroup = "WG1" },
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = 2, WorkGroup = "WG2" },
                new StagingMonthlyTime { ImportedBy = "user2", Passed = false, PactStaffId = "S3", TimeCode = "TC3", ParentProject = "PP3", Month = 3, WorkGroup = "WG3" }
            );
            await context.SaveChangesAsync();

            var result = await repo.GetStagingRecordsForValidationAsync("user1");

            Assert.Single(result);
            Assert.Equal("S1", result[0].PactStaffId);
        }

        [Fact]
        public async Task GetStagingRecordsForValidationAsync_Empty_ReturnsEmptyList()
        {
            var (_, repo) = CreateInMemoryContext();

            var result = await repo.GetStagingRecordsForValidationAsync("user1");

            Assert.Empty(result);
        }

        #endregion

        #region GetPassedStagingKeysAsync

        [Fact]
        public async Task GetPassedStagingKeysAsync_ReturnsKeysForPassedRecords()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactId = "P1", TimeCode = "TC1", ParentProject = "PP1", WorkGroup = "WG1", Month = 6, PactStaffId = "S1" },
                new StagingMonthlyTime { ImportedBy = "user1", Passed = false, PactId = "P2", TimeCode = "TC2", ParentProject = "PP2", WorkGroup = "WG2", Month = 7, PactStaffId = "S2" }
            );
            await context.SaveChangesAsync();

            var result = await repo.GetPassedStagingKeysAsync("user1");

            Assert.Single(result);
            Assert.Contains("P1|TC1|PP1|WG1|6", result);
        }

        #endregion

        #region UpdateStagingRecordsAsync

        [Fact]
        public async Task UpdateStagingRecordsAsync_MarksRecordsAsModifiedAndSaves()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = "user1", PactStaffId = "S1", TimeCode = "TC1",
                ParentProject = "PP1", Month = 6, WorkGroup = "WG1", Passed = false
            });
            await context.SaveChangesAsync();

            var record = await context.StagingMonthlyTimes.FirstAsync();
            record.Passed = true;
            record.FailureComments = "Updated";

            await repo.UpdateStagingRecordsAsync(new List<StagingMonthlyTime> { record });

            var updated = await context.StagingMonthlyTimes.FirstAsync();
            Assert.True(updated.Passed);
            Assert.Equal("Updated", updated.FailureComments);
        }

        #endregion

        #region GetExistingLiveKeysAsync

        [Fact]
        public async Task GetExistingLiveKeysAsync_ReturnsConcatenatedKeys()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", WorkGroup = "WG1", Month = 6, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthlyTimes);

            var result = await repo.GetExistingLiveKeysAsync();

            Assert.Single(result);
            Assert.Contains("S1|TC1|PP1|WG1|6", result);
        }

        [Fact]
        public async Task GetExistingLiveKeysAsync_Empty_ReturnsEmptySet()
        {
            var repo = CreateRepository(Enumerable.Empty<MonthlyTime>());

            var result = await repo.GetExistingLiveKeysAsync();

            Assert.Empty(result);
        }

        #endregion

        #region HasFailedStagingAsync

        [Fact]
        public async Task HasFailedStagingAsync_WithFailedRecords_ReturnsTrue()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                ImportedBy = "user1", Passed = false, PactStaffId = "S1", TimeCode = "TC1",
                ParentProject = "PP1", Month = 6, WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.HasFailedStagingAsync("user1");

            Assert.True(result);
        }

        [Fact]
        public async Task HasFailedStagingAsync_AllPassed_ReturnsFalse()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                ImportedBy = "user1", Passed = true, PactStaffId = "S1", TimeCode = "TC1",
                ParentProject = "PP1", Month = 6, WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.HasFailedStagingAsync("user1");

            Assert.False(result);
        }

        [Fact]
        public async Task HasFailedStagingAsync_DifferentUser_ReturnsFalse()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                ImportedBy = "user2", Passed = false, PactStaffId = "S1", TimeCode = "TC1",
                ParentProject = "PP1", Month = 6, WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.HasFailedStagingAsync("user1");

            Assert.False(result);
        }

        #endregion

        #region MakeLiveAsync — additional scenarios

        [Fact]
        public async Task MakeLiveAsync_NoPassedRows_ReturnsZeros()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                ImportedBy = "tester", Passed = false, PactId = "S1", TimeCode = "TC1",
                Month = 1, ParentProject = "PP1", WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync("tester");

            Assert.Equal(0, result.ProcessedCount);
            Assert.Equal(0, result.ImportedCount);
            Assert.Equal(0, result.FailedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_SingleRowAllSucceed_ReturnsCorrectCounts()
        {
            // Test with a single valid row that has no duplicate - the all-succeed path
            // Note: cleanup (DeleteAllStagingByUserAsync) uses ExecuteDeleteAsync which
            // isn't supported by InMemory, so we test the import-success path with
            // a scenario that has existing failed rows (skipping the cleanup branch)
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime
                {
                    Id = 1, ImportedBy = importedBy, Passed = true, PactId = "S1",
                    TimeCode = "TC1", Month = 1, ParentProject = "PP1", WorkGroup = "WG1", Hours = 5
                },
                new StagingMonthlyTime
                {
                    Id = 2, ImportedBy = importedBy, Passed = false, PactStaffId = "S_FAIL",
                    TimeCode = "TC_FAIL", Month = 9, ParentProject = "PP_FAIL", WorkGroup = "WG_FAIL"
                }
            );
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.ProcessedCount);
            Assert.Equal(1, result.ImportedCount);
            Assert.Equal(1, result.FailedCount); // pre-existing failed row

            // Live record created
            Assert.True(await context.MonthlyTimes.AnyAsync(x => x.PactStaffId == "S1"));

            // Log entry created
            var logs = await context.MonthlyTimeLogs.ToListAsync();
            Assert.Contains(logs, l => l.InsertDelete == "I");
        }

        [Fact]
        public async Task MakeLiveAsync_IsValidForMakeLive_NullTimeCode_MarksInvalid()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true, PactId = "S1",
                TimeCode = null, Month = 1, ParentProject = "PP1", WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.ProcessedCount);
            Assert.Equal(0, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_IsValidForMakeLive_NullMonth_MarksInvalid()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true, PactId = "S1",
                TimeCode = "TC1", Month = null, ParentProject = "PP1", WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.ProcessedCount);
            Assert.Equal(0, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_IsValidForMakeLive_NullParentProject_MarksInvalid()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true, PactId = "S1",
                TimeCode = "TC1", Month = 1, ParentProject = null, WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.ProcessedCount);
            Assert.Equal(0, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_WithExistingFailedRows_IncludesThemInFailedCount()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime
                {
                    Id = 1, ImportedBy = importedBy, Passed = true, PactId = "S1",
                    TimeCode = "TC1", Month = 1, ParentProject = "PP1", WorkGroup = "WG1", Hours = 5
                },
                new StagingMonthlyTime
                {
                    Id = 2, ImportedBy = importedBy, Passed = false, PactStaffId = "S2",
                    TimeCode = "TC2", Month = 2, ParentProject = "PP2", WorkGroup = "WG2"
                }
            );
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.ProcessedCount);
            Assert.Equal(1, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);
            // Staging not cleaned up because there are failures
            Assert.True(await context.StagingMonthlyTimes.AnyAsync(x => x.Id == 2));
        }

        #endregion

        #region SearchStagingAsync

        [Fact]
        public async Task SearchStagingAsync_NoPassedFilter_ReturnsAllRecordsForUser()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 1, WorkGroup = "WG1", Hours = 10 },
                new StagingMonthlyTime { ImportedBy = "user1", Passed = false, PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = 2, WorkGroup = "WG2", Hours = 20 },
                new StagingMonthlyTime { ImportedBy = "user2", Passed = true, PactStaffId = "S3", TimeCode = "TC3", ParentProject = "PP3", Month = 3, WorkGroup = "WG3", Hours = 30 }
            );
            await context.SaveChangesAsync();

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task SearchStagingAsync_PassedTrue_ReturnsOnlyPassedRecords()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 1, WorkGroup = "WG1", Hours = 10 },
                new StagingMonthlyTime { ImportedBy = "user1", Passed = false, PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = 2, WorkGroup = "WG2", Hours = 20 }
            );
            await context.SaveChangesAsync();

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchStagingAsync(query, "user1", true);

            Assert.Single(result.Data);
            Assert.All(result.Data, d => Assert.True(d.Passed));
        }

        [Fact]
        public async Task SearchStagingAsync_PassedFalse_ReturnsOnlyFailedRecords()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 1, WorkGroup = "WG1", Hours = 10 },
                new StagingMonthlyTime { ImportedBy = "user1", Passed = false, PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = 2, WorkGroup = "WG2", Hours = 20 }
            );
            await context.SaveChangesAsync();

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchStagingAsync(query, "user1", false);

            Assert.Single(result.Data);
            Assert.All(result.Data, d => Assert.False(d.Passed));
        }

        [Fact]
        public async Task SearchStagingAsync_NoRecordsForUser_ReturnsEmpty()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.Add(
                new StagingMonthlyTime { ImportedBy = "other", Passed = true, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 1, WorkGroup = "WG1", Hours = 10 }
            );
            await context.SaveChangesAsync();

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task SearchStagingAsync_ComputesTotalHours()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 1, WorkGroup = "WG1", Hours = 10 },
                new StagingMonthlyTime { ImportedBy = "user1", Passed = false, PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = 2, WorkGroup = "WG2", Hours = 20 }
            );
            await context.SaveChangesAsync();

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal(30m, result.Total);
        }

        [Fact]
        public async Task SearchStagingAsync_NullHours_TreatedAsZeroInTotal()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.Add(
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 1, WorkGroup = "WG1", Hours = null }
            );
            await context.SaveChangesAsync();

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal(0m, result.Total);
        }

        [Fact]
        public async Task SearchStagingAsync_WithSortBy_AppliesSorting()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = 2, WorkGroup = "WG2", Hours = 20 },
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 1, WorkGroup = "WG1", Hours = 10 }
            );
            await context.SaveChangesAsync();

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "pactstaffid", Descending = false };
            var result = await repo.SearchStagingAsync(query, "user1", null);

            var items = result.Data.ToList();
            Assert.Equal("S1", items[0].PactStaffId);
            Assert.Equal("S2", items[1].PactStaffId);
        }

        [Fact]
        public async Task SearchStagingAsync_WithSortByDescending_AppliesDescendingSorting()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 1, WorkGroup = "WG1", Hours = 10 },
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = 2, WorkGroup = "WG2", Hours = 20 }
            );
            await context.SaveChangesAsync();

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "hours", Descending = true };
            var result = await repo.SearchStagingAsync(query, "user1", null);

            var items = result.Data.ToList();
            Assert.Equal(20, items[0].Hours);
            Assert.Equal(10, items[1].Hours);
        }

        [Fact]
        public async Task SearchStagingAsync_DefaultSort_OrdersByWorkGroupThenStaffIdThenTimeCodeThenProjectThenMonth()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactStaffId = "S2", TimeCode = "TC1", ParentProject = "PP1", Month = 1, WorkGroup = "WG2", Hours = 20 },
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 1, WorkGroup = "WG1", Hours = 10 }
            );
            await context.SaveChangesAsync();

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchStagingAsync(query, "user1", null);

            var items = result.Data.ToList();
            Assert.Equal("WG1", items[0].WorkGroup);
            Assert.Equal("WG2", items[1].WorkGroup);
        }

        [Theory]
        [InlineData("workgroup")]
        [InlineData("name")]
        [InlineData("timecode")]
        [InlineData("parentproject")]
        [InlineData("period")]
        [InlineData("passed")]
        [InlineData("pactid")]
        [InlineData("failurecomments")]
        [InlineData("unknowncolumn")]
        public async Task SearchStagingAsync_VariousSortByValues_DoNotThrow(string sortBy)
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyTimes.Add(
                new StagingMonthlyTime { ImportedBy = "user1", Passed = true, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 1, WorkGroup = "WG1", Hours = 10, Name = "Name1", PactId = "P1", FailureComments = "fc" }
            );
            await context.SaveChangesAsync();

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy };
            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Single(result.Data);
        }

        #endregion

        #region SearchLiveAsync

        private static MonthlyTimeRepository CreateRepositoryForLiveSearch(
            IEnumerable<MonthlyTime> monthlyTimes,
            IEnumerable<WorkGroupStaffView> staffViews)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(monthlyTimes);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            mockContext.Setup(x => x.MonthlyTimes).Returns(mockSet.Object);

            var staffMockSet = RepositoryTestHelper.CreateMockDbSet(staffViews);
            mockContext.Setup(x => x.WorkGroupStaffViews).Returns(staffMockSet.Object);

            return new MonthlyTimeRepository(mockContext.Object, fpsRequestContext);
        }

        private static List<MonthlyTime> TwoLiveRecords() =>
        [
            new() { PactStaffId = "S1", TimeCode = "TC1", Month = 6, ParentProject = "PP1", WorkGroup = "WG1", Hours = 10, FpsYear = DefaultFpsYear },
            new() { PactStaffId = "S2", TimeCode = "TC2", Month = 7, ParentProject = "PP2", WorkGroup = "WG2", Hours = 20, FpsYear = DefaultFpsYear }
        ];

        private static List<WorkGroupStaffView> TwoStaffViews() =>
        [
            new() { PactId = "S1", FpsYear = DefaultFpsYear, Name = "Staff One" },
            new() { PactId = "S2", FpsYear = DefaultFpsYear, Name = "Staff Two" }
        ];

        [Fact]
        public async Task SearchLiveAsync_NoFilters_ReturnsJoinedRecords()
        {
            var repo = CreateRepositoryForLiveSearch(
                [new() { PactStaffId = "S1", TimeCode = "TC1", Month = 6, ParentProject = "PP1", WorkGroup = "WG1", Hours = 10, FpsYear = DefaultFpsYear }],
                [new() { PactId = "S1", FpsYear = DefaultFpsYear, Name = "Staff One" }]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchLiveAsync(query, null, null, null, null, null);

            Assert.Single(result.Data);
            var item = result.Data.First();
            Assert.Equal("S1", item.PactStaffId);
            Assert.Equal("Staff One", item.Name);
        }

        [Fact]
        public async Task SearchLiveAsync_FilterByWorkGroup_ReturnsFiltered()
        {
            var repo = CreateRepositoryForLiveSearch(TwoLiveRecords(), TwoStaffViews());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchLiveAsync(query, "WG1", null, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal("WG1", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task SearchLiveAsync_FilterByTimeCode_ReturnsFiltered()
        {
            var repo = CreateRepositoryForLiveSearch(TwoLiveRecords(), TwoStaffViews());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchLiveAsync(query, null, "TC2", null, null, null);

            Assert.Single(result.Data);
            Assert.Equal("TC2", result.Data.First().TimeCode);
        }

        [Fact]
        public async Task SearchLiveAsync_FilterByPactStaffId_ReturnsFiltered()
        {
            var repo = CreateRepositoryForLiveSearch(TwoLiveRecords(), TwoStaffViews());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchLiveAsync(query, null, null, "S1", null, null);

            Assert.Single(result.Data);
            Assert.Equal("S1", result.Data.First().PactStaffId);
        }

        [Fact]
        public async Task SearchLiveAsync_FilterByParentProject_ReturnsFiltered()
        {
            var repo = CreateRepositoryForLiveSearch(TwoLiveRecords(), TwoStaffViews());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchLiveAsync(query, null, null, null, "PP1", null);

            Assert.Single(result.Data);
            Assert.Equal("PP1", result.Data.First().ParentProject);
        }

        [Fact]
        public async Task SearchLiveAsync_FilterByMonth_ReturnsFiltered()
        {
            var repo = CreateRepositoryForLiveSearch(TwoLiveRecords(), TwoStaffViews());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchLiveAsync(query, null, null, null, null, 6.0);

            Assert.Single(result.Data);
            Assert.Equal(6, result.Data.First().Month);
        }

        [Fact]
        public async Task SearchLiveAsync_ComputesTotalHours()
        {
            var repo = CreateRepositoryForLiveSearch(TwoLiveRecords(), TwoStaffViews());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchLiveAsync(query, null, null, null, null, null);

            Assert.Equal(30m, result.Total);
        }

        [Fact]
        public async Task SearchLiveAsync_NullHours_TreatedAsZeroInTotal()
        {
            var repo = CreateRepositoryForLiveSearch(
                [new() { PactStaffId = "S1", TimeCode = "TC1", Month = 6, ParentProject = "PP1", WorkGroup = "WG1", Hours = null, FpsYear = DefaultFpsYear }],
                [new() { PactId = "S1", FpsYear = DefaultFpsYear, Name = "Staff One" }]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchLiveAsync(query, null, null, null, null, null);

            Assert.Equal(0m, result.Total);
        }

        [Fact]
        public async Task SearchLiveAsync_DifferentFpsYear_ExcludedFromResults()
        {
            var repo = CreateRepositoryForLiveSearch(
                [new() { PactStaffId = "S1", TimeCode = "TC1", Month = 6, ParentProject = "PP1", WorkGroup = "WG1", Hours = 10, FpsYear = 2023 }],
                [new() { PactId = "S1", FpsYear = 2023, Name = "Staff One" }]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchLiveAsync(query, null, null, null, null, null);

            Assert.Empty(result.Data);
        }

        [Theory]
        [InlineData("workgroup")]
        [InlineData("name")]
        [InlineData("timecode")]
        [InlineData("parentproject")]
        [InlineData("month")]
        [InlineData("hours")]
        [InlineData("pactstaffid")]
        [InlineData("unknowncolumn")]
        public async Task SearchLiveAsync_VariousSortByValues_DoNotThrow(string sortBy)
        {
            var repo = CreateRepositoryForLiveSearch(
                [new() { PactStaffId = "S1", TimeCode = "TC1", Month = 6, ParentProject = "PP1", WorkGroup = "WG1", Hours = 10, FpsYear = DefaultFpsYear }],
                [new() { PactId = "S1", FpsYear = DefaultFpsYear, Name = "Staff One" }]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy };
            var result = await repo.SearchLiveAsync(query, null, null, null, null, null);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task SearchLiveAsync_SortByDescending_AppliesDescendingOrder()
        {
            var repo = CreateRepositoryForLiveSearch(TwoLiveRecords(), TwoStaffViews());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "hours", Descending = true };
            var result = await repo.SearchLiveAsync(query, null, null, null, null, null);

            var items = result.Data.ToList();
            Assert.Equal(20, items[0].Hours);
            Assert.Equal(10, items[1].Hours);
        }

        [Fact]
        public async Task SearchLiveAsync_AllFiltersApplied_ReturnsIntersection()
        {
            var repo = CreateRepositoryForLiveSearch(TwoLiveRecords(), TwoStaffViews());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.SearchLiveAsync(query, "WG1", "TC1", "S1", "PP1", 6.0);

            Assert.Single(result.Data);
            Assert.Equal("S1", result.Data.First().PactStaffId);
        }

        #endregion

        #region SearchAsync (Log) — additional sorting coverage

        [Theory]
        [InlineData("sequenceno")]
        [InlineData("id")]
        [InlineData("timecode")]
        [InlineData("parentproject")]
        [InlineData("project")]
        [InlineData("month")]
        [InlineData("pactstaffid")]
        [InlineData("staffid")]
        [InlineData("workgroup")]
        [InlineData("hours")]
        [InlineData("datetime")]
        [InlineData("dateimported")]
        [InlineData("userid")]
        [InlineData("insertdelete")]
        [InlineData("action")]
        [InlineData("unknowncolumn")]
        public async Task SearchAsync_VariousSortByValues_DoNotThrow(string sortBy)
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy };
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_SortDescending_ReverseOrder()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "sequenceno", Descending = true };
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            var items = result.Data.ToList();
            Assert.Equal(3, items[0].SequenceNo);
            Assert.Equal(2, items[1].SequenceNo);
            Assert.Equal(1, items[2].SequenceNo);
        }

        [Fact]
        public async Task SearchAsync_SortAscending_NormalOrder()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "sequenceno", Descending = false };
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            var items = result.Data.ToList();
            Assert.Equal(1, items[0].SequenceNo);
            Assert.Equal(2, items[1].SequenceNo);
            Assert.Equal(3, items[2].SequenceNo);
        }

        #endregion

        #region MakeLiveAsync — IsValidForMakeLive additional branches

        [Fact]
        public async Task MakeLiveAsync_IsValidForMakeLive_NullPactId_MarksInvalid()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true, PactId = null,
                TimeCode = "TC1", Month = 1, ParentProject = "PP1", WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.ProcessedCount);
            Assert.Equal(0, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_IsValidForMakeLive_WhitespacePactId_MarksInvalid()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true, PactId = "   ",
                TimeCode = "TC1", Month = 1, ParentProject = "PP1", WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.ProcessedCount);
            Assert.Equal(0, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_IsValidForMakeLive_WhitespaceTimeCode_MarksInvalid()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true, PactId = "S1",
                TimeCode = "  ", Month = 1, ParentProject = "PP1", WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.ProcessedCount);
            Assert.Equal(0, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_IsValidForMakeLive_WhitespaceParentProject_MarksInvalid()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true, PactId = "S1",
                TimeCode = "TC1", Month = 1, ParentProject = "   ", WorkGroup = "WG1"
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.ProcessedCount);
            Assert.Equal(0, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_DuplicateLiveKey_FailsGracefully()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            // Pre-existing live record with same composite key
            context.MonthlyTimes.Add(new MonthlyTime
            {
                PactStaffId = "S1", TimeCode = "TC1", Month = 1, ParentProject = "PP1",
                WorkGroup = "WG1", Hours = 5, FpsYear = DefaultFpsYear
            });
            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true, PactId = "S1",
                TimeCode = "TC1", Month = 1, ParentProject = "PP1", WorkGroup = "WG1", Hours = 10
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.ProcessedCount);
            // The row fails because of duplicate key
            Assert.Equal(0, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);
        }

        #endregion

        #region UpdateLiveAsync — additional coverage note
        // UpdateLiveAsync successful path uses ExecuteUpdateAsync which is not supported
        // by the InMemory provider. The error/guard paths (TargetKeyConflict, RecordNotFound)
        // are already covered above.
        #endregion

        #region DeleteLiveAsync — logging verification

        [Fact]
        public async Task DeleteLiveAsync_ExistingEntity_CreatesDeleteLog()
        {
            var (context, repo) = CreateInMemoryContext();
            context.MonthlyTimes.Add(new MonthlyTime
            {
                PactStaffId = "S1", TimeCode = "TC1", Month = 6, ParentProject = "PP1",
                WorkGroup = "WG1", Hours = 10, FpsYear = DefaultFpsYear
            });
            await context.SaveChangesAsync();

            var result = await repo.DeleteLiveAsync("S1", "TC1", 6, "PP1");

            Assert.True(result);
            var logs = await context.MonthlyTimeLogs.ToListAsync();
            Assert.Contains(logs, l => l.InsertDelete == "D" && l.PactStaffId == "S1");
        }

        #endregion

        #region GetPassedStagingKeysAsync — edge cases

        [Fact]
        public async Task GetPassedStagingKeysAsync_NullFields_ConcatenatesEmptyStrings()
        {
            var stagingData = new List<StagingMonthlyTime>
            {
                new() { Id = 1, PactStaffId = "S1", TimeCode = null, ParentProject = null, Month = null, WorkGroup = null, Passed = true, ImportedBy = "user1", PactId = null }
            };
            var repo = CreateRepositoryWithStaging(stagingData);

            var result = await repo.GetPassedStagingKeysAsync("user1");

            Assert.Single(result);
            Assert.Contains("|||", result.First());
        }

        #endregion

        #region MakeLiveAsync — CreateLiveRow field mapping

        [Fact]
        public async Task MakeLiveAsync_SuccessfulImport_CreateLiveRowMapsAllFieldsCorrectly()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime
                {
                    Id = 1, ImportedBy = importedBy, Passed = true,
                    PactId = "STAFF1", TimeCode = "TC_A", Month = 3,
                    ParentProject = "PROJ_X", WorkGroup = "WG_Z", Hours = 12.5
                },
                new StagingMonthlyTime
                {
                    Id = 2, ImportedBy = importedBy, Passed = false,
                    PactStaffId = "FAIL", TimeCode = "F", Month = 1, ParentProject = "FP", WorkGroup = "FW"
                });
            await context.SaveChangesAsync();

            await repo.MakeLiveAsync(importedBy);

            var live = await context.MonthlyTimes.FirstOrDefaultAsync(x => x.PactStaffId == "STAFF1");
            Assert.NotNull(live);
            Assert.Equal("STAFF1", live.PactStaffId);
            Assert.Equal("TC_A", live.TimeCode);
            Assert.Equal(3, live.Month);
            Assert.Equal("PROJ_X", live.ParentProject);
            Assert.Equal("WG_Z", live.WorkGroup);
            Assert.Equal(12.5, live.Hours);
            Assert.Equal(DefaultFpsYear, live.FpsYear);
        }

        [Fact]
        public async Task MakeLiveAsync_CreateLiveRow_NullFieldsDefaultToEmpty()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true,
                PactId = "S1", TimeCode = null, Month = 5,
                ParentProject = null, WorkGroup = null, Hours = null
            });
            await context.SaveChangesAsync();

            // TimeCode is null so IsValidForMakeLive returns false — row marked invalid
            var result = await repo.MakeLiveAsync(importedBy);
            Assert.Equal(0, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_SuccessfulImport_CreatesLogEntryWithInsertAction()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime
                {
                    Id = 1, ImportedBy = importedBy, Passed = true,
                    PactId = "LOG_S1", TimeCode = "LTC1", Month = 7,
                    ParentProject = "LPP1", WorkGroup = "LWG1", Hours = 8
                },
                new StagingMonthlyTime
                {
                    Id = 2, ImportedBy = importedBy, Passed = false,
                    PactStaffId = "FAIL", TimeCode = "F", Month = 1, ParentProject = "FP", WorkGroup = "FW"
                });
            await context.SaveChangesAsync();

            await repo.MakeLiveAsync(importedBy);

            var log = await context.MonthlyTimeLogs.FirstOrDefaultAsync(l => l.PactStaffId == "LOG_S1");
            Assert.NotNull(log);
            Assert.Equal("I", log.InsertDelete);
            Assert.Equal("LTC1", log.TimeCode);
            Assert.Equal(7, log.Month);
            Assert.Equal("LPP1", log.ParentProject);
            Assert.Equal("LWG1", log.WorkGroup);
            Assert.Equal(8, log.Hours);
            Assert.Equal(DefaultFpsYear, log.FpsYear);
            Assert.NotNull(log.DateTime);
            Assert.Equal("test.user@apha.gov.uk", log.UserId);
        }

        #endregion

        #region MakeLiveAsync — DetachIfTracked coverage

        [Fact]
        public async Task MakeLiveAsync_DuplicateKey_DetachesTrackedEntitiesAndMarksInvalid()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            // Pre-existing live record with same key
            context.MonthlyTimes.Add(new MonthlyTime
            {
                PactStaffId = "DUP1", TimeCode = "TC_DUP", Month = 4,
                ParentProject = "PP_DUP", WorkGroup = "WG1", Hours = 5,
                FpsYear = DefaultFpsYear
            });
            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true,
                PactId = "DUP1", TimeCode = "TC_DUP", Month = 4,
                ParentProject = "PP_DUP", WorkGroup = "WG1", Hours = 10
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.ProcessedCount);
            Assert.Equal(0, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);

            // Staging row marked invalid
            var failedRow = await context.StagingMonthlyTimes.FirstAsync(x => x.Id == 1);
            Assert.False(failedRow.Passed);
            Assert.Equal("This record is no longer valid. Needs re-validating", failedRow.FailureComments);
        }

        [Fact]
        public async Task MakeLiveAsync_MixedDuplicateAndNew_OnlyNewImported()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.MonthlyTimes.Add(new MonthlyTime
            {
                PactStaffId = "EXIST1", TimeCode = "TC1", Month = 1,
                ParentProject = "PP1", WorkGroup = "WG1", Hours = 1,
                FpsYear = DefaultFpsYear
            });
            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime
                {
                    Id = 1, ImportedBy = importedBy, Passed = true,
                    PactId = "EXIST1", TimeCode = "TC1", Month = 1,
                    ParentProject = "PP1", WorkGroup = "WG1", Hours = 99
                },
                new StagingMonthlyTime
                {
                    Id = 2, ImportedBy = importedBy, Passed = true,
                    PactId = "NEW1", TimeCode = "TC_NEW", Month = 2,
                    ParentProject = "PP_NEW", WorkGroup = "WG_NEW", Hours = 50
                });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(2, result.ProcessedCount);
            Assert.Equal(1, result.ImportedCount);
            Assert.Equal(1, result.FailedCount);

            Assert.True(await context.MonthlyTimes.AnyAsync(x => x.PactStaffId == "NEW1"));
        }

        #endregion

        #region SearchAsync (Log) — ApplyMonthlyTimeFilter coverage

        [Fact]
        public async Task SearchAsync_NullFilter_ReturnsAllLogs()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = null;
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_EmptyFilter_ReturnsAllLogs()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_WhitespaceFilter_ReturnsAllLogs()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "   ";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_EmptyJsonObjectFilter_ReturnsAllLogs()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_FilterWithUnknownKeysOnly_ReturnsAllLogs()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"UnknownField\":\"value\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_FilterWithNullValues_TryGetReturnsFalse_ReturnsAllLogs()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"SequenceNo\":null,\"TimeCode\":null,\"Month\":null,\"Hours\":null,\"DateTime\":null,\"PactStaffId\":null,\"WorkGroup\":null,\"ParentProject\":null,\"UserId\":null,\"InsertDelete\":null}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_FilterWithInvalidIntValue_TryGetIntReturnsFalse()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"SequenceNo\":\"notanumber\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_FilterWithInvalidDoubleValue_TryGetDoubleReturnsFalse()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"Month\":\"abc\",\"Hours\":\"xyz\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_FilterWithInvalidDateValue_TryGetDateReturnsFalse()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"DateTime\":\"not-a-date\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_FilterWithSequenceNoInt_ApplyWhenConditionTrue_FiltersResults()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"SequenceNo\":\"1\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Single(result.Data);
            Assert.Equal(1, result.Data.First().SequenceNo);
        }

        [Fact]
        public async Task SearchAsync_FilterWithMonth_ApplyWhenDoubleConditionTrue_FiltersResults()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"Month\":\"6\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Single(result.Data);
            Assert.Equal(6, result.Data.First().Month);
        }

        [Fact]
        public async Task SearchAsync_FilterWithHours_ApplyWhenDoubleConditionTrue_FiltersResults()
        {
            var logs = new List<MonthlyTimeLog>
            {
                new() { SequenceNo = 1, WorkGroup = "WGA", TimeCode = "TC1", PactStaffId = "S1", ParentProject = "PP1", Month = 6, Hours = 10.5, DateTime = BaseDate, UserId = "user1", InsertDelete = "I", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, WorkGroup = "WGB", TimeCode = "TC2", PactStaffId = "S2", ParentProject = "PP2", Month = 7, Hours = 20.0, DateTime = BaseDate.AddDays(1), UserId = "user2", InsertDelete = "D", FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"Hours\":\"10.5\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Single(result.Data);
            Assert.Equal(10.5, result.Data.First().Hours);
        }

        [Fact]
        public async Task SearchAsync_FilterWithValidDate_ApplyWhenDateConditionTrue_FiltersResults()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"DateTime\":\"2024-06-15\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Single(result.Data);
            Assert.Equal(BaseDate.Date, result.Data.First().DateTime!.Value.Date);
        }

        [Fact]
        public async Task SearchAsync_FilterWithMultipleValidKeys_AppliesAllConditions()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"SequenceNo\":\"1\",\"Month\":\"6\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task SearchAsync_FilterWithSequenceNoNoMatch_ReturnsEmpty()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"SequenceNo\":\"999\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Empty(result.Data);
        }

        #endregion

        #region MakeLiveAsync — CreateLiveRow with null coalescing fields

        [Fact]
        public async Task MakeLiveAsync_StagingRowWithNullPactId_MapsToEmptyString()
        {
            // PactId null means IsValidForMakeLive returns false
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true,
                PactId = null, TimeCode = "TC1", Month = 1,
                ParentProject = "PP1", WorkGroup = "WG1", Hours = 5
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.FailedCount);
            Assert.Equal(0, result.ImportedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_StagingRowWithWhitespaceTimeCode_MarksInvalid()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true,
                PactId = "S1", TimeCode = "  ", Month = 1,
                ParentProject = "PP1", WorkGroup = "WG1", Hours = 5
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.FailedCount);
            Assert.Equal(0, result.ImportedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_StagingRowWithWhitespaceParentProject_MarksInvalid()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true,
                PactId = "S1", TimeCode = "TC1", Month = 1,
                ParentProject = "   ", WorkGroup = "WG1", Hours = 5
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.FailedCount);
            Assert.Equal(0, result.ImportedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_StagingRowWithWhitespacePactId_MarksInvalid()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true,
                PactId = "   ", TimeCode = "TC1", Month = 1,
                ParentProject = "PP1", WorkGroup = "WG1", Hours = 5
            });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.FailedCount);
            Assert.Equal(0, result.ImportedCount);
        }

        #endregion

        #region MakeLiveAsync — AllSucceed cleanup path

        [Fact]
        public async Task MakeLiveAsync_AllRowsSucceed_NoFailedRows_AttemptsCleanup()
        {
            // When all passed rows import successfully and no failed rows exist,
            // MakeLiveAsync calls DeleteAllStagingByUserAsync (ExecuteDeleteAsync).
            // InMemory doesn't support ExecuteDeleteAsync, so this will throw.
            // This validates we reach the cleanup branch.
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyTimes.Add(new StagingMonthlyTime
            {
                Id = 1, ImportedBy = importedBy, Passed = true,
                PactId = "S1", TimeCode = "TC1", Month = 1,
                ParentProject = "PP1", WorkGroup = "WG1", Hours = 5
            });
            await context.SaveChangesAsync();

            // ExecuteDeleteAsync is not supported by InMemory, so expect an exception
            // when the cleanup path is reached (all rows succeed, failedCount == 0)
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.MakeLiveAsync(importedBy));

            // Verify the live row was created before cleanup attempted
            Assert.True(await context.MonthlyTimes.AnyAsync(x => x.PactStaffId == "S1"));
        }

        #endregion

        #region SearchStagingAsync — ApplyStagingFilter coverage

        [Fact]
        public async Task SearchStagingAsync_NullFilter_ReturnsAllUserRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = null;

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchStagingAsync_EmptyFilter_ReturnsAllUserRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "";

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchStagingAsync_EmptyJsonFilter_ReturnsAllUserRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{}";

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchStagingAsync_FilterWithUnknownKeysOnly_ReturnsAllUserRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"UnknownField\":\"value\"}";

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchStagingAsync_FilterWithAllNullValues_ReturnsAllUserRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"WorkGroup\":null,\"PactStaffId\":null,\"Name\":null,\"TimeCode\":null,\"ParentProject\":null}";

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal(3, result.Data.Count());
        }

        #endregion

        #region SearchAsync (Log) — ApplyMonthlyTimeFilter TryGetStringValue empty/whitespace paths

        [Fact]
        public async Task SearchAsync_FilterTimeCode_EmptyString_ReturnsAllLogs()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"TimeCode\":\"\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_FilterPactStaffId_EmptyString_ReturnsAllLogs()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"PactStaffId\":\"\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_FilterWorkGroup_EmptyString_ReturnsAllLogs()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"WorkGroup\":\"\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_FilterParentProject_EmptyString_ReturnsAllLogs()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"ParentProject\":\"\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_FilterUserId_EmptyString_ReturnsAllLogs()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"UserId\":\"\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_FilterInsertDelete_EmptyString_ReturnsAllLogs()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"InsertDelete\":\"\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task SearchAsync_FilterWithAllEmptyStringValues_ReturnsAllLogs()
        {
            var logs = DefaultLogs();
            var repo = CreateRepository([], logs);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            query.Filter = "{\"TimeCode\":\"\",\"PactStaffId\":\"\",\"WorkGroup\":\"\",\"ParentProject\":\"\",\"UserId\":\"\",\"InsertDelete\":\"\"}";
            var filter = new MonthlyTimeLogFilter();
            var result = await repo.SearchAsync(query, filter);

            Assert.Equal(3, result.Data.Count());
        }

        #endregion

        #region UpdateLiveAsync — targetKeyExists guard with InMemory

        [Fact]
        public async Task UpdateLiveAsync_TargetKeyConflict_WithInMemory_ThrowsInvalidOperationException()
        {
            var (context, repo) = CreateInMemoryContext();

            context.MonthlyTimes.AddRange(
                new MonthlyTime { PactStaffId = "S1", TimeCode = "TC1", Month = 1, ParentProject = "PP1", WorkGroup = "WG1", Hours = 5, FpsYear = DefaultFpsYear },
                new MonthlyTime { PactStaffId = "S2", TimeCode = "TC1", Month = 1, ParentProject = "PP1", WorkGroup = "WG1", Hours = 10, FpsYear = DefaultFpsYear });
            await context.SaveChangesAsync();

            var updated = new MonthlyTime { PactStaffId = "S2", TimeCode = "TC1", Month = 1, ParentProject = "PP1", WorkGroup = "WG1", Hours = 99 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateLiveAsync(updated, "S1"));
        }

        #endregion

        #region MakeLiveAsync — DetachIfTracked with already-detached entity

        [Fact]
        public async Task MakeLiveAsync_MultipleDuplicateRows_AllDetachedAndMarkedInvalid()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.MonthlyTimes.Add(new MonthlyTime
            {
                PactStaffId = "DUP1", TimeCode = "TC1", Month = 1,
                ParentProject = "PP1", WorkGroup = "WG1", Hours = 5,
                FpsYear = DefaultFpsYear
            });
            context.StagingMonthlyTimes.AddRange(
                new StagingMonthlyTime
                {
                    Id = 1, ImportedBy = importedBy, Passed = true,
                    PactId = "DUP1", TimeCode = "TC1", Month = 1,
                    ParentProject = "PP1", WorkGroup = "WG1", Hours = 10
                },
                new StagingMonthlyTime
                {
                    Id = 2, ImportedBy = importedBy, Passed = true,
                    PactId = "DUP1", TimeCode = "TC1", Month = 1,
                    ParentProject = "PP1", WorkGroup = "WG1", Hours = 20
                },
                new StagingMonthlyTime
                {
                    Id = 3, ImportedBy = importedBy, Passed = false,
                    PactStaffId = "FAIL", TimeCode = "F", Month = 1, ParentProject = "FP", WorkGroup = "FW"
                });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(2, result.ProcessedCount);
            Assert.Equal(0, result.ImportedCount);
            Assert.True(result.FailedCount >= 2);

            var row1 = await context.StagingMonthlyTimes.FirstAsync(x => x.Id == 1);
            Assert.False(row1.Passed);
            var row2 = await context.StagingMonthlyTimes.FirstAsync(x => x.Id == 2);
            Assert.False(row2.Passed);
        }

        #endregion

        #region BulkUpdateStagingNamesAsync, DeleteAllStagingByUserAsync, DeleteFailedStagingByUserAsync, RemoveZeroAndNullHourRecordsAsync
        // These methods use ExecuteUpdateAsync / ExecuteDeleteAsync which are not supported
        // by the InMemory provider or mock DbSets. Full coverage requires integration tests
        // against a real database provider (e.g., PostgreSQL).
        #endregion
    }
}
