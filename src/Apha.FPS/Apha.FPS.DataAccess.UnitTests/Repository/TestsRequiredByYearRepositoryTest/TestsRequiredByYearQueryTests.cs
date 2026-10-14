using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.TestsRequiredByYearRepositoryTest
{
    public class TestsRequiredByYearQueryTests
    {
        private const int Year = 2025;

        private static FpsDbContext CreateContext(int contextYear = Year)
        {
            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var requestContext = new Mock<IFpsRequestContext>();
            requestContext.Setup(x => x.FpsYear).Returns(contextYear);
            requestContext.Setup(x => x.UserEmailId).Returns("test@example.com");

            return new FpsDbContext(options, requestContext.Object);
        }

        private static void AddWorkgroup(FpsDbContext ctx, string name, string pc, int year = Year) =>
            ctx.Workgroups.Add(new Workgroup { WorkGroupName = name, ProfitCentre = pc, FpsYear = year });

        private static void AddRequirement(FpsDbContext ctx, string test, string buyer, double? noRequired, int year = Year) =>
            ctx.TestRequirements.Add(new TestRequirement { TestCode = test, Buyer = buyer, NoRequired = noRequired, FpsYear = year });

        private static void AddProduct(FpsDbContext ctx, string code, string? desc, decimal? price, int year = Year) =>
            ctx.TestOrProducts.Add(new TestOrProduct { ItemCode = code, ItemDescription = desc, UnitPriceVla = price, FpsYear = year });

        private static void AddCapability(FpsDbContext ctx, string test, string wg, int year = Year) =>
            ctx.TestCapabilities.Add(new TestCapability { TestCode = test, WorkGroup = wg, PlanPortfolio = "P", FpsYear = year });

        private static void AddOutput(FpsDbContext ctx, short year, double month, string test, string wg, double? volume, string buyer = "B1") =>
            ctx.ArchiveMonthlyOutputs.Add(new ArchiveMonthlyOutput { Year = year, Month = month, TestCode = test, WorkGroup = wg, Volume = volume, Buyer = buyer });

        /// <summary>Two workgroups in ASU, one in Bact; release window Apr-2024..Mar-2025.</summary>
        private static FpsDbContext CreateSeededContext()
        {
            var ctx = CreateContext();
            ctx.ArchiveYears.AddRange(
                new ArchiveYear { Year = 2025, LatestMonthReleased = 3 },
                new ArchiveYear { Year = 2026, LatestMonthReleased = 1 });

            AddWorkgroup(ctx, "WG1", "ASU");
            AddWorkgroup(ctx, "WG2", "ASU");
            AddWorkgroup(ctx, "WG3", "Bact");
            AddWorkgroup(ctx, "WG1", "OldPc", 2024);

            AddRequirement(ctx, "TC1", "B1", 60);
            AddRequirement(ctx, "tc1 ", "B2", 40);
            AddRequirement(ctx, "TC2", "B1", 10);
            AddRequirement(ctx, "TC3", "B1", 0);
            AddRequirement(ctx, "TC4", "B1", 5);
            AddRequirement(ctx, "TC1", "B1", 999, 2024);

            AddProduct(ctx, "TC1", "Test one", 2.5m);
            AddProduct(ctx, "TC2", "Test two", 4m);
            AddProduct(ctx, "TC4", "Test four", 1m);

            AddCapability(ctx, "TC2", "WG3");
            AddCapability(ctx, "TC2", "WG2");
            AddCapability(ctx, "TC4", "UNKNOWN");

            ctx.TestRCCosts.Add(new TestRCCost { TestCode = "TC1", ProfitCentre = "ASU", Price = 9m, FpsYear = Year });

            AddOutput(ctx, 2024, 4, "TC1", "WG1", 30);
            AddOutput(ctx, 2025, 3, "TC1", "WG1", 30, "B2");
            AddOutput(ctx, 2024, 12, "TC1", "WG3", 40);
            AddOutput(ctx, 2024, 3, "TC1", "WG3", 1000);
            AddOutput(ctx, 2025, 4, "TC1", "WG3", 1000);

            ctx.SaveChanges();
            return ctx;
        }

        [Fact]
        public async Task Wg_SplitsRequirementByLastYearVolume()
        {
            using var ctx = CreateSeededContext();
            var rows = await new TestsRequiredByWgRepository(ctx).GetTestsRequiredByWgByYearAsync(null, Year);

            var wg1 = Assert.Single(rows, r => r.TestCode == "TC1" && r.WorkGroup == "WG1");
            Assert.Equal("ASU", wg1.ProfitCentre);
            Assert.Equal(60, wg1.ProjectedTotal);
            Assert.Equal(9m, wg1.UnitPrice);
            Assert.Equal("Test one", wg1.ItemDescription);

            var wg3 = Assert.Single(rows, r => r.TestCode == "TC1" && r.WorkGroup == "WG3");
            Assert.Equal("Bact", wg3.ProfitCentre);
            Assert.Equal(40, wg3.ProjectedTotal);
            Assert.Equal(2.5m, wg3.UnitPrice);
        }

        [Fact]
        public async Task Wg_WithoutHistory_UsesFirstCapableWorkgroup()
        {
            using var ctx = CreateSeededContext();
            var rows = await new TestsRequiredByWgRepository(ctx).GetTestsRequiredByWgByYearAsync(null, Year);

            var tc2 = Assert.Single(rows, r => r.TestCode == "TC2");
            Assert.Equal("WG2", tc2.WorkGroup);
            Assert.Equal(10, tc2.ProjectedTotal);
            Assert.Equal(4m, tc2.UnitPrice);
        }

        [Fact]
        public async Task Wg_ExcludesZeroRequirementsUnmappedWorkgroupsAndOtherYears()
        {
            using var ctx = CreateSeededContext();
            var rows = await new TestsRequiredByWgRepository(ctx).GetTestsRequiredByWgByYearAsync(null, Year);

            Assert.DoesNotContain(rows, r => r.TestCode == "TC3");
            Assert.DoesNotContain(rows, r => r.TestCode == "TC4");
            Assert.DoesNotContain(rows, r => r.ProfitCentre == "OldPc");
        }

        [Fact]
        public async Task Wg_IsOrderedByProfitCentreWorkGroupTestCode()
        {
            using var ctx = CreateSeededContext();
            var rows = await new TestsRequiredByWgRepository(ctx).GetTestsRequiredByWgByYearAsync(null, Year);

            Assert.Equal(
                new[] { "ASU|WG1|TC1", "ASU|WG2|TC2", "Bact|WG3|TC1" },
                rows.Select(r => $"{r.ProfitCentre}|{r.WorkGroup}|{r.TestCode}"));
        }

        [Theory]
        [InlineData("asu")]
        [InlineData(" ASU ")]
        public async Task Wg_FiltersByProfitCentreCaseInsensitive(string profitCentre)
        {
            using var ctx = CreateSeededContext();
            var rows = await new TestsRequiredByWgRepository(ctx).GetTestsRequiredByWgByYearAsync(profitCentre, Year);

            Assert.Equal(2, rows.Count);
            Assert.All(rows, r => Assert.Equal("ASU", r.ProfitCentre));
        }

        [Fact]
        public async Task Wg_ForYearWithNoData_ReturnsEmpty()
        {
            using var ctx = CreateSeededContext();
            Assert.Empty(await new TestsRequiredByWgRepository(ctx).GetTestsRequiredByWgByYearAsync(null, 2030));
        }

        [Fact]
        public async Task Wg_WithNoReleasedYear_UsesRequirementAndDefaultWorkgroup()
        {
            using var ctx = CreateContext();
            AddWorkgroup(ctx, "WG1", "ASU");
            AddRequirement(ctx, "TC1", "B1", 2.5);
            AddCapability(ctx, "TC1", "wg1 ");
            ctx.SaveChanges();

            var row = Assert.Single(await new TestsRequiredByWgRepository(ctx).GetTestsRequiredByWgByYearAsync(null, Year));

            Assert.Equal("WG1", row.WorkGroup);
            Assert.Equal(3, row.ProjectedTotal);
            Assert.Null(row.UnitPrice);
            Assert.Null(row.ItemDescription);
        }

        [Fact]
        public async Task Wg_WhenHistoryTotalIsZero_UsesFullRequirement()
        {
            using var ctx = CreateContext();
            ctx.ArchiveYears.Add(new ArchiveYear { Year = 2025, LatestMonthReleased = 12 });
            AddWorkgroup(ctx, "WG1", "ASU");
            AddRequirement(ctx, "TC1", "B1", 7);
            AddOutput(ctx, 2025, 1, "TC1", "WG1", 0);
            AddOutput(ctx, 2025, 2, "TC1", "WG1", null, "B2");
            ctx.SaveChanges();

            var row = Assert.Single(await new TestsRequiredByWgRepository(ctx).GetTestsRequiredByWgByYearAsync(null, Year));

            Assert.Equal(7, row.ProjectedTotal);
        }

        [Fact]
        public async Task Wg_WhenAllHistoryVolumesNull_UsesFullRequirement()
        {
            using var ctx = CreateContext();
            ctx.ArchiveYears.Add(new ArchiveYear { Year = 2025, LatestMonthReleased = 12 });
            AddWorkgroup(ctx, "WG1", "ASU");
            AddRequirement(ctx, "TC1", "B1", 4);
            AddOutput(ctx, 2025, 6, "TC1", "WG1", null);
            ctx.SaveChanges();

            var row = Assert.Single(await new TestsRequiredByWgRepository(ctx).GetTestsRequiredByWgByYearAsync(null, Year));

            Assert.Equal(4, row.ProjectedTotal);
        }

        [Fact]
        public async Task Wg_RoundsProportionalShareUp()
        {
            using var ctx = CreateContext();
            ctx.ArchiveYears.Add(new ArchiveYear { Year = 2025, LatestMonthReleased = 12 });
            AddWorkgroup(ctx, "WG1", "ASU");
            AddWorkgroup(ctx, "WG2", "ASU");
            AddRequirement(ctx, "TC1", "B1", 10);
            AddOutput(ctx, 2025, 1, "TC1", "WG1", 1);
            AddOutput(ctx, 2025, 1, "TC1", "WG2", 2);
            ctx.SaveChanges();

            var rows = await new TestsRequiredByWgRepository(ctx).GetTestsRequiredByWgByYearAsync(null, Year);

            Assert.Equal(4, rows.Single(r => r.WorkGroup == "WG1").ProjectedTotal);
            Assert.Equal(7, rows.Single(r => r.WorkGroup == "WG2").ProjectedTotal);
        }

        [Fact]
        public async Task Wg_RequirementWithOnlyNullNoRequired_IsExcluded()
        {
            using var ctx = CreateContext();
            AddWorkgroup(ctx, "WG1", "ASU");
            AddRequirement(ctx, "TC1", "B1", null);
            AddCapability(ctx, "TC1", "WG1");
            ctx.SaveChanges();

            Assert.Empty(await new TestsRequiredByWgRepository(ctx).GetTestsRequiredByWgByYearAsync(null, Year));
        }

        [Fact]
        public async Task Wg_IgnoresContextYearQueryFilter()
        {
            using var ctx = CreateContext(contextYear: 2020);
            AddWorkgroup(ctx, "WG1", "ASU");
            AddRequirement(ctx, "TC1", "B1", 3);
            AddCapability(ctx, "TC1", "WG1");
            ctx.SaveChanges();

            var row = Assert.Single(await new TestsRequiredByWgRepository(ctx).GetTestsRequiredByWgByYearAsync(null, Year));

            Assert.Equal(3, row.ProjectedTotal);
        }

        [Fact]
        public async Task Rc_AggregatesWorkgroupRowsByProfitCentre()
        {
            using var ctx = CreateContext();
            ctx.ArchiveYears.Add(new ArchiveYear { Year = 2025, LatestMonthReleased = 12 });
            AddWorkgroup(ctx, "WG1", "ASU");
            AddWorkgroup(ctx, "WG2", "ASU");
            AddWorkgroup(ctx, "WG3", "Bact");
            AddRequirement(ctx, "TC1", "B1", 30);
            AddProduct(ctx, "TC1", "Test one", 2m);
            AddOutput(ctx, 2025, 1, "TC1", "WG1", 1);
            AddOutput(ctx, 2025, 1, "TC1", "WG2", 1);
            AddOutput(ctx, 2025, 1, "TC1", "WG3", 1);
            ctx.SaveChanges();

            var rows = await new TestsRequiredByRcRepository(ctx).GetTestsRequiredByRcByYearAsync(null, Year);

            Assert.Equal(new[] { "ASU", "Bact" }, rows.Select(r => r.ProfitCentre));
            Assert.Equal(20, rows[0].ProjectedTotal);
            Assert.Equal(10, rows[1].ProjectedTotal);
            Assert.All(rows, r => Assert.Equal(2m, r.UnitPrice));
        }

        [Fact]
        public async Task Rc_FiltersByProfitCentre()
        {
            using var ctx = CreateSeededContext();
            var rows = await new TestsRequiredByRcRepository(ctx).GetTestsRequiredByRcByYearAsync("Bact", Year);

            var row = Assert.Single(rows);
            Assert.Equal("TC1", row.TestCode);
            Assert.Equal(40, row.ProjectedTotal);
        }

        [Fact]
        public async Task Rc_ForYearWithNoData_ReturnsEmpty()
        {
            using var ctx = CreateSeededContext();
            Assert.Empty(await new TestsRequiredByRcRepository(ctx).GetTestsRequiredByRcByYearAsync(null, 2030));
        }
    }
}
