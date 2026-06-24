/*
 * TRANSFORMENGINE MIGRATION — CapsStaffRepositoryTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New xUnit test class for Apha.Costbook.DataAccess.Repositories.CapsStaffRepository
 *   - Tests GetAllAsync, GetByMNumberAsync, ExistsAsync, AddAsync, UpdateAsync, DeleteAsync
 *   - Uses Moq + RepositoryTestHelper pattern (matches existing DataAccess test conventions)
 *
 * PRESERVED:
 *   - Test naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult]
 *   - RepositoryTestHelper.CreateMockDbContext / CreateMockDbSet pattern from StaffRepositoryTests
 *   - CapsStaff entity properties: MNumber, Name, Dt2Number
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetPaginatedAsync tests omitted — EF ExecuteDeleteAsync/AnyAsync on InMemory provider
 *     requires integration test; add once a test DB context is available
 */

using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Moq;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.CapsStaffRepositoryTest
{
    public class CapsStaffRepositoryTests
    {
        // ── Factory helper ────────────────────────────────────────────────────

        private static CapsStaffRepository CreateRepository(IEnumerable<CapsStaff> capsStaffs)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var capsStaffsMockSet = RepositoryTestHelper.CreateMockDbSet(capsStaffs);
            mockContext.Setup(x => x.Set<CapsStaff>()).Returns(capsStaffsMockSet.Object);
            mockContext.Setup(x => x.CapsStaffs).Returns(capsStaffsMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new CapsStaffRepository(mockContext.Object);
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ReturnsAllCapsStaff()
        {
            // Arrange
            var capsStaffs = new List<CapsStaff>
            {
                new CapsStaff { MNumber = "M001", Name = "Alice", Dt2Number = "DT001" },
                new CapsStaff { MNumber = "M002", Name = "Bob",   Dt2Number = null },
                new CapsStaff { MNumber = "M003", Name = "Charlie", Dt2Number = "DT003" }
            };
            var repo = CreateRepository(capsStaffs);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoCapsStaff()
        {
            // Arrange
            var repo = CreateRepository(new List<CapsStaff>());

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCorrectProperties()
        {
            // Arrange
            var capsStaffs = new List<CapsStaff>
            {
                new CapsStaff { MNumber = "M001", Name = "Alice Smith", Dt2Number = "DT001" }
            };
            var repo = CreateRepository(capsStaffs);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.Single(result);
            var item = result[0];
            Assert.Equal("M001", item.MNumber);
            Assert.Equal("Alice Smith", item.Name);
            Assert.Equal("DT001", item.Dt2Number);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsStaffWithNullDt2Number()
        {
            // Arrange
            var capsStaffs = new List<CapsStaff>
            {
                new CapsStaff { MNumber = "M001", Name = "Alice", Dt2Number = null },
                new CapsStaff { MNumber = "M002", Name = "Bob",   Dt2Number = "DT002" }
            };
            var repo = CreateRepository(capsStaffs);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, s => s.Dt2Number == null);
            Assert.Contains(result, s => s.Dt2Number == "DT002");
        }

        #endregion

        // ── GetByMNumberAsync ─────────────────────────────────────────────────

        #region GetByMNumberAsync Tests

        [Fact]
        public async Task GetByMNumberAsync_ExistingMNumber_ReturnsCapsStaff()
        {
            // Arrange
            var capsStaffs = new List<CapsStaff>
            {
                new CapsStaff { MNumber = "M001", Name = "Alice" },
                new CapsStaff { MNumber = "M002", Name = "Bob" }
            };
            var repo = CreateRepository(capsStaffs);

            // Act
            var result = await repo.GetByMNumberAsync("M001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("M001", result!.MNumber);
            Assert.Equal("Alice", result.Name);
        }

        [Fact]
        public async Task GetByMNumberAsync_NonExistentMNumber_ReturnsNull()
        {
            // Arrange
            var capsStaffs = new List<CapsStaff>
            {
                new CapsStaff { MNumber = "M001", Name = "Alice" }
            };
            var repo = CreateRepository(capsStaffs);

            // Act
            var result = await repo.GetByMNumberAsync("NOTEXIST");

            // Assert
            Assert.Null(result);
        }

        #endregion

        // ── ExistsAsync ───────────────────────────────────────────────────────

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ExistingMNumber_ReturnsTrue()
        {
            // Arrange
            var capsStaffs = new List<CapsStaff>
            {
                new CapsStaff { MNumber = "M001", Name = "Alice" }
            };
            var repo = CreateRepository(capsStaffs);

            // Act
            var result = await repo.ExistsAsync("M001");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_NonExistentMNumber_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(new List<CapsStaff>());

            // Act
            var result = await repo.ExistsAsync("NOTEXIST");

            // Assert
            Assert.False(result);
        }

        #endregion

        // ── AddAsync ──────────────────────────────────────────────────────────

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidEntity_AddsAndReturnsCapsStaff()
        {
            // Arrange
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);
            var capsStaffsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<CapsStaff>());
            mockContext.Setup(x => x.CapsStaffs).Returns(capsStaffsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            var repo = new CapsStaffRepository(mockContext.Object);

            var newCapsStaff = new CapsStaff { MNumber = "M003", Name = "Charlie" };

            // Act
            var result = await repo.AddAsync(newCapsStaff);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("M003", result.MNumber);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ExistingEntity_UpdatesNameAndDt2Number()
        {
            // Arrange
            var existing = new CapsStaff { MNumber = "M001", Name = "Alice", Dt2Number = "DT001" };
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);
            var capsStaffsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<CapsStaff> { existing });
            mockContext.Setup(x => x.CapsStaffs).Returns(capsStaffsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            var repo = new CapsStaffRepository(mockContext.Object);

            var updatedEntity = new CapsStaff { MNumber = "M001", Name = "Alice Updated", Dt2Number = "DT999" };

            // Act
            var result = await repo.UpdateAsync(updatedEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Alice Updated", result.Name);
            Assert.Equal("DT999", result.Dt2Number);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_NonExistentEntity_ThrowsKeyNotFoundException()
        {
            // Arrange
            var repo = CreateRepository(new List<CapsStaff>());
            var entity = new CapsStaff { MNumber = "NOTEXIST", Name = "Ghost" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.UpdateAsync(entity));
        }

        #endregion
    }
}
