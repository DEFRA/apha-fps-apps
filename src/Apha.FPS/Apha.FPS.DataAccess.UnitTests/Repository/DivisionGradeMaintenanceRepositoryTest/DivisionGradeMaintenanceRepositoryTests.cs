using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.DivisionGradeMaintenanceRepositoryTest
{
    public class DivisionGradeMaintenanceRepositoryTests
    {
        #region Helpers

        private static DivisionGradeMaintenance BuildDivisionGrade(
            string code = "A-VSD",
            string gradeCode = "A",
            string division = "VSD",
            int fpsYear = 2025,
            decimal? chargeRate = 100m,
            decimal? directRate = 90m,
            decimal? payRate = 80m,
            decimal? npr = 10m,
            decimal? ohr = 5m) =>
            new()
            {
                DivisionGradeCode = code,
                GradeCode = gradeCode,
                Division = division,
                FpsYear = fpsYear,
                ChargeRate = chargeRate,
                DirectRate = directRate,
                PayRate = payRate,
                Npr = npr,
                Ohr = ohr
            };

        private static DivisionGradeMaintenanceRepository CreateRepository(
            IEnumerable<DivisionGradeMaintenance>? divisionGrades = null,
            IEnumerable<Grade>? grades = null)
        {
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            mockFpsYearContext.Setup(x => x.FpsYear).Returns(2025);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            if (divisionGrades != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(divisionGrades);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.DivisionGrades).Returns(mockSet.Object);
            }

            if (grades != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(grades);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.Grades).Returns(mockSet.Object);
            }

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new DivisionGradeMaintenanceRepository(mockContext.Object);
        }

        private static (
            DivisionGradeMaintenanceRepository Repo,
            Mock<DbSet<DivisionGradeMaintenance>> DbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<DivisionGradeMaintenance>? divisionGrades = null)
        {
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            mockFpsYearContext.Setup(x => x.FpsYear).Returns(2025);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var dbSet = RepositoryTestHelper.CreateMockDbSet(divisionGrades ?? []);
            RepositoryTestHelper.SetupDbSetOperations(dbSet);
            mockContext.Setup(x => x.DivisionGrades).Returns(dbSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new DivisionGradeMaintenanceRepository(mockContext.Object);
            return (repo, dbSet, mockContext);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DivisionGradeMaintenanceRepository(null!));
        }

        #endregion

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            var repo = CreateRepository(divisionGrades: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetAllPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsEmptyPagedData_WhenNoRecords()
        {
            var repo = CreateRepository(divisionGrades: []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllPagedAsync(query);
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsAllRecords()
        {
            var grades = new List<DivisionGradeMaintenance>
            {
                BuildDivisionGrade("A-VSD"),
                BuildDivisionGrade("B-VSD"),
                BuildDivisionGrade("C-VSD")
            };
            var repo = CreateRepository(divisionGrades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsCorrectPage()
        {
            var grades = new List<DivisionGradeMaintenance>
            {
                BuildDivisionGrade("A-VSD"), BuildDivisionGrade("B-VSD"), BuildDivisionGrade("C-VSD"),
                BuildDivisionGrade("D-VSD"), BuildDivisionGrade("E-VSD")
            };
            var repo = CreateRepository(divisionGrades: grades);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetAllPagedAsync_FiltersByDivisionGradeCode()
        {
            var grades = new List<DivisionGradeMaintenance>
            {
                BuildDivisionGrade("A-VSD"),
                BuildDivisionGrade("B-VSD"),
                BuildDivisionGrade("A-BSD")
            };
            var repo = CreateRepository(divisionGrades: grades);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "DivisionGradeCode", "A-" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_FiltersByGradeCode()
        {
            var grades = new List<DivisionGradeMaintenance>
            {
                BuildDivisionGrade("A-VSD", gradeCode: "A"),
                BuildDivisionGrade("B-VSD", gradeCode: "B"),
                BuildDivisionGrade("C-VSD", gradeCode: "A")
            };
            var repo = CreateRepository(divisionGrades: grades);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "GradeCode", "A" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_FiltersByDivision()
        {
            var grades = new List<DivisionGradeMaintenance>
            {
                BuildDivisionGrade("A-VSD", division: "VSD"),
                BuildDivisionGrade("A-BSD", division: "BSD"),
                BuildDivisionGrade("B-VSD", division: "VSD")
            };
            var repo = CreateRepository(divisionGrades: grades);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "Division", "VSD" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_OrdersByDivisionGradeCodeAscByDefault()
        {
            var grades = new List<DivisionGradeMaintenance>
            {
                BuildDivisionGrade("C-VSD"),
                BuildDivisionGrade("A-VSD"),
                BuildDivisionGrade("B-VSD")
            };
            var repo = CreateRepository(divisionGrades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllPagedAsync(query);
            var list = result.Data.ToList();
            Assert.Equal("A-VSD", list[0].DivisionGradeCode);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCodeIsNullOrWhiteSpace()
        {
            var repo = CreateRepository(divisionGrades: []);
            var result = await repo.GetByIdAsync("");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCodeIsWhiteSpace()
        {
            var repo = CreateRepository(divisionGrades: []);
            var result = await repo.GetByIdAsync("   ");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsRecord_WhenFound()
        {
            var grades = new List<DivisionGradeMaintenance>
            {
                BuildDivisionGrade("A-VSD", gradeCode: "A", division: "VSD")
            };
            var repo = CreateRepository(divisionGrades: grades);
            var result = await repo.GetByIdAsync("A-VSD");
            Assert.NotNull(result);
            Assert.Equal("A-VSD", result.DivisionGradeCode);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            var repo = CreateRepository(divisionGrades: []);
            var result = await repo.GetByIdAsync("NONEXISTENT");
            Assert.Null(result);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository(divisionGrades: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_ThrowsInvalidOperationException_WhenCodeAlreadyExists()
        {
            var existing = BuildDivisionGrade("A-VSD");
            var (repo, _, _) = CreateRepositoryWithMocks([existing]);
            var duplicate = BuildDivisionGrade("A-VSD");
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.CreateAsync(duplicate));
        }

        [Fact]
        public async Task CreateAsync_SetsYearFromContext()
        {
            var (repo, dbSet, context) = CreateRepositoryWithMocks([]);
            var entity = BuildDivisionGrade("NEW-VSD");
            entity.FpsYear = 0;

            dbSet.Setup(x => x.Add(It.IsAny<DivisionGradeMaintenance>()));

            var result = await repo.CreateAsync(entity);
            Assert.Equal(2025, result.FpsYear);
        }

        [Fact]
        public async Task CreateAsync_AddsEntityAndSavesChanges()
        {
            var (repo, dbSet, context) = CreateRepositoryWithMocks([]);
            dbSet.Setup(x => x.Add(It.IsAny<DivisionGradeMaintenance>()));

            var entity = BuildDivisionGrade("NEW-VSD");
            var result = await repo.CreateAsync(entity);

            Assert.NotNull(result);
            Assert.Equal("NEW-VSD", result.DivisionGradeCode);
            dbSet.Verify(x => x.Add(It.Is<DivisionGradeMaintenance>(d => d.DivisionGradeCode == "NEW-VSD")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository(divisionGrades: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync("A-VSD", null!));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentException_WhenOriginalCodeIsEmpty()
        {
            var repo = CreateRepository(divisionGrades: []);
            var entity = BuildDivisionGrade("A-VSD");
            await Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateAsync("", entity));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentException_WhenOriginalCodeIsWhiteSpace()
        {
            var repo = CreateRepository(divisionGrades: []);
            var entity = BuildDivisionGrade("A-VSD");
            await Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateAsync("   ", entity));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenCodeChangesButOriginalNotFound()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var entity = BuildDivisionGrade("B-VSD");
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAsync("A-VSD", entity));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenOriginalCodeNotFound()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var entity = BuildDivisionGrade("A-VSD");
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAsync("NOTEXIST", entity));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenSameCodeButRecordNotFound()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var entity = BuildDivisionGrade("A-VSD");
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAsync("A-VSD", entity));
        }

        [Fact]
        public async Task UpdateAsync_UpdatesFieldsInPlace_WhenCodeIsUnchanged()
        {
            var existing = BuildDivisionGrade("A-VSD", gradeCode: "A", division: "VSD",
                chargeRate: 100m, directRate: 90m, payRate: 80m, npr: 10m, ohr: 5m);
            var (repo, dbSet, context) = CreateRepositoryWithMocks([existing]);

            var updated = BuildDivisionGrade("A-VSD", gradeCode: "B", division: "BSD",
                chargeRate: 200m, directRate: 180m, payRate: 160m, npr: 20m, ohr: 10m);

            var result = await repo.UpdateAsync("A-VSD", updated);

            Assert.Equal("A-VSD", result.DivisionGradeCode);
            Assert.Equal("B", result.GradeCode);
            Assert.Equal("BSD", result.Division);
            Assert.Equal(200m, result.ChargeRate);
            Assert.Equal(180m, result.DirectRate);
            Assert.Equal(160m, result.PayRate);
            Assert.Equal(20m, result.Npr);
            Assert.Equal(10m, result.Ohr);
            Assert.Equal(2025, result.FpsYear);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        [Fact]
        public async Task UpdateAsync_DeletesAndReinserts_WhenCodeChanges()
        {
            var existing = BuildDivisionGrade("A-VSD");
            var (repo, dbSet, context) = CreateRepositoryWithMocks([existing]);
            dbSet.Setup(x => x.Remove(It.IsAny<DivisionGradeMaintenance>()));
            dbSet.Setup(x => x.Add(It.IsAny<DivisionGradeMaintenance>()));

            var updated = BuildDivisionGrade("B-VSD", gradeCode: "B", division: "VSD");

            var result = await repo.UpdateAsync("A-VSD", updated);

            Assert.Equal("B-VSD", result.DivisionGradeCode);
            Assert.Equal(2025, result.FpsYear);
            dbSet.Verify(x => x.Remove(It.Is<DivisionGradeMaintenance>(d => d.DivisionGradeCode == "A-VSD")), Times.Once);
            dbSet.Verify(x => x.Add(It.Is<DivisionGradeMaintenance>(d => d.DivisionGradeCode == "B-VSD")), Times.Once);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenCodeIsNullOrWhiteSpace()
        {
            var repo = CreateRepository(divisionGrades: []);
            var result = await repo.DeleteAsync("");
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenCodeIsWhiteSpace()
        {
            var repo = CreateRepository(divisionGrades: []);
            var result = await repo.DeleteAsync("   ");
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenRecordNotFound()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var result = await repo.DeleteAsync("NOTEXIST");
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
        {
            var existing = BuildDivisionGrade("A-VSD");
            var (repo, dbSet, context) = CreateRepositoryWithMocks([existing]);
            dbSet.Setup(x => x.Remove(It.IsAny<DivisionGradeMaintenance>()));
            var result = await repo.DeleteAsync("A-VSD");
            Assert.True(result);
        }

        #endregion

        #region GetAllGradeCodesAsync Tests

        [Fact]
        public async Task GetAllGradeCodesAsync_ReturnsDistinctGradeCodes()
        {
            var grades = new List<Grade>
            {
                new Grade { GradeCode = "A" },
                new Grade { GradeCode = "B" },
                new Grade { GradeCode = "A" }
            };
            var repo = CreateRepository(grades: grades);
            var result = await repo.GetAllGradeCodesAsync();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains("A", result);
            Assert.Contains("B", result);
        }

        [Fact]
        public async Task GetAllGradeCodesAsync_ReturnsEmpty_WhenNoGrades()
        {
            var repo = CreateRepository(grades: []);
            var result = await repo.GetAllGradeCodesAsync();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllGradeCodesAsync_ReturnsOrderedGradeCodes()
        {
            var grades = new List<Grade>
            {
                new Grade { GradeCode = "C" },
                new Grade { GradeCode = "A" },
                new Grade { GradeCode = "B" }
            };
            var repo = CreateRepository(grades: grades);
            var result = await repo.GetAllGradeCodesAsync();
            Assert.Equal("A", result[0]);
            Assert.Equal("B", result[1]);
            Assert.Equal("C", result[2]);
        }

        #endregion

        #region Sorting Tests

        [Theory]
        [InlineData("gradecode", false, "A", "B")]
        [InlineData("gradecode", true, "B", "A")]
        [InlineData("division", false, "BSD", "VSD")]
        [InlineData("division", true, "VSD", "BSD")]
        [InlineData("chargerate", false, "50", "100")]
        [InlineData("chargerate", true, "100", "50")]
        [InlineData("directrate", false, "40", "90")]
        [InlineData("directrate", true, "90", "40")]
        [InlineData("payrate", false, "30", "80")]
        [InlineData("payrate", true, "80", "30")]
        [InlineData("npr", false, "5", "10")]
        [InlineData("npr", true, "10", "5")]
        [InlineData("ohr", false, "2", "5")]
        [InlineData("ohr", true, "5", "2")]
        public async Task GetAllPagedAsync_AppliesSorting_Correctly(string sortBy, bool descending, string expectedFirst, string expectedSecond)
        {
            var grades = new List<DivisionGradeMaintenance>
            {
                BuildDivisionGrade("B-VSD", gradeCode: "B", division: "VSD", chargeRate: 100m, directRate: 90m, payRate: 80m, npr: 10m, ohr: 5m),
                BuildDivisionGrade("A-BSD", gradeCode: "A", division: "BSD", chargeRate: 50m, directRate: 40m, payRate: 30m, npr: 5m, ohr: 2m)
            };
            var repo = CreateRepository(divisionGrades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };
            var result = await repo.GetAllPagedAsync(query);
            var list = result.Data.ToList();
            var actualFirst = sortBy switch
            {
                "gradecode" => list[0].GradeCode,
                "division" => list[0].Division,
                "chargerate" => list[0].ChargeRate?.ToString(),
                "directrate" => list[0].DirectRate?.ToString(),
                "payrate" => list[0].PayRate?.ToString(),
                "npr" => list[0].Npr?.ToString(),
                "ohr" => list[0].Ohr?.ToString(),
                _ => list[0].DivisionGradeCode
            };
            Assert.Equal(expectedFirst, actualFirst);
        }

        [Fact]
        public async Task GetAllPagedAsync_SortsByDivisionGradeCodeAsc_WhenSortByIsUnknown()
        {
            var grades = new List<DivisionGradeMaintenance>
            {
                BuildDivisionGrade("C-VSD"),
                BuildDivisionGrade("A-VSD"),
                BuildDivisionGrade("B-VSD")
            };
            var repo = CreateRepository(divisionGrades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unknown" };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal("A-VSD", result.Data.First().DivisionGradeCode);
        }

        [Fact]
        public async Task GetAllPagedAsync_SortsByDivisionGradeCodeAsc_WhenSortByIsNull()
        {
            var grades = new List<DivisionGradeMaintenance>
            {
                BuildDivisionGrade("C-VSD"),
                BuildDivisionGrade("A-VSD")
            };
            var repo = CreateRepository(divisionGrades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = null };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal("A-VSD", result.Data.First().DivisionGradeCode);
        }

        [Fact]
        public async Task GetAllPagedAsync_SortsByDivisionGradeCodeAsc_WhenSortByIsDivisionGradeCode()
        {
            var grades = new List<DivisionGradeMaintenance>
            {
                BuildDivisionGrade("A-VSD"),
                BuildDivisionGrade("C-VSD"),
                BuildDivisionGrade("B-VSD")
            };
            var repo = CreateRepository(divisionGrades: grades);
            // "divisiongradeCode" toLowerInvariant = "divisiongradecode" which hits the default _ branch (ascending)
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "divisiongradeCode", Descending = true };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal("A-VSD", result.Data.First().DivisionGradeCode);
        }

        #endregion
    }
}
