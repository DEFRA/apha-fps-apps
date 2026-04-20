using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.WorkGroupRepositoryTest
{
    public class WorkGroupRepositoryTests
    {
        /// <summary>
        /// WorkGroupRepository has no IFpsYearContext dependency and only reads data.
        /// All query logic (AsNoTracking, OrderBy, ToListAsync) is exercised through the mock DbSet.
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
    }
}
