using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.TestOrProductRepositoryTest
{
    public class TestPriceCheckRepositoryTests
    {
        private const string UserEmail = "test.user@example.com";
        private const int DefaultFpsYear = 2024;

        // ── Moq-based factory used by read tests (GetPaged / GetByKey) ─────────────
        // BuildTestPriceCheckBaseQuery joins TestRequirements → ProjectViews → TestorProducts
        // and filters by EF.Functions.ILike(p.UserEmail, UserEmailId).
        // The in-memory provider does not support ILike; the Moq path feeds real data
        // through TestAsyncQueryProvider whose LikeRewriter converts ILike → Contains.
        private static TestorProductRepository CreateRepositoryWithMocks(
            IEnumerable<TestRequirement>?  testRequirements = null,
            IEnumerable<ProjectView>?      projectViews     = null,
            IEnumerable<TestorProduct>?    testorProducts   = null,
            int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);
            fpsRequestContext.UserEmailId.Returns(UserEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var testReqSet      = RepositoryTestHelper.CreateMockDbSet(testRequirements ?? []);
            var projectViewSet  = RepositoryTestHelper.CreateMockDbSet(projectViews     ?? []);
            var testorProductSet = RepositoryTestHelper.CreateMockDbSet(testorProducts  ?? []);

            RepositoryTestHelper.SetupDbSetOperations(testReqSet);
            RepositoryTestHelper.SetupDbSetOperations(projectViewSet);
            RepositoryTestHelper.SetupDbSetOperations(testorProductSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TestRequirements).Returns(testReqSet.Object);
            mockContext.Setup(x => x.ProjectViews).Returns(projectViewSet.Object);
            mockContext.Setup(x => x.TestorProducts).Returns(testorProductSet.Object);

            return new TestorProductRepository(mockContext.Object, fpsRequestContext);
        }

        // ── Shared seed data ──────────────────────────────────────────────────────
        // T001 / JOB001 — non-Defra  → NormalPrice = UnitPriceVla  = 50m, TestPrice = 50m (standard)
        // T002 / JOB002 — Defra      → NormalPrice = DefraUnitPrice = 120m, TestPrice = 0m (zero)

        private static IEnumerable<TestRequirement> SeedRequirements() =>
        [
            new TestRequirement { TestCode = "T001", Buyer = "JOB001", NoRequired = 5,  UnitPrice = 50m, FpsYear = DefaultFpsYear },
            new TestRequirement { TestCode = "T002", Buyer = "JOB002", NoRequired = 10, UnitPrice = 0m,  FpsYear = DefaultFpsYear }
        ];

        private static IEnumerable<ProjectView> SeedProjectViews() =>
        [
            new ProjectView { ParentProject = "JOB001", IsDefraProject = 0,  Program = "PROG1", Manager = "Smith", UserEmail = UserEmail },
            new ProjectView { ParentProject = "JOB002", IsDefraProject = -1, Program = "PROG2", Manager = "Jones", UserEmail = UserEmail }
        ];

        private static IEnumerable<TestorProduct> SeedTestorProducts() =>
        [
            new TestorProduct { ItemCode = "T001", UnitPriceVla = 50m,  DefraUnitPrice = 80m,  Owner = "AB", FpsYear = DefaultFpsYear },
            new TestorProduct { ItemCode = "T002", UnitPriceVla = 100m, DefraUnitPrice = 120m, Owner = "CD", FpsYear = DefaultFpsYear }
        ];

        // ── In-memory factory — used only by UpdateTestPriceCheckAsync test ───────
        private static (FpsDbContext Context, TestorProductRepository Repo) CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);
            fpsRequestContext.UserEmailId.Returns(UserEmail);
            var context = new FpsDbContext(options, fpsRequestContext);
            var repo    = new TestorProductRepository(context, fpsRequestContext);
            return (context, repo);
        }

        private static async Task SeedInMemoryAsync(FpsDbContext context)
        {
            context.TestorProducts.AddRange(SeedTestorProducts());
            context.Projects.AddRange(
                new Project { ParentProject = "JOB001", IsDefraProject = 0,  Program = "PROG1", Manager = "Smith", ProjectTitle = "Project One", Customer = "CUST1", Disease = "DIS1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear },
                new Project { ParentProject = "JOB002", IsDefraProject = -1, Program = "PROG2", Manager = "Jones", ProjectTitle = "Project Two", Customer = "CUST2", Disease = "DIS2", Contract = "CON2", ProjectStatus = "A", IncomeAccountCode = "IAC2", FpsYear = DefaultFpsYear }
            );
            context.TestRequirements.AddRange(SeedRequirements());
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        #region GetTestPriceCheckPagedAsync

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_PriceFilterAll_ReturnsMatchingRows()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.NotNull(result);
            Assert.True(result.Data.Count > 0);
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_PriceFilterZero_ReturnsOnlyZeroPriceRows()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "zero", null);

            Assert.NotNull(result);
            Assert.All(result.Data, row => Assert.Equal(0m, row.TestPrice));
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_OwnerFilter_ReturnsOnlyMatchingOwner()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", "AB");

            Assert.NotNull(result);
            Assert.All(result.Data, row => Assert.Equal("AB", row.Owner));
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_SetsNormalPriceOnRows()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.All(result.Data, row =>
            {
                var expected = row.IsDefraProject != 0 ? row.DefraUnitPrice : row.UnitPriceVla;
                Assert.Equal(expected, row.NormalPrice);
            });
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_SetsIsZeroPriceFlag()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.All(result.Data, row =>
                Assert.Equal(row.TestPrice == 0m, row.IsZeroPrice));
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_NoMatchingData_ReturnsEmpty()
        {
            var repo = CreateRepositoryWithMocks();
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetTestPriceCheckByKeyAsync

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_ExistingKey_ReturnsRow()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());

            var result = await repo.GetTestPriceCheckByKeyAsync("T001", "JOB001");

            Assert.NotNull(result);
            Assert.Equal("T001",   result.TestCode);
            Assert.Equal("JOB001", result.JobCode);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_ExistingKey_SetsNormalPrice()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());

            var result = await repo.GetTestPriceCheckByKeyAsync("T001", "JOB001");

            Assert.NotNull(result);
            // T001/JOB001 — IsDefraProject=0 → NormalPrice = UnitPriceVla = 50m
            Assert.Equal(50m, result.NormalPrice);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_DefraProject_SetsNormalPriceToDefraUnitPrice()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());

            var result = await repo.GetTestPriceCheckByKeyAsync("T002", "JOB002");

            Assert.NotNull(result);
            // T002/JOB002 — IsDefraProject=-1 → NormalPrice = DefraUnitPrice = 120m
            Assert.Equal(120m, result.NormalPrice);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_NonExistentKey_ReturnsNull()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());

            var result = await repo.GetTestPriceCheckByKeyAsync("MISSING", "MISSING");

            Assert.Null(result);
        }

        #endregion

        #region UpdateTestPriceCheckAsync

        // ExecuteUpdateAsync issues a bulk SQL UPDATE and is not supported by the EF Core
        // in-memory provider. Update behaviour is tested at the service layer instead.
        // This test confirms the expected provider limitation is thrown, ensuring the method
        // is wired to ExecuteUpdateAsync (not a load-and-save pattern).
        [Fact]
        public async Task UpdateTestPriceCheckAsync_NotSupportedByInMemoryProvider_ThrowsInvalidOperationException()
        {
            var (context, repo) = CreateInMemoryContext();
            await SeedInMemoryAsync(context);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.UpdateTestPriceCheckAsync("T001", "JOB001", 0, 50m, 80m));
        }

        #endregion
    }
}
