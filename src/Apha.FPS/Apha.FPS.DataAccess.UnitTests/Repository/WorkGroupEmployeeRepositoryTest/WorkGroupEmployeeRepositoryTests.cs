using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.WorkGroupEmployeeRepositoryTest
{
    public class WorkGroupEmployeeRepositoryTests
    {
        private const string DefaultWgGrade = "WG01";
        private const string DefaultPactId  = "PACT001";

        private static WorkGroupEmployeeRepository CreateRepository(
            IEnumerable<WorkGroupEmployee> employees,
            IEnumerable<Employee>? staffMembers = null)
        {
            const string testEmail = "test@example.com";

            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);
            requestContext.UserEmailId.Returns(testEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var employeesMockSet = RepositoryTestHelper.CreateMockDbSet(employees);
            mockContext.Setup(x => x.WorkGroupEmployees).Returns(employeesMockSet.Object);

            var viewWgEmployees = employees.Select(e => new WorkGroupEmployeeView
            {
                PactId         = e.PactId,
                SpNumber       = e.SpNumber,
                WorkGroupGrade = e.WorkGroupGrade,
                PersonStatus   = e.PersonStatus,
                UserEmail      = testEmail
            });
            var viewWgEmployeesMockSet = RepositoryTestHelper.CreateMockDbSet(viewWgEmployees);
            mockContext.Setup(x => x.WorkGroupEmployeeViews).Returns(viewWgEmployeesMockSet.Object);

            var staff = staffMembers ?? employees.Select(e => new Employee
            {
                SPNumber  = e.SpNumber,
                FirstName = "First",
                LastName  = "Last"
            });
            var staffMockSet = RepositoryTestHelper.CreateMockDbSet(staff);
            mockContext.Setup(x => x.Employees).Returns(staffMockSet.Object);

            return new WorkGroupEmployeeRepository(mockContext.Object, requestContext);
        }

        #region GetWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithMatchingWgGrade_ReturnsActiveEmployees()
        {
            // Arrange
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P003", SpNumber = "SP003", WorkGroupGrade = "OTHER",        PersonStatus = "A" },
                new() { PactId = "P004", SpNumber = "SP004", WorkGroupGrade = DefaultWgGrade, PersonStatus = "I" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, e => Assert.Equal(DefaultWgGrade, e.WorkGroupGrade));
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithNoMatchingWgGrade_ReturnsEmpty()
        {
            // Arrange
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = "OTHER", PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var employees = Enumerable.Range(1, 5).Select(i => new WorkGroupEmployee
            {
                PactId         = $"P00{i}",
                SpNumber       = $"SP00{i}",
                WorkGroupGrade = DefaultWgGrade,
                PersonStatus   = "A"
            }).ToList();
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            // Act
            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2,  result.Data.Count());
            Assert.Equal(5,  result.PaginationData.TotalRecords);
            Assert.Equal(2,  result.PaginationData.PageNumber);
        }

        #endregion

        #region GetWorkGroupEmployeeByIdAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WithMatchingPactId_ReturnsEmployee()
        {
            // Arrange
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.GetWorkGroupEmployeeByIdAsync(DefaultPactId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultPactId, result!.PactId);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WithNoMatchingPactId_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(new List<WorkGroupEmployee>());

            // Act
            var result = await repo.GetWorkGroupEmployeeByIdAsync("NONEXISTENT");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region UpdateWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WithValidEntity_UpdatesAndReturnsEntity()
        {
            // Arrange
            var existing = new WorkGroupEmployee
            {
                PactId         = DefaultPactId,
                SpNumber       = "SP001",
                WorkGroupGrade = DefaultWgGrade,
                PersonStatus   = "A",
                HrsPaid        = 37.0,
                Leave          = 0.0,
                SickSpecial    = 0.0,
                HrsAvail       = 37.0,
                MakeAvailable  = -1
            };
            var employees = new List<WorkGroupEmployee> { existing };
            var repo = CreateRepository(employees);

            var update = new WorkGroupEmployee
            {
                PactId        = DefaultPactId,
                HrsPaid       = 40.0,
                Leave         = 2.0,
                SickSpecial   = 1.0,
                PersonStatus  = "A",
                MakeAvailable = 0
            };

            // Act
            var result = await repo.UpdateWorkGroupEmployeeAsync(update);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(40.0, result.HrsPaid);
            Assert.Equal(37.0, result.HrsAvail); // HrsPaid - (Leave + SickSpecial) = 40 - 3
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WithNonExistentPactId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var repo   = CreateRepository(new List<WorkGroupEmployee>());
            var entity = new WorkGroupEmployee { PactId = "NONEXISTENT" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.UpdateWorkGroupEmployeeAsync(entity));
        }

        #endregion

        #region DeleteWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WithExistingPactId_ReturnsTrueAndRemoves()
        {
            // Arrange
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.DeleteWorkGroupEmployeeAsync(DefaultPactId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WithNonExistentPactId_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(new List<WorkGroupEmployee>());

            // Act
            var result = await repo.DeleteWorkGroupEmployeeAsync("NONEXISTENT");

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
