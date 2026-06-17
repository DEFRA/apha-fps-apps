using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.TestOrProductRepositoryTest
{
    public class TestPriceCheckRepositoryTests
    {
        private static (FpsDbContext Context, TestorProductRepository Repo) CreateInMemoryContext(int fpsYear = 2024)
        {
            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);
            var context = new FpsDbContext(options, fpsRequestContext);
            var repo = new TestorProductRepository(context, fpsRequestContext);
            return (context, repo);
        }

        private static async Task SeedTestPriceCheckDataAsync(FpsDbContext context)
        {
            context.TestorProducts.AddRange(
                new TestorProduct { ItemCode = "T001", UnitPriceVla = 50m,  DefraUnitPrice = 80m,  Owner = "AB", FpsYear = 2024 },
                new TestorProduct { ItemCode = "T002", UnitPriceVla = 100m, DefraUnitPrice = 120m, Owner = "CD", FpsYear = 2024 }
            );
            context.Projects.AddRange(
                new Project
                {
                    ParentProject = "JOB001", IsDefraProject = 0,  Program = "PROG1", Manager = "Smith",
                    ProjectTitle = "Project One", Customer = "CUST1", Disease = "DIS1",
                    Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = 2024
                },
                new Project
                {
                    ParentProject = "JOB002", IsDefraProject = -1, Program = "PROG2", Manager = "Jones",
                    ProjectTitle = "Project Two", Customer = "CUST2", Disease = "DIS2",
                    Contract = "CON2", ProjectStatus = "A", IncomeAccountCode = "IAC2", FpsYear = 2024
                }
            );
            context.TestRequirements.AddRange(
                new TestRequirement { TestCode = "T001", Buyer = "JOB001", NoRequired = 5,  UnitPrice = 50m, FpsYear = 2024 },
                new TestRequirement { TestCode = "T002", Buyer = "JOB002", NoRequired = 10, UnitPrice = 0m,  FpsYear = 2024 }
            );
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        #region GetTestPriceCheckPagedAsync

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_PriceFilterAll_ReturnsMatchingRows()
        {
            var (context, repo) = CreateInMemoryContext();
            await SeedTestPriceCheckDataAsync(context);

            var parameters = new Apha.PACT.Core.Pagination.PaginationParameters<string>
            {
                Page = 1, PageSize = 10
            };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.NotNull(result);
            Assert.True(result.Data.Count > 0);
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_PriceFilterZero_ReturnsOnlyZeroPriceRows()
        {
            var (context, repo) = CreateInMemoryContext();
            await SeedTestPriceCheckDataAsync(context);

            var parameters = new Apha.PACT.Core.Pagination.PaginationParameters<string>
            {
                Page = 1, PageSize = 10
            };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "zero", null);

            Assert.NotNull(result);
            Assert.All(result.Data, row => Assert.Equal(0m, row.TestPrice));
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_OwnerFilter_ReturnsOnlyMatchingOwner()
        {
            var (context, repo) = CreateInMemoryContext();
            await SeedTestPriceCheckDataAsync(context);

            var parameters = new Apha.PACT.Core.Pagination.PaginationParameters<string>
            {
                Page = 1, PageSize = 10
            };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", "AB");

            Assert.NotNull(result);
            Assert.All(result.Data, row => Assert.Equal("AB", row.Owner));
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_SetsNormalPriceOnRows()
        {
            var (context, repo) = CreateInMemoryContext();
            await SeedTestPriceCheckDataAsync(context);

            var parameters = new Apha.PACT.Core.Pagination.PaginationParameters<string>
            {
                Page = 1, PageSize = 10
            };

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
            var (context, repo) = CreateInMemoryContext();
            await SeedTestPriceCheckDataAsync(context);

            var parameters = new Apha.PACT.Core.Pagination.PaginationParameters<string>
            {
                Page = 1, PageSize = 10
            };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.All(result.Data, row =>
                Assert.Equal(row.TestPrice == 0m, row.IsZeroPrice));
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_NoMatchingData_ReturnsEmpty()
        {
            var (context, repo) = CreateInMemoryContext();

            var parameters = new Apha.PACT.Core.Pagination.PaginationParameters<string>
            {
                Page = 1, PageSize = 10
            };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetTestPriceCheckByKeyAsync

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_ExistingKey_ReturnsRow()
        {
            var (context, repo) = CreateInMemoryContext();
            await SeedTestPriceCheckDataAsync(context);

            var result = await repo.GetTestPriceCheckByKeyAsync("T001", "JOB001");

            Assert.NotNull(result);
            Assert.Equal("T001",   result.TestCode);
            Assert.Equal("JOB001", result.JobCode);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_ExistingKey_SetsNormalPrice()
        {
            var (context, repo) = CreateInMemoryContext();
            await SeedTestPriceCheckDataAsync(context);

            var result = await repo.GetTestPriceCheckByKeyAsync("T001", "JOB001");

            Assert.NotNull(result);
            // T001/JOB001 — IsDefraProject=0 so NormalPrice = UnitPriceVla = 50m
            Assert.Equal(50m, result.NormalPrice);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_DefraProject_SetsNormalPriceToDefraUnitPrice()
        {
            var (context, repo) = CreateInMemoryContext();
            await SeedTestPriceCheckDataAsync(context);

            var result = await repo.GetTestPriceCheckByKeyAsync("T002", "JOB002");

            Assert.NotNull(result);
            // T002/JOB002 — IsDefraProject=-1 so NormalPrice = DefraUnitPrice = 120m
            Assert.Equal(120m, result.NormalPrice);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_NonExistentKey_ReturnsNull()
        {
            var (context, repo) = CreateInMemoryContext();
            await SeedTestPriceCheckDataAsync(context);

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
            await SeedTestPriceCheckDataAsync(context);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.UpdateTestPriceCheckAsync("T001", "JOB001", 0, 50m, 80m));
        }

        #endregion
    }
}
