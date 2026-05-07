using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Moq;
using Xunit;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.StaffRepositoryTest
{
    public class StaffRepositoryTests
    {
        private static StaffRepository CreateRepository(IEnumerable<Staff> staffs)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var staffsMockSet = RepositoryTestHelper.CreateMockDbSet(staffs);
            mockContext.Setup(x => x.Set<Staff>()).Returns(staffsMockSet.Object);
            mockContext.Setup(x => x.Staffs).Returns(staffsMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new StaffRepository(mockContext.Object);
        }

        [Fact]
        public async Task GetAllStaffAsync_ReturnsAllStaff()
        {
            // Arrange
            var staffs = new List<Staff>
            {
                new() { Mnumber = "M001", Name = "Alice" },
                new() { Mnumber = "M002", Name = "Bob" },
                new() { Mnumber = "M003", Name = "Charlie" }
            };
            var repo = CreateRepository(staffs);

            // Act
            var result = await repo.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllStaffAsync_ReturnsEmptyList_WhenNoStaff()
        {
            // Arrange
            var repo = CreateRepository(new List<Staff>());

            // Act
            var result = await repo.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllStaffAsync_ReturnsCorrectStaffProperties()
        {
            // Arrange
            var staffs = new List<Staff>
            {
                new() { Mnumber = "M001", Name = "Alice Smith", Dt2number = "DT001" }
            };
            var repo = CreateRepository(staffs);

            // Act
            var result = await repo.GetAllStaffAsync();

            // Assert
            Assert.Single(result);
            var staff = result[0];
            Assert.Equal("M001", staff.Mnumber);
            Assert.Equal("Alice Smith", staff.Name);
            Assert.Equal("DT001", staff.Dt2number);
        }

        [Fact]
        public async Task GetAllStaffAsync_ReturnsSingleStaff_WhenOnlyOneExists()
        {
            // Arrange
            var staffs = new List<Staff>
            {
                new() { Mnumber = "M001", Name = "Only Staff" }
            };
            var repo = CreateRepository(staffs);

            // Act
            var result = await repo.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("M001", result[0].Mnumber);
        }

        [Fact]
        public async Task GetAllStaffAsync_ReturnsStaffWithNullDt2Number()
        {
            // Arrange
            var staffs = new List<Staff>
            {
                new() { Mnumber = "M001", Name = "Staff A", Dt2number = null },
                new() { Mnumber = "M002", Name = "Staff B", Dt2number = "DT002" }
            };
            var repo = CreateRepository(staffs);

            // Act
            var result = await repo.GetAllStaffAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, s => s.Dt2number == null);
            Assert.Contains(result, s => s.Dt2number == "DT002");
        }
    }
}
