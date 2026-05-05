using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.WorkgroupGradeRepositoryTest
{
    public class WorkgroupGradeRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        private static WorkgroupGradeRepository CreateRepository(
            IEnumerable<WorkgroupGrade> workgroupGrades,
            IEnumerable<WgEmployee>? wgEmployees = null,
            IEnumerable<ProfitCentreGrade>? pcGrades = null,
            IEnumerable<Grade>? grades = null,
            IEnumerable<Workgroup>? workgroups = null,
            int fpsYear = DefaultTestFpsYear)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(fpsYear);
            requestContext.UserEmailId.Returns("test@example.com");

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var wgGradesMockSet = RepositoryTestHelper.CreateMockDbSet(workgroupGrades);
            var wgEmployeesMockSet = RepositoryTestHelper.CreateMockDbSet(wgEmployees ?? []);
            var pcGradesMockSet = RepositoryTestHelper.CreateMockDbSet(pcGrades ?? []);
            var gradesMockSet = RepositoryTestHelper.CreateMockDbSet(grades ?? []);
            var workgroupsMockSet = RepositoryTestHelper.CreateMockDbSet(workgroups ?? []);

            RepositoryTestHelper.SetupDbSetOperations(wgGradesMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.WorkgroupGrades).Returns(wgGradesMockSet.Object);
            mockContext.Setup(x => x.WgEmployees).Returns(wgEmployeesMockSet.Object);
            mockContext.Setup(x => x.ProfitcentreGrades).Returns(pcGradesMockSet.Object);
            mockContext.Setup(x => x.Grades).Returns(gradesMockSet.Object);
            mockContext.Setup(x => x.Workgroups).Returns(workgroupsMockSet.Object);

            return new WorkgroupGradeRepository(mockContext.Object, requestContext);
        }

        private static (
            WorkgroupGradeRepository Repo,
            Mock<DbSet<WorkgroupGrade>> WgGradesDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<WorkgroupGrade> workgroupGrades,
                IEnumerable<WgEmployee>? wgEmployees = null,
                int fpsYear = DefaultTestFpsYear)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(fpsYear);
            requestContext.UserEmailId.Returns("test@example.com");

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var wgGradesMockSet = RepositoryTestHelper.CreateMockDbSet(workgroupGrades);
            var wgEmployeesMockSet = RepositoryTestHelper.CreateMockDbSet(wgEmployees ?? []);

            RepositoryTestHelper.SetupDbSetOperations(wgGradesMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.WorkgroupGrades).Returns(wgGradesMockSet.Object);
            mockContext.Setup(x => x.WgEmployees).Returns(wgEmployeesMockSet.Object);

            var repo = new WorkgroupGradeRepository(mockContext.Object, requestContext);
            return (repo, wgGradesMockSet, mockContext);
        }

        #region GetAllWorkgroupGradesPagedAsync

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_ReturnsAllRecords()
        {
            var data = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" },
                new() { WgGrade = "WG02", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "HR" }
            };
            var repo = CreateRepository(data);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_AppliesPaging()
        {
            var data = Enumerable.Range(1, 25).Select(i => new WorkgroupGrade
            {
                WgGrade = $"WG{i:D2}", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT"
            }).ToList();
            var repo = CreateRepository(data);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal(10, result.Data.Count());
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_ThrowsOnNullQuery()
        {
            var repo = CreateRepository([]);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => repo.GetAllWorkgroupGradesPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_AppliesFilter()
        {
            var data = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" },
                new() { WgGrade = "WG02", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "HR" }
            };
            var repo = CreateRepository(data);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"WgGrade\":\"WG01\"}"
            };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("WG01", result.Data.First().WgGrade);
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_EmptyFilter_ReturnsAll()
        {
            var data = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" },
                new() { WgGrade = "WG02", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "HR" }
            };
            var repo = CreateRepository(data);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_AppliesSortingAscending()
        {
            var data = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG02", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "HR" },
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var repo = CreateRepository(data);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "WgGrade", Descending = false };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal("WG01", result.Data.First().WgGrade);
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_AppliesSortingDescending()
        {
            var data = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" },
                new() { WgGrade = "WG02", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "HR" }
            };
            var repo = CreateRepository(data);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "WgGrade", Descending = true };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal("WG02", result.Data.First().WgGrade);
        }

        [Theory]
        [InlineData("profitcentregrade")]
        [InlineData("gradecode")]
        [InlineData("workgroup")]
        public async Task GetAllWorkgroupGradesPagedAsync_SortsByDifferentColumns(string sortBy)
        {
            var data = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "IT" },
                new() { WgGrade = "WG02", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "HR" }
            };
            var repo = CreateRepository(data);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = false };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_UnknownSortColumn_ReturnsUnsorted()
        {
            var data = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var repo = CreateRepository(data);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unknown" };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Single(result.Data);
        }

        [Theory]
        [InlineData("{\"ProfitCentreGrade\":\"PC01\"}")]
        [InlineData("{\"GradeCode\":\"G01\"}")]
        [InlineData("{\"Workgroup\":\"IT\"}")]
        public async Task GetAllWorkgroupGradesPagedAsync_FiltersByDifferentColumns(string filter)
        {
            var data = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" },
                new() { WgGrade = "WG02", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "HR" }
            };
            var repo = CreateRepository(data);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Single(result.Data);
        }

        #endregion

        #region GetByWgGradeAsync

        [Fact]
        public async Task GetByWgGradeAsync_ReturnsEntity_WhenFound()
        {
            var data = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetByWgGradeAsync("WG01");

            Assert.NotNull(result);
            Assert.Equal("WG01", result.WgGrade);
        }

        [Fact]
        public async Task GetByWgGradeAsync_ReturnsNull_WhenNotFound()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetByWgGradeAsync("INVALID");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetByWgGradeAsync_ReturnsNull_WhenCodeIsNullOrWhitespace(string? wgGrade)
        {
            var repo = CreateRepository([]);

            var result = await repo.GetByWgGradeAsync(wgGrade!);

            Assert.Null(result);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_AddsEntity_AndSetsFpsYear()
        {
            const int customYear = 2025;
            var (repo, wgGradesDbSet, mockContext) = CreateRepositoryWithMocks([], fpsYear: customYear);

            var entity = new WorkgroupGrade { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };

            var result = await repo.CreateAsync(entity);

            Assert.NotNull(result);
            Assert.Equal("WG01", result.WgGrade);
            Assert.Equal(customYear, result.FpsYear);
            wgGradesDbSet.Verify(x => x.Add(It.IsAny<WorkgroupGrade>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository([]);

            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateAsync(null!));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_UpdatesExistingEntity()
        {
            var existing = new WorkgroupGrade { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var (repo, _, mockContext) = CreateRepositoryWithMocks([existing]);

            var updated = new WorkgroupGrade { WgGrade = "WG01", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "HR" };

            var result = await repo.UpdateAsync(updated);

            Assert.NotNull(result);
            Assert.Equal("PC02", result.ProfitCentreGrade);
            Assert.Equal("G02", result.GradeCode);
            Assert.Equal("HR", result.Workgroup);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenNotFound()
        {
            var repo = CreateRepository([]);

            var entity = new WorkgroupGrade { WgGrade = "INVALID", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.UpdateAsync(entity));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository([]);

            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_SetsFpsYear_FromContext()
        {
            const int customYear = 2025;
            var existing = new WorkgroupGrade { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var (repo, _, _) = CreateRepositoryWithMocks([existing], fpsYear: customYear);

            var updated = new WorkgroupGrade { WgGrade = "WG01", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "HR" };

            var result = await repo.UpdateAsync(updated);

            Assert.Equal(customYear, result.FpsYear);
        }

        #endregion

        #region HasAssociatedStaffAsync

        [Fact]
        public async Task HasAssociatedStaffAsync_ReturnsTrue_WhenAssociationsExist()
        {
            var employees = new List<WgEmployee>
            {
                new() { PactId = "P1", SpNumber = "SP1", WorkGroupGrade = "WG01", PersonStatus = "Active", HrsPaid = 0, Leave = 0, SickSpecial = 0, HrsAvail = 0, MakeAvailable = 0, TimeRecorder = 0 }
            };
            var repo = CreateRepository([], wgEmployees: employees);

            var result = await repo.HasAssociatedStaffAsync("WG01");

            Assert.True(result);
        }

        [Fact]
        public async Task HasAssociatedStaffAsync_ReturnsFalse_WhenNoAssociations()
        {
            var repo = CreateRepository([], wgEmployees: []);

            var result = await repo.HasAssociatedStaffAsync("WG01");

            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task HasAssociatedStaffAsync_ReturnsFalse_WhenCodeIsNullOrWhitespace(string? wgGrade)
        {
            var repo = CreateRepository([]);

            var result = await repo.HasAssociatedStaffAsync(wgGrade!);

            Assert.False(result);
        }

        #endregion

        #region GetAllPcGradesAsync

        [Fact]
        public async Task GetAllPcGradesAsync_ReturnsDistinctOrderedGrades()
        {
            var pcGrades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "PC02", DivisionGrade = "D1", GradeCode = "G1", ProfitCentre = "P1" },
                new() { PcGrade = "PC01", DivisionGrade = "D1", GradeCode = "G1", ProfitCentre = "P1" },
                new() { PcGrade = "PC02", DivisionGrade = "D2", GradeCode = "G2", ProfitCentre = "P2" }
            };
            var repo = CreateRepository([], pcGrades: pcGrades);

            var result = await repo.GetAllPcGradesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("PC01", result[0]);
            Assert.Equal("PC02", result[1]);
        }

        #endregion

        #region GetAllGradeCodesAsync

        [Fact]
        public async Task GetAllGradeCodesAsync_ReturnsOrderedCodes()
        {
            var grades = new List<Grade>
            {
                new() { GradeCode = "G02" },
                new() { GradeCode = "G01" }
            };
            var repo = CreateRepository([], grades: grades);

            var result = await repo.GetAllGradeCodesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("G01", result[0]);
            Assert.Equal("G02", result[1]);
        }

        #endregion

        #region GetAllWorkgroupNamesAsync

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_ReturnsOrderedNames()
        {
            var workgroups = new List<Workgroup>
            {
                new() { WorkgroupName = "IT", ProfitCentre = "P1" },
                new() { WorkgroupName = "HR", ProfitCentre = "P1" }
            };
            var repo = CreateRepository([], workgroups: workgroups);

            var result = await repo.GetAllWorkgroupNamesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("HR", result[0]);
            Assert.Equal("IT", result[1]);
        }

        #endregion

        #region DeleteAsync

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteAsync_ReturnsFalse_WhenCodeIsNullOrWhitespace(string? wgGrade)
        {
            // ExecuteDeleteAsync() is not mockable with Moq; full delete logic is covered by integration tests.
            var repo = CreateRepository([]);

            var result = await repo.DeleteAsync(wgGrade!);

            Assert.False(result);
        }

        #endregion
    }
}
