using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.EmployeeRepositoryTest
{
    public class EmployeeRepositoryTests
    {
        /// <summary>
        /// Default test FPS year used across repository tests.
        /// </summary>
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a mocked IFpsYearContext with specified year.
        /// </summary>
        private static Mock<IFpsRequestContext> CreateMockFpsYearContext(int year = DefaultTestFpsYear)
        {
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            mockFpsYearContext.Setup(x => x.FpsYear).Returns(year);
            return mockFpsYearContext;
        }

        private static EmployeeRepository CreateRepository(
            IEnumerable<Employee> employees,
            IEnumerable<StaffActiveView>? staffActiveViews = null,
            IEnumerable<WorkgroupGradeGeneralView>? workgroupGrades = null,
            IEnumerable<WorkGroupEmployee>? wgEmployees = null,
            int fpsYear = DefaultTestFpsYear)
        {
            var mockFpsYearContext = CreateMockFpsYearContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            // Setup Employees DbSet
            var employeesMockSet = RepositoryTestHelper.CreateMockDbSet(employees);
            mockContext.Setup(x => x.Employees).Returns(employeesMockSet.Object);

            // Setup WgEmployees DbSet (for DeleteEmployeeAsync guard)
            var wgEmployeesMockSet = RepositoryTestHelper.CreateMockDbSet(wgEmployees ?? Enumerable.Empty<WorkGroupEmployee>());
            mockContext.Setup(x => x.WorkGroupEmployees).Returns(wgEmployeesMockSet.Object);

            // Setup StaffActiveView
            if (staffActiveViews != null)
            {
                var staffMockSet = RepositoryTestHelper.CreateMockDbSet(staffActiveViews);
                mockContext.Setup(x => x.StaffActiveView).Returns(staffMockSet.Object);
            }

            // Setup WorkgroupGradeGeneralView DbSet (for GetAllManagersAsync)
            if (workgroupGrades != null)
            {
                var gradeMockSet = RepositoryTestHelper.CreateMockDbSet(workgroupGrades);
                mockContext.Setup(x => x.WorkgroupGradeGeneralViews).Returns(gradeMockSet.Object);
            }

            return new EmployeeRepository(mockContext.Object, mockFpsYearContext.Object);
        }

        #region GetAllEmployeesAsync Tests

        [Fact]
        public async Task GetAllEmployeesAsync_ReturnsAllEmployees_OrderedBySPNumber()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP003", FirstName = "Charlie", LastName = "Brown", Title = "Manager" },
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith", Title = "Developer" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones", Title = "Analyst" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.GetAllEmployeesAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.Equal("SP001", resultList[0].SPNumber);
            Assert.Equal("SP002", resultList[1].SPNumber);
            Assert.Equal("SP003", resultList[2].SPNumber);
        }

        [Fact]
        public async Task GetAllEmployeesAsync_ReturnsEmptyList_WhenNoEmployees()
        {
            // Arrange
            var repo = CreateRepository(new List<Employee>());

            // Act
            var result = await repo.GetAllEmployeesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetEmployeesByPrefixAsync Tests

        [Fact]
        public async Task GetEmployeesByPrefixAsync_ReturnsFilteredEmployees_ByPrefix()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" },
                new() { SPNumber = "EMP001", FirstName = "Charlie", LastName = "Brown" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.GetEmployeesByPrefixAsync("SP");

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, e => Assert.StartsWith("SP", e.SPNumber));
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_ReturnsEmptyList_WhenNoPrefixMatch()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.GetEmployeesByPrefixAsync("EMP");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_ReturnsOrderedResults()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP003", FirstName = "Charlie", LastName = "Brown" },
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.GetEmployeesByPrefixAsync("SP");

            // Assert
            var resultList = result.ToList();
            Assert.Equal("SP001", resultList[0].SPNumber);
            Assert.Equal("SP002", resultList[1].SPNumber);
            Assert.Equal("SP003", resultList[2].SPNumber);
        }

        #endregion

        #region GetEmployeesByPrefixAsync with Pagination Tests

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithPagination_ReturnsPagedData()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith", Title = "Dev" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones", Title = "Analyst" },
                new() { SPNumber = "SP003", FirstName = "Charlie", LastName = "Brown", Title = "Manager" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 2,
                SortBy = "SPNumber",
                Descending = false
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
            Assert.Equal(1, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithFilter_FiltersBySPNumber()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" },
                new() { SPNumber = "SP003", FirstName = "Charlie", LastName = "Brown" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"SPNumber\":\"001\"}"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("SP001", result.Data.First().SPNumber);
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithFilter_FiltersByFirstName()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" },
                new() { SPNumber = "SP003", FirstName = "Alice", LastName = "Brown" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"FirstName\":\"Alice\"}"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, e => Assert.Contains("Alice", e.FirstName));
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithFilter_FiltersByLastName()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new Employee { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" },
                new Employee { SPNumber = "SP003", FirstName = "Charlie", LastName = "Smith" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"LastName\":\"Smith\"}"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, e => Assert.Contains("Smith", e.LastName));
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithFilter_FiltersByTitle()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith", Title = "Manager" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones", Title = "Developer" },
                new() { SPNumber = "SP003", FirstName = "Charlie", LastName = "Brown", Title = "Manager" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Title\":\"Manager\"}"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, e => Assert.Contains("Manager", e.Title));
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithMultipleFilters_FiltersCorrectly()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith", Title = "Manager" },
                new() { SPNumber = "SP002", FirstName = "Alice", LastName = "Jones", Title = "Developer" },
                new() { SPNumber = "SP003", FirstName = "Bob", LastName = "Smith", Title = "Manager" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"FirstName\":\"Alice\",\"LastName\":\"Smith\"}"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("SP001", result.Data.First().SPNumber);
        }

        [Theory]
        [InlineData("SPNumber", false, "SP001")]
        [InlineData("SPNumber", true, "SP003")]
        [InlineData("FirstName", false, "Alice")]
        [InlineData("FirstName", true, "Charlie")]
        [InlineData("LastName", false, "Brown")]
        [InlineData("LastName", true, "Smith")]
        [InlineData("Title", false, "Analyst")]
        [InlineData("Title", true, "Manager")]
        public async Task GetEmployeesByPrefixAsync_WithSorting_SortsCorrectly(
            string sortBy,
            bool descending,
            string expectedFirstValue)
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones", Title = "Developer" },
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith", Title = "Analyst" },
                new() { SPNumber = "SP003", FirstName = "Charlie", LastName = "Brown", Title = "Manager" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count());
            var firstEmployee = result.Data.First();
            string? actualValue = sortBy.ToLower() switch
            {
                "spnumber" => firstEmployee.SPNumber,
                "firstname" => firstEmployee.FirstName,
                "lastname" => firstEmployee.LastName,
                "title" => firstEmployee.Title,
                _ => firstEmployee.SPNumber
            };
            Assert.Equal(expectedFirstValue, actualValue);
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithNoSortBy_DefaultsToSPNumber()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP003", FirstName = "Charlie" },
                new() { SPNumber = "SP001", FirstName = "Alice" },
                new() { SPNumber = "SP002", FirstName = "Bob" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.Equal("SP001", result.Data.First().SPNumber);
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithInvalidSortBy_DefaultsToSPNumber()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP003", FirstName = "Charlie" },
                new() { SPNumber = "SP001", FirstName = "Alice" },
                new() { SPNumber = "SP002", FirstName = "Bob" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "InvalidProperty"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.Equal("SP001", result.Data.First().SPNumber);
        }

        #endregion

        #region GetEmployeeByIdAsync Tests

        [Fact]
        public async Task GetEmployeeByIdAsync_ReturnsEmployee_WhenFound()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.GetEmployeeByIdAsync("SP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("SP001", result.SPNumber);
            Assert.Equal("Alice", result.FirstName);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.GetEmployeeByIdAsync("SP999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_ReturnsNull_WhenEmployeesEmpty()
        {
            // Arrange
            var repo = CreateRepository(new List<Employee>());

            // Act
            var result = await repo.GetEmployeeByIdAsync("SP001");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddEmployeeAsync Tests

        [Fact]
        public async Task AddEmployeeAsync_AddsEmployee_WithFpsYear()
        {
            // Arrange
            var mockFpsYearContext = CreateMockFpsYearContext(2025);
            var (mockContext, employeesMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, Employee>(
                    new List<Employee>(),
                    mockFpsYearContext.Object);

            mockContext.Setup(x => x.Employees).Returns(employeesMockSet.Object);

            var repo = new EmployeeRepository(mockContext.Object, mockFpsYearContext.Object);
            var newEmployee = new Employee
            {
                SPNumber = "SP100",
                FirstName = "John",
                LastName = "Doe",
                Title = "Developer"
            };

            // Act
            var result = await repo.AddEmployeeAsync(newEmployee);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("SP100", result.SPNumber);
            Assert.Equal(2025, result.FpsYear);
            RepositoryTestHelper.VerifyAdd(employeesMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddEmployeeAsync_OverwritesFpsYear_WithContextYear()
        {
            // Arrange
            var mockFpsYearContext = CreateMockFpsYearContext(2026);
            var (mockContext, employeesMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, Employee>(
                    new List<Employee>(),
                    mockFpsYearContext.Object);

            mockContext.Setup(x => x.Employees).Returns(employeesMockSet.Object);

            var repo = new EmployeeRepository(mockContext.Object, mockFpsYearContext.Object);
            var newEmployee = new Employee
            {
                SPNumber = "SP101",
                FirstName = "Jane",
                LastName = "Doe",
                Title = "Manager",
                FpsYear = 2020 // Should be overwritten
            };

            // Act
            var result = await repo.AddEmployeeAsync(newEmployee);

            // Assert
            Assert.Equal(2026, result.FpsYear);
        }

        #endregion

        #region UpdateEmployeeAsync Tests

        [Fact]
        public async Task UpdateEmployeeAsync_UpdatesEmployee_WithFpsYear()
        {
            // Arrange
            var existingEmployee = new Employee
            {
                SPNumber = "SP001",
                FirstName = "Alice",
                LastName = "Smith",
                Title = "Developer",
                FpsYear = 2023
            };
            var employees = new List<Employee> { existingEmployee };

            var mockFpsYearContext = CreateMockFpsYearContext(2025);
            var (mockContext, employeesMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, Employee>(
                    employees,
                    mockFpsYearContext.Object);

            mockContext.Setup(x => x.Employees).Returns(employeesMockSet.Object);

            // Don't setup Entry - just verify it gets called and handle the exception
            var entryWasCalled = false;
            mockContext.Setup(x => x.Entry(It.IsAny<Employee>()))
                .Callback(() => entryWasCalled = true)
                .Throws(new NotSupportedException("Mocked DbContext does not support Entry()"));

            var repo = new EmployeeRepository(mockContext.Object, mockFpsYearContext.Object);
            var updatedEmployee = new Employee
            {
                SPNumber = "SP001",
                FirstName = "Alice Updated",
                LastName = "Smith Updated",
                Title = "Senior Developer"
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateEmployeeAsync(updatedEmployee));

            // Verify the FPS year was set before Entry was called
            Assert.Equal(2025, updatedEmployee.FpsYear);
            Assert.True(entryWasCalled);
        }

        #endregion

        #region DeleteEmployeeAsync Tests

        [Fact]
        public async Task DeleteEmployeeAsync_DeletesEmployee_WhenFound()
        {
            // Arrange
            var employee = new Employee
            {
                SPNumber = "SP001",
                FirstName = "Alice",
                LastName = "Smith",
                FpsYear = DefaultTestFpsYear
            };

            var mockFpsYearContext = CreateMockFpsYearContext(DefaultTestFpsYear);
            var (mockContext, employeesMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, Employee>(
                    new List<Employee> { employee },
                    mockFpsYearContext.Object);

            mockContext.Setup(x => x.Employees).Returns(employeesMockSet.Object);

            var wgEmployeesMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<WorkGroupEmployee>());
            mockContext.Setup(x => x.WorkGroupEmployees).Returns(wgEmployeesMockSet.Object);

            var repo = new EmployeeRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            var result = await repo.DeleteEmployeeAsync("SP001");

            // Assert
            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(employeesMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_ThrowsInvalidOperation_WhenNotFound()
        {
            // Arrange
            var repo = CreateRepository(new List<Employee>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.DeleteEmployeeAsync("SP999"));
        }

        [Fact]
        public async Task DeleteEmployeeAsync_ThrowsInvalidOperation_WhenFpsYearMismatch()
        {
            // Arrange
            var employee = new Employee
            {
                SPNumber = "SP001",
                FirstName = "Alice",
                LastName = "Smith",
                FpsYear = 2020 // Different year from context
            };
            var repo = CreateRepository(new List<Employee> { employee }, fpsYear: 2024);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.DeleteEmployeeAsync("SP001"));
        }

        [Fact]
        public async Task DeleteEmployeeAsync_ThrowsInvalidOperation_WhenLinkedWgEmployeeExists()
        {
            // Arrange
            var employee = new Employee
            {
                SPNumber = "SP001",
                FirstName = "Alice",
                LastName = "Smith",
                FpsYear = DefaultTestFpsYear
            };
            var linkedWgEmployee = new WorkGroupEmployee
            {
                SpNumber = "SP001",
                FpsYear = DefaultTestFpsYear
            };
            var repo = CreateRepository(
                new List<Employee> { employee },
                wgEmployees: new List<WorkGroupEmployee> { linkedWgEmployee });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.DeleteEmployeeAsync("SP001"));
            Assert.Contains("SP001", ex.Message);
        }

        #endregion

        #region GetAllManagersAsync Tests

        [Fact]
        public async Task GetAllManagersAsync_ReturnsManagers_WithValidGrades()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "John Manager", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = "Jane Director", WorkgroupGrade = "WG02" },
                new() { StaffID = "S003", Name = "General User", WorkgroupGrade = "WG03" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = "D01", WorkGroup = "Directors" },
                new() { WgGrade = "WG03", GradeCode = "G01", WorkGroup = "General" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count); // Excludes 'general' name and 'G' grade
            Assert.DoesNotContain(resultList, m => m.Name!.Contains("general", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(resultList, m => m.GradeCode!.StartsWith('G'));
        }

        [Fact]
        public async Task GetAllManagersAsync_ExcludesGeneralNames()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "John Manager", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = "General Staff", WorkgroupGrade = "WG02" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = "M02", WorkGroup = "Management" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("John Manager", resultList[0].Name);
        }

        [Fact]
        public async Task GetAllManagersAsync_ExcludesVacancyNames()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "John Manager", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = "Vacancy Position", WorkgroupGrade = "WG02" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = "M02", WorkGroup = "Management" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("John Manager", resultList[0].Name);
        }

        [Fact]
        public async Task GetAllManagersAsync_ExcludesGGrades()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "Manager One", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = "Manager Two", WorkgroupGrade = "WG02" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = "G01", WorkGroup = "General" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("Manager One", resultList[0].Name);
        }

        [Fact]
        public async Task GetAllManagersAsync_ReturnsOrderedByName()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "Charlie Manager", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = "Alice Manager", WorkgroupGrade = "WG02" },
                new() { StaffID = "S003", Name = "Bob Manager", WorkgroupGrade = "WG03" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = "M02", WorkGroup = "Management" },
                new() { WgGrade = "WG03", GradeCode = "M03", WorkGroup = "Management" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.Equal("Alice Manager", resultList[0].Name);
            Assert.Equal("Bob Manager", resultList[1].Name);
            Assert.Equal("Charlie Manager", resultList[2].Name);
        }

        [Fact]
        public async Task GetAllManagersAsync_SetsExpr1Property()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "Manager", WorkgroupGrade = "WG01" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("M", resultList[0].Expr1);
        }

        [Fact]
        public async Task GetAllManagersAsync_ReturnsEmpty_WhenNoValidData()
        {
            // Arrange
            var repo = CreateRepository(
                new List<Employee>(),
                new List<StaffActiveView>(),
                new List<WorkgroupGradeGeneralView>());

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllManagersAsync_ExcludesNullOrEmptyGradeCodes()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "Manager One", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = "Manager Two", WorkgroupGrade = "WG02" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = null, WorkGroup = "Management" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("Manager One", resultList[0].Name);
        }

        #endregion
    }
}
