using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.DivisionRepositoryTest
{
    public class DivisionRepositoryTests
    {
        private readonly Mock<FpsDbContext> _mockContext;
        private readonly DivisionRepository _repository;
        private readonly Mock<DbSet<Division>> _mockDivisionSet;
        private readonly Mock<DbSet<ProfitCentre>> _mockProfitCentreSet;
        private readonly Mock<DbSet<DivisionGrade>> _mockDivisionGradeSet;

        public DivisionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var mockFpsContext = new Mock<IFpsRequestContext>();
            mockFpsContext.Setup(c => c.FpsYear).Returns(2024);
            mockFpsContext.Setup(c => c.UserEmailId).Returns("testuser@test.com");

            _mockContext = new Mock<FpsDbContext>(options, mockFpsContext.Object) { CallBase = true };
            _repository = new DivisionRepository(_mockContext.Object);

            _mockDivisionSet = new Mock<DbSet<Division>>();
            _mockProfitCentreSet = new Mock<DbSet<ProfitCentre>>();
            _mockDivisionGradeSet = new Mock<DbSet<DivisionGrade>>();

            _mockContext.Setup(c => c.Divisions).Returns(_mockDivisionSet.Object);
            _mockContext.Setup(c => c.Set<ProfitCentre>()).Returns(_mockProfitCentreSet.Object);
            _mockContext.Setup(c => c.Set<DivisionGrade>()).Returns(_mockDivisionGradeSet.Object);
        }

        #region GetAllDivisionsAsync Tests

        [Fact]
        public async Task GetAllDivisionsAsync_ReturnsAllDivisions_OrderedByDivName()
        {
            // Arrange
            var divisions = new List<Division>
            {
                new Division { DivName = "VSD", DivisionId = 2, AgencyId = 1 },
                new Division { DivName = "ACDP", DivisionId = 1, AgencyId = 1 }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);

            // Act
            var result = await _repository.GetAllDivisionsAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("ACDP", result[0].DivName); // Ordered by DivName
            Assert.Equal("VSD", result[1].DivName);
        }

        [Fact]
        public async Task GetAllDivisionsAsync_ReturnsEmptyList_WhenNoDivisionsExist()
        {
            // Arrange
            var divisions = new List<Division>().AsQueryable();
            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);

            // Act
            var result = await _repository.GetAllDivisionsAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetAllDivisionsPagedAsync Tests

        [Fact]
        public async Task GetAllDivisionsPagedAsync_ReturnsPagedData_WithCorrectCounts()
        {
            // Arrange
            var divisions = new List<Division>
            {
                new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1 },
                new Division { DivName = "ACDP", DivisionId = 2, AgencyId = 1 },
                new Division { DivName = "VCJD", DivisionId = 3, AgencyId = 2 }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);

            var query = new Core.Pagination.PaginationParameters<string>
            {
                Page = 1,
                PageSize = 2,
                SortBy = "DivName",
                Descending = false
            };

            // Act
            var result = await _repository.GetAllDivisionsPagedAsync(query);

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
            Assert.Equal(1, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_AppliesFiltering_ByDivName()
        {
            // Arrange
            var divisions = new List<Division>
            {
                new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1 },
                new Division { DivName = "VCJD", DivisionId = 2, AgencyId = 1 },
                new Division { DivName = "ACDP", DivisionId = 3, AgencyId = 2 }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);

            var query = new Core.Pagination.PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"DivName\":\"V\"}"
            };

            // Act
            var result = await _repository.GetAllDivisionsPagedAsync(query);

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, d => Assert.Contains("V", d.DivName));
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_AppliesSorting_ByDivisionId_Descending()
        {
            // Arrange
            var divisions = new List<Division>
            {
                new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1 },
                new Division { DivName = "ACDP", DivisionId = 3, AgencyId = 1 },
                new Division { DivName = "VCJD", DivisionId = 2, AgencyId = 2 }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);

            var query = new Core.Pagination.PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "DivisionId",
                Descending = true
            };

            // Act
            var result = await _repository.GetAllDivisionsPagedAsync(query);

            // Assert
            var dataList = result.Data.ToList();
            Assert.Equal(3, dataList[0].DivisionId); // Highest first
            Assert.Equal(2, dataList[1].DivisionId);
            Assert.Equal(1, dataList[2].DivisionId);
        }

        #endregion

        #region GetDivisionByNameAsync Tests

        [Fact]
        public async Task GetDivisionByNameAsync_ReturnsDiv_WhenExists()
        {
            // Arrange
            var divisions = new List<Division>
            {
                new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);

            // Act
            var result = await _repository.GetDivisionByNameAsync("VSD");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("VSD", result.DivName);
        }

        [Fact]
        public async Task GetDivisionByNameAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var divisions = new List<Division>().AsQueryable();
            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);

            // Act
            var result = await _repository.GetDivisionByNameAsync("NONEXISTENT");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDivisionByNameAsync_ReturnsNull_WhenDivNameIsEmpty()
        {
            // Act
            var result = await _repository.GetDivisionByNameAsync("");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region CreateDivisionAsync Tests

        [Fact]
        public async Task CreateDivisionAsync_AddsDivision_AndReturnsSavedEntity()
        {
            // Arrange
            var division = new Division { DivName = "NEW", DivisionId = 99, AgencyId = 2 };
            
            _mockDivisionSet.Setup(m => m.Add(It.IsAny<Division>()));
            _mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _repository.CreateDivisionAsync(division);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NEW", result.DivName);
            _mockDivisionSet.Verify(m => m.Add(It.Is<Division>(d => d.DivName == "NEW")), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task CreateDivisionAsync_ThrowsArgumentNullException_WhenDivisionIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _repository.CreateDivisionAsync(null!));
        }

        #endregion

        #region UpdateDivisionAsync Tests

        [Fact]
        public async Task UpdateDivisionAsync_UpdatesExistingDivision_WhenPrimaryKeyNotChanged()
        {
            // Arrange
            var originalDivision = new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1, CentOverhead = 100 };
            var updatedDivision = new Division { DivName = "VSD", DivisionId = 2, AgencyId = 2, CentOverhead = 200 };

            var divisions = new List<Division> { originalDivision }.AsQueryable();
            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);
            _mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _repository.UpdateDivisionAsync("VSD", updatedDivision);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.DivisionId);
            Assert.Equal(2, result.AgencyId);
            Assert.Equal(200, result.CentOverhead);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateDivisionAsync_DeletesAndCreates_WhenPrimaryKeyChanges()
        {
            // Arrange
            var originalDivision = new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1 };
            var updatedDivision = new Division { DivName = "NEWNAME", DivisionId = 1, AgencyId = 1 };

            var divisions = new List<Division> { originalDivision }.AsQueryable();
            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);
            _mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _repository.UpdateDivisionAsync("VSD", updatedDivision);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NEWNAME", result.DivName);
            mockSet.Verify(m => m.Remove(It.IsAny<Division>()), Times.Once);
            mockSet.Verify(m => m.Add(It.IsAny<Division>()), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdateDivisionAsync_ThrowsInvalidOperationException_WhenDivisionNotFound()
        {
            // Arrange
            var divisions = new List<Division>().AsQueryable();
            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);

            var updatedDivision = new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1 };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _repository.UpdateDivisionAsync("NONEXISTENT", updatedDivision));
        }

        #endregion

        #region DeleteDivisionAsync Tests

        [Fact]
        public async Task DeleteDivisionAsync_RemovesDivision_AndReturnsTrue()
        {
            // Arrange
            var division = new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1 };
            var divisions = new List<Division> { division }.AsQueryable();
            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);
            _mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _repository.DeleteDivisionAsync("VSD");

            // Assert
            Assert.True(result);
            mockSet.Verify(m => m.Remove(It.IsAny<Division>()), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteDivisionAsync_ReturnsFalse_WhenDivisionNotFound()
        {
            // Arrange
            var divisions = new List<Division>().AsQueryable();
            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);

            // Act
            var result = await _repository.DeleteDivisionAsync("NONEXISTENT");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteDivisionAsync_ReturnsFalse_WhenDivNameIsEmpty()
        {
            // Act
            var result = await _repository.DeleteDivisionAsync("");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region DivisionExistsAsync Tests

        [Fact]
        public async Task DivisionExistsAsync_ReturnsTrue_WhenDivisionExists()
        {
            // Arrange
            var divisions = new List<Division>
            {
                new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);

            // Act
            var result = await _repository.DivisionExistsAsync("VSD");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DivisionExistsAsync_ReturnsFalse_WhenDivisionDoesNotExist()
        {
            // Arrange
            var divisions = new List<Division>().AsQueryable();
            var mockSet = CreateMockDbSet(divisions);
            _mockContext.Setup(c => c.Divisions).Returns(mockSet.Object);

            // Act
            var result = await _repository.DivisionExistsAsync("NONEXISTENT");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetDivisionForeignKeyReferencesAsync Tests

        [Fact]
        public async Task GetDivisionForeignKeyReferencesAsync_ReturnsEmpty_WhenNoReferences()
        {
            // Arrange
            var profitCentres = new List<ProfitCentre>().AsQueryable();
            var divisionGrades = new List<DivisionGrade>().AsQueryable();

            var mockPcSet = CreateMockDbSet(profitCentres);
            var mockDgSet = CreateMockDbSet(divisionGrades);

            _mockContext.Setup(c => c.Set<ProfitCentre>()).Returns(mockPcSet.Object);
            _mockContext.Setup(c => c.Set<DivisionGrade>()).Returns(mockDgSet.Object);

            // Act
            var result = await _repository.GetDivisionForeignKeyReferencesAsync("VSD");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDivisionForeignKeyReferencesAsync_ReturnsProfitCentre_WhenReferenced()
        {
            // Arrange
            var profitCentres = new List<ProfitCentre>
            {
                new ProfitCentre { Division = "VSD" }
            }.AsQueryable();
            var divisionGrades = new List<DivisionGrade>().AsQueryable();

            var mockPcSet = CreateMockDbSet(profitCentres);
            var mockDgSet = CreateMockDbSet(divisionGrades);

            _mockContext.Setup(c => c.Set<ProfitCentre>()).Returns(mockPcSet.Object);
            _mockContext.Setup(c => c.Set<DivisionGrade>()).Returns(mockDgSet.Object);

            // Act
            var result = await _repository.GetDivisionForeignKeyReferencesAsync("VSD");

            // Assert
            Assert.Single(result);
            Assert.Contains("tblkpprofitcentre", result);
        }

        [Fact]
        public async Task GetDivisionForeignKeyReferencesAsync_ReturnsBothTables_WhenReferencedInBoth()
        {
            // Arrange
            var profitCentres = new List<ProfitCentre>
            {
                new ProfitCentre { Division = "VSD" }
            }.AsQueryable();
            var divisionGrades = new List<DivisionGrade>
            {
                new DivisionGrade { Division = "VSD" }
            }.AsQueryable();

            var mockPcSet = CreateMockDbSet(profitCentres);
            var mockDgSet = CreateMockDbSet(divisionGrades);

            _mockContext.Setup(c => c.Set<ProfitCentre>()).Returns(mockPcSet.Object);
            _mockContext.Setup(c => c.Set<DivisionGrade>()).Returns(mockDgSet.Object);

            // Act
            var result = await _repository.GetDivisionForeignKeyReferencesAsync("VSD");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("tblkpprofitcentre", result);
            Assert.Contains("divisiongrade", result);
        }

        #endregion

        #region Helper Methods

        private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            return data.ToList().BuildMockDbSet();
        }

        #endregion

        // Helper class for JSON parsing
        private class JsonResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
        }
    }
}
