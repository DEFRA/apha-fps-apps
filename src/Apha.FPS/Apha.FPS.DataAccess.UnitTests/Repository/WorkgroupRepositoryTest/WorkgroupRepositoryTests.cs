using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.WorkGroupRepositoryTest
{
    public class WorkGroupRepositoryTests
    {
        private const string DefaultUserEmail = "test@example.com";
        private const int    DefaultFpsYear   = 2024;

        private static WorkGroupRepository CreateRepository(
            IEnumerable<Workgroup>?      workgroups  = null,
            IEnumerable<WorkGroupView>?  wgViews     = null,
            string                       userEmail   = DefaultUserEmail,
            int                          fpsYear     = DefaultFpsYear)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(fpsYear);
            requestContext.UserEmailId.Returns(userEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(workgroups ?? Enumerable.Empty<Workgroup>());
            mockContext.Setup(x => x.Workgroups).Returns(mockSet.Object);

            var viewSet = RepositoryTestHelper.CreateMockDbSet(wgViews ?? Enumerable.Empty<WorkGroupView>());
            mockContext.Setup(x => x.WorkGroupViews).Returns(viewSet.Object);

            return new WorkGroupRepository(mockContext.Object, requestContext);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRequestContext_ThrowsArgumentNullException()
        {
            var dummyCtx = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(dummyCtx);
            Assert.Throws<ArgumentNullException>(() => new WorkGroupRepository(mockContext.Object, null!));
        }

        #endregion

        #region GetAllWorkGroupNamesAsync Tests

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithData_ReturnsOrderedNames()
        {
            // Arrange
            var workgroups = new List<Workgroup>
            {
                new() { WorkgroupName = "WG03", ProfitCentre = "PC01", FpsYear = 2024 },
                new() { WorkgroupName = "WG01", ProfitCentre = "PC01", FpsYear = 2024 },
                new() { WorkgroupName = "WG02", ProfitCentre = "PC01", FpsYear = 2024 }
            };
            var repo = CreateRepository(workgroups);

            // Act
            var result = await repo.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("WG01", result[0]);
            Assert.Equal("WG02", result[1]);
            Assert.Equal("WG03", result[2]);
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithEmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(new List<Workgroup>());

            // Act
            var result = await repo.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithSingleWorkgroup_ReturnsSingleName()
        {
            // Arrange
            var workgroups = new List<Workgroup>
            {
                new() { WorkgroupName = "WG01", ProfitCentre = "PC01", FpsYear = 2024 }
            };
            var repo = CreateRepository(workgroups);

            // Act
            var result = await repo.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("WG01", result[0]);
        }

        #endregion

        #region GetWorkGroupsByProfitCentreAsync Tests

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_WithMatchingData_ReturnsFilteredWorkGroups()
        {
            // Arrange
            var views = new List<WorkGroupView>
            {
                new() { WorkgroupName = "WG01", ProfitCentre = "PC01", FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { WorkgroupName = "WG02", ProfitCentre = "PC01", FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { WorkgroupName = "WG03", ProfitCentre = "PC02", FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(wgViews: views);

            // Act
            var result = await repo.GetWorkGroupsByProfitCentreAsync("PC01");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, w => Assert.Equal("PC01", w.ProfitCentre));
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_FiltersOutDifferentFpsYear()
        {
            // Arrange
            var views = new List<WorkGroupView>
            {
                new() { WorkgroupName = "WG01", ProfitCentre = "PC01", FpsYear = DefaultFpsYear,     UserEmail = DefaultUserEmail },
                new() { WorkgroupName = "WG02", ProfitCentre = "PC01", FpsYear = DefaultFpsYear - 1, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(wgViews: views);

            // Act
            var result = await repo.GetWorkGroupsByProfitCentreAsync("PC01");

            // Assert
            Assert.Single(result);
            Assert.Equal("WG01", result[0].WorkgroupName);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_FiltersOutDifferentUserEmail()
        {
            // Arrange
            var views = new List<WorkGroupView>
            {
                new() { WorkgroupName = "WG01", ProfitCentre = "PC01", FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { WorkgroupName = "WG02", ProfitCentre = "PC01", FpsYear = DefaultFpsYear, UserEmail = "other@example.com" }
            };
            var repo = CreateRepository(wgViews: views);

            // Act
            var result = await repo.GetWorkGroupsByProfitCentreAsync("PC01");

            // Assert
            Assert.Single(result);
            Assert.Equal("WG01", result[0].WorkgroupName);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_FiltersOutNullUserEmail()
        {
            // Arrange
            var views = new List<WorkGroupView>
            {
                new() { WorkgroupName = "WG01", ProfitCentre = "PC01", FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { WorkgroupName = "WG02", ProfitCentre = "PC01", FpsYear = DefaultFpsYear, UserEmail = null }
            };
            var repo = CreateRepository(wgViews: views);

            // Act
            var result = await repo.GetWorkGroupsByProfitCentreAsync("PC01");

            // Assert
            Assert.Single(result);
            Assert.Equal("WG01", result[0].WorkgroupName);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_IsCaseInsensitiveForEmail()
        {
            // Arrange
            var views = new List<WorkGroupView>
            {
                new() { WorkgroupName = "WG01", ProfitCentre = "PC01", FpsYear = DefaultFpsYear, UserEmail = "TEST@EXAMPLE.COM" }
            };
            var repo = CreateRepository(wgViews: views, userEmail: "test@example.com");

            // Act
            var result = await repo.GetWorkGroupsByProfitCentreAsync("PC01");

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_ReturnsOrderedByWorkgroupName()
        {
            // Arrange
            var views = new List<WorkGroupView>
            {
                new() { WorkgroupName = "WG03", ProfitCentre = "PC01", FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { WorkgroupName = "WG01", ProfitCentre = "PC01", FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { WorkgroupName = "WG02", ProfitCentre = "PC01", FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(wgViews: views);

            // Act
            var result = await repo.GetWorkGroupsByProfitCentreAsync("PC01");

            // Assert
            Assert.Equal("WG01", result[0].WorkgroupName);
            Assert.Equal("WG02", result[1].WorkgroupName);
            Assert.Equal("WG03", result[2].WorkgroupName);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_WithNonMatchingProfitCentre_ReturnsEmpty()
        {
            // Arrange
            var views = new List<WorkGroupView>
            {
                new() { WorkgroupName = "WG01", ProfitCentre = "PC01", FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(wgViews: views);

            // Act
            var result = await repo.GetWorkGroupsByProfitCentreAsync("NOTEXIST");

            // Assert
            Assert.Empty(result);
        }

        #endregion
    }
}
