using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.ProjectManagerRepositoryTest
{
    public class ProjectManagerRepositoryTests
    {
        private static ProjectManagerRepository CreateRepository(IEnumerable<ProjectManager>? managers = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var managersMockSet = RepositoryTestHelper.CreateMockDbSet(managers ?? Enumerable.Empty<ProjectManager>());

            RepositoryTestHelper.SetupDbSetOperations(managersMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.ProjectManagers).Returns(managersMockSet.Object);

            return new ProjectManagerRepository(mockContext.Object);
        }

        private static (ProjectManagerRepository Repo, Mock<DbSet<ProjectManager>> ManagersDbSet, Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<ProjectManager>? managers = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var managersMockSet = RepositoryTestHelper.CreateMockDbSet(managers ?? Enumerable.Empty<ProjectManager>());

            RepositoryTestHelper.SetupDbSetOperations(managersMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.ProjectManagers).Returns(managersMockSet.Object);

            var repo = new ProjectManagerRepository(mockContext.Object);
            return (repo, managersMockSet, mockContext);
        }

        private static ProjectManager MakeManager(string name = "J. Smith", string? email = null, string? loginEmail = null, string? mNumber = null, bool disable = false) =>
            new()
            {
                Projectmanager = name,
                Email = email,
                LoginEmail = loginEmail,
                Mnumber = mNumber,
                Disable = disable
            };

        [Fact]
        public async Task GetAllProjectManagersAsync_ReturnsAllManagers()
        {
            var repo = CreateRepository(new List<ProjectManager>
            {
                MakeManager("Smith, J."),
                MakeManager("Jones, A.")
            });

            var result = await repo.GetAllProjectManagersAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetProjectManagerByNameAsync_ReturnsManager_WhenExactCaseExists()
        {
            var repo = CreateRepository(new List<ProjectManager> { MakeManager("Smith, J.") });

            var result = await repo.GetProjectManagerByNameAsync("Smith, J.");

            Assert.NotNull(result);
            Assert.Equal("Smith, J.", result!.Projectmanager);
        }

        [Fact]
        public async Task GetProjectManagerByNameAsync_ReturnsNull_WhenNotFound()
        {
            var repo = CreateRepository(new List<ProjectManager> { MakeManager("Smith, J.") });

            var result = await repo.GetProjectManagerByNameAsync("Unknown");

            Assert.Null(result);
        }

        [Fact]
        public async Task AddProjectManagerAsync_ReturnsSameEntity_AndCallsSave()
        {
            var (repo, managersDbSet, context) = CreateRepositoryWithMocks();
            var manager = MakeManager("New Manager", disable: true);

            var result = await repo.AddProjectManagerAsync(manager);

            Assert.Same(manager, result);
            managersDbSet.Verify(x => x.Add(It.Is<ProjectManager>(m => m.Projectmanager == "New Manager")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context, 1);
        }

        [Fact]
        public async Task UpdateProjectManagerAsync_ReturnsUpdatedEntity_AndCallsSave()
        {
            var (repo, _, context) = CreateRepositoryWithMocks(new List<ProjectManager> { MakeManager("Smith, J.") });
            var entity = MakeManager("Smith, J.", email: "new@apha.gov.uk", disable: true);

            var result = await repo.UpdateProjectManagerAsync(entity);

            Assert.Equal("new@apha.gov.uk", result.Email);
            Assert.True(result.Disable);
            RepositoryTestHelper.VerifySaveChanges(context, 1);
        }

        [Fact]
        public async Task ProjectManagerExistsAsync_ReturnsTrue_WhenNameExists()
        {
            var repo = CreateRepository(new List<ProjectManager> { MakeManager("Smith, J.") });

            var result = await repo.ProjectManagerExistsAsync("Smith, J.");

            Assert.True(result);
        }

        [Fact]
        public async Task ProjectManagerExistsAsync_ReturnsFalse_WhenNameDoesNotExist()
        {
            var repo = CreateRepository(new List<ProjectManager> { MakeManager("Smith, J.") });

            var result = await repo.ProjectManagerExistsAsync("Unknown");

            Assert.False(result);
        }

        [Fact]
        public async Task GetManagerNamesAsync_ReturnsDistinctOrderedNames()
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var projects = RepositoryTestHelper.CreateMockDbSet(new List<Projects>
            {
                new() { Program = "P1", Manager = "B Manager" },
                new() { Program = "P2", Manager = "A Manager" },
                new() { Program = "P1", Manager = "B Manager" }
            });
            var progs = RepositoryTestHelper.CreateMockDbSet(new List<RadtrackProg>
            {
                new() { Program = "P1" },
                new() { Program = "P2" }
            });
            var mgrs = RepositoryTestHelper.CreateMockDbSet(new List<ProjectManager>());

            RepositoryTestHelper.SetupDbSetOperations(projects);
            RepositoryTestHelper.SetupDbSetOperations(progs);
            RepositoryTestHelper.SetupDbSetOperations(mgrs);

            mockContext.Setup(x => x.MyTlkpProjects).Returns(projects.Object);
            mockContext.Setup(x => x.RadtrackProgs).Returns(progs.Object);
            mockContext.Setup(x => x.ProjectManagers).Returns(mgrs.Object);

            var repo = new ProjectManagerRepository(mockContext.Object);

            var result = await repo.GetManagerNamesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("A Manager", result[0]);
            Assert.Equal("B Manager", result[1]);
        }
    }
}
