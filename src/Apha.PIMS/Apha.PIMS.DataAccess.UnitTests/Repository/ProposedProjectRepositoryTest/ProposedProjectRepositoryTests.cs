using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.ProposedProjectRepositoryTest
{
    public class ProposedProjectRepositoryTests
    {
        /// <summary>
        /// Creates a ProposedProjectRepository with in-memory data for all DbSets.
        /// All parameters are optional — omitted sets are initialised as empty.
        /// </summary>
        private static ProposedProjectRepository CreateRepository(
            IEnumerable<Project>? fpsProjects = null,
            IEnumerable<ProposedProject>? proposedProjects = null,
            IEnumerable<ProjectLatestDetail>? projectLatestDetails = null,
            IEnumerable<RadtrackProg>? radtrackProgs = null,
            IEnumerable<ProjectStatus>? projectStatuses = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var projectsMockSet             = RepositoryTestHelper.CreateMockDbSet(fpsProjects           ?? Enumerable.Empty<Project>());
            var proposedProjectsMockSet     = RepositoryTestHelper.CreateMockDbSet(proposedProjects       ?? Enumerable.Empty<ProposedProject>());
            var projectLatestDetailsMockSet = RepositoryTestHelper.CreateMockDbSet(projectLatestDetails   ?? Enumerable.Empty<ProjectLatestDetail>());
            var radtrackProgsMockSet        = RepositoryTestHelper.CreateMockDbSet(radtrackProgs           ?? Enumerable.Empty<RadtrackProg>());
            var projectStatusesMockSet      = RepositoryTestHelper.CreateMockDbSet(projectStatuses         ?? Enumerable.Empty<ProjectStatus>());

            RepositoryTestHelper.SetupDbSetOperations(proposedProjectsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.ProposedProjects).Returns(proposedProjectsMockSet.Object);
            mockContext.Setup(x => x.ProjectLatestDetails).Returns(projectLatestDetailsMockSet.Object);
            mockContext.Setup(x => x.RadtrackProgs).Returns(radtrackProgsMockSet.Object);
            mockContext.Setup(x => x.ProjectStatuses).Returns(projectStatusesMockSet.Object);

            return new ProposedProjectRepository(mockContext.Object);
        }

        /// <summary>
        /// Returns the repository alongside its mocked DbSet and DbContext
        /// for tests that need to verify Add / SaveChanges calls.
        /// </summary>
        private static (
            ProposedProjectRepository Repo,
            Mock<DbSet<ProposedProject>> ProposedProjectsDbSet,
            Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<Project>? fpsProjects = null,
                IEnumerable<ProposedProject>? proposedProjects = null,
                IEnumerable<ProjectLatestDetail>? projectLatestDetails = null,
                IEnumerable<RadtrackProg>? radtrackProgs = null,
                IEnumerable<ProjectStatus>? projectStatuses = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var projectsMockSet             = RepositoryTestHelper.CreateMockDbSet(fpsProjects           ?? Enumerable.Empty<Project>());
            var proposedProjectsMockSet     = RepositoryTestHelper.CreateMockDbSet(proposedProjects       ?? Enumerable.Empty<ProposedProject>());
            var projectLatestDetailsMockSet = RepositoryTestHelper.CreateMockDbSet(projectLatestDetails   ?? Enumerable.Empty<ProjectLatestDetail>());
            var radtrackProgsMockSet        = RepositoryTestHelper.CreateMockDbSet(radtrackProgs           ?? Enumerable.Empty<RadtrackProg>());
            var projectStatusesMockSet      = RepositoryTestHelper.CreateMockDbSet(projectStatuses         ?? Enumerable.Empty<ProjectStatus>());

            RepositoryTestHelper.SetupDbSetOperations(proposedProjectsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.ProposedProjects).Returns(proposedProjectsMockSet.Object);
            mockContext.Setup(x => x.ProjectLatestDetails).Returns(projectLatestDetailsMockSet.Object);
            mockContext.Setup(x => x.RadtrackProgs).Returns(radtrackProgsMockSet.Object);
            mockContext.Setup(x => x.ProjectStatuses).Returns(projectStatusesMockSet.Object);

            var repo = new ProposedProjectRepository(mockContext.Object);
            return (repo, proposedProjectsMockSet, mockContext);
        }

        #region GetFpsProjectByIdAsync

        [Fact]
        public async Task GetFpsProjectByIdAsync_ReturnsProject_WhenProjectExists()
        {
            // Arrange
            var fpsProjects = new List<Project>
            {
                new() { Parentproject = "PP001", Projecttitle = "FMD Survey",     Disease = "FMD", Contract = "CON001", Projectstatus = "Active" },
                new() { Parentproject = "PP002", Projecttitle = "TB Eradication", Disease = "TB",  Contract = "CON002", Projectstatus = "Active" }
            };
            var repo = CreateRepository(fpsProjects: fpsProjects);

            // Act
            var result = await repo.GetFpsProjectByIdAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PP001",      result.Parentproject);
            Assert.Equal("FMD Survey", result.Projecttitle);
            Assert.Equal("CON001",     result.Contract);
            Assert.Equal("FMD",        result.Disease);
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_ReturnsNull_WhenProjectDoesNotExist()
        {
            // Arrange
            var fpsProjects = new List<Project>
            {
                new() { Parentproject = "PP001", Projecttitle = "FMD Survey" }
            };
            var repo = CreateRepository(fpsProjects: fpsProjects);

            // Act
            var result = await repo.GetFpsProjectByIdAsync("UNKNOWN");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_ReturnsNull_WhenFpsProjectsIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(fpsProjects: new List<Project>());

            // Act
            var result = await repo.GetFpsProjectByIdAsync("PP001");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("NONEXISTENT")]
        public async Task GetFpsProjectByIdAsync_ReturnsNull_WhenIdDoesNotMatch(string parentproject)
        {
            // Arrange
            var fpsProjects = new List<Project>
            {
                new() { Parentproject = "PP001", Projecttitle = "FMD Survey" }
            };
            var repo = CreateRepository(fpsProjects: fpsProjects);

            // Act
            var result = await repo.GetFpsProjectByIdAsync(parentproject);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetProposedProjectByIdAsync

        [Fact]
        public async Task GetProposedProjectByIdAsync_ReturnsProposedProject_WhenExists()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Id = 1, Parentproject = "PP001", Projecttitle = "TB Project",  Program = "PROG1", Customer = "CUST1", Projectstatus = "Proposed", Disease = "TB"  },
                new() { Id = 2, Parentproject = "PP002", Projecttitle = "FMD Project", Program = "PROG2", Customer = "CUST2", Projectstatus = "Active",   Disease = "FMD" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);

            // Act
            var result = await repo.GetProposedProjectByIdAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1,            result.Id);
            Assert.Equal("PP001",      result.Parentproject);
            Assert.Equal("TB Project", result.Projecttitle);
            Assert.Equal("Proposed",   result.Projectstatus);
        }

        [Fact]
        public async Task GetProposedProjectByIdAsync_ReturnsNull_WhenProjectDoesNotExist()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Id = 1, Parentproject = "PP001", Projecttitle = "TB Project" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);

            // Act
            var result = await repo.GetProposedProjectByIdAsync("UNKNOWN");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProposedProjectByIdAsync_ReturnsNull_WhenProposedProjectsIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(proposedProjects: new List<ProposedProject>());

            // Act
            var result = await repo.GetProposedProjectByIdAsync("PP001");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("NONEXISTENT")]
        public async Task GetProposedProjectByIdAsync_ReturnsNull_WhenIdDoesNotMatch(string parentproject)
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Id = 1, Parentproject = "PP001", Projecttitle = "TB Project" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);

            // Act
            var result = await repo.GetProposedProjectByIdAsync(parentproject);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddProposedProjectAsync

        [Fact]
        public async Task AddProjectAsync_AddsEntityAndReturnsIt()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var newEntity = new ProposedProject
            {
                Parentproject = "PP001",
                Projecttitle  = "New Project",
                Program       = "PROG1",
                Customer      = "CUST1",
                Manager       = "MGR1",
                Projectstatus = "Proposed",
                Disease       = "FMD"
            };

            // Act
            var result = await repo.AddProposedProjectAsync(newEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(newEntity, result);
            Assert.Equal("PP001",       result.Parentproject);
            Assert.Equal("New Project", result.Projecttitle);
            Assert.Equal("Proposed",    result.Projectstatus);
        }

        [Fact]
        public async Task AddProjectAsync_WithAllFields_ReturnsEntityWithAllFieldsPopulated()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var newEntity = new ProposedProject
            {
                Parentproject = "PP010",
                Projecttitle  = "Full Fields Project",
                Program       = "PROG5",
                Customer      = "CUST5",
                Manager       = "MGR5",
                Projectstatus = "Active",
                Disease       = "TB"
            };

            // Act
            var result = await repo.AddProposedProjectAsync(newEntity);

            // Assert
            Assert.Equal("PP010",               result.Parentproject);
            Assert.Equal("Full Fields Project", result.Projecttitle);
            Assert.Equal("PROG5",               result.Program);
            Assert.Equal("CUST5",               result.Customer);
            Assert.Equal("MGR5",                result.Manager);
            Assert.Equal("Active",              result.Projectstatus);
            Assert.Equal("TB",                  result.Disease);
        }

        [Fact]
        public async Task AddProjectAsync_WithMinimalFields_ReturnsEntityWithMinimalFieldsPopulated()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var newEntity = new ProposedProject { Parentproject = "PP011" };

            // Act
            var result = await repo.AddProposedProjectAsync(newEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(newEntity, result);
            Assert.Equal("PP011", result.Parentproject);
            Assert.Null(result.Projecttitle);
            Assert.Null(result.Program);
        }

        [Fact]
        public async Task AddProjectAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, proposedProjectsDbSet, _) = CreateRepositoryWithMocks();
            var newEntity = new ProposedProject { Parentproject = "PP001", Projecttitle = "New Project" };

            // Act
            await repo.AddProposedProjectAsync(newEntity);

            // Assert
            proposedProjectsDbSet.Verify(x => x.Add(newEntity), Times.Once);
        }

        [Fact]
        public async Task AddProjectAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var newEntity = new ProposedProject { Parentproject = "PP001", Projecttitle = "New Project" };

            // Act
            await repo.AddProposedProjectAsync(newEntity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task AddProjectAsync_DoesNotCallSaveChanges_MoreThanOnce()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var newEntity = new ProposedProject { Parentproject = "PP001" };

            // Act
            await repo.AddProposedProjectAsync(newEntity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region GetProjectProgramsAsync

        [Fact]
        public async Task GetProjectProgramsAsync_ReturnsDistinctProgramsOrderedAscending()
        {
            // Arrange
            var radtrackProgs = new List<RadtrackProg>
            {
                new() { Program = "PROG3" },
                new() { Program = "PROG1" },
                new() { Program = "PROG2" },
                new() { Program = "PROG1" }
            };
            var repo = CreateRepository(radtrackProgs: radtrackProgs);

            // Act
            var result = await repo.GetProjectProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(new List<string> { "PROG1", "PROG2", "PROG3" }, result);
        }

        [Fact]
        public async Task GetProjectProgramsAsync_ReturnsEmptyList_WhenNoProgramsExist()
        {
            // Arrange
            var repo = CreateRepository(radtrackProgs: new List<RadtrackProg>());

            // Act
            var result = await repo.GetProjectProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProjectProgramsAsync_ReturnsSingleItem_WhenAllEntriesAreDuplicates()
        {
            // Arrange
            var radtrackProgs = new List<RadtrackProg>
            {
                new() { Program = "PROG1" },
                new() { Program = "PROG1" },
                new() { Program = "PROG1" }
            };
            var repo = CreateRepository(radtrackProgs: radtrackProgs);

            // Act
            var result = await repo.GetProjectProgramsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("PROG1", result[0]);
        }

        [Fact]
        public async Task GetProjectProgramsAsync_ReturnsOnlyNonEmptyProgramStrings()
        {
            // Arrange
            var radtrackProgs = new List<RadtrackProg>
            {
                new() { Program = "ALPHA" },
                new() { Program = "BETA" }
            };
            var repo = CreateRepository(radtrackProgs: radtrackProgs);

            // Act
            var result = await repo.GetProjectProgramsAsync();

            // Assert
            Assert.All(result, p => Assert.False(string.IsNullOrWhiteSpace(p)));
        }

        #endregion

        #region GetProjectCustomersAsync

        [Fact]
        public async Task GetProjectCustomersAsync_ReturnsDistinctCustomersOrderedAscending()
        {
            // Arrange
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST3", Active = "Y" },
                new() { ParentProject = "PP002", Program = "PROG1", Customer = "CUST1", Active = "Y" },
                new() { ParentProject = "PP003", Program = "PROG2", Customer = "CUST2", Active = "Y" },
                new() { ParentProject = "PP004", Program = "PROG2", Customer = "CUST1", Active = "Y" }
            };
            var repo = CreateRepository(projectLatestDetails: projectLatestDetails);

            // Act
            var result = await repo.GetProjectCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(new List<string> { "CUST1", "CUST2", "CUST3" }, result);
        }

        [Fact]
        public async Task GetProjectCustomersAsync_ReturnsEmptyList_WhenNoCustomersExist()
        {
            // Arrange
            var repo = CreateRepository(projectLatestDetails: new List<ProjectLatestDetail>());

            // Act
            var result = await repo.GetProjectCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProjectCustomersAsync_ExcludesNullCustomers()
        {
            // Arrange
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST1", Active = "Y" },
                new() { ParentProject = "PP002", Program = "PROG1", Customer = null,    Active = "Y" }
            };
            var repo = CreateRepository(projectLatestDetails: projectLatestDetails);

            // Act
            var result = await repo.GetProjectCustomersAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("CUST1", result[0]);
        }

        [Fact]
        public async Task GetProjectCustomersAsync_ReturnsSingleItem_WhenAllEntriesAreDuplicates()
        {
            // Arrange
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST1", Active = "Y" },
                new() { ParentProject = "PP002", Program = "PROG1", Customer = "CUST1", Active = "Y" },
                new() { ParentProject = "PP003", Program = "PROG2", Customer = "CUST1", Active = "Y" }
            };
            var repo = CreateRepository(projectLatestDetails: projectLatestDetails);

            // Act
            var result = await repo.GetProjectCustomersAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("CUST1", result[0]);
        }

        [Fact]
        public async Task GetProjectCustomersAsync_ReturnsOnlyNonNullCustomers()
        {
            // Arrange
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "ALPHA", Active = "Y" },
                new() { ParentProject = "PP002", Program = "PROG2", Customer = "BETA",  Active = "Y" }
            };
            var repo = CreateRepository(projectLatestDetails: projectLatestDetails);

            // Act
            var result = await repo.GetProjectCustomersAsync();

            // Assert
            Assert.All(result, c => Assert.False(string.IsNullOrWhiteSpace(c)));
        }

        #endregion

        #region GetProjectStatusesAsync

        [Fact]
        public async Task GetProjectStatusesAsync_ReturnsOnlyPimsStatuses()
        {
            // Arrange
            var projectStatuses = new List<ProjectStatus>
            {
                new() { Projectstatus = "Active",    IsFps = true,  IsPims = true  },
                new() { Projectstatus = "Proposed",  IsFps = false, IsPims = true  },
                new() { Projectstatus = "FpsOnly",   IsFps = true,  IsPims = false },
                new() { Projectstatus = "Completed", IsFps = false, IsPims = true  }
            };
            var repo = CreateRepository(projectStatuses: projectStatuses);

            // Act
            var result = await repo.GetProjectStatusesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, s => s.Projectstatus == "FpsOnly");
            Assert.DoesNotContain(result, s => s.Projectstatus == "Completed");
        }

        [Fact]
        public async Task GetProjectStatusesAsync_ReturnsEmptyList_WhenNoStatusesExist()
        {
            // Arrange
            var repo = CreateRepository(projectStatuses: new List<ProjectStatus>());

            // Act
            var result = await repo.GetProjectStatusesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProjectStatusesAsync_ExcludesCompletedStatus()
        {
            // Arrange
            var projectStatuses = new List<ProjectStatus>
            {
                new() { Projectstatus = "Active",    IsFps = true,  IsPims = true },
                new() { Projectstatus = "Completed", IsFps = false, IsPims = true }
            };
            var repo = CreateRepository(projectStatuses: projectStatuses);

            // Act
            var result = await repo.GetProjectStatusesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Active", result[0].Projectstatus);
        }

        [Fact]
        public async Task GetProjectStatusesAsync_ExcludesNonPimsStatuses()
        {
            // Arrange
            var projectStatuses = new List<ProjectStatus>
            {
                new() { Projectstatus = "Active",  IsFps = true,  IsPims = true  },
                new() { Projectstatus = "FpsOnly", IsFps = true,  IsPims = false }
            };
            var repo = CreateRepository(projectStatuses: projectStatuses);

            // Act
            var result = await repo.GetProjectStatusesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Active", result[0].Projectstatus);
        }

        [Fact]
        public async Task GetProjectStatusesAsync_ReturnsAllPimsNonCompletedStatuses()
        {
            // Arrange
            var projectStatuses = new List<ProjectStatus>
            {
                new() { Projectstatus = "Active",   IsFps = true,  IsPims = true },
                new() { Projectstatus = "Proposed", IsFps = false, IsPims = true },
                new() { Projectstatus = "Pending",  IsFps = true,  IsPims = true }
            };
            var repo = CreateRepository(projectStatuses: projectStatuses);

            // Act
            var result = await repo.GetProjectStatusesAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.All(result, s => Assert.True(s.IsPims));
            Assert.All(result, s => Assert.NotEqual("Completed", s.Projectstatus));
        }

        #endregion
    }
}
