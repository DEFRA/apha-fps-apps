using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectGroupStaffPlanRepositoryTest
{
    public class ProjectGroupStaffPlanRepositoryTests
    {
        private const string DefaultUserEmail = "test@example.com";

        /// <summary>
        /// Creates a ProjectGroupStaffPlanRepository with an in-memory ProjectGroupStaffPlanViews DbSet.
        /// EF.Functions.ILike column filters and ExecuteDeleteAsync are covered by integration tests.
        /// </summary>
        private static ProjectGroupStaffPlanRepository CreateRepository(
            IEnumerable<ProjectGroupStaffPlanView>? views = null,
            string userEmailId = DefaultUserEmail)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);
            requestContext.UserEmailId.Returns(userEmailId);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(views ?? Enumerable.Empty<ProjectGroupStaffPlanView>());
            mockContext.Setup(x => x.ProjectGroupStaffPlanViews).Returns(mockSet.Object);

            return new ProjectGroupStaffPlanRepository(mockContext.Object, requestContext);
        }

        private static PaginationParameters<string> DefaultQuery(
            int page = 1, int pageSize = 10,
            string? filter = null, string? sortBy = null, bool descending = false)
            => new()
            {
                Page       = page,
                PageSize   = pageSize,
                Filter     = filter,
                SortBy     = sortBy,
                Descending = descending
            };

        /// <summary>
        /// Sample data — UserEmail matches DefaultUserEmail so all rows pass the user filter by default.
        /// </summary>
        private static List<ProjectGroupStaffPlanView> SampleData() =>
        [
            new() { ProjectGroup = "GROUP_A", ResourceCentre = "RC1", WorkGroup = "WG1", GradeCode = "G1", Name = "Alice Smith",  Manager = "Manager_A", JobCode = "JC1", ProjectStatus = "Active",    Hrs = 100.0, ChargeRate = 500m, Fee = 250m, UserEmail = DefaultUserEmail },
            new() { ProjectGroup = "GROUP_A", ResourceCentre = "RC2", WorkGroup = "WG2", GradeCode = "G2", Name = "Bob Jones",    Manager = "Manager_B", JobCode = "JC2", ProjectStatus = "Completed", Hrs = 80.0,  ChargeRate = 400m, Fee = 200m, UserEmail = DefaultUserEmail },
            new() { ProjectGroup = "GROUP_B", ResourceCentre = "RC1", WorkGroup = "WG1", GradeCode = "G1", Name = "Carol White",  Manager = "Manager_A", JobCode = "JC3", ProjectStatus = "Active",    Hrs = 60.0,  ChargeRate = 300m, Fee = 150m, UserEmail = DefaultUserEmail },
            new() { ProjectGroup = "GROUP_C", ResourceCentre = "RC3", WorkGroup = "WG3", GradeCode = "G3", Name = "Dave Brown",   Manager = "Manager_C", JobCode = "JC4", ProjectStatus = "Pending",   Hrs = 40.0,  ChargeRate = 200m, Fee = 100m, UserEmail = DefaultUserEmail }
        ];

        #region User email filtering

        [Fact]
        public async Task GetPagedAsync_ReturnsRows_WhenUserEmailMatchesExactly()
        {
            // Arrange
            var repo = CreateRepository(SampleData(), userEmailId: DefaultUserEmail);

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery());

            // Assert
            Assert.Equal(4, result.Data.Count());
        }

        [Theory]
        [InlineData("Test@Example.COM")]
        [InlineData("TEST@EXAMPLE.COM")]
        [InlineData("Test@example.com")]
        public async Task GetPagedAsync_ReturnsRows_WhenDbEmailIsMixedCase(string dbEmail)
        {
            // Arrange — DB stores mixed-case email; middleware normalises incoming to lowercase.
            // The query must use ToLower() so the comparison still matches.
            var views = new List<ProjectGroupStaffPlanView>
            {
                new() { ProjectGroup = "GROUP_A", Manager = "Manager_A", UserEmail = dbEmail }
            };
            var repo = CreateRepository(views, userEmailId: "test@example.com");

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery());

            // Assert — must find the record despite casing mismatch in DB
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedAsync_ExcludesRows_WhenEmailBelongsToDifferentUser()
        {
            // Arrange — two rows with different emails; only the matching one should be returned
            var views = new List<ProjectGroupStaffPlanView>
            {
                new() { ProjectGroup = "GROUP_A", Manager = "Manager_A", UserEmail = DefaultUserEmail },
                new() { ProjectGroup = "GROUP_B", Manager = "Manager_B", UserEmail = "other@example.com" }
            };
            var repo = CreateRepository(views, userEmailId: DefaultUserEmail);

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery());

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("GROUP_A", result.Data.First().ProjectGroup);
        }

        [Fact]
        public async Task GetPagedAsync_ExcludesRows_WhenDbEmailIsNull()
        {
            // Arrange — null UserEmail in DB must not match any user
            var views = new List<ProjectGroupStaffPlanView>
            {
                new() { ProjectGroup = "GROUP_A", Manager = "Manager_A", UserEmail = null }
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery());

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsEmpty_WhenUserHasNoRows()
        {
            // Arrange — all rows belong to a different user
            var views = new List<ProjectGroupStaffPlanView>
            {
                new() { ProjectGroup = "GROUP_A", UserEmail = "other@example.com" }
            };
            var repo = CreateRepository(views, userEmailId: DefaultUserEmail);

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery());

            // Assert
            Assert.Empty(result.Data);
        }

        #endregion

        #region Happy path

        [Fact]
        public async Task GetPagedAsync_ReturnsAllRows_WhenNoFilter()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsEmpty_WhenNoData()
        {
            // Arrange
            var repo = CreateRepository([]);

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedAsync_DefaultSort_OrdersByProjectGroupThenManager()
        {
            // Arrange — insert in reverse order to verify sort is applied
            var views = new List<ProjectGroupStaffPlanView>
            {
                new() { ProjectGroup = "GROUP_C", Manager = "Manager_C", UserEmail = DefaultUserEmail },
                new() { ProjectGroup = "GROUP_A", Manager = "Manager_B", UserEmail = DefaultUserEmail },
                new() { ProjectGroup = "GROUP_A", Manager = "Manager_A", UserEmail = DefaultUserEmail },
                new() { ProjectGroup = "GROUP_B", Manager = "Manager_A", UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery());
            var list = result.Data.ToList();

            // Assert
            Assert.Equal("GROUP_A", list[0].ProjectGroup);
            Assert.Equal("Manager_A", list[0].Manager);
            Assert.Equal("GROUP_A", list[1].ProjectGroup);
            Assert.Equal("Manager_B", list[1].Manager);
            Assert.Equal("GROUP_B", list[2].ProjectGroup);
            Assert.Equal("GROUP_C", list[3].ProjectGroup);
        }

        [Fact]
        public async Task GetPagedAsync_PaginationMetadata_IsCorrect()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(page: 1, pageSize: 10));

            // Assert
            Assert.Equal(4, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
        }

        #endregion

        #region Paging

        [Fact]
        public async Task GetPagedAsync_Paging_ReturnsCorrectPage()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(page: 1, pageSize: 2));

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(4, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedAsync_Paging_SecondPage_ReturnsRemainingRows()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(page: 2, pageSize: 2));

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_Paging_PageBeyondData_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(page: 99, pageSize: 10));

            // Assert
            Assert.Empty(result.Data);
        }

        #endregion

        #region Filtering

        [Fact]
        public async Task GetPagedAsync_NullFilter_ReturnsAllMatchingRows()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(filter: null));

            // Assert
            Assert.Equal(4, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_EmptyFilter_ReturnsAllMatchingRows()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{}"));

            // Assert
            Assert.Equal(4, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_FilterByProjectGroup_ReturnsMatchingRows()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"ProjectGroup\":\"GROUP_A\"}"));

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("GROUP_A", r.ProjectGroup, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAsync_FilterByProjectGroup_NoMatch_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"ProjectGroup\":\"ZZZZ\"}"));

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedAsync_FilterByResourceCentre_ReturnsMatchingRows()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"ResourceCentre\":\"RC1\"}"));

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("RC1", r.ResourceCentre, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAsync_FilterByWorkGroup_ReturnsMatchingRows()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"WorkGroup\":\"WG1\"}"));

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("WG1", r.WorkGroup, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAsync_FilterByGradeCode_ReturnsMatchingRows()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"GradeCode\":\"G1\"}"));

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("G1", r.GradeCode, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAsync_FilterByName_ReturnsMatchingRows()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"Name\":\"Alice\"}"));

            // Assert
            Assert.Single(result.Data);
            Assert.Contains("Alice", result.Data.First().Name, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPagedAsync_FilterByManager_ReturnsMatchingRows()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"Manager\":\"Manager_A\"}"));

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("Manager_A", r.Manager, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAsync_FilterByJobCode_ReturnsMatchingRows()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"JobCode\":\"JC1\"}"));

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("JC1", result.Data.First().JobCode);
        }

        [Fact]
        public async Task GetPagedAsync_FilterByProjectStatus_ReturnsMatchingRows()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"ProjectStatus\":\"Active\"}"));

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("Active", r.ProjectStatus, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAsync_MultipleFilters_ReturnsNarrowedResults()
        {
            // Arrange — GROUP_A + Manager_A → only Alice
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(
                filter: "{\"ProjectGroup\":\"GROUP_A\",\"Manager\":\"Manager_A\"}"));

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("Alice Smith", result.Data.First().Name);
        }

        #endregion

        #region Sorting

        [Fact]
        public async Task GetPagedAsync_SortByProjectGroup_Ascending()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "ProjectGroup", descending: false));
            var list   = result.Data.ToList();

            // Assert
            Assert.True(string.Compare(list[0].ProjectGroup, list[^1].ProjectGroup, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByProjectGroup_Descending()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "ProjectGroup", descending: true));
            var list   = result.Data.ToList();

            // Assert
            Assert.True(string.Compare(list[0].ProjectGroup, list[^1].ProjectGroup, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByManager_Ascending()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "Manager", descending: false));
            var list   = result.Data.ToList();

            // Assert
            Assert.True(string.Compare(list[0].Manager, list[^1].Manager, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByManager_Descending()
        {
            // Arrange
            var repo = CreateRepository(SampleData());

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "Manager", descending: true));
            var list   = result.Data.ToList();

            // Assert
            Assert.True(string.Compare(list[0].Manager, list[^1].Manager, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByResourceCentre_Ascending()
        {
            var repo = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "ResourceCentre", descending: false));
            var list   = result.Data.ToList();
            Assert.True(string.Compare(list[0].ResourceCentre, list[^1].ResourceCentre, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByWorkGroup_Ascending()
        {
            var repo = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "WorkGroup", descending: false));
            var list   = result.Data.ToList();
            Assert.True(string.Compare(list[0].WorkGroup, list[^1].WorkGroup, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByGradeCode_Ascending()
        {
            var repo = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "GradeCode", descending: false));
            var list   = result.Data.ToList();
            Assert.True(string.Compare(list[0].GradeCode, list[^1].GradeCode, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByName_Ascending()
        {
            var repo = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "Name", descending: false));
            var list   = result.Data.ToList();
            Assert.True(string.Compare(list[0].Name, list[^1].Name, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByJobCode_Ascending()
        {
            var repo = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "JobCode", descending: false));
            var list   = result.Data.ToList();
            Assert.True(string.Compare(list[0].JobCode, list[^1].JobCode, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByProjectStatus_Ascending()
        {
            var repo = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "ProjectStatus", descending: false));
            var list   = result.Data.ToList();
            Assert.True(string.Compare(list[0].ProjectStatus, list[^1].ProjectStatus, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByHrs_Ascending()
        {
            var repo = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "Hrs", descending: false));
            var list   = result.Data.ToList();
            Assert.True(list[0].Hrs <= list[^1].Hrs);
        }

        [Fact]
        public async Task GetPagedAsync_SortByHrs_Descending()
        {
            var repo = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "Hrs", descending: true));
            var list   = result.Data.ToList();
            Assert.True(list[0].Hrs >= list[^1].Hrs);
        }

        [Fact]
        public async Task GetPagedAsync_SortByChargeRate_Ascending()
        {
            var repo = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "ChargeRate", descending: false));
            var list   = result.Data.ToList();
            Assert.True(list[0].ChargeRate <= list[^1].ChargeRate);
        }

        [Fact]
        public async Task GetPagedAsync_SortByFee_Descending()
        {
            var repo = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "Fee", descending: true));
            var list   = result.Data.ToList();
            Assert.True(list[0].Fee >= list[^1].Fee);
        }

        [Fact]
        public async Task GetPagedAsync_SortByUnknownColumn_FallsBackToDefaultSort()
        {
            // Arrange — unknown sort column → default: OrderBy ProjectGroup ThenBy Manager
            var views = new List<ProjectGroupStaffPlanView>
            {
                new() { ProjectGroup = "GROUP_C", Manager = "Manager_C", UserEmail = DefaultUserEmail },
                new() { ProjectGroup = "GROUP_A", Manager = "Manager_A", UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "NonExistentColumn"));
            var list   = result.Data.ToList();

            // Assert — should fall back to ProjectGroup ascending
            Assert.Equal("GROUP_A", list[0].ProjectGroup);
            Assert.Equal("GROUP_C", list[1].ProjectGroup);
        }

        #endregion
    }
}
