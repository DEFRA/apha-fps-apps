using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectRepositoryTest
{
    public class ProjectRepositoryTests
    {
        /// <summary>
        /// Creates a ProjectRepository with in-memory Projects and ProjectViews data.
        /// GetAllProjectsAsync() view/join logic with UserId = 42 filtering is covered by integration tests.
        /// </summary>
        private static ProjectRepository CreateRepository(
            IEnumerable<Project>? projects = null,
            IEnumerable<ProjectView>? projectViews = null)
        {
            var mockFpsYearContext = new Mock<IFpsYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            if (projects != null)
            {
                var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects);
                mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            }

            if (projectViews != null)
            {
                var projectViewsMockSet = RepositoryTestHelper.CreateMockDbSet(projectViews);
                mockContext.Setup(x => x.ProjectViews).Returns(projectViewsMockSet.Object);
            }

            return new ProjectRepository(mockContext.Object, mockFpsYearContext.Object);
        }

        #region GetAllProjectsAsync Tests

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsProjects_ForUserId42()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One",   Program = "P001", Customer = "DEFRA", UserId = 42 },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two",   Program = "P002", Customer = "APHA",  UserId = 42 },
                new() { ParentProject = "PP003", ProjectTitle = "Project Three", Program = "P003", Customer = "DEFRA", UserId = 99 } // different user — excluded
            };
            var repo = CreateRepository(projectViews: projectViews);

            // Act
            var result = await repo.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, p => Assert.NotNull(p.ParentProject));
        }

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsEmptyList_WhenNoProjectViews()
        {
            // Arrange
            var repo = CreateRepository(projectViews: []);

            // Act
            var result = await repo.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsEmptyList_WhenNoMatchingUserId()
        {
            // Arrange — all views belong to a different user
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One", UserId = 99 },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two", UserId = 99 }
            };
            var repo = CreateRepository(projectViews: projectViews);

            // Act
            var result = await repo.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProjectsAsync_MapsProgramViewFieldsCorrectly()
        {
            // Arrange
            var dateCreated  = new DateTime(2024, 1, 15);
            var dateCosted   = new DateTime(2024, 3, 10);
            var projectViews = new List<ProjectView>
            {
                new()
                {
                    ParentProject     = "PP001",
                    ProjectTitle      = "Alpha Project",
                    Program           = "P001",
                    Customer          = "DEFRA",
                    Manager           = "Alice",
                    TransferIncome    = 1000m,
                    CustIncome        = 2000m,
                    WipEoy            = 500m,
                    WipLimit          = 600m,
                    WipCurrent        = 450m,
                    ProjectStatus     = "Active",
                    CostBookNo        = "CB001",
                    DateCreated       = dateCreated,
                    FecCost           = 3000m,
                    Profit            = 100m,
                    BudgetCvl         = 200m,
                    DateCosted        = dateCosted,
                    Disease           = "D001",
                    Contract          = "C001",
                    ProjectParent     = "ROOT",
                    ShortTitle        = "Alpha",
                    CaseWorkSub       = 10m,
                    PvsIncome         = 50m,
                    PlanCaseWorkDebit = 20m,
                    Finished          = 0,
                    OwningRc          = "RC01",
                    Comments          = "Test comment",
                    CarryOver         = 300m,
                    CarryOverSeed     = 150m,
                    IsDefraProject    = 1,
                    CostCentre        = 9001.0,
                    OracleProjectCode = "ORA001",
                    SubAccountCode    = "SUB001",
                    ProjectGroup      = "GRP001",
                    IncomeAccountCode = "INC001",
                    FpsYear        = 2024,
                    UserId            = 42
                }
            };
            var repo = CreateRepository(projectViews: projectViews);

            // Act
            var result = await repo.GetAllProjectsAsync();

            // Assert
            var project = Assert.Single(result);
            Assert.Equal("PP001",        project.ParentProject);
            Assert.Equal("Alpha Project", project.ProjectTitle);
            Assert.Equal("P001",         project.Program);
            Assert.Equal("DEFRA",        project.Customer);
            Assert.Equal("Alice",        project.Manager);
            Assert.Equal(1000m,          project.TransferIncome);
            Assert.Equal(2000m,          project.CustIncome);
            Assert.Equal(500m,           project.WipEoy);
            Assert.Equal(600m,           project.WipLimit);
            Assert.Equal(450m,           project.WipCurrent);
            Assert.Equal("Active",       project.ProjectStatus);
            Assert.Equal("CB001",        project.CostBookNo);
            Assert.Equal(dateCreated,    project.DateCreated);
            Assert.Equal(3000m,          project.FecCost);
            Assert.Equal(100m,           project.Profit);
            Assert.Equal(200m,           project.BudgetCvl);
            Assert.Equal(dateCosted,     project.DateCosted);
            Assert.Equal("D001",         project.Disease);
            Assert.Equal("C001",         project.Contract);
            Assert.Equal("ROOT",         project.ProjectParent);
            Assert.Equal("Alpha",        project.ShortTitle);
            Assert.Equal(10m,            project.CaseWorkSub);
            Assert.Equal(50m,            project.PvsIncome);
            Assert.Equal(20m,            project.PlanCaseWorkDebit);
            Assert.Equal((short)0,       project.Finished);
            Assert.Equal("RC01",         project.OwningRc);
            Assert.Equal("Test comment", project.Comments);
            Assert.Equal(300m,           project.CarryOver);
            Assert.Equal(150m,           project.CarryOverSeed);
            Assert.Equal((short)1,       project.IsDefraProject);
            Assert.Equal(9001.0,         project.CostCentre);
            Assert.Equal("ORA001",       project.OracleProjectCode);
            Assert.Equal("SUB001",       project.SubAccountCode);
            Assert.Equal("GRP001",       project.ProjectGroup);
            Assert.Equal("INC001",       project.IncomeAccountCode);
            Assert.Equal(2024,           project.FpsYear);
        }

        [Fact]
        public async Task GetAllProjectsAsync_AppliesNullCoalescing_ForRequiredStringFields()
        {
            // Arrange — all nullable string fields are null on the view
            var projectViews = new List<ProjectView>
            {
                new()
                {
                    ParentProject     = null,
                    ProjectTitle      = null,
                    Program           = null,
                    Customer          = null,
                    Disease           = null,
                    Contract          = null,
                    IncomeAccountCode = null,
                    TransferIncome    = null,
                    CustIncome        = null,
                    IsDefraProject    = null,
                    UserId            = 42
                }
            };
            var repo = CreateRepository(projectViews: projectViews);

            // Act
            var result = await repo.GetAllProjectsAsync();

            // Assert — null-coalescing in the repository ensures no null reference on required fields
            var project = Assert.Single(result);
            Assert.Equal(string.Empty, project.ParentProject);
            Assert.Equal(string.Empty, project.ProjectTitle);
            Assert.Equal(string.Empty, project.Program);
            Assert.Equal(string.Empty, project.Customer);
            Assert.Equal(string.Empty, project.Disease);
            Assert.Equal(string.Empty, project.Contract);
            Assert.Equal(string.Empty, project.IncomeAccountCode);
            Assert.Equal(0m,           project.TransferIncome);
            Assert.Equal(0m,           project.CustIncome);
            Assert.Equal((short)0,     project.IsDefraProject);
        }

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsMultipleProjects_AllBelongingToUserId42()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One",   Program = "P001", Customer = "DEFRA", UserId = 42 },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two",   Program = "P002", Customer = "APHA",  UserId = 42 },
                new() { ParentProject = "PP003", ProjectTitle = "Project Three", Program = "P001", Customer = "DEFRA", UserId = 42 }
            };
            var repo = CreateRepository(projectViews: projectViews);

            // Act
            var result = await repo.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        #endregion

        #region GetProjectByIdAsync Tests

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsProject_WhenFound()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One",   Program = "P001", Customer = "DEFRA", ProjectStatus = "Active",   Disease = "D001", Contract = "C001", IncomeAccountCode = "INC001" },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two",   Program = "P002", Customer = "APHA",  ProjectStatus = "Inactive", Disease = "D002", Contract = "C002", IncomeAccountCode = "INC002" }
            };
            var repo = CreateRepository(projects: projects);

            // Act
            var result = await repo.GetProjectByIdAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PP001",       result.ParentProject);
            Assert.Equal("Project One", result.ProjectTitle);
            Assert.Equal("P001",        result.Program);
            Assert.Equal("DEFRA",       result.Customer);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D001", Contract = "C001", IncomeAccountCode = "INC001" }
            };
            var repo = CreateRepository(projects: projects);

            // Act
            var result = await repo.GetProjectByIdAsync("PP999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNull_WhenProjectsIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(projects: []);

            // Act
            var result = await repo.GetProjectByIdAsync("PP001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectByIdAsync_IsCaseSensitive()
        {
            // Arrange — match is on exact ParentProject string
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D001", Contract = "C001", IncomeAccountCode = "INC001" }
            };
            var repo = CreateRepository(projects: projects);

            // Act
            var result = await repo.GetProjectByIdAsync("pp001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsFirstMatch_WhenMultipleProjectsExist()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One",   Program = "P001", Customer = "DEFRA", ProjectStatus = "Active",   Disease = "D001", Contract = "C001", IncomeAccountCode = "INC001" },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two",   Program = "P002", Customer = "APHA",  ProjectStatus = "Inactive", Disease = "D002", Contract = "C002", IncomeAccountCode = "INC002" },
                new() { ParentProject = "PP003", ProjectTitle = "Project Three", Program = "P003", Customer = "DEFRA", ProjectStatus = "Active",   Disease = "D003", Contract = "C003", IncomeAccountCode = "INC003" }
            };
            var repo = CreateRepository(projects: projects);

            // Act
            var result = await repo.GetProjectByIdAsync("PP002");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PP002",       result.ParentProject);
            Assert.Equal("Project Two", result.ProjectTitle);
        }

        #endregion

        #region GetProjectsByProgramAsync Tests

        [Fact]
        public async Task GetProjectsByProgramAsync_ReturnsOnlyProjectsMatchingProgramAndUserId()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", BudgetCvl = 1000m, IsDefraProject = 1, UserId = 42 },
                new() { ParentProject = "PP002", ProjectTitle = "Beta",  Program = "P001", BudgetCvl = 2000m, IsDefraProject = 0, UserId = 42 },
                new() { ParentProject = "PP003", ProjectTitle = "Gamma", Program = "P002", BudgetCvl = 3000m, IsDefraProject = 0, UserId = 42 }, // different program
                new() { ParentProject = "PP004", ProjectTitle = "Delta", Program = "P001", BudgetCvl = 4000m, IsDefraProject = 0, UserId = 99 }, // different user
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Equal("P001", p.Program));
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_ReturnsEmpty_WhenNoProgramMatches()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", UserId = 42 }
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P999");

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_ReturnsEmpty_WhenNoMatchingUserId()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", UserId = 99 }
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_MapsFieldsCorrectly()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001", BudgetCvl = 1500m, IsDefraProject = 1, UserId = 42 }
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            var project = Assert.Single(result.Data);
            Assert.Equal("PP001",         project.ParentProject);
            Assert.Equal("Alpha Project", project.ProjectTitle);
            Assert.Equal("P001",          project.Program);
            Assert.Equal(1500m,           project.BudgetCvl);
            Assert.Equal((short)1,        project.IsDefraProject);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_AppliesNullCoalescing_ForNullableFields()
        {
            // Arrange — ParentProject, ProjectTitle and IsDefraProject are null; Program is set to match the filter
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = null, ProjectTitle = null, Program = "P001", IsDefraProject = null, BudgetCvl = null, UserId = 42 }
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            var project = Assert.Single(result.Data);
            Assert.Equal(string.Empty, project.ParentProject);
            Assert.Equal(string.Empty, project.ProjectTitle);
            Assert.Equal("P001",       project.Program);
            Assert.Equal((short)0,     project.IsDefraProject);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_FilterByJobCode_ReturnsMatchingProjects()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", UserId = 42 },
                new() { ParentProject = "PP002", ProjectTitle = "Beta",  Program = "P001", UserId = 42 },
                new() { ParentProject = "XY003", ProjectTitle = "Gamma", Program = "P001", UserId = 42 },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"JobCode\":\"PP\"}"
            };

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("PP", p.ParentProject));
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_FilterByJobDescription_ReturnsMatchingProjects()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "FMD Survey",     Program = "P001", UserId = 42 },
                new() { ParentProject = "PP002", ProjectTitle = "TB Eradication", Program = "P001", UserId = 42 },
                new() { ParentProject = "PP003", ProjectTitle = "FMD Outbreak",   Program = "P001", UserId = 42 },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"JobDescription\":\"FMD\"}"
            };

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("FMD", p.ProjectTitle));
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByParentProjectAscending_ByDefault()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "CC003", ProjectTitle = "Gamma", Program = "P001", UserId = 42 },
                new() { ParentProject = "AA001", ProjectTitle = "Alpha", Program = "P001", UserId = 42 },
                new() { ParentProject = "BB002", ProjectTitle = "Beta",  Program = "P001", UserId = 42 },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10); // SortBy = "" by default

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("AA001", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("CC003", items[2].ParentProject);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByParentProjectDescending_WhenDescendingIsTrue()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "AA001", ProjectTitle = "Alpha", Program = "P001", UserId = 42 },
                new() { ParentProject = "CC003", ProjectTitle = "Gamma", Program = "P001", UserId = 42 },
                new() { ParentProject = "BB002", ProjectTitle = "Beta",  Program = "P001", UserId = 42 },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "parentproject", descending: true, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("CC003", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("AA001", items[2].ParentProject);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByProjectTitleAscending()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP003", ProjectTitle = "Gamma Survey", Program = "P001", UserId = 42 },
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Survey", Program = "P001", UserId = 42 },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Survey",  Program = "P001", UserId = 42 },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "projecttitle", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("PP001", items[0].ParentProject); // Alpha
            Assert.Equal("PP002", items[1].ParentProject); // Beta
            Assert.Equal("PP003", items[2].ParentProject); // Gamma
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByBudgetCvlDescending()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", BudgetCvl = 500m,  UserId = 42 },
                new() { ParentProject = "PP002", ProjectTitle = "Beta",  Program = "P001", BudgetCvl = 1500m, UserId = 42 },
                new() { ParentProject = "PP003", ProjectTitle = "Gamma", Program = "P001", BudgetCvl = 1000m, UserId = 42 },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "budgetcvl", descending: true, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            var items = result.Data.ToList();
            Assert.Equal(1500m, items[0].BudgetCvl);
            Assert.Equal(1000m, items[1].BudgetCvl);
            Assert.Equal(500m,  items[2].BudgetCvl);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_AppliesPaging_ReturnsCorrectPage()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "AA001", Program = "P001", UserId = 42 },
                new() { ParentProject = "BB002", Program = "P001", UserId = 42 },
                new() { ParentProject = "CC003", Program = "P001", UserId = 42 },
                new() { ParentProject = "DD004", Program = "P001", UserId = 42 },
                new() { ParentProject = "EE005", Program = "P001", UserId = 42 },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 2, pageSize: 2);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(2, result.PaginationData.PageSize);
            Assert.Equal(3, result.PaginationData.TotalPages);
            Assert.Equal("CC003", result.Data.First().ParentProject);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_ReturnsPaginationMetadata_Correctly()
        {
            // Arrange
            var projectViews = Enumerable.Range(1, 15)
                .Select(i => new ProjectView
                {
                    ParentProject = $"PP{i:D3}",
                    ProjectTitle  = $"Project {i}",
                    Program       = "P001",
                    UserId        = 42
                }).ToList();
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(15, result.PaginationData.TotalRecords);
            Assert.Equal(10, result.Data.Count());
            Assert.Equal(2,  result.PaginationData.TotalPages);
        }

        #endregion
    }
}