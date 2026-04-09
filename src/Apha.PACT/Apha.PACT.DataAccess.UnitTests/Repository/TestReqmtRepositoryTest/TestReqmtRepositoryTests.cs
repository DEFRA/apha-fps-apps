using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.TestReqmtRepositoryTest
{
    public class TestReqmtRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        private static (
            TestReqmtRepository Repo,
            Mock<DbSet<TestReqmt>> TestReqmtsDbSet,
            Mock<DbSet<MonthlyOutput>> MonthlyOutputsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<TestReqmt>? testReqmts = null,
                IEnumerable<MonthlyOutput>? monthlyOutputs = null,
                int fpsYear = DefaultFpsYear)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(fpsYear);

            var currentUserContext = Substitute.For<ICurrentUserContext>();
            currentUserContext.UserId.Returns("test-user");

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var testReqmtsMockSet = RepositoryTestHelper.CreateMockDbSet(testReqmts ?? []);
            RepositoryTestHelper.SetupDbSetOperations(testReqmtsMockSet);
            testReqmtsMockSet
                .Setup(x => x.AddAsync(It.IsAny<TestReqmt>(), It.IsAny<CancellationToken>()))
                .Returns((TestReqmt _, CancellationToken __) => new ValueTask<EntityEntry<TestReqmt>>());

            var monthlyOutputsMockSet = RepositoryTestHelper.CreateMockDbSet(monthlyOutputs ?? []);

            var testReqLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<TestReqLog>());
            RepositoryTestHelper.SetupDbSetOperations(testReqLogsMockSet);
            testReqLogsMockSet
                .Setup(x => x.AddAsync(It.IsAny<TestReqLog>(), It.IsAny<CancellationToken>()))
                .Returns((TestReqLog _, CancellationToken __) => new ValueTask<EntityEntry<TestReqLog>>());

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TestReqmts).Returns(testReqmtsMockSet.Object);
            mockContext.Setup(x => x.MonthlyOutputs).Returns(monthlyOutputsMockSet.Object);
            mockContext.Setup(x => x.TestReqLogs).Returns(testReqLogsMockSet.Object);

            var repo = new TestReqmtRepository(mockContext.Object, fpsYearContext, currentUserContext);
            return (repo, testReqmtsMockSet, monthlyOutputsMockSet, mockContext);
        }

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_MatchingTestCode_ReturnsMatchingRecords()
        {
            var testReqmts = new List<TestReqmt>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_NoMatchingTestCode_ReturnsEmptyList()
        {
            var testReqmts = new List<TestReqmt>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "MISSING");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithBuyerFilter_FiltersCorrectly()
        {
            var testReqmts = new List<TestReqmt>
            {
                new() { TestCode = "BLOOD", Buyer = "ALPHA", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "BETA",  FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"ALP\"}"
            };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("ALPHA", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_EmptyRepository_ReturnsEmpty()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_MatchingRecord_ReturnsEntity()
        {
            var testReqmts = new List<TestReqmt>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.GetByIdAsync("BLOOD", "PRJ1");

            Assert.NotNull(result);
            Assert.Equal("BLOOD", result.TestCode);
            Assert.Equal("PRJ1", result.Buyer);
        }

        [Fact]
        public async Task GetByIdAsync_TestCodeNotFound_ReturnsNull()
        {
            var testReqmts = new List<TestReqmt>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.GetByIdAsync("MISSING", "PRJ1");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_BuyerNotFound_ReturnsNull()
        {
            var testReqmts = new List<TestReqmt>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.GetByIdAsync("BLOOD", "PRJ_WRONG");

            Assert.Null(result);
        }

        #endregion

        #region ExistsByTestBuyerCodeAsync

        [Fact]
        public async Task ExistsByTestBuyerCodeAsync_CodeExists_ReturnsTrue()
        {
            var testReqmts = new List<TestReqmt>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", TestBuyerCode = "BLOOD-WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.ExistsByTestBuyerCodeAsync("BLOOD-WG1");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByTestBuyerCodeAsync_CodeNotExists_ReturnsFalse()
        {
            var testReqmts = new List<TestReqmt>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", TestBuyerCode = "BLOOD-WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.ExistsByTestBuyerCodeAsync("MISSING-CODE");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTestBuyerCodeAsync_EmptyRepository_ReturnsFalse()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();

            var result = await repo.ExistsByTestBuyerCodeAsync("ANY-CODE");

            Assert.False(result);
        }

        #endregion

        #region ExistsByTestCodeAndBuyerInMonthlyOutputAsync

        [Fact]
        public async Task ExistsByTestCodeAndBuyerInMonthlyOutputAsync_RecordExists_ReturnsTrue()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", WorkGroup = "WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(monthlyOutputs: monthlyOutputs);

            var result = await repo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ1");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndBuyerInMonthlyOutputAsync_NoRecord_ReturnsFalse()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();

            var result = await repo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ1");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndBuyerInMonthlyOutputAsync_DifferentBuyer_ReturnsFalse()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", WorkGroup = "WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(monthlyOutputs: monthlyOutputs);

            var result = await repo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ_OTHER");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndBuyerInMonthlyOutputAsync_DifferentTestCode_ReturnsFalse()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", WorkGroup = "WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(monthlyOutputs: monthlyOutputs);

            var result = await repo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("URINE", "PRJ1");

            Assert.False(result);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_SetsEntityFpsYearFromContext()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks(fpsYear: 2025);
            var entity = new TestReqmt { TestCode = "BLOOD", Buyer = "PRJ1" };

            var result = await repo.AddAsync(entity);

            Assert.Equal(2025, result.FpsYear);
        }

        [Fact]
        public async Task AddAsync_SetsDateCreated()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();
            var entity = new TestReqmt { TestCode = "BLOOD", Buyer = "PRJ1" };

            await repo.AddAsync(entity);

            Assert.NotNull(entity.DateCreated);
        }

        [Fact]
        public async Task AddAsync_CallsSaveChanges()
        {
            var (repo, _, _, mockContext) = CreateRepositoryWithMocks();
            var entity = new TestReqmt { TestCode = "BLOOD", Buyer = "PRJ1" };

            await repo.AddAsync(entity);

            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_RecordExistsWithCorrectYear_RemovesAndReturnsTrue()
        {
            var testReqmts = new List<TestReqmt>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, dbSetMock, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.DeleteAsync("BLOOD", "PRJ1");

            Assert.True(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestReqmt>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_RecordNotFound_ReturnsFalse()
        {
            var testReqmts = new List<TestReqmt>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, dbSetMock, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.DeleteAsync("MISSING", "PRJ1");

            Assert.False(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestReqmt>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_RecordExistsButWrongFpsYear_ReturnsFalse()
        {
            var testReqmts = new List<TestReqmt>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = 2023 }
            };
            var (repo, dbSetMock, _, _) = CreateRepositoryWithMocks(testReqmts, fpsYear: DefaultFpsYear);

            var result = await repo.DeleteAsync("BLOOD", "PRJ1");

            Assert.False(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestReqmt>()), Times.Never);
        }

        #endregion
    }
}
