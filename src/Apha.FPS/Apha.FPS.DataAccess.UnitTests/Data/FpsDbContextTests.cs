using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Data
{
    /// <summary>
    /// Tests that exercise <see cref="FpsDbContext"/> construction and model configuration,
    /// targeting the newly added <see cref="FpsDbContext.ProjectProfitabilityVlaViews"/> DbSet
    /// and its associated <see cref="ProjectProfitabilityVlaViewMap"/> registration.
    /// Uses the EF Core InMemory provider so that <c>OnModelCreating</c> is executed
    /// without requiring a real PostgreSQL connection.
    /// </summary>
    public class FpsDbContextTests
    {
        private const int TestFpsYear = 2024;

        private static FpsDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var requestContext = new Mock<IFpsRequestContext>();
            requestContext.Setup(x => x.FpsYear).Returns(TestFpsYear);
            requestContext.Setup(x => x.UserEmailId).Returns("test@example.com");

            return new FpsDbContext(options, requestContext.Object);
        }

        // ── Constructor / property ────────────────────────────────────────────────

        [Fact]
        public void Constructor_SetsFilterFpsYear_FromRequestContext()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            Assert.Equal(TestFpsYear, ctx.FilterFpsYear);
        }

        [Fact]
        public void ProjectProfitabilityVlaViews_PropertyIsAccessible()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            Assert.NotNull(ctx.ProjectProfitabilityVlaViews);
        }

        // ── OnModelCreating — keyless registration ────────────────────────────────

        [Fact]
        public void OnModelCreating_ProjectProfitabilityVlaView_IsRegisteredAsKeyless()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());

            var entityType = ctx.Model.FindEntityType(typeof(ProjectProfitabilityVlaView));

            Assert.NotNull(entityType);
            Assert.Null(entityType.FindPrimaryKey());   // HasNoKey() leaves no PK
        }

        [Fact]
        public void OnModelCreating_ProjectProfitabilityVlaView_MapsToCorrectView()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());

            var entityType = ctx.Model.FindEntityType(typeof(ProjectProfitabilityVlaView));

            Assert.NotNull(entityType);
            Assert.Equal("vprojectprofitabilityvla", entityType.GetViewName());
            Assert.Equal("fps", entityType.GetViewSchema());
        }

        // ── Column mappings ───────────────────────────────────────────────────────

        [Theory]
        [InlineData(nameof(ProjectProfitabilityVlaView.Id),             "id")]
        [InlineData(nameof(ProjectProfitabilityVlaView.JobCode),        "jobcode")]
        [InlineData(nameof(ProjectProfitabilityVlaView.Program),        "program")]
        [InlineData(nameof(ProjectProfitabilityVlaView.Customer),       "customer")]
        [InlineData(nameof(ProjectProfitabilityVlaView.Manager),        "manager")]
        [InlineData(nameof(ProjectProfitabilityVlaView.Status),         "status")]
        [InlineData(nameof(ProjectProfitabilityVlaView.StaffCosts),     "staffcosts")]
        [InlineData(nameof(ProjectProfitabilityVlaView.TestCost),       "testcost")]
        [InlineData(nameof(ProjectProfitabilityVlaView.AnimalCosts),    "animalcosts")]
        [InlineData(nameof(ProjectProfitabilityVlaView.AdditionalCosts),"additionalcosts")]
        [InlineData(nameof(ProjectProfitabilityVlaView.TotalCosts),     "totalcosts")]
        [InlineData(nameof(ProjectProfitabilityVlaView.Budget),         "budget")]
        [InlineData(nameof(ProjectProfitabilityVlaView.Profit),         "profit")]
        [InlineData(nameof(ProjectProfitabilityVlaView.TargetProfit),   "targetprofit")]
        [InlineData(nameof(ProjectProfitabilityVlaView.OffTarget),      "offtarget")]
        public void OnModelCreating_ProjectProfitabilityVlaView_ColumnsMappedCorrectly(
            string propertyName, string expectedColumnName)
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());

            var entityType = ctx.Model.FindEntityType(typeof(ProjectProfitabilityVlaView));
            Assert.NotNull(entityType);

            var property = entityType.FindProperty(propertyName);
            Assert.NotNull(property);
            Assert.Equal(expectedColumnName, property.GetColumnName());
        }

        // ── VQryFrmTimeSellerPcViews / TimeSellerPcViewMap ────────────────────────

        [Fact]
        public void VQryFrmTimeSellerPcViews_PropertyIsAccessible()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            Assert.NotNull(ctx.VQryFrmTimeSellerPcViews);
        }

        [Fact]
        public void OnModelCreating_TimeSellerPcView_IsRegisteredAsKeyless()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());

            var entityType = ctx.Model.FindEntityType(typeof(TimeSellerPcView));

            Assert.NotNull(entityType);
            Assert.Null(entityType.FindPrimaryKey());
        }

        [Fact]
        public void OnModelCreating_TimeSellerPcView_MapsToCorrectView()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());

            var entityType = ctx.Model.FindEntityType(typeof(TimeSellerPcView));

            Assert.NotNull(entityType);
            Assert.Equal("vqryfrmtimesellerpc", entityType.GetViewName());
            Assert.Equal("fps", entityType.GetViewSchema());
        }

        [Theory]
        [InlineData(nameof(TimeSellerPcView.ContTarget),        "conttarget")]
        [InlineData(nameof(TimeSellerPcView.SellingPc),         "sellingpc")]
        [InlineData(nameof(TimeSellerPcView.ChargeRate),        "chargerate")]
        [InlineData(nameof(TimeSellerPcView.Ohr),               "ohr")]
        [InlineData(nameof(TimeSellerPcView.SumOfGenBid),       "sumofgenbid")]
        [InlineData(nameof(TimeSellerPcView.WorkGroup),         "workgroup")]
        [InlineData(nameof(TimeSellerPcView.ProfitCentreGrade), "profitcentregrade")]
        [InlineData(nameof(TimeSellerPcView.WgGrade),           "wggrade")]
        [InlineData(nameof(TimeSellerPcView.AppHours),          "apphours")]
        [InlineData(nameof(TimeSellerPcView.Hrs),               "hrs")]
        [InlineData(nameof(TimeSellerPcView.AvHrs),             "avhrs")]
        [InlineData(nameof(TimeSellerPcView.Fec),               "fec")]
        [InlineData(nameof(TimeSellerPcView.AppFec),            "appfec")]
        [InlineData(nameof(TimeSellerPcView.Contribution),      "contribution")]
        [InlineData(nameof(TimeSellerPcView.FpsYear),           "fpsyear")]
        [InlineData(nameof(TimeSellerPcView.UserId),            "user_id")]
        [InlineData(nameof(TimeSellerPcView.Dt2Username),       "dt2username")]
        [InlineData(nameof(TimeSellerPcView.UserEmail),         "useremail")]
        public void OnModelCreating_TimeSellerPcView_ColumnsMappedCorrectly(
            string propertyName, string expectedColumnName)
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());

            var entityType = ctx.Model.FindEntityType(typeof(TimeSellerPcView));
            Assert.NotNull(entityType);

            var property = entityType.FindProperty(propertyName);
            Assert.NotNull(property);
            Assert.Equal(expectedColumnName, property.GetColumnName());
        }
    }
}
