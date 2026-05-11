using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.TimeCostCalcsRepositoryTest
{
    public class TimeCostCalcsRepositoryTests
    {
        private const int DefaultFpsYear = 2024;
        private const string DefaultUserEmail = "test@example.com";

        private static TimeCostCalcsRepository CreateRepository(
            IEnumerable<TimeCostCalcsView>? timeCostCalcsViews = null,
            IEnumerable<TimeCostCalcs>? timeCostCalcs = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);
            mockRequestContext.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            if (timeCostCalcsViews != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(timeCostCalcsViews);
                mockContext.Setup(x => x.TimeCostCalcsViews).Returns(mockSet.Object);
            }

            if (timeCostCalcs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(timeCostCalcs);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.TimeCostCalcs).Returns(mockSet.Object);
                RepositoryTestHelper.SetupSaveChanges(mockContext);
            }

            return new TimeCostCalcsRepository(mockContext.Object, mockRequestContext.Object);
        }

        private static PaginationParameters<string> DefaultQuery(
            int page = 1, int pageSize = 10,
            string? filter = null, string? sortBy = null, bool descending = false)
            => new PaginationParameters<string>
            {
                Page       = page,
                PageSize   = pageSize,
                Filter     = filter,
                SortBy     = sortBy,
                Descending = descending
            };

        #region GetTimeCostCalcsByProjectAsync â€” Happy path

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_ReturnsRowsForMatchingProject()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", WorkGroup = "WG1", GradeCode = "G1", JobCode = "JOB1", StaffId = "S01", Name = "Alice", Month = 1, Time = 8, Cost = 100, UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", WorkGroup = "WG2", GradeCode = "G2", JobCode = "JOB2", StaffId = "S02", Name = "Bob",   Month = 2, Time = 6, Cost = 80,  UserEmail = DefaultUserEmail },
                new() { Project = "OTHER",  WorkGroup = "WG3", GradeCode = "G3", JobCode = "JOB3", StaffId = "S03", Name = "Carol", Month = 1, Time = 4, Cost = 60,  UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(), "AH0033");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Equal("AH0033", r.Project));
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_ReturnsEmpty_WhenNoMatchingProject()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "OTHER1", WorkGroup = "WG1", StaffId = "S01", Name = "Alice", UserEmail = DefaultUserEmail },
                new() { Project = "OTHER2", WorkGroup = "WG2", StaffId = "S02", Name = "Bob",   UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(), "AH0033");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_ReturnsEmpty_WhenNoData()
        {
            // Arrange
            var repo = CreateRepository(timeCostCalcsViews: []);

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(), "AH0033");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetTimeCostCalcsByProjectAsync â€” Filtering

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_FilterByName_ReturnsMatchingRows()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", Name = "Alice Smith",   WorkGroup = "WG1", StaffId = "S01", UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", Name = "Bob Jones",     WorkGroup = "WG2", StaffId = "S02", UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", Name = "Alice Johnson", WorkGroup = "WG1", StaffId = "S03", UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(filter: "{\"Name\":\"Alice\"}"), "AH0033");

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("alice", r.Name!.ToLower()));
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_FilterByWorkGroup_ReturnsMatchingRows()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", WorkGroup = "APHA_WG1", StaffId = "S01", Name = "Alice", UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", WorkGroup = "OTHER_WG",  StaffId = "S02", Name = "Bob",   UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", WorkGroup = "APHA_WG2", StaffId = "S03", Name = "Carol", UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(filter: "{\"WorkGroup\":\"APHA\"}"), "AH0033");

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_EmptyFilter_ReturnsAllRows()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", StaffId = "S01", Name = "Alice", UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", StaffId = "S02", Name = "Bob",   UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(filter: null), "AH0033");

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region GetTimeCostCalcsByProjectAsync — JSON filter (ApplyJsonFilter)

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_JsonFilter_ByWorkGroup_ReturnsMatchingRows()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", WorkGroup = "APHA_WG1", GradeCode = "G1", JobCode = "JB1", StaffId = "S01", Name = "Alice", UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", WorkGroup = "OTHER_WG",  GradeCode = "G2", JobCode = "JB2", StaffId = "S02", Name = "Bob",   UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);
            var jsonFilter = """{"WorkGroup":"APHA_WG1"}""";

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(filter: jsonFilter), "AH0033");

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("APHA_WG1", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_JsonFilter_ByGradeCode_ReturnsMatchingRows()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", WorkGroup = "WG1", GradeCode = "G1", JobCode = "JB1", StaffId = "S01", Name = "Alice", UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", WorkGroup = "WG2", GradeCode = "G2", JobCode = "JB2", StaffId = "S02", Name = "Bob",   UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);
            var jsonFilter = """{"GradeCode":"G1"}""";

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(filter: jsonFilter), "AH0033");

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("G1", result.Data.First().GradeCode);
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_JsonFilter_ByJobCode_ReturnsMatchingRows()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", WorkGroup = "WG1", GradeCode = "G1", JobCode = "JB1", StaffId = "S01", Name = "Alice", UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", WorkGroup = "WG2", GradeCode = "G2", JobCode = "JB2", StaffId = "S02", Name = "Bob",   UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);
            var jsonFilter = """{"JobCode":"JB2"}""";

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(filter: jsonFilter), "AH0033");

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("JB2", result.Data.First().JobCode);
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_JsonFilter_ByName_ReturnsMatchingRows()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", WorkGroup = "WG1", GradeCode = "G1", JobCode = "JB1", StaffId = "S01", Name = "Alice Smith", UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", WorkGroup = "WG2", GradeCode = "G2", JobCode = "JB2", StaffId = "S02", Name = "Bob Jones",   UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);
            var jsonFilter = """{"Name":"Alice"}""";

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(filter: jsonFilter), "AH0033");

            // Assert
            Assert.Single(result.Data);
            Assert.Contains("Alice", result.Data.First().Name);
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_JsonFilter_AllFields_ReturnsMatchingRows()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", WorkGroup = "WG1", GradeCode = "G1", JobCode = "JB1", StaffId = "S01", Name = "Alice", UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", WorkGroup = "WG2", GradeCode = "G2", JobCode = "JB2", StaffId = "S02", Name = "Bob",   UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);
            var jsonFilter = """{"WorkGroup":"WG1","GradeCode":"G1","JobCode":"JB1","Name":"Alice"}""";

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(filter: jsonFilter), "AH0033");

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("S01", result.Data.First().StaffId);
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_JsonFilter_EmptyObject_ReturnsAllRows()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", WorkGroup = "WG1", StaffId = "S01", Name = "Alice", UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", WorkGroup = "WG2", StaffId = "S02", Name = "Bob",   UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);
            var jsonFilter = "{}";

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(filter: jsonFilter), "AH0033");

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region GetTimeCostCalcsByProjectAsync â€” Sorting

        [Theory]
        [InlineData("name",      false)]
        [InlineData("name",      true)]
        [InlineData("workgroup", false)]
        [InlineData("workgroup", true)]
        [InlineData("gradecode", false)]
        [InlineData("month",     false)]
        [InlineData("time",      false)]
        [InlineData("cost",      false)]
        [InlineData("jobcode",   false)]
        public async Task GetTimeCostCalcsByProjectAsync_WithSortBy_DoesNotThrow(string sortBy, bool descending)
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", Name = "Bob",   WorkGroup = "WG2", GradeCode = "G2", JobCode = "JB2", Month = 2, Time = 6, Cost = 80,  UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", Name = "Alice", WorkGroup = "WG1", GradeCode = "G1", JobCode = "JB1", Month = 1, Time = 8, Cost = 100, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(sortBy: sortBy, descending: descending), "AH0033");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_UnrecognisedSortBy_ReturnsUnsortedResults()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", StaffId = "S01", Name = "Alice", UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", StaffId = "S02", Name = "Bob",   UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(sortBy: "unknown_column"), "AH0033");

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region GetTimeCostCalcsByProjectAsync â€” Pagination

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_Pagination_ReturnsCorrectPage()
        {
            // Arrange
            var views = Enumerable.Range(1, 15)
                .Select(i => new TimeCostCalcsView { Project = "AH0033", StaffId = $"S{i:D2}", Name = $"Staff{i}", UserEmail = DefaultUserEmail })
                .ToList();
            var repo = CreateRepository(timeCostCalcsViews: views);

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(page: 2, pageSize: 5), "AH0033");

            // Assert
            Assert.Equal(5, result.Data.Count());
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_PageSizeLargerThanData_ReturnsAllRows()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", StaffId = "S01", Name = "Alice", UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", StaffId = "S02", Name = "Bob",   UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(page: 1, pageSize: 50), "AH0033");

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region GetTimeCostCalcsByProjectAsync â€” ProjectCode edge cases

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTimeCostCalcsByProjectAsync_EmptyOrWhitespaceProjectCode_ReturnsEmpty(string projectCode)
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", StaffId = "S01", Name = "Alice", UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);

            // Act
            var result = await repo.GetTimeCostCalcsByProjectAsync(DefaultQuery(), projectCode);

            // Assert
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetTotalActualByProjectAsync

        [Fact]
        public async Task GetTotalActualByProjectAsync_ReturnsCorrectSums()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", WorkGroup = "WG1", GradeCode = "G1", JobCode = "J1", StaffId = "S01", Name = "Alice", Time = 8,  Cost = 200, FpsYear = 2024, UserEmail = DefaultUserEmail },
                new() { Project = "AH0033", WorkGroup = "WG2", GradeCode = "G2", JobCode = "J2", StaffId = "S02", Name = "Bob",   Time = 6,  Cost = 150, FpsYear = 2024, UserEmail = DefaultUserEmail },
                new() { Project = "OTHER",  WorkGroup = "WG3", GradeCode = "G3", JobCode = "J3", StaffId = "S03", Name = "Carol", Time = 10, Cost = 300, FpsYear = 2024, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);

            // Act
            var (totalHours, totalCost) = await repo.GetTotalActualByProjectAsync("AH0033");

            // Assert
            Assert.Equal(14.0, totalHours);
            Assert.Equal(350.0, totalCost);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WhenNoMatchingData_ReturnsZeroTotals()
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "OTHER", WorkGroup = "WG1", StaffId = "S01", Time = 8, Cost = 200, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);

            // Act
            var (totalHours, totalCost) = await repo.GetTotalActualByProjectAsync("AH0033");

            // Assert
            Assert.Equal(0.0, totalHours);
            Assert.Equal(0.0, totalCost);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WhenNoData_ReturnsZeroTotals()
        {
            // Arrange
            var repo = CreateRepository(timeCostCalcsViews: []);

            // Act
            var (totalHours, totalCost) = await repo.GetTotalActualByProjectAsync("AH0033");

            // Assert
            Assert.Equal(0.0, totalHours);
            Assert.Equal(0.0, totalCost);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTotalActualByProjectAsync_WithEmptyOrWhitespaceProjectCode_ReturnsZeroTotals(string projectCode)
        {
            // Arrange
            var views = new List<TimeCostCalcsView>
            {
                new() { Project = "AH0033", WorkGroup = "WG1", StaffId = "S01", Time = 8, Cost = 200, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(timeCostCalcsViews: views);

            // Act
            var (totalHours, totalCost) = await repo.GetTotalActualByProjectAsync(projectCode);

            // Assert
            Assert.Equal(0.0, totalHours);
            Assert.Equal(0.0, totalCost);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WhenRecordExists_RemovesAndReturnsTrue()
        {
            // Arrange
            var entity = new TimeCostCalcs
            {
                WorkGroup = "WG1", JobCode = "JOB1", Project = "AH0033",
                Month = 1, StaffId = "S01", FpsYear = 2024
            };
            var repo = CreateRepository(timeCostCalcs: new List<TimeCostCalcs> { entity });

            // Act
            var result = await repo.DeleteAsync("WG1", "JOB1", "AH0033", 1, "S01");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenRecordNotFound_ReturnsFalse()
        {
            // Arrange
            var entities = new List<TimeCostCalcs>
            {
                new() { WorkGroup = "WG1", JobCode = "JOB1", Project = "AH0033", Month = 1, StaffId = "S01" }
            };
            var repo = CreateRepository(timeCostCalcs: entities);

            // Act
            var result = await repo.DeleteAsync("WG1", "JOB1", "AH0033", 1, "UNKNOWN");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenNoRecords_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(timeCostCalcs: new List<TimeCostCalcs>());

            // Act
            var result = await repo.DeleteAsync("WG1", "JOB1", "AH0033", 1, "S01");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("",    "JOB1", "AH0033", "S01")]
        [InlineData("WG1", "",     "AH0033", "S01")]
        [InlineData("WG1", "JOB1", "",       "S01")]
        [InlineData("WG1", "JOB1", "AH0033", ""   )]
        public async Task DeleteAsync_WithMissingRequiredParam_ReturnsFalse(
            string workgroup, string jobCode, string project, string staffId)
        {
            // Arrange
            var repo = CreateRepository(timeCostCalcs: new List<TimeCostCalcs>
            {
                new() { WorkGroup = "WG1", JobCode = "JOB1", Project = "AH0033", Month = 1, StaffId = "S01" }
            });

            // Act
            var result = await repo.DeleteAsync(workgroup, jobCode, project, 1, staffId);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
