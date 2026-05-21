using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.ProfitCentreRepositoryTest
{
    public class ProfitCentreRepositoryTests
    {
        private static (
            ProfitCentreRepository Repo,
            Mock<DbSet<PactProfitCentreView>> ProfitCentreViewsDbSet,
            Mock<DbSet<ProfitCentre>> ProfitCentresDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<PactProfitCentreView>? views = null,
                IEnumerable<ProfitCentre>? profitCentres = null)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var viewsMockSet = RepositoryTestHelper.CreateMockDbSet(views ?? []);
            var profitCentresMockSet = RepositoryTestHelper.CreateMockDbSet(profitCentres ?? []);

            mockContext.Setup(x => x.PactProfitCentreViews).Returns(viewsMockSet.Object);
            mockContext.Setup(x => x.ProfitCentres).Returns(profitCentresMockSet.Object);

            var repo = new ProfitCentreRepository(mockContext.Object);
            return (repo, viewsMockSet, profitCentresMockSet, mockContext);
        }

        private static ProfitCentreRepository CreateRepository(
            IEnumerable<PactProfitCentreView>? views = null,
            IEnumerable<ProfitCentre>? profitCentres = null)
            => CreateRepositoryWithMocks(views, profitCentres).Repo;

        #region GetAllProfitCentresAsync

        [Fact]
        public async Task GetAllProfitCentresAsync_WithData_ReturnsOrderedByProfitCentre()
        {
            var views = new List<PactProfitCentreView>
            {
                new() { ProfitCentre = "ZPC", ProfitCentreName = "Z Centre" },
                new() { ProfitCentre = "APC", ProfitCentreName = "A Centre" },
                new() { ProfitCentre = "MPC", ProfitCentreName = "M Centre" }
            };
            var repo = CreateRepository(views);

            var result = (await repo.GetAllProfitCentresAsync()).ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal("APC", result[0].ProfitCentre);
            Assert.Equal("MPC", result[1].ProfitCentre);
            Assert.Equal("ZPC", result[2].ProfitCentre);
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_EmptyData_ReturnsEmptyList()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetAllProfitCentresAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_SingleEntry_ReturnsSingleItem()
        {
            var views = new List<PactProfitCentreView>
            {
                new() { ProfitCentre = "PC1", ProfitCentreName = "Centre One" }
            };
            var repo = CreateRepository(views);

            var result = (await repo.GetAllProfitCentresAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("PC1", result[0].ProfitCentre);
        }

        #endregion

        #region GetProfitCentreSettingsAsync

        [Fact]
        public async Task GetProfitCentreSettingsAsync_MatchingProfitCentre_ReturnsEntity()
        {
            var views = new List<PactProfitCentreView>
            {
                new() { ProfitCentre = "PC1", Timesheet = 1, Outputsheet = 2, TimesheetLayout = 1 },
                new() { ProfitCentre = "PC2", Timesheet = 0, Outputsheet = 0, TimesheetLayout = 2 }
            };
            var repo = CreateRepository(views);

            var result = await repo.GetProfitCentreSettingsAsync("PC1");

            Assert.NotNull(result);
            Assert.Equal("PC1", result.ProfitCentre);
            Assert.Equal(1, result.Timesheet);
            Assert.Equal(2, result.Outputsheet);
            Assert.Equal((short)1, result.TimesheetLayout);
        }

        [Fact]
        public async Task GetProfitCentreSettingsAsync_NoMatchingProfitCentre_ReturnsNull()
        {
            var views = new List<PactProfitCentreView>
            {
                new() { ProfitCentre = "PC1" }
            };
            var repo = CreateRepository(views);

            var result = await repo.GetProfitCentreSettingsAsync("MISSING");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProfitCentreSettingsAsync_EmptyData_ReturnsNull()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetProfitCentreSettingsAsync("PC1");

            Assert.Null(result);
        }

        #endregion

        #region UpdateProfitCentreSettingsAsync

        // UpdateProfitCentreSettingsAsync uses ExecuteUpdateAsync which requires a real
        // EF Core provider and cannot be tested with the mock LINQ provider.
        // Behaviour is exercised through integration/acceptance tests.

        #endregion
    }
}
