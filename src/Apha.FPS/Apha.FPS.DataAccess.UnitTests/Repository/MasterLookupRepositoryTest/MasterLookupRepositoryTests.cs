using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.MasterLookupRepositoryTest
{
    public class MasterLookupRepositoryTests
    {
        private static Mock<FpsDbContext> CreateMockContext(
            IEnumerable<MasterLookup>? masterLookups = null,
            IEnumerable<Directorate>? directorates = null,
            IEnumerable<Disease>? diseases = null,
            IEnumerable<Customer>? customers = null)
        {
            var fpsRequestContext = new Mock<IFpsRequestContext>();
            fpsRequestContext.Setup(x => x.FpsYear).Returns(2024);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext.Object);

            mockContext.Setup(x => x.MasterLookups)
                .Returns(RepositoryTestHelper.CreateMockDbSet(masterLookups ?? new List<MasterLookup>()).Object);
            mockContext.Setup(x => x.Directorates)
                .Returns(RepositoryTestHelper.CreateMockDbSet(directorates ?? new List<Directorate>()).Object);
            mockContext.Setup(x => x.Diseases)
                .Returns(RepositoryTestHelper.CreateMockDbSet(diseases ?? new List<Disease>()).Object);
            mockContext.Setup(x => x.Customers)
                .Returns(RepositoryTestHelper.CreateMockDbSet(customers ?? new List<Customer>()).Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return mockContext;
        }

        #region GetAllMasterLookupsAsync

        [Fact]
        public async Task GetAllMasterLookupsAsync_ReturnsAllOrderedByName()
        {
            // Arrange
            var lookups = new List<MasterLookup>
            {
                new() { MasterTableName = "disease" },
                new() { MasterTableName = "customer" },
                new() { MasterTableName = "directorate" }
            };
            var repo = new MasterLookupRepository(CreateMockContext(masterLookups: lookups).Object);

            // Act
            var result = (await repo.GetAllMasterLookupsAsync()).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("customer", result[0].MasterTableName);
            Assert.Equal("directorate", result[1].MasterTableName);
            Assert.Equal("disease", result[2].MasterTableName);
        }

        [Fact]
        public async Task GetAllMasterLookupsAsync_Empty_ReturnsEmpty()
        {
            // Arrange
            var repo = new MasterLookupRepository(CreateMockContext().Object);

            // Act
            var result = await repo.GetAllMasterLookupsAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetLookupItemsAsync

        [Fact]
        public async Task GetLookupItemsAsync_Directorate_ReturnsOrderedNames()
        {
            // Arrange
            var directorates = new List<Directorate>
            {
                new() { DirectorateName = "Beta" },
                new() { DirectorateName = "Alpha" }
            };
            var repo = new MasterLookupRepository(CreateMockContext(directorates: directorates).Object);

            // Act
            var result = (await repo.GetLookupItemsAsync("directorate")).ToList();

            // Assert
            Assert.Equal(new[] { "Alpha", "Beta" }, result);
        }

        [Fact]
        public async Task GetLookupItemsAsync_Disease_ReturnsOrderedNames()
        {
            // Arrange
            var diseases = new List<Disease>
            {
                new() { DiseaseName = "Rabies" },
                new() { DiseaseName = "Anthrax" }
            };
            var repo = new MasterLookupRepository(CreateMockContext(diseases: diseases).Object);

            // Act
            var result = (await repo.GetLookupItemsAsync("disease")).ToList();

            // Assert
            Assert.Equal(new[] { "Anthrax", "Rabies" }, result);
        }

        [Fact]
        public async Task GetLookupItemsAsync_Customer_ReturnsOrderedNames()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new() { CustomerName = "Zeta" },
                new() { CustomerName = "Delta" }
            };
            var repo = new MasterLookupRepository(CreateMockContext(customers: customers).Object);

            // Act
            var result = (await repo.GetLookupItemsAsync("customer")).ToList();

            // Assert
            Assert.Equal(new[] { "Delta", "Zeta" }, result);
        }

        [Fact]
        public async Task GetLookupItemsAsync_IsCaseInsensitiveOnTableName()
        {
            // Arrange
            var directorates = new List<Directorate> { new() { DirectorateName = "Alpha" } };
            var repo = new MasterLookupRepository(CreateMockContext(directorates: directorates).Object);

            // Act
            var result = (await repo.GetLookupItemsAsync(" DIRECTORATE ")).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Alpha", result[0]);
        }

        [Fact]
        public async Task GetLookupItemsAsync_UnknownTable_ThrowsArgumentException()
        {
            // Arrange
            var repo = new MasterLookupRepository(CreateMockContext().Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetLookupItemsAsync("unknown"));
        }

        [Fact]
        public async Task GetLookupItemsAsync_EmptyTableName_ThrowsArgumentException()
        {
            // Arrange
            var repo = new MasterLookupRepository(CreateMockContext().Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetLookupItemsAsync(" "));
        }

        #endregion

        #region GetLookupItemsPagedAsync

        [Fact]
        public async Task GetLookupItemsPagedAsync_ReturnsPagedResult()
        {
            // Arrange
            var directorates = new List<Directorate>
            {
                new() { DirectorateName = "Alpha" },
                new() { DirectorateName = "Beta" },
                new() { DirectorateName = "Gamma" }
            };
            var repo = new MasterLookupRepository(CreateMockContext(directorates: directorates).Object);
            var query = new PaginationParameters<string>(page: 1, pageSize: 2);

            // Act
            var result = await repo.GetLookupItemsPagedAsync("directorate", query);

            // Assert
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal("Alpha", result.Data.First());
        }

        [Fact]
        public async Task GetLookupItemsPagedAsync_SecondPage_ReturnsRemaining()
        {
            // Arrange
            var directorates = new List<Directorate>
            {
                new() { DirectorateName = "Alpha" },
                new() { DirectorateName = "Beta" },
                new() { DirectorateName = "Gamma" }
            };
            var repo = new MasterLookupRepository(CreateMockContext(directorates: directorates).Object);
            var query = new PaginationParameters<string>(page: 2, pageSize: 2);

            // Act
            var result = await repo.GetLookupItemsPagedAsync("directorate", query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("Gamma", result.Data.First());
        }

        [Fact]
        public async Task GetLookupItemsPagedAsync_DescendingSort_OrdersDescending()
        {
            // Arrange
            var directorates = new List<Directorate>
            {
                new() { DirectorateName = "Alpha" },
                new() { DirectorateName = "Beta" }
            };
            var repo = new MasterLookupRepository(CreateMockContext(directorates: directorates).Object);
            var query = new PaginationParameters<string>(sortBy: "Value", descending: true, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetLookupItemsPagedAsync("directorate", query);

            // Assert
            Assert.Equal(new[] { "Beta", "Alpha" }, result.Data.ToList());
        }

        [Fact]
        public async Task GetLookupItemsPagedAsync_NullQuery_ThrowsArgumentNullException()
        {
            // Arrange
            var repo = new MasterLookupRepository(CreateMockContext().Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => repo.GetLookupItemsPagedAsync("directorate", null!));
        }

        [Fact]
        public async Task GetLookupItemsPagedAsync_UnknownTable_ThrowsArgumentException()
        {
            // Arrange
            var repo = new MasterLookupRepository(CreateMockContext().Object);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => repo.GetLookupItemsPagedAsync("unknown", query));
        }

        #endregion

        #region CreateLookupItemAsync

        [Fact]
        public async Task CreateLookupItemAsync_Directorate_SavesAndReturnsTrue()
        {
            // Arrange
            var mockContext = CreateMockContext();
            var repo = new MasterLookupRepository(mockContext.Object);

            // Act
            var result = await repo.CreateLookupItemAsync("directorate", "NewDir");

            // Assert
            Assert.True(result);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateLookupItemAsync_Disease_SavesAndReturnsTrue()
        {
            // Arrange
            var mockContext = CreateMockContext();
            var repo = new MasterLookupRepository(mockContext.Object);

            // Act
            var result = await repo.CreateLookupItemAsync("disease", "NewDisease");

            // Assert
            Assert.True(result);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateLookupItemAsync_Customer_SavesAndReturnsTrue()
        {
            // Arrange
            var mockContext = CreateMockContext();
            var repo = new MasterLookupRepository(mockContext.Object);

            // Act
            var result = await repo.CreateLookupItemAsync("customer", "NewCustomer");

            // Assert
            Assert.True(result);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateLookupItemAsync_UnknownTable_ThrowsArgumentException()
        {
            // Arrange
            var repo = new MasterLookupRepository(CreateMockContext().Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => repo.CreateLookupItemAsync("unknown", "value"));
        }

        #endregion

        #region UpdateLookupItemAsync / DeleteLookupItemAsync (validation)

        [Fact]
        public async Task UpdateLookupItemAsync_UnknownTable_ThrowsArgumentException()
        {
            // Arrange
            var repo = new MasterLookupRepository(CreateMockContext().Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => repo.UpdateLookupItemAsync("unknown", "Old", "New"));
        }

        [Fact]
        public async Task DeleteLookupItemAsync_UnknownTable_ThrowsArgumentException()
        {
            // Arrange
            var repo = new MasterLookupRepository(CreateMockContext().Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => repo.DeleteLookupItemAsync("unknown", "value"));
        }

        #endregion
    }
}
