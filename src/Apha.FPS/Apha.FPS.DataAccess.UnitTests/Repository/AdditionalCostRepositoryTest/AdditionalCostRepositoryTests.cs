using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.AdditionalCostRepositoryTest
{
    public class AdditionalCostRepositoryTests
    {
        private const int DefaultFpsYear = 2024;
        private const string DefaultUserEmail = "test@example.com";

        private static Mock<IFpsRequestContext> CreateMockRequestContext(int year = DefaultFpsYear)
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(year);
            mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            return mock;
        }

        private static AdditionalCostRepository CreateRepository(
            IEnumerable<AdditionalCost>? additionalCosts = null,
            IEnumerable<AdditionalCostView>? additionalCostViews = null,
            IEnumerable<AdditionalCostLog>? additionalCostLogs = null,
            IEnumerable<AccountCategory>? accountCategories = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockRequestContext = CreateMockRequestContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            if (additionalCosts != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(additionalCosts);
                mockContext.Setup(x => x.AdditionalCosts).Returns(mockSet.Object);
            }

            if (additionalCostViews != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(additionalCostViews);
                mockContext.Setup(x => x.AdditionalCostViews).Returns(mockSet.Object);
            }

            if (additionalCostLogs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(additionalCostLogs);
                mockContext.Setup(x => x.AdditionalCostLogs).Returns(mockSet.Object);
            }

            if (accountCategories != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(accountCategories);
                mockContext.Setup(x => x.AccountCategories).Returns(mockSet.Object);
            }

            return new AdditionalCostRepository(mockContext.Object, mockRequestContext.Object);
        }

        #region GetByJobCodeAsync

        [Fact]
        public async Task GetByJobCodeAsync_ReturnsPagedData_WithValidJobCode()
        {
            // Arrange
            var viewData = new List<AdditionalCostView>
            {
                new() { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { JobCode = "JOB001", Account = "ACC2", Description = "Desc2", ItemCost = 200m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };

            var repo = CreateRepository(additionalCostViews: viewData);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetByJobCodeAsync(query, "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetByJobCodeAsync_WithNonMatchingJobCode_ReturnsEmpty()
        {
            // Arrange
            var viewData = new List<AdditionalCostView>
            {
                new() { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };

            var repo = CreateRepository(additionalCostViews: viewData);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetByJobCodeAsync(query, "NOTEXIST");

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetByJobCodeAsync_AppliesFilter_ByDescription()
        {
            // Arrange
            var viewData = new List<AdditionalCostView>
            {
                new() { JobCode = "JOB001", Account = "ACC1", Description = "Alpha", ItemCost = 100m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { JobCode = "JOB001", Account = "ACC2", Description = "Beta",  ItemCost = 200m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };

            var repo = CreateRepository(additionalCostViews: viewData);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Description\":\"Alpha\"}"
            };

            // Act
            var result = await repo.GetByJobCodeAsync(query, "JOB001");

            // Assert
            Assert.Single(result.Data);
            Assert.Contains("Alpha", result.Data.First().Description);
        }

        [Fact]
        public async Task GetByJobCodeAsync_AppliesFilter_BySupplier()
        {
            // Arrange
            var viewData = new List<AdditionalCostView>
            {
                new() { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", Supplier = "SupplierA", ItemCost = 100m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { JobCode = "JOB001", Account = "ACC2", Description = "Desc2", Supplier = "SupplierB", ItemCost = 200m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };

            var repo = CreateRepository(additionalCostViews: viewData);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Supplier\":\"SupplierA\"}"
            };

            // Act
            var result = await repo.GetByJobCodeAsync(query, "JOB001");

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("SupplierA", result.Data.First().Supplier);
        }

        [Theory]
        [InlineData("description", false, "Alpha")]
        [InlineData("description", true,  "Gamma")]
        [InlineData("itemcost",    false, 50.0)]
        [InlineData("itemcost",    true,  300.0)]
        public async Task GetByJobCodeAsync_AppliesSorting_Correctly(string sortBy, bool descending, object expectedFirst)
        {
            // Arrange
            var viewData = new List<AdditionalCostView>
            {
                new() { JobCode = "JOB001", Account = "ACC1", Description = "Beta",  ItemCost = 100m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { JobCode = "JOB001", Account = "ACC2", Description = "Alpha", ItemCost = 50m,  FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { JobCode = "JOB001", Account = "ACC3", Description = "Gamma", ItemCost = 300m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };

            var repo = CreateRepository(additionalCostViews: viewData);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = sortBy, Descending = descending
            };

            // Act
            var result = await repo.GetByJobCodeAsync(query, "JOB001");

            // Assert
            var first = result.Data.First();
            var actual = sortBy.ToLower() switch
            {
                "description" => (object?)first.Description,
                "itemcost"    => (object)first.ItemCost,
                _             => (object?)first.Description
            };
            Assert.Equal(expectedFirst.ToString(), actual?.ToString());
        }

        [Fact]
        public async Task GetByJobCodeAsync_AppliesPaging_Correctly()
        {
            // Arrange
            var viewData = Enumerable.Range(1, 15)
                .Select(i => new AdditionalCostView
                {
                    JobCode = "JOB001", Account = $"ACC{i}", Description = $"Desc{i:D2}",
                    ItemCost = i * 10m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail
                }).ToList();

            var repo = CreateRepository(additionalCostViews: viewData);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 5 };

            // Act
            var result = await repo.GetByJobCodeAsync(query, "JOB001");

            // Assert
            Assert.Equal(5, result.Data.Count());
            Assert.Equal(15, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        #endregion

        #region GetTotalItemCostAsync

        [Fact]
        public async Task GetTotalItemCostAsync_WithMatchingRecords_ReturnsSum()
        {
            // Arrange
            var additionalCosts = new List<AdditionalCost>
            {
                new() { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m, FpsYear = DefaultFpsYear },
                new() { JobCode = "JOB001", Account = "ACC2", Description = "Desc2", ItemCost = 250m, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(additionalCosts: additionalCosts);

            // Act
            var result = await repo.GetTotalItemCostAsync("JOB001");

            // Assert
            Assert.Equal(350m, result);
        }

        [Fact]
        public async Task GetTotalItemCostAsync_WithNoRecords_ReturnsZero()
        {
            // Arrange
            var repo = CreateRepository(additionalCosts: new List<AdditionalCost>());

            // Act
            var result = await repo.GetTotalItemCostAsync("JOB001");

            // Assert
            Assert.Equal(0m, result);
        }

        #endregion

        #region GetAccountCategoriesAsync

        [Fact]
        public async Task GetAccountCategoriesAsync_ReturnsProjectSpecificCategories()
        {
            // Arrange
            var categories = new List<AccountCategory>
            {
                new() { AccShortName = "ACC1", AccountDescription = "Specific", ProjectSpecific = -1, FpsYear = DefaultFpsYear },
                new() { AccShortName = "ACC2", AccountDescription = "NonSpecific", ProjectSpecific = 0, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(accountCategories: categories);

            // Act
            var result = await repo.GetAccountCategoriesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("ACC1", result.First().AccShortName);
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_WithNoCategories_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepository(accountCategories: new List<AccountCategory>());

            // Act
            var result = await repo.GetAccountCategoriesAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WithExistingCompositeKey_ReturnsRecord()
        {
            // Arrange
            var additionalCosts = new List<AdditionalCost>
            {
                new() { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m, FpsYear = DefaultFpsYear },
                new() { JobCode = "JOB001", Account = "ACC2", Description = "Desc2", ItemCost = 200m, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(additionalCosts: additionalCosts);

            // Act
            var result = await repo.GetByIdAsync("JOB001", "ACC1", "Desc1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ACC1", result!.Account);
            Assert.Equal("Desc1", result.Description);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingKey_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(additionalCosts: new List<AdditionalCost>());

            // Act
            var result = await repo.GetByIdAsync("JOB999", "ACC999", "NoExist");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithPartialKeyMatch_ReturnsNull()
        {
            // Arrange
            var additionalCosts = new List<AdditionalCost>
            {
                new() { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(additionalCosts: additionalCosts);

            // Act — same JobCode and Account, different Description
            var result = await repo.GetByIdAsync("JOB001", "ACC1", "WrongDesc");

            // Assert
            Assert.Null(result);
        }

        #endregion
    }
}
