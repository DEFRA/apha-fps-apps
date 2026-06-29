using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.StoredProcRepositoryTest
{
    public class StoredProcRepositoryTest
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a StoredProcRepository with in-memory Workgroups data.
        /// IFpsYearContext is substituted via NSubstitute.
        /// Workgroup has a FpsCalYear query filter in FpsDbContext — seed data is
        /// pre-filtered per test to simulate what EF's query filter produces.
        /// </summary>
        private static StoredProcRepository CreateRepository(IEnumerable<Workgroup> workgroups)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            fpsYearContext.FpsYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var workgroupsMockSet = RepositoryTestHelper.CreateMockDbSet(workgroups);
            mockContext.Setup(x => x.Workgroups).Returns(workgroupsMockSet.Object);

            return new StoredProcRepository(mockContext.Object);
        }

        #region GetAllCostCentreWorkgroupAsync

        [Fact]
        public async Task GetAllCostCentreWorkgroupAsync_ReturnsGroupedResult_WhenDataExists()
        {
            // Arrange
            var workgroups = new List<Workgroup>
            {
                new() { WorkGroupName = "WG-A", CostCentre = 100, ProfitCentre = "PC1", FpsYear = DefaultTestFpsYear },
                new() { WorkGroupName = "WG-B", CostCentre = 200, ProfitCentre = "PC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(workgroups);

            // Act
            var result = await repo.GetAllCostCentreWorkgroupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllCostCentreWorkgroupAsync_ReturnsEmptyCollection_WhenNoWorkgroupsExist()
        {
            // Arrange
            var repo = CreateRepository(new List<Workgroup>());

            // Act
            var result = await repo.GetAllCostCentreWorkgroupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCostCentreWorkgroupAsync_ExcludesWorkgroups_WhenCostCentreIsNull()
        {
            // Arrange — one workgroup has null CostCentre and must be excluded by the Where filter
            var workgroups = new List<Workgroup>
            {
                new() { WorkGroupName = "WG-A", CostCentre = 100,  ProfitCentre = "PC1", FpsYear = DefaultTestFpsYear },
                new() { WorkGroupName = "WG-B", CostCentre = null, ProfitCentre = "PC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(workgroups);

            // Act
            var result = await repo.GetAllCostCentreWorkgroupAsync();

            // Assert
            Assert.Single(result);
            Assert.All(result, r => Assert.NotNull(r.CostCentre));
        }

        [Fact]
        public async Task GetAllCostCentreWorkgroupAsync_ReturnsEmptyCollection_WhenAllCostCentresAreNull()
        {
            // Arrange — all workgroups have null CostCentre, none should pass the Where filter
            var workgroups = new List<Workgroup>
            {
                new() { WorkGroupName = "WG-A", CostCentre = null, ProfitCentre = "PC1", FpsYear = DefaultTestFpsYear },
                new() { WorkGroupName = "WG-B", CostCentre = null, ProfitCentre = "PC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(workgroups);

            // Act
            var result = await repo.GetAllCostCentreWorkgroupAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCostCentreWorkgroupAsync_GroupsByKeyAndJoinsWorkGroupNames_WhenMultipleWorkgroupsShareSameKey()
        {
            // Arrange — two workgroups share the same CostCentre + ProfitCentre key and must be merged into one row
            var workgroups = new List<Workgroup>
            {
                new() { WorkGroupName = "WG-A", CostCentre = 100, ProfitCentre = "PC1", FpsYear = DefaultTestFpsYear },
                new() { WorkGroupName = "WG-B", CostCentre = 100, ProfitCentre = "PC1", FpsYear = DefaultTestFpsYear },
                new() { WorkGroupName = "WG-C", CostCentre = 200, ProfitCentre = "PC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(workgroups);

            // Act
            var result = await repo.GetAllCostCentreWorkgroupAsync();

            // Assert
            var list = result.ToList();
            Assert.Equal(2, list.Count);

            var grouped = list.First(r => r.CostCentre == 100 && r.ProfitCentre == "PC1");
            Assert.Contains("WG-A", grouped.WGs);
            Assert.Contains("WG-B", grouped.WGs);
        }

        [Fact]
        public async Task GetAllCostCentreWorkgroupAsync_ReturnsSingleGroupWithCorrectFields_WhenOnlyOneWorkgroupExists()
        {
            // Arrange
            var workgroups = new List<Workgroup>
            {
                new() { WorkGroupName = "WG-A", CostCentre = 100, ProfitCentre = "PC1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(workgroups);

            // Act
            var result = await repo.GetAllCostCentreWorkgroupAsync();

            // Assert
            var single = Assert.Single(result);
            Assert.Equal(100,   single.CostCentre);
            Assert.Equal("PC1", single.ProfitCentre);
            Assert.Equal("WG-A", single.WGs);
        }

        #endregion
    }
}