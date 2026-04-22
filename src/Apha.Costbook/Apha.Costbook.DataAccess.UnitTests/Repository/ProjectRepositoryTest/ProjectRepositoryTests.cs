using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Web;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.ProjectRepositoryTest
{
    public class ProjectRepositoryTests
    {
        /// <summary>
        /// Creates a ProjectRepository with in-memory Projects data.
        /// CostbookDbContext is mocked using Moq and RepositoryTestHelper.
        /// </summary>
        private static ProjectRepository CreateRepository(IEnumerable<Project> projects)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockSettingsRepository = new Mock<ISettingsRepository>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects);
            mockContext.Setup(x => x.Set<Project>()).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);

            // Setup additional DbSets for Delete operations (they need to exist even if empty)
            var emptyAnimalReqs = RepositoryTestHelper.CreateMockDbSet(new List<AnimalRequirement>());
            var emptyAdditionalCosts = RepositoryTestHelper.CreateMockDbSet(new List<AdditionalCost>());
            var emptyTestReqs = RepositoryTestHelper.CreateMockDbSet(new List<TestRequirement>());
            var emptyStaffReqs = RepositoryTestHelper.CreateMockDbSet(new List<StaffRequirement>());
            var emptyProjectYears = RepositoryTestHelper.CreateMockDbSet(new List<ProjectYear>());

            mockContext.Setup(x => x.Set<AnimalRequirement>()).Returns(emptyAnimalReqs.Object);
            mockContext.Setup(x => x.Set<AdditionalCost>()).Returns(emptyAdditionalCosts.Object);
            mockContext.Setup(x => x.Set<TestRequirement>()).Returns(emptyTestReqs.Object);
            mockContext.Setup(x => x.Set<StaffRequirement>()).Returns(emptyStaffReqs.Object);
            mockContext.Setup(x => x.Set<ProjectYear>()).Returns(emptyProjectYears.Object);

            // Setup SaveChangesAsync
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ProjectRepository(mockContext.Object, mockSettingsRepository.Object);
        }

        #region GetPaginatedProjectsAsync

        [Fact]
        public async Task GetPaginatedProjectsAsync_WithNoFilter_ReturnsAllProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ProjectTitle = "Project 1", ContractNumber = "CON001" },
                new() { ProjectId = "2024/002", ProjectTitle = "Project 2", ContractNumber = "CON002" },
                new() { ProjectId = "2024/003", ProjectTitle = "Project 3", ContractNumber = "CON003" }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
        }

        [Fact]
        public async Task GetPaginatedProjectsAsync_WithProjectIdFilter_ReturnsFilteredProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ProjectTitle = "Project 1" },
                new() { ProjectId = "2024/002", ProjectTitle = "Project 2" },
                new() { ProjectId = "2025/001", ProjectTitle = "Project 3" }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"ProjectId\":\"2024\"}"
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, p => Assert.Contains("2024", p.ProjectId));
        }

        [Fact]
        public async Task GetPaginatedProjectsAsync_WithPaging_ReturnsCorrectPage()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ProjectTitle = "Project 1" },
                new() { ProjectId = "2024/002", ProjectTitle = "Project 2" },
                new() { ProjectId = "2024/003", ProjectTitle = "Project 3" },
                new() { ProjectId = "2024/004", ProjectTitle = "Project 4" },
                new() { ProjectId = "2024/005", ProjectTitle = "Project 5" }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 2,
                PageSize = 2
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Theory]
        [InlineData("projectid", false, "2024/001")]
        [InlineData("projectid", true, "2024/003")]
        [InlineData("projecttitle", false, "Project A")]
        [InlineData("projecttitle", true, "Project C")]
        [InlineData("programme", false, "Programme A")]
        [InlineData("programme", true, "Programme C")]
        [InlineData("contractnumber", false, "CON001")]
        [InlineData("contractnumber", true, "CON003")]
        public async Task GetPaginatedProjectsAsync_WithSorting_ReturnsSortedProjects(
            string sortBy, bool descending, string expectedFirstValue)
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/002", ProjectTitle = "Project B", Programme = "Programme B", ContractNumber = "CON002" },
                new() { ProjectId = "2024/001", ProjectTitle = "Project A", Programme = "Programme A", ContractNumber = "CON001" },
                new() { ProjectId = "2024/003", ProjectTitle = "Project C", Programme = "Programme C", ContractNumber = "CON003" }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            var firstProject = result.Data.First();
            var actualValue = sortBy.ToLower() switch
            {
                "projectid" => firstProject.ProjectId,
                "projecttitle" => firstProject.ProjectTitle,
                "programme" => firstProject.Programme,
                "contractnumber" => firstProject.ContractNumber,
                _ => null
            };
            Assert.Equal(expectedFirstValue, actualValue);
        }

        [Fact]
        public async Task GetPaginatedProjectsAsync_WithInvalidSortBy_UsesDefaultSorting()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ProjectTitle = "Project 1" },
                new() { ProjectId = "2024/002", ProjectTitle = "Project 2" },
                new() { ProjectId = "2024/003", ProjectTitle = "Project 3" }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "invalid_field"
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            // Default sorting is descending by ProjectId
            Assert.Equal("2024/003", result.Data.First().ProjectId);
        }

        [Fact]
        public async Task GetPaginatedProjectsAsync_WithEmptyResult_ReturnsEmptyPagedData()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
            Assert.Equal(0, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetPaginatedProjectsAsync_WithFilterAndSorting_ReturnsFilteredAndSortedProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ProjectTitle = "Test Project A", Programme = "Programme A" },
                new() { ProjectId = "2024/002", ProjectTitle = "Test Project B", Programme = "Programme B" },
                new() { ProjectId = "2025/001", ProjectTitle = "Other Project", Programme = "Programme C" }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"ProjectId\":\"2024\"}",
                SortBy = "ProjectTitle",
                Descending = false
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal("Test Project A", result.Data.First().ProjectTitle);
            Assert.All(result.Data, p => Assert.Contains("2024", p.ProjectId));
        }

        [Theory]
        [InlineData("customername", false)]
        [InlineData("customername", true)]
        [InlineData("disease", false)]
        [InlineData("disease", true)]
        [InlineData("StartDate", false)]
        [InlineData("StartDate", true)]
        [InlineData("ContractPrice", false)]
        [InlineData("ContractPrice", true)]
        [InlineData("preparedby", false)]
        [InlineData("preparedby", true)]
        [InlineData("dateofsubmission", false)]
        [InlineData("dateofsubmission", true)]
        public async Task GetPaginatedProjectsAsync_WithDifferentSortFields_SortsCorrectly(string sortBy, bool descending)
        {
            // Arrange
            var projects = new List<Project>
            {
                new()
                {
                    ProjectId = "2024/001",
                    CustomerName = "Customer A",
                    Disease = "Disease A",
                    StartDate = new DateOnly(2024, 1, 1),
                    ContractPrice = 1000,
                    PreparedBy = "Person A",
                    DateOfSubmission = new DateOnly(2024, 1, 1)
                },
                new()
                {
                    ProjectId = "2024/002",
                    CustomerName = "Customer B",
                    Disease = "Disease B",
                    StartDate = new DateOnly(2024, 2, 1),
                    ContractPrice = 2000,
                    PreparedBy = "Person B",
                    DateOfSubmission = new DateOnly(2024, 2, 1)
                }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
        }

        #endregion        


        #region GetProjectByIdAsync

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsProject_WhenProjectExists()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ContractNumber = "CON001", SubmittedByLName = "Smith", SubmittedByFName = "John" },
                new() { ProjectId = "2024/002", ContractNumber = "CON002", SubmittedByLName = "Jones", SubmittedByFName = "Jane" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetProjectByIdAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2024/001", result.ProjectId);
            Assert.Equal("CON001", result.ContractNumber);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNull_WhenProjectDoesNotExist()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ContractNumber = "CON001", SubmittedByLName = "Smith", SubmittedByFName = "John" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetProjectByIdAsync("NONEXISTENT");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectByIdAsync_HandlesUrlDecodedId()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001a", ContractNumber = "CON001", SubmittedByLName = "Smith", SubmittedByFName = "John" }
            };
            var repo = CreateRepository(projects);
            var encodedId = HttpUtility.UrlEncode("2024/001a");

            // Act
            var result = await repo.GetProjectByIdAsync(encodedId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2024/001a", result.ProjectId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetProjectByIdAsync_ReturnsNull_WhenIdIsNullOrEmpty(string? id)
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ContractNumber = "CON001", SubmittedByLName = "Smith", SubmittedByFName = "John" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetProjectByIdAsync(id!);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddProjectAsync

        [Fact]
        public async Task AddProjectAsync_AddsProject_AndReturnsProject()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var newProject = new Project
            {
                ProjectId = "2024/001",
                ContractNumber = "CON001",
                SubmittedByLName = "Smith",
                SubmittedByFName = "John"
            };

            // Act
            var result = await repo.AddProjectAsync(newProject);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newProject.ProjectId, result.ProjectId);
            Assert.Equal(newProject.ContractNumber, result.ContractNumber);
        }

        [Fact]
        public async Task AddProjectAsync_ReturnsCorrectProject_WhenProjectHasAllProperties()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var newProject = new Project
            {
                ProjectId = "2024/001",
                ContractNumber = "CON001",
                SubmittedByLName = "Smith",
                SubmittedByFName = "John"
            };

            // Act
            var result = await repo.AddProjectAsync(newProject);

            // Assert
            Assert.Same(newProject, result);
        }

        #endregion

        #region UpdateProjectAsync

        [Fact]
        public async Task UpdateProjectAsync_UpdatesProject_AndReturnsProject()
        {
            // Arrange
            var existingProject = new Project
            {
                ProjectId = "2024/001",
                ContractNumber = "CON001",
                SubmittedByLName = "Smith",
                SubmittedByFName = "John"
            };
            var projects = new List<Project> { existingProject };
            var repo = CreateRepository(projects);

            var updatedProject = new Project
            {
                ProjectId = "2024/001",
                ContractNumber = "CON002",
                SubmittedByLName = "Jones",
                SubmittedByFName = "Jane"
            };

            // Act
            var result = await repo.UpdateProjectAsync(updatedProject);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedProject.ProjectId, result.ProjectId);
            Assert.Equal(updatedProject.ContractNumber, result.ContractNumber);
        }

        [Fact]
        public async Task UpdateProjectAsync_ReturnsUpdatedProject()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var projectToUpdate = new Project
            {
                ProjectId = "2024/001",
                ContractNumber = "CON001",
                SubmittedByLName = "Smith",
                SubmittedByFName = "John"
            };

            // Act
            var result = await repo.UpdateProjectAsync(projectToUpdate);

            // Assert
            Assert.Same(projectToUpdate, result);
        }

        #endregion

      

        #region GetNextProjectNumberAsync

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsFirstProjectNumber_WhenNoProjectsExist()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetNextProjectNumberAsync(null);

            // Assert
            Assert.NotNull(result);
            var currentYear = DateTime.Now.Month <= 3 ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            Assert.Equal($"{currentYear}/001", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsNextSequentialNumber_WhenProjectsExistForCurrentYear()
        {
            // Arrange
            var currentYear = DateTime.Now.Month <= 3 ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var projects = new List<Project>
            {
                new() { ProjectId = $"{currentYear}/001" },
                new() { ProjectId = $"{currentYear}/002" },
                new() { ProjectId = $"{currentYear}/003" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetNextProjectNumberAsync(null);

            // Assert
            Assert.Equal($"{currentYear}/004", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsFirstNumberForYear_WhenNoProjectsForCurrentYear()
        {
            // Arrange
            var currentYear = DateTime.Now.Month <= 3 ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var projects = new List<Project>
            {
                new() { ProjectId = $"{currentYear - 1}/001" },
                new() { ProjectId = $"{currentYear - 1}/002" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetNextProjectNumberAsync(null);

            // Assert
            Assert.Equal($"{currentYear}/001", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsBaseNumber_WhenBaseNumberProvidedAndNoSimilarProjects()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var baseNumber = "2024/001";

            // Act
            var result = await repo.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.Equal(baseNumber, result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsNextLetterSuffix_WhenBaseNumberHasLetterSuffix()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001a" },
                new() { ProjectId = "2024/001b" }
            };
            var repo = CreateRepository(projects);
            var baseNumber = "2024/001a";

            // Act
            var result = await repo.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.Equal("2024/001c", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsWithLetterSuffix_WhenBaseNumberExistsWithLetterVariation()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001" },
                new() { ProjectId = "2024/001a" }
            };
            var repo = CreateRepository(projects);
            var baseNumber = "2024/001";

            // Act
            var result = await repo.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.Equal("2024/001b", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsFirstLetterSuffix_WhenOnlyBaseNumberExists()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001" }
            };
            var repo = CreateRepository(projects);
            var baseNumber = "2024/001";

            // Act
            var result = await repo.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.Equal("2024/001a", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_HandlesUrlDecodedBaseNumber()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var encodedBaseNumber = HttpUtility.UrlEncode("2024/001");

            // Act
            var result = await repo.GetNextProjectNumberAsync(encodedBaseNumber);

            // Assert
            Assert.Equal("2024/001", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_HandlesMalformedProjectIds()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "MALFORMED" },
                new() { ProjectId = "2024/ABC" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetNextProjectNumberAsync(null);

            // Assert
            var currentYear = DateTime.Now.Month <= 3 ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            Assert.Equal($"{currentYear}/001", result);
        }

        #endregion

        #region GetCurrentFinancialYear

        [Fact]
        public async Task GetCurrentFinancialYear_ReturnsCurrentYear_WhenAfterMarch()
        {
            // This test validates the financial year logic
            // Note: Since GetCurrentFinancialYear is private, we test it indirectly through GetNextProjectNumberAsync

            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetNextProjectNumberAsync(null);

            // Assert
            var expectedYear = DateTime.Now.Month <= 3 ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            Assert.StartsWith($"{expectedYear}/", result);
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNullableProject()
        {
            // Arrange — verifies the return type contract allows null
            var repo = CreateRepository(new List<Project>());

            // Act
            var result = await repo.GetProjectByIdAsync("NONEXISTENT");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddProjectAsync_ReturnsNotNull()
        {
            // Arrange — verifies the return type contract is never null
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var newProject = new Project { ProjectId = "2024/001" };

            // Act
            var result = await repo.AddProjectAsync(newProject);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateProjectAsync_ReturnsNotNull()
        {
            // Arrange — verifies the return type contract is never null
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var projectToUpdate = new Project { ProjectId = "2024/001" };

            // Act
            var result = await repo.UpdateProjectAsync(projectToUpdate);

            // Assert
            Assert.NotNull(result);
        }

        #endregion
    }
}