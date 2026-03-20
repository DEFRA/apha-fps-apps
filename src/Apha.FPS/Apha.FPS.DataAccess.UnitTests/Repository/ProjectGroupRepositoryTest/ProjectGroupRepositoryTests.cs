using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectGroupRepositoryTest
{
    public class ProjectGroupRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a ProjectGroupRepository with in-memory ProjectGroups data.
        /// IFpsYearContext is substituted via NSubstitute.
        /// ProjectGroup has no FpsCalYear query filter, so year value is irrelevant.
        /// </summary>
        private static ProjectGroupRepository CreateRepository(IEnumerable<ProjectGroup> projectGroups)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var projectGroupsMockSet = RepositoryTestHelper.CreateMockDbSet(projectGroups);
            mockContext.Setup(x => x.ProjectGroups).Returns(projectGroupsMockSet.Object);

            return new ProjectGroupRepository(mockContext.Object);
        }

        #region GetAllProjectGroupsAsync

        [Fact]
        public async Task GetAllProjectGroupsAsync_ReturnsAllProjectGroups_WhenDataExists()
        {
            // Arrange
            var projectGroups = new List<ProjectGroup>
            {
                new() { ProjectGroupName = "Group A" },
                new() { ProjectGroupName = "Group B" },
                new() { ProjectGroupName = "Group C" }
            };
            var repo = CreateRepository(projectGroups);

            // Act
            var result = await repo.GetAllProjectGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetAllProjectGroupsAsync_ReturnsEmptyCollection_WhenNoProjectGroupsExist()
        {
            // Arrange
            var repo = CreateRepository(new List<ProjectGroup>());

            // Act
            var result = await repo.GetAllProjectGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProjectGroupsAsync_ReturnsCorrectData_WhenSingleProjectGroupExists()
        {
            // Arrange
            var projectGroups = new List<ProjectGroup>
            {
                new() { ProjectGroupName = "Group A" }
            };
            var repo = CreateRepository(projectGroups);

            // Act
            var result = await repo.GetAllProjectGroupsAsync();

            // Assert
            var single = Assert.Single(result);
            Assert.Equal("Group A", single.ProjectGroupName);
        }

        [Fact]
        public async Task GetAllProjectGroupsAsync_ReturnsIEnumerable_NotNull()
        {
            // Arrange — verifies the return type contract is always IEnumerable, never null
            var repo = CreateRepository(new List<ProjectGroup>());

            // Act
            var result = await repo.GetAllProjectGroupsAsync();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<ProjectGroup>>(result);
        }

        #endregion
    }
}