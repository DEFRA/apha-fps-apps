using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.YearMasterRepositoryTest
{
    public class YearMasterRepositoryTests
    {
        /// <summary>
        /// Creates a YearMasterRepository with mocked dependencies.
        /// </summary>
        private static YearMasterRepository CreateRepository(IEnumerable<YearMaster> yearMasters)
        {
            // Create a mock IFpsRequestContext (needed for DbContext mock)
            var mockFpsRequestContext = new Mock<IFpsRequestContext>();
            mockFpsRequestContext.Setup(x => x.FpsYear).Returns(2024);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsRequestContext.Object);

            // Setup YearMasters DbSet
            var yearMastersMockSet = RepositoryTestHelper.CreateMockDbSet(yearMasters);
            mockContext.Setup(x => x.YearMasters).Returns(yearMastersMockSet.Object);

            return new YearMasterRepository(mockContext.Object);
        }

        #region GetAllYearMastersAsync Tests

        [Fact]
        public async Task GetAllYearMastersAsync_ReturnsActiveYears_OrderedByFpsYearDescending()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2022, FpsYearCode = "2022", YearStatus = "Closed", Active = false } // Inactive
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetAllFpsYearsAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count); // Only active years
            Assert.Equal(2025, resultList[0].FpsYear); // Descending order
            Assert.Equal(2024, resultList[1].FpsYear);
            Assert.Equal(2023, resultList[2].FpsYear);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_ExcludesInactiveYears()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = false },
                new() { FpsYear = 2022, FpsYearCode = "2022", YearStatus = "Closed", Active = false }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetAllFpsYearsAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal(2024, resultList[0].FpsYear);
            Assert.All(resultList, y => Assert.True(y.Active));
        }

        [Fact]
        public async Task GetAllYearMastersAsync_ReturnsEmptyList_WhenNoActiveYears()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = false },
                new() { FpsYear = 2022, FpsYearCode = "2022", YearStatus = "Closed", Active = false }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetAllFpsYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_ReturnsEmptyList_WhenNoYears()
        {
            // Arrange
            var repo = CreateRepository(new List<YearMaster>());

            // Act
            var result = await repo.GetAllFpsYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_HandlesMultipleYears_WithDifferentStatuses()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true },
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetAllFpsYearsAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.Contains(resultList, y => y.YearStatus == "Open");
            Assert.Contains(resultList, y => y.YearStatus == "Planned");
            Assert.Contains(resultList, y => y.YearStatus == "Closed");
        }

        #endregion

        #region GetAllYearMastersAsync with Pagination Tests

        [Fact]
        public async Task GetAllYearMastersAsync_WithPagination_ReturnsPagedData()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 2,
                SortBy = null,
                Descending = false
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
            Assert.Equal(1, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithPagination_SecondPage_ReturnsRemainingRecords()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2021, FpsYearCode = "2021", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2022, FpsYearCode = "2022", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 2,
                PageSize = 2
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.PaginationData.TotalPages);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithFilter_FiltersByFpsYear()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 2024
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(2024, result.Data.First().FpsYear);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithZeroFilter_ReturnsAllActiveYears()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 0 // Zero means no filter
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithNullFilter_ReturnsAllActiveYears()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10
                // Filter not set - uses default value
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithInvalidFilter_ReturnsEmpty()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 9999 // Non-existent year
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Theory]
        [InlineData("FpsYear", false, 2023)]
        [InlineData("FpsYear", true, 2025)]
        [InlineData("YearStatus", false, "Closed")]
        [InlineData("YearStatus", true, "Planned")]
        [InlineData("FpsYearCode", false, "2023")]
        [InlineData("FpsYearCode", true, "2025")]
        public async Task GetAllYearMastersAsync_WithSorting_SortsCorrectly(
            string sortBy,
            bool descending,
            object expectedFirstValue)
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count());
            var firstYear = result.Data.First();
            var actualValue = sortBy switch
            {
                "FpsYear" => (object)firstYear.FpsYear,
                "YearStatus" => firstYear.YearStatus,
                "FpsYearCode" => firstYear.FpsYearCode,
                _ => firstYear.FpsYear
            };
            Assert.Equal(expectedFirstValue, actualValue);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithNullSortBy_DoesNotSort()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10,
                SortBy = null
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithEmptySortBy_DoesNotSort()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10,
                SortBy = ""
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithWhitespaceSortBy_DoesNotSort()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "   "
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithFilterAndSorting_AppliesBoth()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true },
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2022, FpsYearCode = "2022", YearStatus = "Closed", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 2024,
                SortBy = "FpsYear",
                Descending = true
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(2024, result.Data.First().FpsYear);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithPaginationAndSorting_WorksCorrectly()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2021, FpsYearCode = "2021", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2022, FpsYearCode = "2022", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 2,
                SortBy = "FpsYear",
                Descending = true
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(2025, result.Data.First().FpsYear);
            Assert.Equal(2024, result.Data.Last().FpsYear);
        }

        #endregion

        #region GetYearMasterByIdAsync Tests

        [Fact]
        public async Task GetYearMasterByIdAsync_ReturnsYear_WhenFound()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetFpsYearByIdAsync(2024);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2024, result.FpsYear);
            Assert.Equal("2024", result.FpsYearCode);
            Assert.Equal("Open", result.YearStatus);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetFpsYearByIdAsync(2025);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_ReturnsInactiveYear_WhenExists()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = false },
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetFpsYearByIdAsync(2023);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2023, result.FpsYear);
            Assert.False(result.Active); // Can retrieve inactive years by ID
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_ReturnsNull_WhenYearsEmpty()
        {
            // Arrange
            var repo = CreateRepository(new List<YearMaster>());

            // Act
            var result = await repo.GetFpsYearByIdAsync(2024);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_ReturnsCorrectYear_WithMultipleYears()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2021, FpsYearCode = "2021", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2022, FpsYearCode = "2022", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetFpsYearByIdAsync(2023);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2023, result.FpsYear);
            Assert.Equal("2023", result.FpsYearCode);
            Assert.Equal("Closed", result.YearStatus);
        }

        [Theory]
        [InlineData(2020)]
        [InlineData(2030)]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetYearMasterByIdAsync_ReturnsNull_ForInvalidYears(int invalidYear)
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetFpsYearByIdAsync(invalidYear);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region Edge Cases and Special Scenarios

        [Fact]
        public async Task GetAllYearMastersAsync_WithPagination_HandlesEmptyResult()
        {
            // Arrange
            var repo = CreateRepository(new List<YearMaster>());
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
            Assert.Equal(0, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithPagination_ExcludesInactiveYears()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = false },
                new() { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, y => Assert.True(y.Active));
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithLargePageSize_ReturnsAllRecords()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 1000
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithPageBeyondRange_ReturnsEmpty()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };
            var repo = CreateRepository(yearMasters);
            var query = new PaginationParameters<int>
            {
                Page = 10,
                PageSize = 10
            };

            // Act
            var result = await repo.GetAllFpsYearsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion
    }
}
