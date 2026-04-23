using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.ProjectListRepositoryTest
{
    public class ProjectListRepositoryTests
    {
        /// <summary>
        /// Creates a ProjectListRepository with in-memory data for all DbSets.
        /// All parameters are optional — omitted sets are initialised as empty.
        /// </summary>
        private static ProjectListRepository CreateRepository(
            IEnumerable<Project>? fpsProjects = null,
            IEnumerable<ProposedProject>? proposedProjects = null,
            IEnumerable<Projects>? yearlyProjects = null,
            IEnumerable<ProjectLatestDetail>? projectLatestDetails = null,
            IEnumerable<RadtrackProg>? radtrackProgs = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(fpsProjects ?? Enumerable.Empty<Project>());
            var proposedProjectsMockSet = RepositoryTestHelper.CreateMockDbSet(proposedProjects ?? Enumerable.Empty<ProposedProject>());
            var yearlyProjectsMockSet = RepositoryTestHelper.CreateMockDbSet(yearlyProjects ?? Enumerable.Empty<Projects>());
            var projectLatestDetailsMockSet = RepositoryTestHelper.CreateMockDbSet(projectLatestDetails ?? Enumerable.Empty<ProjectLatestDetail>());
            var radtrackProgsMockSet = RepositoryTestHelper.CreateMockDbSet(radtrackProgs ?? Enumerable.Empty<RadtrackProg>());

            RepositoryTestHelper.SetupDbSetOperations(proposedProjectsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.ProposedProjects).Returns(proposedProjectsMockSet.Object);
            mockContext.Setup(x => x.MyTlkpProjects).Returns(yearlyProjectsMockSet.Object);
            mockContext.Setup(x => x.ProjectLatestDetails).Returns(projectLatestDetailsMockSet.Object);
            mockContext.Setup(x => x.RadtrackProgs).Returns(radtrackProgsMockSet.Object);

            return new ProjectListRepository(mockContext.Object);
        }

        /// <summary>
        /// Returns the repository alongside its mocked DbSet and DbContext
        /// for tests that need to verify Add / SaveChanges calls.
        /// </summary>
        private static (
            ProjectListRepository Repo,
            Mock<DbSet<ProposedProject>> ProposedProjectsDbSet,
            Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<Project>? fpsProjects = null,
                IEnumerable<ProposedProject>? proposedProjects = null,
                IEnumerable<Projects>? yearlyProjects = null,
                IEnumerable<ProjectLatestDetail>? projectLatestDetails = null,
                IEnumerable<RadtrackProg>? radtrackProgs = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(fpsProjects ?? Enumerable.Empty<Project>());
            var proposedProjectsMockSet = RepositoryTestHelper.CreateMockDbSet(proposedProjects ?? Enumerable.Empty<ProposedProject>());
            var yearlyProjectsMockSet = RepositoryTestHelper.CreateMockDbSet(yearlyProjects ?? Enumerable.Empty<Projects>());
            var projectLatestDetailsMockSet = RepositoryTestHelper.CreateMockDbSet(projectLatestDetails ?? Enumerable.Empty<ProjectLatestDetail>());
            var radtrackProgsMockSet = RepositoryTestHelper.CreateMockDbSet(radtrackProgs ?? Enumerable.Empty<RadtrackProg>());

            RepositoryTestHelper.SetupDbSetOperations(proposedProjectsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.ProposedProjects).Returns(proposedProjectsMockSet.Object);
            mockContext.Setup(x => x.MyTlkpProjects).Returns(yearlyProjectsMockSet.Object);
            mockContext.Setup(x => x.ProjectLatestDetails).Returns(projectLatestDetailsMockSet.Object);
            mockContext.Setup(x => x.RadtrackProgs).Returns(radtrackProgsMockSet.Object);

            var repo = new ProjectListRepository(mockContext.Object);
            return (repo, proposedProjectsMockSet, mockContext);
        }

        #region GetAllProjectsAsync — no filter

        [Fact]
        public async Task GetAllProjectsAsync_WithNoFilter_ReturnsAllProjects()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2" },
                new() { Parentproject = "PP003", Program = "PROG3", Customer = "CUST3" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithEmptyProposedProjects_ReturnsEmptyPagedData()
        {
            // Arrange
            var repo = CreateRepository(proposedProjects: new List<ProposedProject>());
            var queryFilter = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
            Assert.Equal(0, result.PaginationData.TotalPages);
        }

        #endregion

        #region GetAllProjectsAsync — OnFps projection

        [Fact]
        public async Task GetAllProjectsAsync_OnFpsFlag_TrueWhenParentProjectExistsInFpsProjects()
        {
            // Arrange
            // PP001 and PP002 appear in projectLatestDetails (Active="Y") with matching radtrackProgs → OnFps="Yes"
            // PP003 is in proposedProjects but NOT in projects → OnFps="No"
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST1", Active = "Y" },
                new() { ParentProject = "PP002", Program = "PROG2", Customer = "CUST2", Active = "Y" }
            };
            var radtrackProgs = new List<RadtrackProg>
            {
                new() { Program = "PROG1" },
                new() { Program = "PROG2" }
            };
            var fpsProjects = new List<Project>
            {
                new() { Parentproject = "PP001" },
                new() { Parentproject = "PP002" }
            };
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2" },
                new() { Parentproject = "PP003", Program = "PROG3", Customer = "CUST3" }
            };
            var repo = CreateRepository(
                fpsProjects: fpsProjects,
                proposedProjects: proposedProjects,
                projectLatestDetails: projectLatestDetails,
                radtrackProgs: radtrackProgs);
            var queryFilter = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal("Yes", result.Data.First(p => p.Parentproject == "PP001").OnFps);
            Assert.Equal("Yes", result.Data.First(p => p.Parentproject == "PP002").OnFps);
            Assert.Equal("No", result.Data.First(p => p.Parentproject == "PP003").OnFps);
        }

        [Fact]
        public async Task GetAllProjectsAsync_OnFpsFlag_FalseWhenNoFpsProjectsExist()
        {
            // Arrange
            // No projectLatestDetails → onFpsQuery returns nothing → all projects get OnFps="No"
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" }
            };
            var repo = CreateRepository(
                fpsProjects: new List<Project>(),
                proposedProjects: proposedProjects,
                projectLatestDetails: new List<ProjectLatestDetail>(),
                radtrackProgs: new List<RadtrackProg>());
            var queryFilter = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal("No", result.Data.First().OnFps);
        }

        [Fact]
        public async Task GetAllProjectsAsync_OnFpsFlag_ExcludesInactiveProjectLatestDetails()
        {
            // Arrange — PP001 is inactive (Active != "Y") so it should not appear with OnFps="Yes"
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST1", Active = "N" }
            };
            var radtrackProgs = new List<RadtrackProg>
            {
                new() { Program = "PROG1" }
            };
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" }
            };
            var repo = CreateRepository(
                proposedProjects: proposedProjects,
                projectLatestDetails: projectLatestDetails,
                radtrackProgs: radtrackProgs);
            var queryFilter = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            // showWhichProjects=1 filters onFpsQuery to Active="Y" only, so the inactive PP001 is excluded
            var result = await repo.GetAllProjectsAsync(queryFilter, 1);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("No", result.Data.First().OnFps);
        }

        #endregion

        #region GetAllProjectsAsync — ApplyFilter

        [Fact]
        public async Task GetAllProjectsAsync_WithNullFilter_ReturnsAllProjects()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithEmptyObjectFilter_ReturnsAllProjects()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithParentprojectFilter_ReturnsFilteredResults()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2" },
                new() { Parentproject = "AA001", Program = "PROG3", Customer = "CUST3" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Parentproject\":\"PP\"}"
            };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, p => Assert.Contains("PP", p.Parentproject));
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithProgramFilter_ReturnsFilteredResults()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { Parentproject = "PP002", Program = "PROG1", Customer = "CUST2" },
                new() { Parentproject = "PP003", Program = "PROG2", Customer = "CUST3" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Program\":\"PROG1\"}"
            };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, p => Assert.Contains("PROG1", p.Program!));
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithCustomerFilter_ReturnsFilteredResults()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST1" },
                new() { Parentproject = "PP003", Program = "PROG3", Customer = "CUST2" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Customer\":\"CUST1\"}"
            };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, p => Assert.Contains("CUST1", p.Customer!));
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithCombinedFilterAndSorting_ReturnsFilteredAndSortedResults()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP003", Program = "PROG1", Customer = "CUST3" },
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Program\":\"PROG1\"}",
                SortBy = "parentproject",
                Descending = false
            };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("PP001", result.Data.First().Parentproject);
            Assert.All(result.Data, p => Assert.Equal("PROG1", p.Program));
        }

        #endregion

        #region GetAllProjectsAsync — ApplySorting

        [Theory]
        [InlineData("parentproject", false, "PP001")]
        [InlineData("parentproject", true, "PP003")]
        [InlineData("program", false, "PROG1")]
        [InlineData("program", true, "PROG3")]
        [InlineData("customer", false, "CUST1")]
        [InlineData("customer", true, "CUST3")]
        public async Task GetAllProjectsAsync_WithSorting_ReturnsSortedResults(
            string sortBy, bool descending, string expectedFirstValue)
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2" },
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { Parentproject = "PP003", Program = "PROG3", Customer = "CUST3" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.NotNull(result);
            var firstItem = result.Data.First();
            var actualValue = sortBy.ToLower() switch
            {
                "parentproject" => firstItem.Parentproject,
                "program" => firstItem.Program,
                "customer" => firstItem.Customer,
                _ => null
            };
            Assert.Equal(expectedFirstValue, actualValue);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetAllProjectsAsync_SortingByOnFps_ReturnsSortedResults(bool descending)
        {
            // Arrange — PP001 is on FPS (via projectLatestDetails + radtrackProgs), PP002 is not
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST1", Active = "Y" }
            };
            var radtrackProgs = new List<RadtrackProg>
            {
                new() { Program = "PROG1" }
            };
            var fpsProjects = new List<Project>
            {
                new() { Parentproject = "PP001" }
            };
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2" },
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" }
            };
            var repo = CreateRepository(
                fpsProjects: fpsProjects,
                proposedProjects: proposedProjects,
                projectLatestDetails: projectLatestDetails,
                radtrackProgs: radtrackProgs);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "onfps",
                Descending = descending
            };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal(2, result.Data.Count);
            // descending = true  → OnFps="Yes" first (PP001)
            // descending = false → OnFps="No"  first (PP002)
            if (descending)
                Assert.Equal("Yes", result.Data.First().OnFps);
            else
                Assert.Equal("No", result.Data.First().OnFps);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithNullSortBy_DefaultsToParentprojectAscending()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP003", Program = "PROG3", Customer = "CUST3" },
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal("PP001", result.Data.First().Parentproject);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithInvalidSortBy_FallsBackToParentprojectAscending()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP003", Program = "PROG3", Customer = "CUST3" },
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "invalid_field"
            };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal("PP001", result.Data.First().Parentproject);
        }

        #endregion

        #region GetAllProjectsAsync — ApplyPaging

        [Fact]
        public async Task GetAllProjectsAsync_WithPaging_ReturnsCorrectPage()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2" },
                new() { Parentproject = "PP003", Program = "PROG3", Customer = "CUST3" },
                new() { Parentproject = "PP004", Program = "PROG4", Customer = "CUST4" },
                new() { Parentproject = "PP005", Program = "PROG5", Customer = "CUST5" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(2, result.PaginationData.PageSize);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithPageLessThan1_DefaultsToPage1()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string> { Page = 0, PageSize = 10 };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithPageSizeLessThan1_DefaultsToPageSize10()
        {
            // Arrange
            var proposedProjects = Enumerable.Range(1, 15)
                .Select(i => new ProposedProject { Parentproject = $"PP{i:D3}", Program = "PROG1", Customer = "CUST1" })
                .ToList();
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string> { Page = 1, PageSize = 0 };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal(10, result.Data.Count);
            Assert.Equal(10, result.PaginationData.PageSize);
        }

        #endregion

        #region GetFpsProjectByIdAsync

        [Fact]
        public async Task GetFpsProjectByIdAsync_ReturnsProject_WhenProjectExists()
        {
            // Arrange
            var fpsProjects = new List<Project>
            {
                new() { Parentproject = "PP001", Projecttitle = "FMD Survey",    Disease = "FMD", Contract = "CON001", Projectstatus = "Active" },
                new() { Parentproject = "PP002", Projecttitle = "TB Eradication", Disease = "TB",  Contract = "CON002", Projectstatus = "Active" }
            };
            var repo = CreateRepository(fpsProjects: fpsProjects);

            // Act
            var result = await repo.GetFpsProjectByIdAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PP001", result.Parentproject);
            Assert.Equal("FMD Survey", result.Projecttitle);
            Assert.Equal("CON001", result.Contract);
            Assert.Equal("FMD", result.Disease);
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
                new() { Id = 1, Parentproject = "PP001", Projecttitle = "TB Project",  Program = "PROG1", Customer = "CUST1", Projectstatus = "Proposed", Disease = "TB" },
                new() { Id = 2, Parentproject = "PP002", Projecttitle = "FMD Project", Program = "PROG2", Customer = "CUST2", Projectstatus = "Active",   Disease = "FMD" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);

            // Act
            var result = await repo.GetProposedProjectByIdAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("PP001", result.Parentproject);
            Assert.Equal("TB Project", result.Projecttitle);
            Assert.Equal("Proposed", result.Projectstatus);
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

        #region GetYearlyDetailsByProjectAsync

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_ReturnsYearlyDetails_WhenProjectExists()
        {
            // Arrange
            var yearlyProjects = new List<Projects>
            {
                new() { Year = (short)2023, Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", Manager = "MGR1" },
                new() { Year = (short)2024, Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", Manager = "MGR1" },
                new() { Year = (short)2024, Parentproject = "PP002", Program = "PROG2", Customer = "CUST2", Manager = "MGR2" }
            };
            var repo = CreateRepository(yearlyProjects: yearlyProjects);

            // Act
            var result = await repo.GetYearlyDetailsByProjectAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal("PP001", p.Parentproject));
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_ReturnsOnlyMatchingParentProject()
        {
            // Arrange
            var yearlyProjects = new List<Projects>
            {
                new() { Year = (short)2022, Parentproject = "PP001", Program = "PROG1" },
                new() { Year = (short)2023, Parentproject = "PP001", Program = "PROG1" },
                new() { Year = (short)2024, Parentproject = "PP002", Program = "PROG2" }
            };
            var repo = CreateRepository(yearlyProjects: yearlyProjects);

            // Act
            var result = await repo.GetYearlyDetailsByProjectAsync("PP001");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal("PP001", p.Parentproject));
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_ReturnsEmptyList_WhenProjectDoesNotExist()
        {
            // Arrange
            var yearlyProjects = new List<Projects>
            {
                new() { Year = (short)2024, Parentproject = "PP001", Program = "PROG1" }
            };
            var repo = CreateRepository(yearlyProjects: yearlyProjects);

            // Act
            var result = await repo.GetYearlyDetailsByProjectAsync("UNKNOWN");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_ReturnsEmptyList_WhenNoYearlyProjectsExist()
        {
            // Arrange
            var repo = CreateRepository(yearlyProjects: new List<Projects>());

            // Act
            var result = await repo.GetYearlyDetailsByProjectAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region AddProjectAsync

        [Fact]
        public async Task AddProjectAsync_AddsEntityAndReturnsIt()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var newEntity = new ProposedProject
            {
                Parentproject = "PP001",
                Projecttitle = "New Project",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Proposed",
                Disease = "FMD"
            };

            // Act
            var result = await repo.AddProjectAsync(newEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(newEntity, result);
            Assert.Equal("PP001", result.Parentproject);
            Assert.Equal("New Project", result.Projecttitle);
            Assert.Equal("Proposed", result.Projectstatus);
        }

        [Fact]
        public async Task AddProjectAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, proposedProjectsDbSet, _) = CreateRepositoryWithMocks();
            var newEntity = new ProposedProject { Parentproject = "PP001", Projecttitle = "New Project" };

            // Act
            await repo.AddProjectAsync(newEntity);

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
            await repo.AddProjectAsync(newEntity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion
    }
}