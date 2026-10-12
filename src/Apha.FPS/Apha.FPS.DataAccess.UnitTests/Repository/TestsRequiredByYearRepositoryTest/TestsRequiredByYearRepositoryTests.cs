using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.TestsRequiredByYearRepositoryTest
{
    /// <summary>
    /// The year-specific queries use PostgreSQL raw SQL (FromSqlRaw), which the InMemory provider
    /// cannot execute. These tests verify the repository builds the raw SQL query and that the
    /// SQL text contains the year/resource centre filtering ported from the export views.
    /// </summary>
    public class TestsRequiredByYearRepositoryTests
    {
        private static FpsDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var requestContext = new Mock<IFpsRequestContext>();
            requestContext.Setup(x => x.FpsYear).Returns(2025);
            requestContext.Setup(x => x.UserEmailId).Returns("test@example.com");

            return new FpsDbContext(options, requestContext.Object);
        }

        [Theory]
        [InlineData("ASU")]
        [InlineData(null)]
        public async Task GetTestsRequiredByWgByYearAsync_ExecutesRawSqlQuery(string? profitCentre)
        {
            using var context = CreateContext();
            var repository = new TestsRequiredByWgRepository(context);

            // InMemory provider rejects raw SQL, proving the FromSqlRaw query was issued.
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repository.GetTestsRequiredByWgByYearAsync(profitCentre, 2025));
        }

        [Theory]
        [InlineData(" ASU ")]
        [InlineData(null)]
        public async Task GetTestsRequiredByRcByYearAsync_ExecutesRawSqlQuery(string? profitCentre)
        {
            using var context = CreateContext();
            var repository = new TestsRequiredByRcRepository(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repository.GetTestsRequiredByRcByYearAsync(profitCentre, 2025));
        }

        [Fact]
        public void ByWorkGroupSql_IsParameterisedOnYearAndResourceCentre()
        {
            var sql = TestsRequiredByYearSql.ByWorkGroup;

            Assert.Contains("{0}::integer AS fpsyear", sql);
            Assert.Contains("{1}::text", sql);
            Assert.Contains("y.year <= p.fpsyear", sql);
            Assert.Contains("JOIN params p ON t.fpsyear = p.fpsyear", sql);
            Assert.Contains("HAVING sum(t.norequired) <> 0", sql);
            Assert.Contains("lower(yw.profitcentre) = lower(p.pc)", sql);
            Assert.Contains("SELECT profitcentre, workgroup, testcode, itemdescription, projectedtotal, unitprice", sql);
            Assert.DoesNotContain("vqrytestsrequiredbywg_export", sql);
        }

        [Fact]
        public void ByResourceCentreSql_AggregatesWorkGroupRowsByResourceCentre()
        {
            var sql = TestsRequiredByYearSql.ByResourceCentre;

            Assert.Contains("{0}::integer AS fpsyear", sql);
            Assert.Contains("sum(projectedtotal)::integer AS projectedtotal", sql);
            Assert.Contains("GROUP BY profitcentre, testcode, itemdescription, unitprice", sql);
            Assert.DoesNotContain("vqrytestsrequiredbyrc_export", sql);
        }
    }
}
