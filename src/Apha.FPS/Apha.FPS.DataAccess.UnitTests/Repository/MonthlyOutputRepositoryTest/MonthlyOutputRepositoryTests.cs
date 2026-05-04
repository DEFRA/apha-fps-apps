using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.MonthlyOutputRepositoryTest
{
    public class MonthlyOutputRepositoryTests
    {
        private static MonthlyOutputRepository CreateRepository(
            IEnumerable<MonthlyOutput>? monthlyOutputs = null)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(2024);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            if (monthlyOutputs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(monthlyOutputs);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.MonthlyOutputs).Returns(mockSet.Object);
                RepositoryTestHelper.SetupSaveChanges(mockContext);
            }

            return new MonthlyOutputRepository(mockContext.Object);
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

        #region GetByProjectAsync -- Happy path

        [Fact]
        public async Task GetByProjectAsync_ReturnsRowsForMatchingProject()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 1, Volume = 5 },
                new() { Buyer = "AH0033", TestCode = "TC02", WorkGroup = "BSU", Month = 2, Volume = 3 },
                new() { Buyer = "OTHER",  TestCode = "TC03", WorkGroup = "CSU", Month = 1, Volume = 2 }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetByProjectAsync(DefaultQuery(), "AH0033");

            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Equal("AH0033", r.Buyer));
        }

        [Fact]
        public async Task GetByProjectAsync_ReturnsEmpty_WhenNoMatchingProject()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "OTHER", TestCode = "TC01", WorkGroup = "CSU", Month = 1 }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetByProjectAsync(DefaultQuery(), "AH0033");

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetByProjectAsync_ReturnsEmpty_WhenNoData()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetByProjectAsync(DefaultQuery(), "AH0033");

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetByProjectAsync -- Plain string filter

        [Fact]
        public async Task GetByProjectAsync_PlainFilter_ByTestCode_ReturnsMatchingRows()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "BOVINE01", WorkGroup = "CSU", Month = 1 },
                new() { Buyer = "AH0033", TestCode = "AVIAN01",  WorkGroup = "BSU", Month = 2 },
                new() { Buyer = "AH0033", TestCode = "BOVINE02", WorkGroup = "CSU", Month = 3 }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetByProjectAsync(DefaultQuery(filter: "bovine"), "AH0033");

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("BOVINE", r.TestCode, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetByProjectAsync_PlainFilter_ByWorkGroup_ReturnsMatchingRows()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "APHA_WG1", Month = 1 },
                new() { Buyer = "AH0033", TestCode = "TC02", WorkGroup = "OTHER_WG",  Month = 2 },
                new() { Buyer = "AH0033", TestCode = "TC03", WorkGroup = "APHA_WG2", Month = 3 }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetByProjectAsync(DefaultQuery(filter: "apha"), "AH0033");

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetByProjectAsync_NullFilter_ReturnsAllRows()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 1 },
                new() { Buyer = "AH0033", TestCode = "TC02", WorkGroup = "BSU", Month = 2 }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetByProjectAsync(DefaultQuery(filter: null), "AH0033");

            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region GetByProjectAsync -- JSON filter

        [Fact]
        public async Task GetByProjectAsync_JsonFilter_ByTestCode_ReturnsMatchingRows()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "BOVINE01", WorkGroup = "CSU", Month = 1 },
                new() { Buyer = "AH0033", TestCode = "AVIAN01",  WorkGroup = "BSU", Month = 2 }
            };
            var repo = CreateRepository(data);
            var jsonFilter = "{\"TestCode\":\"BOVINE\"}";

            var result = await repo.GetByProjectAsync(DefaultQuery(filter: jsonFilter), "AH0033");

            Assert.Single(result.Data);
            Assert.Equal("BOVINE01", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetByProjectAsync_JsonFilter_ByWorkGroup_ReturnsMatchingRows()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "APHA_CSU",  Month = 1 },
                new() { Buyer = "AH0033", TestCode = "TC02", WorkGroup = "OTHER_BSU", Month = 2 }
            };
            var repo = CreateRepository(data);
            var jsonFilter = "{\"WorkGroup\":\"APHA_CSU\"}";

            var result = await repo.GetByProjectAsync(DefaultQuery(filter: jsonFilter), "AH0033");

            Assert.Single(result.Data);
            Assert.Equal("APHA_CSU", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task GetByProjectAsync_JsonFilter_ByMonth_ReturnsMatchingRows()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 3 },
                new() { Buyer = "AH0033", TestCode = "TC02", WorkGroup = "BSU", Month = 7 },
                new() { Buyer = "AH0033", TestCode = "TC03", WorkGroup = "CSU", Month = 3 }
            };
            var repo = CreateRepository(data);
            var jsonFilter = "{\"Month\":\"3\"}";

            var result = await repo.GetByProjectAsync(DefaultQuery(filter: jsonFilter), "AH0033");

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Equal(3, r.Month));
        }

        [Fact]
        public async Task GetByProjectAsync_JsonFilter_AllFields_ReturnsMatchingRows()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "BOVINE01", WorkGroup = "CSU", Month = 3 },
                new() { Buyer = "AH0033", TestCode = "AVIAN01",  WorkGroup = "BSU", Month = 7 }
            };
            var repo = CreateRepository(data);
            var jsonFilter = "{\"TestCode\":\"BOVINE01\",\"WorkGroup\":\"CSU\",\"Month\":\"3\"}";

            var result = await repo.GetByProjectAsync(DefaultQuery(filter: jsonFilter), "AH0033");

            Assert.Single(result.Data);
            Assert.Equal("BOVINE01", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetByProjectAsync_JsonFilter_EmptyObject_ReturnsAllRows()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 1 },
                new() { Buyer = "AH0033", TestCode = "TC02", WorkGroup = "BSU", Month = 2 }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetByProjectAsync(DefaultQuery(filter: "{}"), "AH0033");

            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region GetByProjectAsync -- Sorting

        [Theory]
        [InlineData("testcode",  false)]
        [InlineData("testcode",  true)]
        [InlineData("workgroup", false)]
        [InlineData("workgroup", true)]
        [InlineData("month",     false)]
        [InlineData("month",     true)]
        [InlineData("volume",    false)]
        [InlineData("volume",    true)]
        public async Task GetByProjectAsync_WithSortBy_DoesNotThrow(string sortBy, bool descending)
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC02", WorkGroup = "BSU", Month = 2, Volume = 3 },
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 1, Volume = 5 }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetByProjectAsync(DefaultQuery(sortBy: sortBy, descending: descending), "AH0033");

            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetByProjectAsync_UnrecognisedSortBy_ReturnsResults()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 1 },
                new() { Buyer = "AH0033", TestCode = "TC02", WorkGroup = "BSU", Month = 2 }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetByProjectAsync(DefaultQuery(sortBy: "unknown_column"), "AH0033");

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetByProjectAsync_DefaultSort_OrdersByTestCodeThenMonthThenWorkGroup()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC02", WorkGroup = "BSU", Month = 1 },
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 3 },
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "BSU", Month = 1 },
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "ASU", Month = 1 }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetByProjectAsync(DefaultQuery(), "AH0033");
            var items = result.Data.ToList();

            Assert.Equal("TC01", items[0].TestCode);
            Assert.Equal("ASU",  items[0].WorkGroup);
            Assert.Equal("TC01", items[1].TestCode);
            Assert.Equal("BSU",  items[1].WorkGroup);
        }

        #endregion

        #region GetByProjectAsync -- Pagination

        [Fact]
        public async Task GetByProjectAsync_Pagination_ReturnsCorrectPage()
        {
            var data = Enumerable.Range(1, 15)
                .Select(i => new MonthlyOutput { Buyer = "AH0033", TestCode = $"TC{i:D2}", WorkGroup = "CSU", Month = i })
                .ToList();
            var repo = CreateRepository(data);

            var result = await repo.GetByProjectAsync(DefaultQuery(page: 2, pageSize: 5), "AH0033");

            Assert.Equal(5, result.Data.Count());
        }

        [Fact]
        public async Task GetByProjectAsync_PageSizeLargerThanData_ReturnsAllRows()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 1 },
                new() { Buyer = "AH0033", TestCode = "TC02", WorkGroup = "BSU", Month = 2 }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetByProjectAsync(DefaultQuery(page: 1, pageSize: 50), "AH0033");

            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region GetTotalActualByProjectAsync

        [Fact]
        public async Task GetTotalActualByProjectAsync_ReturnsSumOfVolume()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 1, Volume = 5  },
                new() { Buyer = "AH0033", TestCode = "TC02", WorkGroup = "BSU", Month = 2, Volume = 3  },
                new() { Buyer = "OTHER",  TestCode = "TC03", WorkGroup = "CSU", Month = 1, Volume = 10 }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetTotalActualByProjectAsync("AH0033");

            Assert.Equal(8.0, result);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_NullVolumes_TreatedAsZero()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 1, Volume = null },
                new() { Buyer = "AH0033", TestCode = "TC02", WorkGroup = "BSU", Month = 2, Volume = 4   }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetTotalActualByProjectAsync("AH0033");

            Assert.Equal(4.0, result);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WhenNoMatchingProject_ReturnsZero()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "OTHER", TestCode = "TC01", WorkGroup = "CSU", Month = 1, Volume = 5 }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetTotalActualByProjectAsync("AH0033");

            Assert.Equal(0.0, result);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WhenNoData_ReturnsZero()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetTotalActualByProjectAsync("AH0033");

            Assert.Equal(0.0, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTotalActualByProjectAsync_EmptyOrWhitespaceProjectCode_ReturnsZero(string projectCode)
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 1, Volume = 5 }
            };
            var repo = CreateRepository(data);

            var result = await repo.GetTotalActualByProjectAsync(projectCode);

            Assert.Equal(0.0, result);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WhenRecordExists_RemovesAndReturnsTrue()
        {
            var entity = new MonthlyOutput
            {
                Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 1
            };
            var repo = CreateRepository([entity]);

            var result = await repo.DeleteAsync("AH0033", "TC01", 1, "CSU");

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenRecordNotFound_ReturnsFalse()
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 1 }
            };
            var repo = CreateRepository(data);

            var result = await repo.DeleteAsync("AH0033", "TC01", 1, "UNKNOWN_WG");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenNoRecords_ReturnsFalse()
        {
            var repo = CreateRepository([]);

            var result = await repo.DeleteAsync("AH0033", "TC01", 1, "CSU");

            Assert.False(result);
        }

        [Theory]
        [InlineData("",       "TC01", 1, "CSU")]
        [InlineData("AH0033", "",     1, "CSU")]
        [InlineData("AH0033", "TC01", 1, ""   )]
        public async Task DeleteAsync_WithMissingRequiredParam_ReturnsFalse(
            string buyer, string testCode, double month, string workGroup)
        {
            var data = new List<MonthlyOutput>
            {
                new() { Buyer = "AH0033", TestCode = "TC01", WorkGroup = "CSU", Month = 1 }
            };
            var repo = CreateRepository(data);

            var result = await repo.DeleteAsync(buyer, testCode, month, workGroup);

            Assert.False(result);
        }

        #endregion
    }
}
