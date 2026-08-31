using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.MonthlyOutputRepositoryTest
{
    public class MonthlyOutputRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        private static MonthlyOutputRepository CreateRepository(
            IEnumerable<MonthlyOutputLog> logs)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(logs);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);

            mockContext.Setup(x => x.MonthlyOutputLogs).Returns(mockSet.Object);

            return new MonthlyOutputRepository(mockContext.Object, fpsRequestContext);
        }

        private static MonthlyOutputRepository CreateRepositoryWithOutputs(
            IEnumerable<MonthlyOutput> monthlyOutputs)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(monthlyOutputs);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);

            mockContext.Setup(x => x.MonthlyOutputs).Returns(mockSet.Object);

            return new MonthlyOutputRepository(mockContext.Object, fpsRequestContext);
        }

        private static PaginationParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new(page: page, pageSize: pageSize);

        private static List<MonthlyOutputLog> SeedData() =>
        [
            new() { SequenceNo = 1, WorkGroup = "WG1", TestCode = "TC1", Buyer = "BUYER_A",  Month = 1,  DateTime = new DateTime(2024, 1, 15), UserId = "SP001", InsertDelete = "I", FpsYear = DefaultFpsYear },
            new() { SequenceNo = 2, WorkGroup = "WG1", TestCode = "TC2", Buyer = "BUYER_B",  Month = 2,  DateTime = new DateTime(2024, 2, 10), UserId = "SP002", InsertDelete = "D", FpsYear = DefaultFpsYear },
            new() { SequenceNo = 3, WorkGroup = "WG2", TestCode = "TC1", Buyer = "BUYER_A",  Month = 3,  DateTime = new DateTime(2024, 3, 20), UserId = "SP001", InsertDelete = "U", FpsYear = DefaultFpsYear },
            new() { SequenceNo = 4, WorkGroup = "WG2", TestCode = "TC3", Buyer = "BUYER_C",  Month = 4,  DateTime = new DateTime(2024, 4, 5),  UserId = "SP003", InsertDelete = "I", FpsYear = DefaultFpsYear },
            new() { SequenceNo = 5, WorkGroup = "WG3", TestCode = "TC4", Buyer = "BUYER_D",  Month = 5,  DateTime = new DateTime(2024, 5, 1),  UserId = null,    InsertDelete = null, FpsYear = DefaultFpsYear },
        ];

        private static List<MonthlyOutput> MonthlyOutputSeedData() =>
        [
            new() { TestCode = "TC1", WorkGroup = "WG1", Buyer = "BUYER_A", Month = 1, FpsYear = DefaultFpsYear },
            new() { TestCode = "TC1", WorkGroup = "WG2", Buyer = "BUYER_B", Month = 2, FpsYear = DefaultFpsYear },
            new() { TestCode = "TC2", WorkGroup = "WG1", Buyer = "BUYER_C", Month = 3, FpsYear = DefaultFpsYear },
        ];

        #region GetMonthlyOutputLogAsync — no filters

        [Fact]
        public async Task GetMonthlyOutputLogAsync_NoFilters_ReturnsAllRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, null);

            Assert.NotNull(result);
            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_EmptyRepository_ReturnsEmptyResult()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, null);

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — WorkGroup filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByWorkGroup_ReturnsMatchingRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), "WG1", null, null, null, null, null, null);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.Equal("WG1", r.WorkGroup));
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByWorkGroup_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), "WG_NONE", null, null, null, null, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByWorkGroup_NullValue_ReturnsAllRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — TestCode filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByTestCode_ReturnsMatchingRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, "TC1", null, null, null, null, null);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.Equal("TC1", r.TestCode));
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByTestCode_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, "TC_NONE", null, null, null, null, null);

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — Buyer filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByBuyer_ReturnsMatchingRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, "BUYER_A", null, null, null, null);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.Equal("BUYER_A", r.Buyer));
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByBuyer_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, "BUYER_NONE", null, null, null, null);

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — DateImported filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByDateImported_ReturnsMatchingRows()
        {
            var repo = CreateRepository(SeedData());
            var targetDate = new DateTime(2024, 1, 15);

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, targetDate, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal(targetDate.Date, result.Data.First().DateTime!.Value.Date);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByDateImported_DatePartOnly_IgnoresTime()
        {
            var data = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 1, DateTime = new DateTime(2024, 6, 1, 9, 30, 0),  FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, DateTime = new DateTime(2024, 6, 1, 18, 0, 0),  FpsYear = DefaultFpsYear },
                new() { SequenceNo = 3, DateTime = new DateTime(2024, 6, 2, 9, 0, 0),   FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepository(data);

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, new DateTime(2024, 6, 1), null, null, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByDateImported_NullDateTime_NotIncluded()
        {
            var data = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 1, DateTime = null,                    FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, DateTime = new DateTime(2024, 6, 1), FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepository(data);

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, new DateTime(2024, 6, 1), null, null, null);

            Assert.Single(result.Data);
            Assert.Equal(2, result.Data.First().SequenceNo);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByDateImported_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, new DateTime(2099, 1, 1), null, null, null);

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — Month filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByMonth_ReturnsMatchingRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, 1, null, null);

            Assert.Single(result.Data);
            Assert.Equal(1, result.Data.First().Month);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByMonth_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, 99, null, null);

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — UserId filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByUserId_ExactMatch_ReturnsMatchingRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, "SP001", null);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.Contains("SP001", r.UserId));
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByUserId_PartialMatch_ReturnsMatchingRows()
        {
            var data = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 1, UserId = "SP001", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, UserId = "SP001-TEMP", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 3, UserId = "SP999", FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepository(data);

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, "SP001", null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByUserId_NullUserId_NotIncluded()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, "SP001", null);

            Assert.DoesNotContain(result.Data, r => r.UserId == null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByUserId_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, "SP_NONE", null);

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — InsertDelete filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByInsertDelete_I_ReturnsInsertedRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, "I");

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.StartsWith("I", r.InsertDelete));
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByInsertDelete_D_ReturnsDeletedRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, "D");

            Assert.Single(result.Data);
            Assert.Equal("D", result.Data.First().InsertDelete);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByInsertDelete_NullValue_NotIncluded()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, "I");

            Assert.DoesNotContain(result.Data, r => r.InsertDelete == null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByInsertDelete_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, "X");

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — combined filters

        [Fact]
        public async Task GetMonthlyOutputLogAsync_CombineWorkGroupAndTestCode_ReturnsIntersection()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), "WG1", "TC1", null, null, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal("WG1", result.Data.First().WorkGroup);
            Assert.Equal("TC1", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_CombineWorkGroupAndBuyer_ReturnsIntersection()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), "WG2", null, "BUYER_A", null, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal("WG2", result.Data.First().WorkGroup);
            Assert.Equal("BUYER_A", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_AllFiltersSet_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(
                DefaultQuery(),
                workGroup: "WG1",
                testCode: "TC1",
                buyer: "BUYER_C",
                dateImported: new DateTime(2024, 1, 15),
                month: 1,
                userId: "SP001",
                insertDelete: "I");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_AllFiltersSet_SingleMatch_ReturnsThatRow()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(
                DefaultQuery(),
                workGroup: "WG1",
                testCode: "TC1",
                buyer: "BUYER_A",
                dateImported: new DateTime(2024, 1, 15),
                month: 1,
                userId: "SP001",
                insertDelete: "I");

            Assert.Single(result.Data);
            Assert.Equal(1, result.Data.First().SequenceNo);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — ordering

        [Fact]
        public async Task GetMonthlyOutputLogAsync_ResultsOrderedByDateTimeDescending()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, null);

            var dateTimes = result.Data
                .Where(r => r.DateTime.HasValue)
                .Select(r => r.DateTime!.Value)
                .ToList();

            Assert.Equal(dateTimes.OrderByDescending(d => d).ToList(), dateTimes);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_SameDateTimeTiedBySequenceNoAscending()
        {
            var sameDate = new DateTime(2024, 7, 1);
            var data = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 3, DateTime = sameDate, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 1, DateTime = sameDate, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, DateTime = sameDate, FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepository(data);

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, null);

            var seqNos = result.Data.Select(r => r.SequenceNo).ToList();
            Assert.Equal([1, 2, 3], seqNos);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — pagination

        [Fact]
        public async Task GetMonthlyOutputLogAsync_PaginationPage1_ReturnsFirstPageRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(page: 1, pageSize: 2), null, null, null, null, null, null, null);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_PaginationPage2_ReturnsSecondPageRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(page: 2, pageSize: 2), null, null, null, null, null, null, null);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_LastPageWithFewerRows_ReturnsRemainingRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(page: 3, pageSize: 2), null, null, null, null, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_PageSizeLargerThanData_ReturnsAllRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(page: 1, pageSize: 100), null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
            Assert.Equal(1, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_PaginationMetadata_IsCorrect()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(page: 2, pageSize: 3), null, null, null, null, null, null, null);

            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(3, result.PaginationData.PageSize);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        #endregion

        #region ExistsByTestCodeAndWorkGroupAsync

        [Fact]
        public async Task ExistsByTestCodeAndWorkGroupAsync_MatchingTestCodeAndWorkGroup_ReturnsTrue()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.ExistsByTestCodeAndWorkGroupAsync("TC1", "WG1");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndWorkGroupAsync_NonMatchingTestCode_ReturnsFalse()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.ExistsByTestCodeAndWorkGroupAsync("UNKNOWN", "WG1");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndWorkGroupAsync_NonMatchingWorkGroup_ReturnsFalse()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.ExistsByTestCodeAndWorkGroupAsync("TC1", "UNKNOWN");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndWorkGroupAsync_EmptyRepository_ReturnsFalse()
        {
            var repo = CreateRepositoryWithOutputs([]);

            var result = await repo.ExistsByTestCodeAndWorkGroupAsync("TC1", "WG1");

            Assert.False(result);
        }

        #endregion

        #region LiveRecordExistsAsync

        [Fact]
        public async Task LiveRecordExistsAsync_WithMatchingCompositeKey_ReturnsTrue()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.LiveRecordExistsAsync("TC1", "BUYER_A", 1, "WG1");

            Assert.True(result);
        }

        [Fact]
        public async Task LiveRecordExistsAsync_WithNonMatchingCompositeKey_ReturnsFalse()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.LiveRecordExistsAsync("TC1", "BUYER_X", 1, "WG1");

            Assert.False(result);
        }

        #endregion

        #region GetLiveByKeyAsync

        [Fact]
        public async Task GetLiveByKeyAsync_WithMatchingCompositeKey_ReturnsEntity()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.GetLiveByKeyAsync("TC1", "BUYER_A", 1, "WG1");

            Assert.NotNull(result);
            Assert.Equal("TC1", result!.TestCode);
            Assert.Equal("BUYER_A", result.Buyer);
            Assert.Equal("WG1", result.WorkGroup);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WithNonMatchingCompositeKey_ReturnsNull()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.GetLiveByKeyAsync("TC9", "BUYER_A", 1, "WG1");

            Assert.Null(result);
        }

        #endregion

        #region Helper — Staging Repository

        private static MonthlyOutputRepository CreateRepositoryWithStaging(
            IEnumerable<StagingMonthlyOutput> stagingData)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(stagingData);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);

            mockContext.Setup(x => x.StagingMonthlyOutputs).Returns(mockSet.Object);

            return new MonthlyOutputRepository(mockContext.Object, fpsRequestContext);
        }

        private static MonthlyOutputRepository CreateRepositoryWithAll(
            IEnumerable<MonthlyOutput> outputs,
            IEnumerable<StagingMonthlyOutput> staging,
            IEnumerable<MonthlyOutputLog> logs)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);
            fpsRequestContext.UserEmailId.Returns("testuser@test.com");

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var mockOutputs = RepositoryTestHelper.CreateMockDbSet(outputs);
            RepositoryTestHelper.SetupDbSetOperations(mockOutputs);
            mockContext.Setup(x => x.MonthlyOutputs).Returns(mockOutputs.Object);

            var mockStaging = RepositoryTestHelper.CreateMockDbSet(staging);
            RepositoryTestHelper.SetupDbSetOperations(mockStaging);
            mockContext.Setup(x => x.StagingMonthlyOutputs).Returns(mockStaging.Object);

            var mockLogs = RepositoryTestHelper.CreateMockDbSet(logs);
            RepositoryTestHelper.SetupDbSetOperations(mockLogs);
            mockContext.Setup(x => x.MonthlyOutputLogs).Returns(mockLogs.Object);

            return new MonthlyOutputRepository(mockContext.Object, fpsRequestContext);
        }

        private static List<StagingMonthlyOutput> StagingSeedData() =>
        [
            new() { Id = 1, TestCode = "TC1", Buyer = "BUYER_A", Month = 1, WorkGroup = "WG1", Volume = 10, Passed = true, ImportedBy = "user1" },
            new() { Id = 2, TestCode = "TC2", Buyer = "BUYER_B", Month = 2, WorkGroup = "WG1", Volume = 20, Passed = false, ImportedBy = "user1", FailureComments = "Bad data" },
            new() { Id = 3, TestCode = "TC3", Buyer = "BUYER_C", Month = 3, WorkGroup = "WG2", Volume = 0, Passed = false, ImportedBy = "user1" },
            new() { Id = 4, TestCode = "TC4", Buyer = "BUYER_D", Month = 4, WorkGroup = "WG2", Volume = null, Passed = true, ImportedBy = "user2" },
        ];

        #endregion

        #region GetExistingLiveKeysAsync

        [Fact]
        public async Task GetExistingLiveKeysAsync_ReturnsHashSetOfKeys()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.GetExistingLiveKeysAsync();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains("TC1|BUYER_A|1|WG1", result);
        }

        [Fact]
        public async Task GetExistingLiveKeysAsync_EmptyRepository_ReturnsEmptyHashSet()
        {
            var repo = CreateRepositoryWithOutputs([]);

            var result = await repo.GetExistingLiveKeysAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetExistingLiveKeysAsync_CaseInsensitiveComparison()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.GetExistingLiveKeysAsync();

            Assert.Contains("tc1|buyer_a|1|wg1", result);
        }

        #endregion

        #region SearchLiveAsync

        [Fact]
        public async Task SearchLiveAsync_NoFilters_ReturnsAllRows()
        {
            var data = MonthlyOutputSeedData();
            data.ForEach(d => d.Volume = 10);
            var repo = CreateRepositoryWithOutputs(data);

            var result = await repo.SearchLiveAsync(DefaultQuery(), null, null, null, null);

            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task SearchLiveAsync_FilterByWorkGroup_ReturnsMatchingRows()
        {
            var data = MonthlyOutputSeedData();
            data.ForEach(d => d.Volume = 10);
            var repo = CreateRepositoryWithOutputs(data);

            var result = await repo.SearchLiveAsync(DefaultQuery(), "WG1", null, null, null);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.Equal("WG1", r.WorkGroup));
        }

        [Fact]
        public async Task SearchLiveAsync_FilterByTestCode_ReturnsMatchingRows()
        {
            var data = MonthlyOutputSeedData();
            data.ForEach(d => d.Volume = 10);
            var repo = CreateRepositoryWithOutputs(data);

            var result = await repo.SearchLiveAsync(DefaultQuery(), null, "TC1", null, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task SearchLiveAsync_FilterByBuyer_ReturnsMatchingRows()
        {
            var data = MonthlyOutputSeedData();
            data.ForEach(d => d.Volume = 10);
            var repo = CreateRepositoryWithOutputs(data);

            var result = await repo.SearchLiveAsync(DefaultQuery(), null, null, "BUYER_A", null);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task SearchLiveAsync_FilterByMonth_ReturnsMatchingRows()
        {
            var data = MonthlyOutputSeedData();
            data.ForEach(d => d.Volume = 10);
            var repo = CreateRepositoryWithOutputs(data);

            var result = await repo.SearchLiveAsync(DefaultQuery(), null, null, null, 1);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task SearchLiveAsync_Total_ReturnsSumOfVolumes()
        {
            var data = new List<MonthlyOutput>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", Buyer = "B1", Month = 1, Volume = 10, FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG1", Buyer = "B2", Month = 2, Volume = 20, FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepositoryWithOutputs(data);

            var result = await repo.SearchLiveAsync(DefaultQuery(), null, null, null, null);

            Assert.Equal(30m, result.Total);
        }

        [Fact]
        public async Task SearchLiveAsync_NullVolume_TreatedAsZero()
        {
            var data = new List<MonthlyOutput>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", Buyer = "B1", Month = 1, Volume = null, FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepositoryWithOutputs(data);

            var result = await repo.SearchLiveAsync(DefaultQuery(), null, null, null, null);

            Assert.Equal(0m, result.Total);
        }

        [Fact]
        public async Task SearchLiveAsync_SortByWorkGroup_Ascending()
        {
            var data = MonthlyOutputSeedData();
            data.ForEach(d => d.Volume = 10);
            var repo = CreateRepositoryWithOutputs(data);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "workgroup", Descending = false };

            var result = await repo.SearchLiveAsync(query, null, null, null, null);

            Assert.Equal("WG1", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task SearchLiveAsync_SortByWorkGroup_Descending()
        {
            var data = MonthlyOutputSeedData();
            data.ForEach(d => d.Volume = 10);
            var repo = CreateRepositoryWithOutputs(data);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "workgroup", Descending = true };

            var result = await repo.SearchLiveAsync(query, null, null, null, null);

            Assert.Equal("WG2", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task SearchLiveAsync_SortByTestCode()
        {
            var data = MonthlyOutputSeedData();
            data.ForEach(d => d.Volume = 10);
            var repo = CreateRepositoryWithOutputs(data);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "testcode" };

            var result = await repo.SearchLiveAsync(query, null, null, null, null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchLiveAsync_SortByBuyer()
        {
            var data = MonthlyOutputSeedData();
            data.ForEach(d => d.Volume = 10);
            var repo = CreateRepositoryWithOutputs(data);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "buyer" };

            var result = await repo.SearchLiveAsync(query, null, null, null, null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchLiveAsync_SortByMonth()
        {
            var data = MonthlyOutputSeedData();
            data.ForEach(d => d.Volume = 10);
            var repo = CreateRepositoryWithOutputs(data);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "month" };

            var result = await repo.SearchLiveAsync(query, null, null, null, null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchLiveAsync_SortByVolume()
        {
            var data = MonthlyOutputSeedData();
            data.ForEach(d => d.Volume = 10);
            var repo = CreateRepositoryWithOutputs(data);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "volume" };

            var result = await repo.SearchLiveAsync(query, null, null, null, null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchLiveAsync_SortByUnknown_UsesDefaultSort()
        {
            var data = MonthlyOutputSeedData();
            data.ForEach(d => d.Volume = 10);
            var repo = CreateRepositoryWithOutputs(data);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "unknownfield" };

            var result = await repo.SearchLiveAsync(query, null, null, null, null);

            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count);
        }

        #endregion

        #region UpdateLiveAsync

        [Fact]
        public async Task UpdateLiveAsync_ExistingRecord_UpdatesAndReturns()
        {
            var repo = CreateRepositoryWithAll(MonthlyOutputSeedData(), [], []);
            var updated = new MonthlyOutput { TestCode = "TC1_NEW", Buyer = "BUYER_A", Month = 1, WorkGroup = "WG1", Volume = 99 };

            var result = await repo.UpdateLiveAsync(updated, "TC1", "BUYER_A", 1, "WG1");

            Assert.Equal("TC1_NEW", result.TestCode);
            Assert.Equal(99, result.Volume);
        }

        [Fact]
        public async Task UpdateLiveAsync_NonExistingRecord_ThrowsKeyNotFoundException()
        {
            var repo = CreateRepositoryWithAll(MonthlyOutputSeedData(), [], []);
            var updated = new MonthlyOutput { TestCode = "TC9", Buyer = "BUYER_X", Month = 99, WorkGroup = "WG9" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.UpdateLiveAsync(updated, "TC9", "BUYER_X", 99, "WG9"));
        }

        #endregion

        #region DeleteLiveAsync

        [Fact]
        public async Task DeleteLiveAsync_ExistingRecord_ReturnsTrueAndRemoves()
        {
            var repo = CreateRepositoryWithAll(MonthlyOutputSeedData(), [], []);

            var result = await repo.DeleteLiveAsync("TC1", "BUYER_A", 1, "WG1");

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteLiveAsync_NonExistingRecord_ReturnsFalse()
        {
            var repo = CreateRepositoryWithAll(MonthlyOutputSeedData(), [], []);

            var result = await repo.DeleteLiveAsync("TC9", "BUYER_X", 99, "WG9");

            Assert.False(result);
        }

        #endregion

        #region SearchStagingAsync

        [Fact]
        public async Task SearchStagingAsync_FiltersByImportedBy()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.SearchStagingAsync(DefaultQuery(), "user1", null);

            Assert.Equal(3, result.Data.Count);
            Assert.All(result.Data, r => Assert.Equal("user1", r.ImportedBy));
        }

        [Fact]
        public async Task SearchStagingAsync_PassedTrue_ReturnsOnlyPassedRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.SearchStagingAsync(DefaultQuery(), "user1", true);

            Assert.Single(result.Data);
            Assert.All(result.Data, r => Assert.True(r.Passed));
        }

        [Fact]
        public async Task SearchStagingAsync_PassedFalse_ReturnsOnlyFailedRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.SearchStagingAsync(DefaultQuery(), "user1", false);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.False(r.Passed));
        }

        [Fact]
        public async Task SearchStagingAsync_PassedNull_ReturnsAllUserRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.SearchStagingAsync(DefaultQuery(), "user1", null);

            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task SearchStagingAsync_Total_ReturnsSumOfVolumes()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.SearchStagingAsync(DefaultQuery(), "user1", null);

            Assert.Equal(30m, result.Total);
        }

        [Fact]
        public async Task SearchStagingAsync_DefaultSort_OrdersByWorkGroupThenTestCodeThenBuyerThenMonth()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.SearchStagingAsync(DefaultQuery(), "user1", null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchStagingAsync_SortByWorkGroup()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "workgroup" };

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchStagingAsync_SortByTestCode()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "testcode" };

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchStagingAsync_SortByBuyer()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "buyer" };

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchStagingAsync_SortByMonth()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "month" };

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchStagingAsync_SortByVolume()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "volume" };

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchStagingAsync_SortByPass()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "pass" };

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchStagingAsync_SortByFailureComments()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "failurecomments" };

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchStagingAsync_SortByUnknown_UsesDefaultSort()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "unknownfield" };

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchStagingAsync_SortDescending()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "workgroup", Descending = true };

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal("WG2", result.Data.First().WorkGroup);
        }

        #endregion

        #region GetStagingByIdAsync

        [Fact]
        public async Task GetStagingByIdAsync_MatchingIdAndUser_ReturnsEntity()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.GetStagingByIdAsync(1, "user1");

            Assert.NotNull(result);
            Assert.Equal(1, result!.Id);
        }

        [Fact]
        public async Task GetStagingByIdAsync_WrongUser_ReturnsNull()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.GetStagingByIdAsync(1, "wronguser");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetStagingByIdAsync_NonExistingId_ReturnsNull()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.GetStagingByIdAsync(999, "user1");

            Assert.Null(result);
        }

        #endregion

        #region CreateStagingAsync

        [Fact]
        public async Task CreateStagingAsync_SetsPassedToFalse_ReturnsEntity()
        {
            var repo = CreateRepositoryWithStaging([]);
            var entity = new StagingMonthlyOutput { Id = 10, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Passed = true };

            var result = await repo.CreateStagingAsync(entity);

            Assert.False(result.Passed);
        }

        #endregion

        #region UpdateStagingAsync

        [Fact]
        public async Task UpdateStagingAsync_ExistingRecord_UpdatesFields()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var updated = new StagingMonthlyOutput { Id = 1, TestCode = "TC1_UPDATED", Buyer = "BUYER_UPDATED", Month = 9, WorkGroup = "WG9", Volume = 99 };

            var result = await repo.UpdateStagingAsync(updated, "user1");

            Assert.Equal("TC1_UPDATED", result.TestCode);
            Assert.Equal("BUYER_UPDATED", result.Buyer);
            Assert.Equal(9, result.Month);
            Assert.Equal("WG9", result.WorkGroup);
            Assert.Equal(99, result.Volume);
            Assert.False(result.Passed);
            Assert.Contains("edited since being validated", result.FailureComments);
        }

        [Fact]
        public async Task UpdateStagingAsync_NonExistingRecord_ThrowsKeyNotFoundException()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var updated = new StagingMonthlyOutput { Id = 999, TestCode = "TC1" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.UpdateStagingAsync(updated, "user1"));
        }

        [Fact]
        public async Task UpdateStagingAsync_WrongUser_ThrowsKeyNotFoundException()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var updated = new StagingMonthlyOutput { Id = 1, TestCode = "TC1" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.UpdateStagingAsync(updated, "wronguser"));
        }

        #endregion

        #region DeleteStagingAsync

        [Fact]
        public async Task DeleteStagingAsync_ExistingRecord_ReturnsTrue()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.DeleteStagingAsync(1, "user1");

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteStagingAsync_NonExistingRecord_ReturnsFalse()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.DeleteStagingAsync(999, "user1");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteStagingAsync_WrongUser_ReturnsFalse()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.DeleteStagingAsync(1, "wronguser");

            Assert.False(result);
        }

        #endregion

        #region DeleteAllStagingByUserAsync

        [Fact]
        public async Task DeleteAllStagingByUserAsync_ReturnsCountOfDeletedRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.DeleteAllStagingByUserAsync("user1");

            Assert.Equal(3, result);
        }

        [Fact]
        public async Task DeleteAllStagingByUserAsync_NoRows_ReturnsZero()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.DeleteAllStagingByUserAsync("nonexistentuser");

            Assert.Equal(0, result);
        }

        #endregion

        #region DeleteFailedStagingByUserAsync

        [Fact]
        public async Task DeleteFailedStagingByUserAsync_ReturnsCountOfFailedRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.DeleteFailedStagingByUserAsync("user1");

            Assert.Equal(2, result);
        }

        [Fact]
        public async Task DeleteFailedStagingByUserAsync_NoFailedRows_ReturnsZero()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.DeleteFailedStagingByUserAsync("user2");

            Assert.Equal(0, result);
        }

        #endregion

        #region ImportStagingAsync

        [Fact]
        public async Task ImportStagingAsync_ReturnsCountOfImportedRows()
        {
            var repo = CreateRepositoryWithStaging([]);
            var rows = new List<StagingMonthlyOutput>
            {
                new() { TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1" },
                new() { TestCode = "TC2", Buyer = "B2", Month = 2, WorkGroup = "WG2" },
            };

            var result = await repo.ImportStagingAsync(rows);

            Assert.Equal(2, result);
        }

        [Fact]
        public async Task ImportStagingAsync_EmptyList_ReturnsZero()
        {
            var repo = CreateRepositoryWithStaging([]);

            var result = await repo.ImportStagingAsync([]);

            Assert.Equal(0, result);
        }

        #endregion

        #region RemoveZeroAndNullVolumeRecordsAsync

        [Fact]
        public async Task RemoveZeroAndNullVolumeRecordsAsync_RemovesZeroAndNullVolumeRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.RemoveZeroAndNullVolumeRecordsAsync("user1");

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task RemoveZeroAndNullVolumeRecordsAsync_NoMatchingRows_ReturnsZero()
        {
            var data = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Volume = 10, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithStaging(data);

            var result = await repo.RemoveZeroAndNullVolumeRecordsAsync("user1");

            Assert.Equal(0, result);
        }

        #endregion

        #region GetStagingRecordsForValidationAsync

        [Fact]
        public async Task GetStagingRecordsForValidationAsync_ReturnsFailedRowsOrderedById()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.GetStagingRecordsForValidationAsync("user1");

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.False(r.Passed));
            Assert.True(result[0].Id < result[1].Id);
        }

        [Fact]
        public async Task GetStagingRecordsForValidationAsync_NoFailedRows_ReturnsEmpty()
        {
            var data = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Passed = true, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithStaging(data);

            var result = await repo.GetStagingRecordsForValidationAsync("user1");

            Assert.Empty(result);
        }

        #endregion

        #region GetPassedStagingKeysAsync

        [Fact]
        public async Task GetPassedStagingKeysAsync_ReturnsKeysForPassedRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.GetPassedStagingKeysAsync("user1");

            Assert.Single(result);
            Assert.Contains("TC1|BUYER_A|1|WG1", result);
        }

        [Fact]
        public async Task GetPassedStagingKeysAsync_NoPassedRows_ReturnsEmpty()
        {
            var data = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Passed = false, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithStaging(data);

            var result = await repo.GetPassedStagingKeysAsync("user1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPassedStagingKeysAsync_CaseInsensitiveComparison()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.GetPassedStagingKeysAsync("user1");

            Assert.Contains("tc1|buyer_a|1|wg1", result);
        }

        #endregion

        #region HasFailedStagingAsync

        [Fact]
        public async Task HasFailedStagingAsync_WithFailedRows_ReturnsTrue()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.HasFailedStagingAsync("user1");

            Assert.True(result);
        }

        [Fact]
        public async Task HasFailedStagingAsync_NoFailedRows_ReturnsFalse()
        {
            var data = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Passed = true, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithStaging(data);

            var result = await repo.HasFailedStagingAsync("user1");

            Assert.False(result);
        }

        [Fact]
        public async Task HasFailedStagingAsync_WrongUser_ReturnsFalse()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.HasFailedStagingAsync("nonexistentuser");

            Assert.False(result);
        }

        #endregion

        #region MakeLiveAsync

        [Fact]
        public async Task MakeLiveAsync_NoPassedRows_ReturnsZeroCounts()
        {
            var staging = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Passed = false, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithAll([], staging, []);

            var (processedCount, importedCount, failedCount) = await repo.MakeLiveAsync("user1");

            Assert.Equal(0, processedCount);
            Assert.Equal(0, importedCount);
            Assert.Equal(0, failedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_ValidPassedRows_ImportsToLive()
        {
            var staging = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Passed = true, ImportedBy = "user1", Volume = 10 },
            };
            var repo = CreateRepositoryWithAll([], staging, []);

            var (processedCount, importedCount, failedCount) = await repo.MakeLiveAsync("user1");

            Assert.Equal(1, processedCount);
            Assert.Equal(1, importedCount);
            Assert.Equal(0, failedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_InvalidRow_BlankTestCode_IncrementsFailedCount()
        {
            var staging = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "", Buyer = "B1", Month = 1, WorkGroup = "WG1", Passed = true, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithAll([], staging, []);

            var (processedCount, importedCount, failedCount) = await repo.MakeLiveAsync("user1");

            Assert.Equal(1, processedCount);
            Assert.Equal(0, importedCount);
            Assert.Equal(1, failedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_InvalidRow_BlankBuyer_IncrementsFailedCount()
        {
            var staging = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = " ", Month = 1, WorkGroup = "WG1", Passed = true, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithAll([], staging, []);

            var (processedCount, importedCount, failedCount) = await repo.MakeLiveAsync("user1");

            Assert.Equal(1, processedCount);
            Assert.Equal(0, importedCount);
            Assert.Equal(1, failedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_InvalidRow_ZeroMonth_IncrementsFailedCount()
        {
            var staging = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 0, WorkGroup = "WG1", Passed = true, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithAll([], staging, []);

            var (processedCount, importedCount, failedCount) = await repo.MakeLiveAsync("user1");

            Assert.Equal(1, processedCount);
            Assert.Equal(0, importedCount);
            Assert.Equal(1, failedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_InvalidRow_BlankWorkGroup_IncrementsFailedCount()
        {
            var staging = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "", Passed = true, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithAll([], staging, []);

            var (processedCount, importedCount, failedCount) = await repo.MakeLiveAsync("user1");

            Assert.Equal(1, processedCount);
            Assert.Equal(0, importedCount);
            Assert.Equal(1, failedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_MixedValidAndInvalid_ReturnsCorrectCounts()
        {
            var staging = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Passed = true, ImportedBy = "user1", Volume = 10 },
                new() { Id = 2, TestCode = "", Buyer = "B2", Month = 2, WorkGroup = "WG2", Passed = true, ImportedBy = "user1" },
                new() { Id = 3, TestCode = "TC3", Buyer = "B3", Month = 3, WorkGroup = "WG3", Passed = false, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithAll([], staging, []);

            var (processedCount, importedCount, failedCount) = await repo.MakeLiveAsync("user1");

            Assert.Equal(2, processedCount);
            Assert.Equal(1, importedCount);
            Assert.True(failedCount >= 1);
        }

        #endregion

        #region Log Sorting Branches

        [Theory]
        [InlineData("sequenceno")]
        [InlineData("id")]
        [InlineData("testcode")]
        [InlineData("buyer")]
        [InlineData("month")]
        [InlineData("workgroup")]
        [InlineData("volume")]
        [InlineData("datetime")]
        [InlineData("dateimported")]
        [InlineData("userid")]
        [InlineData("insertdelete")]
        [InlineData("action")]
        public async Task GetMonthlyOutputLogAsync_SortBy_VariousProperties_DoesNotThrow(string sortBy)
        {
            var repo = CreateRepository(SeedData());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = sortBy };

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_SortDescending_BySequenceNo()
        {
            var repo = CreateRepository(SeedData());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "sequenceno", Descending = true };

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.First().SequenceNo);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_SortAscending_BySequenceNo()
        {
            var repo = CreateRepository(SeedData());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "sequenceno", Descending = false };

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(1, result.Data.First().SequenceNo);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_SortByUnknownField_UsesDefaultSort()
        {
            var repo = CreateRepository(SeedData());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { SortBy = "unknownfield" };

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.NotNull(result);
            Assert.Equal(5, result.Data.Count);
        }

        #endregion

        #region UpdateStagingRecordsAsync

        private static (FpsDbContext Context, MonthlyOutputRepository Repo) CreateInMemoryContext(int fpsYear = DefaultFpsYear)
        {
            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);
            fpsRequestContext.UserEmailId.Returns("test.user@apha.gov.uk");

            var context = new FpsDbContext(options, fpsRequestContext);
            var repo = new MonthlyOutputRepository(context, fpsRequestContext);
            return (context, repo);
        }

        [Fact]
        public async Task UpdateStagingRecordsAsync_MarksRecordsAsModifiedAndSaves()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyOutputs.Add(new StagingMonthlyOutput
            {
                Id = 1, ImportedBy = "user1", TestCode = "TC1", Buyer = "B1",
                Month = 1, WorkGroup = "WG1", Passed = false
            });
            await context.SaveChangesAsync();

            var record = await context.StagingMonthlyOutputs.FirstAsync();
            record.Passed = true;
            record.FailureComments = "Updated";

            await repo.UpdateStagingRecordsAsync(new List<StagingMonthlyOutput> { record });

            var updated = await context.StagingMonthlyOutputs.FirstAsync();
            Assert.True(updated.Passed);
            Assert.Equal("Updated", updated.FailureComments);
        }

        [Fact]
        public async Task UpdateStagingRecordsAsync_EmptyList_DoesNotThrow()
        {
            var (_, repo) = CreateInMemoryContext();

            await repo.UpdateStagingRecordsAsync([]);

            Assert.True(true);
        }

        [Fact]
        public async Task UpdateStagingRecordsAsync_MultipleRecords_AllModified()
        {
            var (context, repo) = CreateInMemoryContext();
            context.StagingMonthlyOutputs.AddRange(
                new StagingMonthlyOutput { Id = 1, ImportedBy = "user1", TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Passed = false },
                new StagingMonthlyOutput { Id = 2, ImportedBy = "user1", TestCode = "TC2", Buyer = "B2", Month = 2, WorkGroup = "WG2", Passed = false }
            );
            await context.SaveChangesAsync();

            var records = await context.StagingMonthlyOutputs.ToListAsync();
            records[0].Passed = true;
            records[1].Passed = true;

            await repo.UpdateStagingRecordsAsync(records);

            var updated = await context.StagingMonthlyOutputs.ToListAsync();
            Assert.All(updated, r => Assert.True(r.Passed));
        }

        #endregion

        #region SearchStagingAsync — Total Volume

        [Fact]
        public async Task SearchStagingAsync_ComputesTotalVolume()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.SearchStagingAsync(DefaultQuery(), "user1", null);

            Assert.Equal(30m, result.Total);
        }

        [Fact]
        public async Task SearchStagingAsync_NullVolume_TreatedAsZeroInTotal()
        {
            var data = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Volume = null, Passed = true, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithStaging(data);

            var result = await repo.SearchStagingAsync(DefaultQuery(), "user1", null);

            Assert.Equal(0m, result.Total);
        }

        [Fact]
        public async Task SearchStagingAsync_NoRecordsForUser_ReturnsEmptyWithZeroTotal()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());

            var result = await repo.SearchStagingAsync(DefaultQuery(), "nonexistent", null);

            Assert.Empty(result.Data);
            Assert.Equal(0m, result.Total);
        }

        #endregion

        #region SearchLiveAsync — additional coverage

        [Fact]
        public async Task SearchLiveAsync_DefaultSort_AppliesWorkGroupThenTestCodeThenBuyerThenMonth()
        {
            var data = new List<MonthlyOutput>
            {
                new() { TestCode = "TC2", WorkGroup = "WG2", Buyer = "B2", Month = 2, Volume = 10, FpsYear = DefaultFpsYear },
                new() { TestCode = "TC1", WorkGroup = "WG1", Buyer = "B1", Month = 1, Volume = 20, FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepositoryWithOutputs(data);

            var result = await repo.SearchLiveAsync(DefaultQuery(), null, null, null, null);

            Assert.Equal("WG1", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task SearchLiveAsync_AllFiltersApplied_ReturnsIntersection()
        {
            var data = new List<MonthlyOutput>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", Buyer = "B1", Month = 1, Volume = 10, FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", Buyer = "B2", Month = 2, Volume = 20, FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepositoryWithOutputs(data);

            var result = await repo.SearchLiveAsync(DefaultQuery(), "WG1", "TC1", "B1", 1);

            Assert.Single(result.Data);
            Assert.Equal("TC1", result.Data.First().TestCode);
        }

        [Fact]
        public async Task SearchLiveAsync_AllFiltersApplied_NoMatch_ReturnsEmpty()
        {
            var data = MonthlyOutputSeedData();
            data.ForEach(d => d.Volume = 10);
            var repo = CreateRepositoryWithOutputs(data);

            var result = await repo.SearchLiveAsync(DefaultQuery(), "WG_NONE", "TC_NONE", "B_NONE", 99);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task SearchLiveAsync_EmptyRepository_ReturnsEmptyResult()
        {
            var repo = CreateRepositoryWithOutputs([]);

            var result = await repo.SearchLiveAsync(DefaultQuery(), null, null, null, null);

            Assert.Empty(result.Data);
            Assert.Equal(0m, result.Total);
        }

        #endregion

        #region UpdateLiveAsync — log verification

        [Fact]
        public async Task UpdateLiveAsync_CreatesUpdateLogEntry()
        {
            var outputs = MonthlyOutputSeedData();
            var repo = CreateRepositoryWithAll(outputs, [], []);
            var updated = new MonthlyOutput { TestCode = "TC1", Buyer = "BUYER_A", Month = 1, WorkGroup = "WG1", Volume = 50 };

            var result = await repo.UpdateLiveAsync(updated, "TC1", "BUYER_A", 1, "WG1");

            Assert.NotNull(result);
            Assert.Equal(50, result.Volume);
        }

        #endregion

        #region DeleteLiveAsync — log verification

        [Fact]
        public async Task DeleteLiveAsync_ExistingRecord_CreatesDeleteLogEntry()
        {
            var repo = CreateRepositoryWithAll(MonthlyOutputSeedData(), [], []);

            var result = await repo.DeleteLiveAsync("TC1", "BUYER_A", 1, "WG1");

            Assert.True(result);
        }

        #endregion

        #region MakeLiveAsync — additional IsValidForMakeLive branches

        [Fact]
        public async Task MakeLiveAsync_InvalidRow_NullTestCode_IncrementsFailedCount()
        {
            var staging = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = null!, Buyer = "B1", Month = 1, WorkGroup = "WG1", Passed = true, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithAll([], staging, []);

            var (processedCount, importedCount, failedCount) = await repo.MakeLiveAsync("user1");

            Assert.Equal(1, processedCount);
            Assert.Equal(0, importedCount);
            Assert.Equal(1, failedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_InvalidRow_NullBuyer_IncrementsFailedCount()
        {
            var staging = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = null!, Month = 1, WorkGroup = "WG1", Passed = true, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithAll([], staging, []);

            var (processedCount, importedCount, failedCount) = await repo.MakeLiveAsync("user1");

            Assert.Equal(1, processedCount);
            Assert.Equal(0, importedCount);
            Assert.Equal(1, failedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_InvalidRow_NullWorkGroup_IncrementsFailedCount()
        {
            var staging = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = null!, Passed = true, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithAll([], staging, []);

            var (processedCount, importedCount, failedCount) = await repo.MakeLiveAsync("user1");

            Assert.Equal(1, processedCount);
            Assert.Equal(0, importedCount);
            Assert.Equal(1, failedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_NoRowsForUser_ReturnsZeroCounts()
        {
            var staging = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Passed = true, ImportedBy = "otheruser" },
            };
            var repo = CreateRepositoryWithAll([], staging, []);

            var (processedCount, importedCount, failedCount) = await repo.MakeLiveAsync("user1");

            Assert.Equal(0, processedCount);
            Assert.Equal(0, importedCount);
            Assert.Equal(0, failedCount);
        }

        [Fact]
        public async Task MakeLiveAsync_CountsPreExistingFailedRows()
        {
            var staging = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Passed = true, ImportedBy = "user1", Volume = 10 },
                new() { Id = 2, TestCode = "TC2", Buyer = "B2", Month = 2, WorkGroup = "WG2", Passed = false, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithAll([], staging, []);

            var (processedCount, importedCount, failedCount) = await repo.MakeLiveAsync("user1");

            Assert.Equal(1, processedCount);
            Assert.Equal(1, importedCount);
            Assert.Equal(1, failedCount);
        }

        #endregion

        #region GetPassedStagingKeysAsync — edge cases

        [Fact]
        public async Task GetPassedStagingKeysAsync_NullFields_ConcatenatesEmptyStrings()
        {
            var data = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = null!, Buyer = null!, Month = 0, WorkGroup = null!, Passed = true, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithStaging(data);

            var result = await repo.GetPassedStagingKeysAsync("user1");

            Assert.Single(result);
            Assert.Contains("||0|", result.First());
        }

        #endregion

        #region RemoveZeroAndNullVolumeRecordsAsync — null volume variant

        [Fact]
        public async Task RemoveZeroAndNullVolumeRecordsAsync_NullVolume_IsRemoved()
        {
            var data = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Volume = null, ImportedBy = "user1" },
                new() { Id = 2, TestCode = "TC2", Buyer = "B2", Month = 2, WorkGroup = "WG2", Volume = 10, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithStaging(data);

            var result = await repo.RemoveZeroAndNullVolumeRecordsAsync("user1");

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task RemoveZeroAndNullVolumeRecordsAsync_ZeroVolume_IsRemoved()
        {
            var data = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Volume = 0, ImportedBy = "user1" },
                new() { Id = 2, TestCode = "TC2", Buyer = "B2", Month = 2, WorkGroup = "WG2", Volume = 10, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithStaging(data);

            var result = await repo.RemoveZeroAndNullVolumeRecordsAsync("user1");

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task RemoveZeroAndNullVolumeRecordsAsync_DifferentUser_ReturnsZero()
        {
            var data = new List<StagingMonthlyOutput>
            {
                new() { Id = 1, TestCode = "TC1", Buyer = "B1", Month = 1, WorkGroup = "WG1", Volume = 0, ImportedBy = "user1" },
            };
            var repo = CreateRepositoryWithStaging(data);

            var result = await repo.RemoveZeroAndNullVolumeRecordsAsync("otheruser");

            Assert.Equal(0, result);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — Month filter with null Month

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByMonth_NullMonth_ExcludesRecord()
        {
            var logs = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 1, Month = null, WorkGroup = "WG1", TestCode = "TC1", Buyer = "B1", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, Month = 3, WorkGroup = "WG1", TestCode = "TC1", Buyer = "B1", FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepository(logs);

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, 3, null, null);

            Assert.Single(result.Data);
            Assert.Equal(2, result.Data.First().SequenceNo);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — ApplyMonthlyOutputFilter coverage

        [Fact]
        public async Task GetMonthlyOutputLogAsync_NullFilter_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = null;

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_EmptyFilter_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_WhitespaceFilter_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "   ";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_EmptyJsonFilter_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterWithUnknownKeys_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"UnknownField\":\"value\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterSequenceNo_FiltersResults()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"SequenceNo\":\"1\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal(1, result.Data.First().SequenceNo);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterSequenceNo_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"SequenceNo\":\"999\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterSequenceNo_InvalidInt_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"SequenceNo\":\"notanumber\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterSequenceNo_NullValue_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"SequenceNo\":null}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterMonth_FiltersResults()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"Month\":\"1\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal(1, (int)result.Data.First().Month!);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterMonth_InvalidDouble_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"Month\":\"abc\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterMonth_NullValue_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"Month\":null}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterVolume_FiltersResults()
        {
            var logs = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 1, Volume = 10.5, WorkGroup = "WG1", TestCode = "TC1", Buyer = "B1", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, Volume = 20.0, WorkGroup = "WG1", TestCode = "TC2", Buyer = "B2", FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepository(logs);
            var query = DefaultQuery();
            query.Filter = "{\"Volume\":\"10.5\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal(10.5, result.Data.First().Volume);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterVolume_InvalidDouble_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"Volume\":\"xyz\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterVolume_NullValue_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"Volume\":null}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterDateTime_FiltersResults()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"DateTime\":\"2024-01-15\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal(new DateTime(2024, 1, 15).Date, result.Data.First().DateTime!.Value.Date);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterDateTime_InvalidDate_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"DateTime\":\"not-a-date\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterDateTime_NullValue_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"DateTime\":null}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterWithAllNullValues_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"SequenceNo\":null,\"TestCode\":null,\"Buyer\":null,\"WorkGroup\":null,\"Month\":null,\"Volume\":null,\"DateTime\":null,\"UserId\":null,\"InsertDelete\":null}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterMultipleValidKeys_AppliesAll()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"SequenceNo\":\"1\",\"Month\":\"1\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Single(result.Data);
        }

        #endregion

        #region MakeLiveAsync — CreateLiveRow field mapping (InMemory)

        [Fact]
        public async Task MakeLiveAsync_SuccessfulImport_CreateLiveRowMapsAllFieldsCorrectly()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyOutputs.AddRange(
                new StagingMonthlyOutput
                {
                    Id = 1, ImportedBy = importedBy, Passed = true,
                    TestCode = "TC_A", Buyer = "BUY_X", Month = 3,
                    WorkGroup = "WG_Z", Volume = 12.5
                },
                new StagingMonthlyOutput
                {
                    Id = 2, ImportedBy = importedBy, Passed = false,
                    TestCode = "FAIL", Buyer = "F", Month = 1, WorkGroup = "FW"
                });
            await context.SaveChangesAsync();

            await repo.MakeLiveAsync(importedBy);

            var live = await context.MonthlyOutputs.FirstOrDefaultAsync(x => x.TestCode == "TC_A");
            Assert.NotNull(live);
            Assert.Equal("TC_A", live.TestCode);
            Assert.Equal("BUY_X", live.Buyer);
            Assert.Equal(3, live.Month);
            Assert.Equal("WG_Z", live.WorkGroup);
            Assert.Equal(12.5, live.Volume);
            Assert.Equal(DefaultFpsYear, live.FpsYear);
        }

        [Fact]
        public async Task MakeLiveAsync_SuccessfulImport_CreatesLogEntryWithInsertAction()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.StagingMonthlyOutputs.AddRange(
                new StagingMonthlyOutput
                {
                    Id = 1, ImportedBy = importedBy, Passed = true,
                    TestCode = "LOG_TC", Buyer = "LOG_B", Month = 7,
                    WorkGroup = "LOG_WG", Volume = 8
                },
                new StagingMonthlyOutput
                {
                    Id = 2, ImportedBy = importedBy, Passed = false,
                    TestCode = "FAIL", Buyer = "F", Month = 1, WorkGroup = "FW"
                });
            await context.SaveChangesAsync();

            await repo.MakeLiveAsync(importedBy);

            var log = await context.MonthlyOutputLogs.FirstOrDefaultAsync(l => l.TestCode == "LOG_TC");
            Assert.NotNull(log);
            Assert.Equal("I", log.InsertDelete);
            Assert.Equal("LOG_B", log.Buyer);
            Assert.Equal(7, log.Month);
            Assert.Equal("LOG_WG", log.WorkGroup);
            Assert.Equal(8, log.Volume);
            Assert.Equal(DefaultFpsYear, log.FpsYear);
            Assert.NotNull(log.DateTime);
            Assert.Equal("test.user@apha.gov.uk", log.UserId);
        }

        #endregion

        #region MakeLiveAsync — DetachIfTracked coverage (InMemory)

        [Fact]
        public async Task MakeLiveAsync_DuplicateKey_DetachesAndMarksInvalid()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.MonthlyOutputs.Add(new MonthlyOutput
            {
                TestCode = "DUP", Buyer = "B1", Month = 4,
                WorkGroup = "WG1", Volume = 5, FpsYear = DefaultFpsYear
            });
            context.StagingMonthlyOutputs.AddRange(
                new StagingMonthlyOutput
                {
                    Id = 1, ImportedBy = importedBy, Passed = true,
                    TestCode = "DUP", Buyer = "B1", Month = 4,
                    WorkGroup = "WG1", Volume = 10
                },
                new StagingMonthlyOutput
                {
                    Id = 2, ImportedBy = importedBy, Passed = false,
                    TestCode = "FAIL", Buyer = "F", Month = 1, WorkGroup = "FW"
                });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(1, result.ProcessedCount);
            Assert.Equal(0, result.ImportedCount);
            Assert.True(result.FailedCount >= 1);

            var failedRow = await context.StagingMonthlyOutputs.FirstOrDefaultAsync(x => x.Id == 1);
            Assert.NotNull(failedRow);
            Assert.False(failedRow.Passed);
            Assert.Equal("This record is no longer valid. Needs re-validating", failedRow.FailureComments);
        }

        [Fact]
        public async Task MakeLiveAsync_MixedDuplicateAndNew_OnlyNewImported()
        {
            const string importedBy = "tester";
            var (context, repo) = CreateInMemoryContext();

            context.MonthlyOutputs.Add(new MonthlyOutput
            {
                TestCode = "EXIST", Buyer = "B1", Month = 1,
                WorkGroup = "WG1", Volume = 1, FpsYear = DefaultFpsYear
            });
            context.StagingMonthlyOutputs.AddRange(
                new StagingMonthlyOutput
                {
                    Id = 1, ImportedBy = importedBy, Passed = true,
                    TestCode = "EXIST", Buyer = "B1", Month = 1,
                    WorkGroup = "WG1", Volume = 99
                },
                new StagingMonthlyOutput
                {
                    Id = 2, ImportedBy = importedBy, Passed = true,
                    TestCode = "NEW1", Buyer = "B_NEW", Month = 2,
                    WorkGroup = "WG_NEW", Volume = 50
                },
                new StagingMonthlyOutput
                {
                    Id = 3, ImportedBy = importedBy, Passed = false,
                    TestCode = "FAIL", Buyer = "F", Month = 1, WorkGroup = "FW"
                });
            await context.SaveChangesAsync();

            var result = await repo.MakeLiveAsync(importedBy);

            Assert.Equal(2, result.ProcessedCount);
            Assert.Equal(1, result.ImportedCount);
            Assert.True(result.FailedCount >= 1);

            Assert.True(await context.MonthlyOutputs.AnyAsync(x => x.TestCode == "NEW1"));
        }

        #endregion

        #region SearchStagingAsync — ApplyStagingFilter coverage

        [Fact]
        public async Task SearchStagingAsync_NullFilter_ReturnsAllUserRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = DefaultQuery();
            query.Filter = null;

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task SearchStagingAsync_EmptyFilter_ReturnsAllUserRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = DefaultQuery();
            query.Filter = "";

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task SearchStagingAsync_EmptyJsonFilter_ReturnsAllUserRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = DefaultQuery();
            query.Filter = "{}";

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task SearchStagingAsync_FilterWithUnknownKeysOnly_ReturnsAllUserRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = DefaultQuery();
            query.Filter = "{\"UnknownField\":\"value\"}";

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task SearchStagingAsync_FilterWithAllNullValues_ReturnsAllUserRows()
        {
            var repo = CreateRepositoryWithStaging(StagingSeedData());
            var query = DefaultQuery();
            query.Filter = "{\"WorkGroup\":null,\"TestCode\":null,\"Buyer\":null,\"FailureComments\":null}";

            var result = await repo.SearchStagingAsync(query, "user1", null);

            Assert.Equal(3, result.Data.Count);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — ApplyContainsFilter TryGetString false paths

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterTestCode_EmptyString_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"TestCode\":\"\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterTestCode_WhitespaceString_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"TestCode\":\"   \"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterBuyer_NullValue_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"Buyer\":null}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterWorkGroup_EmptyString_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"WorkGroup\":\"\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterUserId_EmptyString_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"UserId\":\"\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterInsertDelete_EmptyString_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"InsertDelete\":\"\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterWithAllEmptyStringValues_ReturnsAllLogs()
        {
            var repo = CreateRepository(SeedData());
            var query = DefaultQuery();
            query.Filter = "{\"TestCode\":\"\",\"Buyer\":\"\",\"WorkGroup\":\"\",\"UserId\":\"\",\"InsertDelete\":\"\"}";

            var result = await repo.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        #endregion
    }
}
