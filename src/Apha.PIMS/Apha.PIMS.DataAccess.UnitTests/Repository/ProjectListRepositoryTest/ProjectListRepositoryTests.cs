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
            IEnumerable<RadtrackProg>? radtrackProgs = null,
            IEnumerable<ProjectStatus>? projectStatuses = null,
            IEnumerable<ProjectRadTrackData>? projectRadtrackdata = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var projectsMockSet             = RepositoryTestHelper.CreateMockDbSet(fpsProjects           ?? Enumerable.Empty<Project>());
            var proposedProjectsMockSet     = RepositoryTestHelper.CreateMockDbSet(proposedProjects       ?? Enumerable.Empty<ProposedProject>());
            var yearlyProjectsMockSet       = RepositoryTestHelper.CreateMockDbSet(yearlyProjects         ?? Enumerable.Empty<Projects>());
            var projectLatestDetailsMockSet = RepositoryTestHelper.CreateMockDbSet(projectLatestDetails   ?? Enumerable.Empty<ProjectLatestDetail>());
            var radtrackProgsMockSet        = RepositoryTestHelper.CreateMockDbSet(radtrackProgs           ?? Enumerable.Empty<RadtrackProg>());
            var projectStatusesMockSet      = RepositoryTestHelper.CreateMockDbSet(projectStatuses         ?? Enumerable.Empty<ProjectStatus>());
            var projectRadtrackdataMockSet  = RepositoryTestHelper.CreateMockDbSet(projectRadtrackdata     ?? Enumerable.Empty<ProjectRadTrackData>());

            RepositoryTestHelper.SetupDbSetOperations(proposedProjectsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.ProposedProjects).Returns(proposedProjectsMockSet.Object);
            mockContext.Setup(x => x.MyTlkpProjects).Returns(yearlyProjectsMockSet.Object);
            mockContext.Setup(x => x.ProjectLatestDetails).Returns(projectLatestDetailsMockSet.Object);
            mockContext.Setup(x => x.RadtrackProgs).Returns(radtrackProgsMockSet.Object);
            mockContext.Setup(x => x.ProjectStatuses).Returns(projectStatusesMockSet.Object);
            mockContext.Setup(x => x.ProjectRadTrackData).Returns(projectRadtrackdataMockSet.Object);

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
                IEnumerable<RadtrackProg>? radtrackProgs = null,
                IEnumerable<ProjectStatus>? projectStatuses = null,
                IEnumerable<ProjectRadTrackData>? projectRadtrackdata = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var projectsMockSet             = RepositoryTestHelper.CreateMockDbSet(fpsProjects           ?? Enumerable.Empty<Project>());
            var proposedProjectsMockSet     = RepositoryTestHelper.CreateMockDbSet(proposedProjects       ?? Enumerable.Empty<ProposedProject>());
            var yearlyProjectsMockSet       = RepositoryTestHelper.CreateMockDbSet(yearlyProjects         ?? Enumerable.Empty<Projects>());
            var projectLatestDetailsMockSet = RepositoryTestHelper.CreateMockDbSet(projectLatestDetails   ?? Enumerable.Empty<ProjectLatestDetail>());
            var radtrackProgsMockSet        = RepositoryTestHelper.CreateMockDbSet(radtrackProgs           ?? Enumerable.Empty<RadtrackProg>());
            var projectStatusesMockSet      = RepositoryTestHelper.CreateMockDbSet(projectStatuses         ?? Enumerable.Empty<ProjectStatus>());
            var projectRadtrackdataMockSet  = RepositoryTestHelper.CreateMockDbSet(projectRadtrackdata     ?? Enumerable.Empty<ProjectRadTrackData>());

            RepositoryTestHelper.SetupDbSetOperations(proposedProjectsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.ProposedProjects).Returns(proposedProjectsMockSet.Object);
            mockContext.Setup(x => x.MyTlkpProjects).Returns(yearlyProjectsMockSet.Object);
            mockContext.Setup(x => x.ProjectLatestDetails).Returns(projectLatestDetailsMockSet.Object);
            mockContext.Setup(x => x.RadtrackProgs).Returns(radtrackProgsMockSet.Object);
            mockContext.Setup(x => x.ProjectStatuses).Returns(projectStatusesMockSet.Object);
            mockContext.Setup(x => x.ProjectRadTrackData).Returns(projectRadtrackdataMockSet.Object);

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
            // PP001 and PP002 appear in projectLatestDetails (Active="Y") with matching radtrackProgs ? OnFps="Yes"
            // PP003 is in proposedProjects but NOT in projects ? OnFps="No"
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
            Assert.Equal("No",  result.Data.First(p => p.Parentproject == "PP003").OnFps);
        }

        [Fact]
        public async Task GetAllProjectsAsync_OnFpsFlag_FalseWhenNoFpsProjectsExist()
        {
            // Arrange
            // No projectLatestDetails ? onFpsQuery returns nothing ? all projects get OnFps="No"
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
            // Arrange — PP001 is inactive (Active="N"), showWhichProjects=1 filters to Active="Y" only
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
            var result = await repo.GetAllProjectsAsync(queryFilter, 1);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("No", result.Data.First().OnFps);
        }

        [Fact]
        public async Task GetAllProjectsAsync_ShowWhichProjects2_IncludesActiveAndInactiveOnFpsProjects()
        {
            // Arrange — PP001 Active="Y", PP002 Active="N"; showWhichProjects=2 ? both included
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST1", Active = "Y" },
                new() { ParentProject = "PP002", Program = "PROG2", Customer = "CUST2", Active = "N" }
            };
            var radtrackProgs = new List<RadtrackProg>
            {
                new() { Program = "PROG1" },
                new() { Program = "PROG2" }
            };
            var repo = CreateRepository(
                projectLatestDetails: projectLatestDetails,
                radtrackProgs: radtrackProgs);
            var queryFilter = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, p => Assert.Equal("Yes", p.OnFps));
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

        [Fact]
        public async Task GetAllProjectsAsync_WithFilterMatchingNoRecords_ReturnsEmpty()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Parentproject\":\"ZZZZ\"}"
            };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetAllProjectsAsync — ApplySorting

        [Theory]
        [InlineData("parentproject", false, "PP001")]
        [InlineData("parentproject", true,  "PP003")]
        [InlineData("program",       false, "PROG1")]
        [InlineData("program",       true,  "PROG3")]
        [InlineData("customer",      false, "CUST1")]
        [InlineData("customer",      true,  "CUST3")]
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
                "program"       => firstItem.Program,
                "customer"      => firstItem.Customer,
                _               => null
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
            // descending=true  ? "Yes" first (PP001)
            // descending=false ? "No"  first (PP002)
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

            // Assert — page=0 is not normalised by BaseRepository; PageNumber reflects the raw value
            Assert.Equal(0, result.PaginationData.PageNumber);
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

            // Assert — pageSize=0 is not normalised by BaseRepository; Take(0) returns no items
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.PageSize);
        }

        [Fact]
        public async Task GetAllProjectsAsync_LastPage_ReturnsRemainingItems()
        {
            // Arrange — 5 items, pageSize=2 ? page 3 should return 1 item
            var proposedProjects = Enumerable.Range(1, 5)
                .Select(i => new ProposedProject { Parentproject = $"PP{i:D3}", Program = "PROG1", Customer = "CUST1" })
                .ToList();
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string> { Page = 3, PageSize = 2 };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetAllProjectsAsync_PageBeyondTotal_ReturnsEmpty()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);
            var queryFilter = new PaginationParameters<string> { Page = 99, PageSize = 10 };

            // Act
            var result = await repo.GetAllProjectsAsync(queryFilter, 2);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetAllProjectsForDropDownAsync

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_ReturnsProjectsOnFpsWithMatchingRadtrackProg()
        {
            // Arrange — PP001 Active="Y" with matching radtrackProg ? included
            //            PP002 Active="N"                            ? excluded
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST1", Active = "Y" },
                new() { ParentProject = "PP002", Program = "PROG2", Customer = "CUST2", Active = "N" }
            };
            var radtrackProgs = new List<RadtrackProg>
            {
                new() { Program = "PROG1" },
                new() { Program = "PROG2" }
            };
            var repo = CreateRepository(
                projectLatestDetails: projectLatestDetails,
                radtrackProgs: radtrackProgs);

            // Act
            var result = await repo.GetAllProjectsForDropDownAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("PP001", result[0].Parentproject);
            Assert.Equal("PP002", result[1].Parentproject);
        }

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_SetsOnFpsToYesForAllResults()
        {
            // Arrange
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
            var repo = CreateRepository(
                projectLatestDetails: projectLatestDetails,
                radtrackProgs: radtrackProgs);

            // Act
            var result = await repo.GetAllProjectsForDropDownAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal("Yes", p.OnFps));
        }

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_ExcludesProjectsWithNoMatchingRadtrackProg()
        {
            // Arrange — PP001 has no matching RadtrackProg ? excluded from the join
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST1", Active = "Y" }
            };
            var radtrackProgs = new List<RadtrackProg>
            {
                new() { Program = "PROG_OTHER" }  // no match for PROG1
            };
            var repo = CreateRepository(
                projectLatestDetails: projectLatestDetails,
                radtrackProgs: radtrackProgs);

            // Act
            var result = await repo.GetAllProjectsForDropDownAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_ReturnsEmptyList_WhenNoActiveProjectsExist()
        {
            // Arrange
            var repo = CreateRepository(
                projectLatestDetails: new List<ProjectLatestDetail>(),
                radtrackProgs: new List<RadtrackProg>());

            // Act
            var result = await repo.GetAllProjectsForDropDownAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_MapsProjectFieldsCorrectly()
        {
            // Arrange
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST_ABC", Active = "Y" }
            };
            var radtrackProgs = new List<RadtrackProg>
            {
                new() { Program = "PROG1" }
            };
            var repo = CreateRepository(
                projectLatestDetails: projectLatestDetails,
                radtrackProgs: radtrackProgs);

            // Act
            var result = await repo.GetAllProjectsForDropDownAsync();

            // Assert
            Assert.Single(result);
            var item = result[0];
            Assert.Equal("PP001",    item.Parentproject);
            Assert.Equal("PROG1",    item.Program);
            Assert.Equal("CUST_ABC", item.Customer);
            Assert.Equal("Yes",      item.OnFps);
        }

        #endregion

        #region GetAllProjectsForMilestone

        [Fact]
        public async Task GetAllProjectsForMilestone_ReturnsJoinedResults()
        {
            // Arrange — PP001 exists in both ProjectRadtrackdata and ProjectLatestDetails
            var projectRadtrackdata = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001" },
                new() { Parentproject = "PP002" }
            };
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST1", ProjectGroup = "GRP1" },
                new() { ParentProject = "PP002", Program = "PROG2", Customer = "CUST2", ProjectGroup = "GRP2" }
            };
            var repo = CreateRepository(
                projectLatestDetails: projectLatestDetails,
                projectRadtrackdata: projectRadtrackdata);

            // Act
            var result = await repo.GetAllProjectsForMilestone();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllProjectsForMilestone_MapsFieldsCorrectly()
        {
            // Arrange
            var projectRadtrackdata = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001" }
            };
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST_ABC", ProjectGroup = "GRP_X" }
            };
            var repo = CreateRepository(
                projectLatestDetails: projectLatestDetails,
                projectRadtrackdata: projectRadtrackdata);

            // Act
            var result = await repo.GetAllProjectsForMilestone();

            // Assert
            Assert.Single(result);
            var item = result[0];
            Assert.Equal("PP001",    item.Parentproject);
            Assert.Equal("PROG1",    item.Program);
            Assert.Equal("CUST_ABC", item.Customer);
            Assert.Equal("GRP_X",    item.ProjectGroup);
        }

        [Fact]
        public async Task GetAllProjectsForMilestone_ReturnsOrderedByParentproject()
        {
            // Arrange
            var projectRadtrackdata = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP003" },
                new() { Parentproject = "PP001" },
                new() { Parentproject = "PP002" }
            };
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP003", Program = "PROG3", Customer = "CUST3" },
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST1" },
                new() { ParentProject = "PP002", Program = "PROG2", Customer = "CUST2" }
            };
            var repo = CreateRepository(
                projectLatestDetails: projectLatestDetails,
                projectRadtrackdata: projectRadtrackdata);

            // Act
            var result = await repo.GetAllProjectsForMilestone();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("PP001", result[0].Parentproject);
            Assert.Equal("PP002", result[1].Parentproject);
            Assert.Equal("PP003", result[2].Parentproject);
        }

        [Fact]
        public async Task GetAllProjectsForMilestone_ExcludesProjectsNotInProjectLatestDetails()
        {
            // Arrange — PP002 is in ProjectRadtrackdata but not in ProjectLatestDetails ? excluded by inner join
            var projectRadtrackdata = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001" },
                new() { Parentproject = "PP002" }
            };
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST1" }
            };
            var repo = CreateRepository(
                projectLatestDetails: projectLatestDetails,
                projectRadtrackdata: projectRadtrackdata);

            // Act
            var result = await repo.GetAllProjectsForMilestone();

            // Assert
            Assert.Single(result);
            Assert.Equal("PP001", result[0].Parentproject);
        }

        [Fact]
        public async Task GetAllProjectsForMilestone_ReturnsEmpty_WhenNoProjectRadtrackdataExists()
        {
            // Arrange
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST1" }
            };
            var repo = CreateRepository(
                projectLatestDetails: projectLatestDetails,
                projectRadtrackdata: new List<ProjectRadTrackData>());

            // Act
            var result = await repo.GetAllProjectsForMilestone();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProjectsForMilestone_ReturnsEmpty_WhenNoProjectLatestDetailsExist()
        {
            // Arrange
            var projectRadtrackdata = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001" }
            };
            var repo = CreateRepository(
                projectLatestDetails: new List<ProjectLatestDetail>(),
                projectRadtrackdata: projectRadtrackdata);

            // Act
            var result = await repo.GetAllProjectsForMilestone();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProjectsForMilestone_ProjectGroupIsNull_WhenNotSetInLatestDetails()
        {
            // Arrange
            var projectRadtrackdata = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001" }
            };
            var projectLatestDetails = new List<ProjectLatestDetail>
            {
                new() { ParentProject = "PP001", Program = "PROG1", Customer = "CUST1", ProjectGroup = null }
            };
            var repo = CreateRepository(
                projectLatestDetails: projectLatestDetails,
                projectRadtrackdata: projectRadtrackdata);

            // Act
            var result = await repo.GetAllProjectsForMilestone();

            // Assert
            Assert.Single(result);
            Assert.Null(result[0].ProjectGroup);
        }

        #endregion
    }
}