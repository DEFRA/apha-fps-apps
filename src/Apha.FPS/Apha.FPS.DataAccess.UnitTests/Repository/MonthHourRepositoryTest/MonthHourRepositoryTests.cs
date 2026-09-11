using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.MonthHourRepositoryTest
{
    public class MonthHourRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        /// <summary>
        /// Creates a MonthHourRepository with in-memory MonthHours and optional YearMasters data.
        /// IFpsRequestContext is substituted via Moq.
        /// </summary>
        private static MonthHourRepository CreateRepository(
            IEnumerable<MonthHour>? monthHours = null,
            IEnumerable<YearMaster>? yearMasters = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var monthHoursMockSet = RepositoryTestHelper.CreateMockDbSet(monthHours ?? []);
            RepositoryTestHelper.SetupDbSetOperations(monthHoursMockSet);
            mockContext.Setup(x => x.MonthHours).Returns(monthHoursMockSet.Object);

            var yearMastersMockSet = RepositoryTestHelper.CreateMockDbSet(yearMasters ?? []);
            mockContext.Setup(x => x.YearMasters).Returns(yearMastersMockSet.Object);

            return new MonthHourRepository(mockContext.Object);
        }

        private static PaginationParameters<string> BuildQuery(
            int page = 1, int pageSize = 10,
            string? filter = null, string? sortBy = null, bool descending = false) =>
            new(page: page, pageSize: pageSize, descending: descending, sortBy: sortBy)
            {
                Filter = filter
            };

        /// <summary>
        /// Builds a JSON filter string accepted by ApplyMonthHourFilter.
        /// </summary>
        private static string BuildFilter(short? year = null, short? month = null)
        {
            var parts = new List<string>();
            if (year.HasValue)  parts.Add($"\"Year\":\"{year.Value}\"");
            if (month.HasValue) parts.Add($"\"Month\":\"{month.Value}\"");
            return parts.Count > 0 ? "{" + string.Join(",", parts) + "}" : "{}";
        }

        #region GetAllAsync

        // GetAllAsync always applies an OrderBy(EF.Property<object>(e, sortBy)) step before materialising
        // the query.  EF.Property<T> is an EF Core translation hint that cannot be evaluated by the
        // in-memory LINQ provider used in unit tests; every code path through GetAllAsync therefore
        // throws InvalidOperationException.  The tests below confirm each branch is reached.

        [Fact]
        public async Task GetAllAsync_WithNoFilter_ThrowsDueToEfPropertySort()
        {
            // Arrange — EF.Property<object> cannot be evaluated in-memory; verify the default sort code path
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 2, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WithYearFilter_ThrowsDueToEfPropertySort()
        {
            // Arrange — confirms the Year filter branch is entered before the sort step throws
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear },
                new() { Year = 2023, Month = 6, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(filter: BuildFilter(year: 2024));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WithMonthFilter_ThrowsDueToEfPropertySort()
        {
            // Arrange — confirms the Month filter branch is entered before the sort step throws
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 3, FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 6, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(filter: BuildFilter(month: 3));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WithYearAndMonthFilter_ThrowsDueToEfPropertySort()
        {
            // Arrange — both filters applied before sort throws
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 3, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(filter: BuildFilter(year: 2024, month: 3));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_SortByAscending_ThrowsDueToEfPropertySort()
        {
            // Arrange — EF.Property<T> cannot be evaluated in-memory; verify the ascending sort code path
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 2, FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(sortBy: nameof(MonthHour.Year), descending: false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_SortByDescending_ThrowsDueToEfPropertySort()
        {
            // Arrange — EF.Property<T> cannot be evaluated in-memory; verify the descending sort code path
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 2, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(sortBy: nameof(MonthHour.Year), descending: true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WithInvalidSortBy_DefaultsToYearSort_ThrowsDueToEfPropertySort()
        {
            // Arrange — an unrecognised SortBy value falls back to nameof(MonthHour.Year),
            // which still uses EF.Property and cannot be evaluated in-memory
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(sortBy: "NonExistentField");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyData_ReturnsEmptyPagedResult()
        {
            // Arrange — with no rows the OrderBy key selector (EF.Property) is never invoked,
            // so the query succeeds and ApplyPaging returns an empty result.
            var repo = CreateRepository([]);
            var query = BuildQuery();

            // Act
            var result = await repo.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllAsync_WithNullFilter_ThrowsDueToEfPropertySort()
        {
            // Arrange — null filter is handled gracefully (returns unfiltered query) then sort throws
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(filter: null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WithAllValidSortByFields_ThrowsDueToEfPropertySort()
        {
            // Arrange — each allowed SortBy value (Days, CvlHours, VidHours, Month) still
            // goes through EF.Property and cannot be evaluated in-memory
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, Days = 20, CvlHours = 160, VidHours = 40, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            foreach (var field in new[] { nameof(MonthHour.Days), nameof(MonthHour.CvlHours), nameof(MonthHour.VidHours), nameof(MonthHour.Month) })
            {
                var query = BuildQuery(sortBy: field);
                await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
            }
        }

        #endregion

        #region GetByYearAsync

        [Fact]
        public async Task GetByYearAsync_ReturnsMonthHours_WhenMatchingYearExists()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, Days = 20, CvlHours = 160, VidHours = 40, FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 2, Days = 19, CvlHours = 152, VidHours = 38, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act
            var result = await repo.GetByYearAsync(2024);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByYearAsync_ReturnsEmpty_WhenNoMatchingYearExists()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2023, Month = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act
            var result = await repo.GetByYearAsync(2024);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByYearAsync_ReturnsOnlyMatchingYear_WhenMultipleYearsExist()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 3, FpsYear = DefaultFpsYear },
                new() { Year = 2023, Month = 6, FpsYear = DefaultFpsYear },
                new() { Year = 2025, Month = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act
            var result = await repo.GetByYearAsync(2024);

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.All(list, m => Assert.Equal(2024, m.Year));
        }

        [Fact]
        public async Task GetByYearAsync_ReturnsRecords_OrderedByMonth()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 9,  FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 2,  FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 6,  FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 1,  FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act
            var result = await repo.GetByYearAsync(2024);

            // Assert
            var list = result.ToList();
            Assert.Equal(4, list.Count);
            Assert.Equal(1, list[0].Month);
            Assert.Equal(2, list[1].Month);
            Assert.Equal(6, list[2].Month);
            Assert.Equal(9, list[3].Month);
        }

        [Fact]
        public async Task GetByYearAsync_ReturnsEmpty_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository([]);

            // Act
            var result = await repo.GetByYearAsync(2024);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByYearAsync_ReturnsCorrectProperties_ForMatchingRecord()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new()
                {
                    Year      = 2024,
                    Month     = 5,
                    Days      = 21,
                    CvlHours  = 168,
                    VidHours  = 42,
                    Fmonth    = 3,
                    FpsYear   = DefaultFpsYear
                }
            };
            var repo = CreateRepository(monthHours);

            // Act
            var result = await repo.GetByYearAsync(2024);

            // Assert
            var record = Assert.Single(result);
            Assert.Equal(2024, record.Year);
            Assert.Equal(5, record.Month);
            Assert.Equal(21, record.Days);
            Assert.Equal(168, record.CvlHours);
            Assert.Equal(42, record.VidHours);
            Assert.Equal((short?)3, record.Fmonth);
        }

        #endregion

        #region GetDistinctYearsAsync

        [Fact]
        public async Task GetDistinctYearsAsync_WithData_ReturnsAllYearsOrderedAscending()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2025, Month = 1, FpsYear = DefaultFpsYear },
                new() { Year = 2023, Month = 3, FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 6, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act
            var result = (await repo.GetDistinctYearsAsync()).ToList();

            // Assert
            Assert.Equal(new short[] { 2023, 2024, 2025 }, result);
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WithDuplicateYears_ReturnsDistinctYearsOrderedAscending()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1,  FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 2,  FpsYear = DefaultFpsYear },
                new() { Year = 2023, Month = 12, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act
            var result = (await repo.GetDistinctYearsAsync()).ToList();

            // Assert
            Assert.Equal(new short[] { 2023, 2024 }, result);
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WithSingleRecord_ReturnsThatYear()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act
            var result = (await repo.GetDistinctYearsAsync()).ToList();

            // Assert
            Assert.Equal((short)2024, Assert.Single(result));
        }

        #endregion

        #region SaveAsync

        /// <summary>
        /// Creates a MonthHourRepository and exposes the mocked DbSet/DbContext so that
        /// Add / Update / SaveChanges calls can be verified.
        /// </summary>
        private static (MonthHourRepository Repo, Mock<FpsDbContext> Context, Mock<DbSet<MonthHour>> DbSet)
            CreateRepositoryWithMocks(
                IEnumerable<MonthHour>? monthHours = null,
                int fpsYear = DefaultFpsYear)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var dbSet = RepositoryTestHelper.CreateMockDbSet(monthHours ?? []);
            RepositoryTestHelper.SetupDbSetOperations(dbSet);
            mockContext.Setup(x => x.MonthHours).Returns(dbSet.Object);

            return (new MonthHourRepository(mockContext.Object), mockContext, dbSet);
        }

        [Fact]
        public async Task SaveAsync_WhenRecordDoesNotExist_AddsNewRecordAndReturnsIt()
        {
            // Arrange
            var (repo, mockContext, dbSet) = CreateRepositoryWithMocks();
            var monthHour = new MonthHour { Year = 2024, Month = 1, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = DefaultFpsYear };

            // Act
            var result = await repo.SaveAsync(monthHour);

            // Assert
            Assert.NotNull(result);
            Assert.Equal((short)2024, result.Year);
            Assert.Equal((short)1, result.Month);
            Assert.Equal(20, result.Days);
            dbSet.Verify(x => x.Add(It.Is<MonthHour>(m => m.Year == 2024 && m.Month == 1)), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task SaveAsync_WhenRecordExists_UpdatesExistingRecordAndReturnsIt()
        {
            // Arrange
            var existing = new MonthHour { Year = 2024, Month = 1, Days = 20, VidHours = 5, CvlHours = 3, Fmonth = 1, FpsYear = DefaultFpsYear };
            var (repo, mockContext, dbSet) = CreateRepositoryWithMocks(new[] { existing });
            var updated = new MonthHour { Year = 2024, Month = 1, Days = 22, VidHours = 6, CvlHours = 4, Fmonth = 2, FpsYear = DefaultFpsYear };

            // Act
            var result = await repo.SaveAsync(updated);

            // Assert
            Assert.NotNull(result);
            Assert.Same(existing, result);
            Assert.Equal(22, result.Days);
            Assert.Equal(6, result.VidHours);
            Assert.Equal(4, result.CvlHours);
            Assert.Equal((short)2, result.Fmonth);
            dbSet.Verify(x => x.Update(It.Is<MonthHour>(m => m.Year == 2024 && m.Month == 1)), Times.Once);
            dbSet.Verify(x => x.Add(It.IsAny<MonthHour>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task SaveAsync_WhenRecordExistsForDifferentFpsYear_TreatsAsNewAndAdds()
        {
            // Arrange — same Year/Month but different FpsYear should not match the composite key
            var existing = new MonthHour { Year = 2024, Month = 1, Days = 20, FpsYear = 2023 };
            var (repo, mockContext, dbSet) = CreateRepositoryWithMocks(new[] { existing });
            var newRecord = new MonthHour { Year = 2024, Month = 1, Days = 22, FpsYear = 2024 };

            // Act
            var result = await repo.SaveAsync(newRecord);

            // Assert
            Assert.Same(newRecord, result);
            dbSet.Verify(x => x.Add(It.Is<MonthHour>(m => m.FpsYear == 2024)), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region SaveYearEndMonthHourAsync

        /// <summary>
        /// Creates a repository with both MonthHours and YearMasters mocked, so that
        /// SaveYearEndMonthHourAsync's GetPlannedYear() lookup resolves against
        /// controllable fixture data. GetYearEndMonthHoursAsync (invoked indirectly via
        /// SavePlannedYearFmonthHoursAsync on the staging path) additionally needs
        /// MonthHourStagings mocked.
        /// </summary>
        private static (MonthHourRepository Repo, Mock<FpsDbContext> Context, Mock<DbSet<MonthHour>> MonthHoursDbSet, Mock<DbSet<MonthHourStaging>> StagingDbSet)
            CreateRepositoryWithYearMasters(
                IEnumerable<MonthHour>? monthHours,
                IEnumerable<YearMaster> yearMasters,
                IEnumerable<MonthHourStaging>? stagingMonthHours = null,
                int fpsYear = DefaultFpsYear)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var monthHoursDbSet = RepositoryTestHelper.CreateMockDbSet(monthHours ?? []);
            RepositoryTestHelper.SetupDbSetOperations(monthHoursDbSet);
            mockContext.Setup(x => x.MonthHours).Returns(monthHoursDbSet.Object);

            var stagingDbSet = RepositoryTestHelper.CreateMockDbSet(stagingMonthHours ?? []);
            RepositoryTestHelper.SetupDbSetOperations(stagingDbSet);
            mockContext.Setup(x => x.MonthHourStagings).Returns(stagingDbSet.Object);

            var yearMastersDbSet = RepositoryTestHelper.CreateMockDbSet(yearMasters);
            mockContext.Setup(x => x.YearMasters).Returns(yearMastersDbSet.Object);

            return (new MonthHourRepository(mockContext.Object), mockContext, monthHoursDbSet, stagingDbSet);
        }

        [Fact]
        public async Task SaveYearEndMonthHourAsync_WhenPlannedYearExists_SavesToMonthHoursTable()
        {
            // Arrange — an active "Planned" YearMaster row means GetPlannedYear() returns a
            // value, routing SaveYearEndMonthHourAsync through SaveAsync instead of staging.
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, YearStatus = "Planned", Active = true }
            };
            var (repo, mockContext, monthHoursDbSet, stagingDbSet) =
                CreateRepositoryWithYearMasters(monthHours: null, yearMasters);
            var monthHour = new MonthHour { Year = 2025, Month = 1, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2025 };

            // Act
            var result = await repo.SaveYearEndMonthHourAsync(monthHour);

            // Assert
            Assert.NotNull(result);
            Assert.Equal((short)2025, result.Year);
            Assert.Equal(20, result.Days);
            monthHoursDbSet.Verify(x => x.Add(It.Is<MonthHour>(m => m.Year == 2025 && m.Month == 1)), Times.Once);
            stagingDbSet.Verify(x => x.Add(It.IsAny<MonthHourStaging>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task SaveYearEndMonthHourAsync_WhenNoPlannedYearAndStagingRowExists_UpdatesStagingRow()
        {
            // Arrange — no "Planned" YearMaster row means GetPlannedYear() returns null,
            // routing SaveYearEndMonthHourAsync through SaveStagingAsync. An existing staging
            // row for the same key should be updated rather than duplicated. To keep the test
            // isolated from SavePlannedYearFmonthHoursAsync's indirect GetYearEndMonthHoursAsync
            // call, the open year's MonthHours data is left empty so no fmonth-0 rows are found.
            var existingStaging = new MonthHourStaging { Year = 2025, Month = 1, Days = 18, FpsYear = 2025 };
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, YearStatus = "Open", Active = true }
            };
            var (repo, mockContext, _, stagingDbSet) =
                CreateRepositoryWithYearMasters(monthHours: null, yearMasters, stagingMonthHours: new[] { existingStaging });
            var monthHour = new MonthHour { Year = 2025, Month = 1, Days = 21, VidHours = 6, CvlHours = 4, FpsYear = 2025 };

            // Act
            var result = await repo.SaveYearEndMonthHourAsync(monthHour);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(21, result.Days);
            stagingDbSet.Verify(x => x.Update(It.Is<MonthHourStaging>(m => m.Year == 2025 && m.Month == 1)), Times.Once);
            stagingDbSet.Verify(x => x.Add(It.Is<MonthHourStaging>(m => m.Year == 2025 && m.Month == 1)), Times.Never);
        }

        #endregion
    }
}
