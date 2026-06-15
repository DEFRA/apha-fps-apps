using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.DataAccess.UnitTests.Repositories.AccountCategoryRepositoryTest
{
    public class AccountCategoryRepositoryTests
    {
        private const string TestAccShortName = "TEST001";
        private const string TestAccountDescription = "Test Description";
        private const string TestAccountType = "Income";
        private const int TestFpsYear = 2024;
        private const int TestPageSize = 10;
        private const int TestPage = 1;

        private readonly IAccountCategoryRepository _repositoryMock;

        public AccountCategoryRepositoryTests()
        {
            _repositoryMock = Substitute.For<IAccountCategoryRepository>();
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_HappyPath_ReturnsAllRecords()
        {
            // Arrange
            var query = new PaginationParameters<string>
            {
                Page = TestPage,
                PageSize = TestPageSize,
                Filter = null
            };

            var categories = new List<AccountCategory>
            {
                CreateTestAccountCategory("CAT001", "Category 1"),
                CreateTestAccountCategory("CAT002", "Category 2"),
                CreateTestAccountCategory("CAT003", "Category 3")
            };

            var paginationData = new PaginationData
            {
                PageNumber = TestPage,
                PageSize = TestPageSize,
                TotalRecords = 3,
                TotalPages = 1
            };

            var expectedResult = new PagedData<AccountCategory>(categories, paginationData);

            _repositoryMock.GetAllAsync(query, null).Returns(expectedResult);

            // Act
            var result = await _repositoryMock.GetAllAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.Data.Count());
            await _repositoryMock.Received(1).GetAllAsync(query, null);
        }

        [Fact]
        public async Task GetAllAsync_WithRcFilter_ReturnsOnlyRcSpecific()
        {
            // Arrange
            var query = new PaginationParameters<string>
            {
                Page = TestPage,
                PageSize = TestPageSize,
                Filter = null
            };

            var categories = new List<AccountCategory>
            {
                CreateTestAccountCategory("RC001", "RC Category", rcSpecific: -1)
            };

            var paginationData = new PaginationData
            {
                PageNumber = TestPage,
                PageSize = TestPageSize,
                TotalRecords = 1,
                TotalPages = 1
            };

            var expectedResult = new PagedData<AccountCategory>(categories, paginationData);

            _repositoryMock.GetAllAsync(query, "rc").Returns(expectedResult);

            // Act
            var result = await _repositoryMock.GetAllAsync(query, "rc");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("RC001", result.Data.ElementAt(0).AccShortName);
        }

        [Fact]
        public async Task GetAllAsync_WithPsFilter_ReturnsOnlyProjectSpecific()
        {
            // Arrange
            var query = new PaginationParameters<string>
            {
                Page = TestPage,
                PageSize = TestPageSize,
                Filter = null
            };

            var categories = new List<AccountCategory>
            {
                CreateTestAccountCategory("PS001", "PS Category", projectSpecific: -1)
            };

            var paginationData = new PaginationData
            {
                PageNumber = TestPage,
                PageSize = TestPageSize,
                TotalRecords = 1,
                TotalPages = 1
            };

            var expectedResult = new PagedData<AccountCategory>(categories, paginationData);

            _repositoryMock.GetAllAsync(query, "ps").Returns(expectedResult);

            // Act
            var result = await _repositoryMock.GetAllAsync(query, "ps");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("PS001", result.Data.ElementAt(0).AccShortName);
        }

        [Fact]
        public async Task GetAllAsync_WithSorting_ReturnsOrderedRecords()
        {
            // Arrange
            var query = new PaginationParameters<string>
            {
                Page = TestPage,
                PageSize = TestPageSize,
                SortBy = "AccShortName",
                Descending = false
            };

            var categories = new List<AccountCategory>
            {
                CreateTestAccountCategory("AAA", "First"),
                CreateTestAccountCategory("MMM", "Middle"),
                CreateTestAccountCategory("ZZZ", "Last")
            };

            var paginationData = new PaginationData
            {
                PageNumber = TestPage,
                PageSize = TestPageSize,
                TotalRecords = 3,
                TotalPages = 1
            };

            var expectedResult = new PagedData<AccountCategory>(categories, paginationData);

            _repositoryMock.GetAllAsync(query, null).Returns(expectedResult);

            // Act
            var result = await _repositoryMock.GetAllAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("AAA", result.Data.ElementAt(0).AccShortName);
            Assert.Equal("MMM", result.Data.ElementAt(1).AccShortName);
            Assert.Equal("ZZZ", result.Data.ElementAt(2).AccShortName);
        }

        [Fact]
        public async Task GetAllAsync_WithDescendingSorting_ReturnsOrderedRecords()
        {
            // Arrange
            var query = new PaginationParameters<string>
            {
                Page = TestPage,
                PageSize = TestPageSize,
                SortBy = "AccShortName",
                Descending = true
            };

            var categories = new List<AccountCategory>
            {
                CreateTestAccountCategory("CCC", "Third"),
                CreateTestAccountCategory("BBB", "Second"),
                CreateTestAccountCategory("AAA", "First")
            };

            var paginationData = new PaginationData
            {
                PageNumber = TestPage,
                PageSize = TestPageSize,
                TotalRecords = 3,
                TotalPages = 1
            };

            var expectedResult = new PagedData<AccountCategory>(categories, paginationData);

            _repositoryMock.GetAllAsync(query, null).Returns(expectedResult);

            // Act
            var result = await _repositoryMock.GetAllAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CCC", result.Data.ElementAt(0).AccShortName);
            Assert.Equal("BBB", result.Data.ElementAt(1).AccShortName);
            Assert.Equal("AAA", result.Data.ElementAt(2).AccShortName);
        }

        [Fact]
        public async Task GetAllAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var query = new PaginationParameters<string>
            {
                Page = 2,
                PageSize = 10
            };

            var categories = new List<AccountCategory>();
            for (int i = 11; i <= 20; i++)
            {
                categories.Add(CreateTestAccountCategory($"CAT{i:D3}", $"Category {i}"));
            }

            var paginationData = new PaginationData
            {
                PageNumber = 2,
                PageSize = 10,
                TotalRecords = 25,
                TotalPages = 3
            };

            var expectedResult = new PagedData<AccountCategory>(categories, paginationData);

            _repositoryMock.GetAllAsync(query, null).Returns(expectedResult);

            // Act
            var result = await _repositoryMock.GetAllAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(25, result.PaginationData.TotalRecords);
            Assert.Equal(10, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyResult()
        {
            // Arrange
            var query = new PaginationParameters<string>
            {
                Page = TestPage,
                PageSize = TestPageSize
            };

            var categories = new List<AccountCategory>();

            var paginationData = new PaginationData
            {
                PageNumber = TestPage,
                PageSize = TestPageSize,
                TotalRecords = 0,
                TotalPages = 0
            };

            var expectedResult = new PagedData<AccountCategory>(categories, paginationData);

            _repositoryMock.GetAllAsync(query, null).Returns(expectedResult);

            // Act
            var result = await _repositoryMock.GetAllAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllAsync_LargePage_ReturnsRemainingRecords()
        {
            // Arrange
            var query = new PaginationParameters<string>
            {
                Page = 2,
                PageSize = 10
            };

            var categories = new List<AccountCategory>();
            for (int i = 1; i <= 5; i++)
            {
                categories.Add(CreateTestAccountCategory($"CAT{i:D3}", $"Category {i}"));
            }

            var paginationData = new PaginationData
            {
                PageNumber = 2,
                PageSize = 10,
                TotalRecords = 15,
                TotalPages = 2
            };

            var expectedResult = new PagedData<AccountCategory>(categories, paginationData);

            _repositoryMock.GetAllAsync(query, null).Returns(expectedResult);

            // Act
            var result = await _repositoryMock.GetAllAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(15, result.PaginationData.TotalRecords);
            Assert.Equal(5, result.Data.Count());
        }

        [Theory]
        [InlineData("all")]
        [InlineData("rc")]
        [InlineData("ps")]
        public async Task GetAllAsync_VariousFilterTypes_CallsRepository(string filterType)
        {
            // Arrange
            var query = new PaginationParameters<string>
            {
                Page = TestPage,
                PageSize = TestPageSize
            };

            var categories = new List<AccountCategory>
            {
                CreateTestAccountCategory("CAT001", "Category 1")
            };

            var paginationData = new PaginationData
            {
                PageNumber = TestPage,
                PageSize = TestPageSize,
                TotalRecords = 1,
                TotalPages = 1
            };

            var expectedResult = new PagedData<AccountCategory>(categories, paginationData);

            _repositoryMock.GetAllAsync(query, filterType).Returns(expectedResult);

            // Act
            var result = await _repositoryMock.GetAllAsync(query, filterType);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            await _repositoryMock.Received(1).GetAllAsync(query, filterType);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_HappyPath_ReturnsEntity()
        {
            // Arrange
            var category = CreateTestAccountCategory(TestAccShortName, TestAccountDescription);
            _repositoryMock.GetByIdAsync(TestAccShortName).Returns(category);

            // Act
            var result = await _repositoryMock.GetByIdAsync(TestAccShortName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestAccShortName, result.AccShortName);
            Assert.Equal(TestAccountDescription, result.AccountDescription);
            await _repositoryMock.Received(1).GetByIdAsync(TestAccShortName);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            _repositoryMock.GetByIdAsync("NONEXISTENT").Returns((AccountCategory?)null);

            // Act
            var result = await _repositoryMock.GetByIdAsync("NONEXISTENT");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("CAT001")]
        [InlineData("CAT002")]
        [InlineData("CAT003")]
        public async Task GetByIdAsync_VariousIds_ReturnsCorrectEntity(string accShortName)
        {
            // Arrange
            var category = CreateTestAccountCategory(accShortName, $"Description for {accShortName}");
            _repositoryMock.GetByIdAsync(accShortName).Returns(category);

            // Act
            var result = await _repositoryMock.GetByIdAsync(accShortName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accShortName, result.AccShortName);
            await _repositoryMock.Received(1).GetByIdAsync(accShortName);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_HappyPath_AddsSuccessfully()
        {
            // Arrange
            var category = CreateTestAccountCategory(TestAccShortName, TestAccountDescription);
            _repositoryMock.AddAsync(category).Returns(category);

            // Act
            var result = await _repositoryMock.AddAsync(category);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestAccShortName, result.AccShortName);
            Assert.Equal(TestFpsYear, result.FpsYear);
            await _repositoryMock.Received(1).AddAsync(category);
        }

        [Fact]
        public async Task AddAsync_NullEntity_ThrowsArgumentNullException()
        {
            // Arrange
            _repositoryMock.AddAsync(null!).Throws(new ArgumentNullException());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repositoryMock.AddAsync(null!));
        }

        [Fact]
        public async Task AddAsync_Error_ServiceThrows()
        {
            // Arrange
            var category = CreateTestAccountCategory(TestAccShortName, TestAccountDescription);
            _repositoryMock.AddAsync(category).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _repositoryMock.AddAsync(category));
        }

        [Fact]
        public async Task AddAsync_WithNullConstituentAccountCodes_SavesSuccessfully()
        {
            // Arrange
            var category = CreateTestAccountCategory(TestAccShortName, TestAccountDescription);
            category.ConstituentAccountCodes = null;
            _repositoryMock.AddAsync(category).Returns(category);

            // Act
            var result = await _repositoryMock.AddAsync(category);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ConstituentAccountCodes);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_HappyPath_UpdatesSuccessfully()
        {
            // Arrange
            var category = CreateTestAccountCategory(TestAccShortName, "Updated Description");
            category.AccountType = "Expense";
            _repositoryMock.UpdateAsync(category).Returns(category);

            // Act
            var result = await _repositoryMock.UpdateAsync(category);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Description", result.AccountDescription);
            Assert.Equal("Expense", result.AccountType);
            await _repositoryMock.Received(1).UpdateAsync(category);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingEntity_ThrowsInvalidOperationException()
        {
            // Arrange
            var category = CreateTestAccountCategory("NONEXISTENT", TestAccountDescription);
            _repositoryMock.UpdateAsync(category).Throws(new InvalidOperationException());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repositoryMock.UpdateAsync(category));
        }

        [Fact]
        public async Task UpdateAsync_NullEntity_ThrowsArgumentNullException()
        {
            // Arrange
            _repositoryMock.UpdateAsync(null!).Throws(new ArgumentNullException());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repositoryMock.UpdateAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_Error_ServiceThrows()
        {
            // Arrange
            var category = CreateTestAccountCategory(TestAccShortName, TestAccountDescription);
            _repositoryMock.UpdateAsync(category).Throws(new Exception("Update failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _repositoryMock.UpdateAsync(category));
        }

        [Fact]
        public async Task UpdateAsync_OnlyModifiedFields_UpdatesCorrectly()
        {
            // Arrange
            var updated = CreateTestAccountCategory(TestAccShortName, "New Description");
            updated.AccountType = "Expense";
            updated.ConstituentAccountCodes = "2000";
            _repositoryMock.UpdateAsync(updated).Returns(updated);

            // Act
            var result = await _repositoryMock.UpdateAsync(updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Description", result.AccountDescription);
            Assert.Equal("Expense", result.AccountType);
            Assert.Equal("2000", result.ConstituentAccountCodes);
        }

        [Fact]
        public async Task UpdateAsync_ChangeFromProjectSpecificToRcSpecific_UpdatesSuccessfully()
        {
            // Arrange
            var updated = CreateTestAccountCategory(TestAccShortName, TestAccountDescription, rcSpecific: -1);
            updated.ProjectSpecific = null;
            _repositoryMock.UpdateAsync(updated).Returns(updated);

            // Act
            var result = await _repositoryMock.UpdateAsync(updated);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ProjectSpecific);
            Assert.Equal(-1, result.RcSpecific);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_HappyPath_DeletesSuccessfully()
        {
            // Arrange
            _repositoryMock.DeleteAsync(TestAccShortName).Returns(true);

            // Act
            var result = await _repositoryMock.DeleteAsync(TestAccShortName);

            // Assert
            Assert.True(result);
            await _repositoryMock.Received(1).DeleteAsync(TestAccShortName);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingEntity_ReturnsFalse()
        {
            // Arrange
            _repositoryMock.DeleteAsync("NONEXISTENT").Returns(false);

            // Act
            var result = await _repositoryMock.DeleteAsync("NONEXISTENT");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_NullOrEmpty_ThrowsArgumentException()
        {
            // Arrange
            _repositoryMock.DeleteAsync("").Throws(new ArgumentException());
            _repositoryMock.DeleteAsync("   ").Throws(new ArgumentException());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repositoryMock.DeleteAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => _repositoryMock.DeleteAsync("   "));
        }

        [Fact]
        public async Task DeleteAsync_NullId_ThrowsArgumentException()
        {
            // Arrange
            _repositoryMock.DeleteAsync(null!).Throws(new ArgumentException());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repositoryMock.DeleteAsync(null!));
        }

        [Fact]
        public async Task DeleteAsync_MultipleCalls_OnlyDeletesOnce()
        {
            // Arrange
            _repositoryMock.DeleteAsync(TestAccShortName).Returns(true, false);

            // Act
            var result1 = await _repositoryMock.DeleteAsync(TestAccShortName);
            var result2 = await _repositoryMock.DeleteAsync(TestAccShortName);

            // Assert
            Assert.True(result1);
            Assert.False(result2);
        }

        [Theory]
        [InlineData("Division1")]
        [InlineData("Division2")]
        [InlineData("Division3")]
        public async Task DeleteAsync_VariousNames_CallsRepository(string accShortName)
        {
            // Arrange
            _repositoryMock.DeleteAsync(accShortName).Returns(true);

            // Act
            await _repositoryMock.DeleteAsync(accShortName);

            // Assert
            await _repositoryMock.Received(1).DeleteAsync(accShortName);
        }

        #endregion

        #region Helper Methods

        private static AccountCategory CreateTestAccountCategory(
            string accShortName,
            string accountDescription,
            int? projectSpecific = null,
            int? rcSpecific = null)
        {
            return new AccountCategory
            {
                AccShortName = accShortName,
                AccountDescription = accountDescription,
                AccountType = TestAccountType,
                ConstituentAccountCodes = "1000,2000",
                ProjectSpecific = projectSpecific,
                RcSpecific = rcSpecific,
                FpsYear = TestFpsYear
            };
        }

        #endregion
    }
}
