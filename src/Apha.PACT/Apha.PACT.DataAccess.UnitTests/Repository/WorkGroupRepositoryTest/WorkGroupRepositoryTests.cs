using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.WorkGroupRepositoryTest
{
    public class WorkGroupRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        /// <summary>
        /// Simple read-only helper: no FpsYear dependency needed for GetAllWorkGroupsAsync.
        /// </summary>
        private static (
            WorkGroupRepository Repo,
            Mock<DbSet<WorkGroup>> WorkGroupsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<WorkGroup> workGroups)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            var workGroupsMockSet = RepositoryTestHelper.CreateMockDbSet(workGroups);

            mockContext.Setup(x => x.WorkGroups).Returns(workGroupsMockSet.Object);

            var repo = new WorkGroupRepository(mockContext.Object);
            return (repo, workGroupsMockSet, mockContext);
        }

        private static WorkGroupRepository CreateRepository(IEnumerable<WorkGroup> workGroups)
            => CreateRepositoryWithMocks(workGroups).Repo;

        /// <summary>
        /// Full helper for methods that use FilterFpsYear, ProfitCentres, or update operations.
        /// </summary>
        private static (
            WorkGroupRepository Repo,
            Mock<DbSet<WorkGroup>> WorkGroupsDbSet,
            Mock<DbSet<ProfitCentre>> ProfitCentresDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithFpsYear(
                IEnumerable<WorkGroup>? workGroups = null,
                IEnumerable<ProfitCentre>? profitCentres = null,
                int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var workGroupsMockSet = RepositoryTestHelper.CreateMockDbSet(workGroups ?? []);
            var profitCentresMockSet = RepositoryTestHelper.CreateMockDbSet(profitCentres ?? []);

            mockContext.Setup(x => x.WorkGroups).Returns(workGroupsMockSet.Object);
            mockContext.Setup(x => x.ProfitCentres).Returns(profitCentresMockSet.Object);

            var repo = new WorkGroupRepository(mockContext.Object);
            return (repo, workGroupsMockSet, profitCentresMockSet, mockContext);
        }

        #region GetAllWorkGroupsAsync

        [Fact]
        public async Task GetAllWorkGroupsAsync_WithData_ReturnsOrderedList()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "ZGroup", ProfitCentre = "PC2" },
                new() { WorkGroupName = "AGroup", ProfitCentre = "PC1" },
                new() { WorkGroupName = "MGroup", ProfitCentre = "PC3" }
            };
            var repo = CreateRepository(workGroups);

            var result = (await repo.GetAllWorkGroupsAsync()).ToList();

            Assert.Equal(3, result.Count);
            // OrderBy(w => w.WorkGroupName) applied in repository
            Assert.Equal("AGroup", result[0].WorkGroupName);
            Assert.Equal("MGroup", result[1].WorkGroupName);
            Assert.Equal("ZGroup", result[2].WorkGroupName);
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_EmptyData_ReturnsEmptyList()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetAllWorkGroupsAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_SingleEntry_ReturnsSingleItem()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1" }
            };
            var repo = CreateRepository(workGroups);

            var result = (await repo.GetAllWorkGroupsAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("WG1", result[0].WorkGroupName);
        }

        #endregion

        #region GetWorkGroupsByProfitCentreAsync

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_NoMatchingProfitCentre_ReturnsEmptyPagedResult()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC2", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithFpsYear(workGroups);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupsByProfitCentreAsync(query, "PC1");

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_FpsYearMismatch_ReturnsEmptyPagedResult()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", FpsYear = 2023 }
            };
            // Context FpsYear is DefaultFpsYear (2024); data row is year 2023 — no match expected.
            var (repo, _, _, _) = CreateRepositoryWithFpsYear(workGroups, fpsYear: DefaultFpsYear);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupsByProfitCentreAsync(query, "PC1");

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        // Tests for non-empty results (matching profit centre + FpsYear), filter branches
        // (ApplyWorkGroupFilter via EF.Functions.ILike), and sort branches (EF.Property<object>)
        // require a real EF Core provider and cannot be exercised with the mock LINQ provider.
        // These are covered by integration/acceptance tests.

        #endregion

        #region SetSendEmailForProfitCentreWorkGroupsAsync

        // ExecuteUpdateAsync requires a real EF Core provider (translates to a SQL UPDATE … SET
        // statement) and cannot be executed against the mocked LINQ provider used in these unit
        // tests. Behaviour is verified through integration/acceptance tests.

        #endregion

        #region SetSendEmailForAllWorkGroupsAsync

        // Same limitation as SetSendEmailForProfitCentreWorkGroupsAsync above.

        #endregion

        #region UpdateWorkGroupEmailAsync

        // Same limitation as SetSendEmailForProfitCentreWorkGroupsAsync above.

        #endregion
    }
}
