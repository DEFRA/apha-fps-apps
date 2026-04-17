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
        /// IFpsRequestContext is substituted via NSubstitute.
        /// GetProjectsByProgramAsync() join/sort logic is covered by integration tests.
        /// </summary>
        private static ProjectRepository CreateRepository(
            IEnumerable<Project>? projects = null,
            IEnumerable<ProjectView>? projectViews = null,
            IEnumerable<PactProjectView>? pactProjectViews = null,
            string userEmailId = "test@example.com",
            int fpsYear = 2024) // always lowercase â€” matches middleware ToLowerInvariant()
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.UserEmailId).Returns(userEmailId);
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

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

            if (pactProjectViews != null)
            {
                var pactProjectViewsMockSet = RepositoryTestHelper.CreateMockDbSet(pactProjectViews);
                mockContext.Setup(x => x.PactProjectViews).Returns(pactProjectViewsMockSet.Object);
            }

            return new ProjectRepository(mockContext.Object, mockRequestContext.Object);
        }

        #region GetAllProjectsAsync Tests

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsProjects_ForUserId42()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One",   Program = "P001", Customer = "DEFRA", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two",   Program = "P002", Customer = "APHA",  UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", ProjectTitle = "Project Three", Program = "P003", Customer = "DEFRA", UserEmail = "other@example.com" } // different user â€” excluded
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
        public async Task GetAllProjectsAsync_ReturnsEmptyList_WhenNoMatchingUserEmail()
        {
            // Arrange â€” all views belong to a different user
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One", UserEmail = "other@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two", UserEmail = "other@example.com" }
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
                    UserEmail         = "test@example.com"
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
        public async Task GetAllProjectsAsync_PreservesNullValues_ForNullableFields()
        {
            // Arrange â€” all nullable fields are null; GetAllProjectsAsync returns ProjectView as-is (no projection)
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
                    UserEmail         = "test@example.com"
                }
            };
            var repo = CreateRepository(projectViews: projectViews);

            // Act
            var result = await repo.GetAllProjectsAsync();

            // Assert â€” ProjectView is returned as-is; null values are preserved
            var project = Assert.Single(result);
            Assert.Null(project.ParentProject);
            Assert.Null(project.ProjectTitle);
            Assert.Null(project.Program);
            Assert.Null(project.Customer);
            Assert.Null(project.Disease);
            Assert.Null(project.Contract);
            Assert.Null(project.IncomeAccountCode);
            Assert.Null(project.TransferIncome);
            Assert.Null(project.CustIncome);
            Assert.Null(project.IsDefraProject);
        }

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsMultipleProjects_AllBelongingToUserId42()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One",   Program = "P001", Customer = "DEFRA", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two",   Program = "P002", Customer = "APHA",  UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", ProjectTitle = "Project Three", Program = "P001", Customer = "DEFRA", UserEmail = "test@example.com" }
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
            // Arrange â€” match is on exact ParentProject string
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
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", BudgetCvl = 1000m, IsDefraProject = 1, UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta",  Program = "P001", BudgetCvl = 2000m, IsDefraProject = 0, UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", ProjectTitle = "Gamma", Program = "P002", BudgetCvl = 3000m, IsDefraProject = 0, UserEmail = "test@example.com" }, // different program
                new() { ParentProject = "PP004", ProjectTitle = "Delta", Program = "P001", BudgetCvl = 4000m, IsDefraProject = 0, UserEmail = "other@example.com" }, // different user
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
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", UserEmail = "test@example.com" }
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
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", UserEmail = "other@example.com" }
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
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001", BudgetCvl = 1500m, IsDefraProject = 1, UserEmail = "test@example.com" }
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
            // Arrange â€” ParentProject, ProjectTitle and IsDefraProject are null; Program is set to match the filter
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = null, ProjectTitle = null, Program = "P001", IsDefraProject = null, BudgetCvl = null, UserEmail = "test@example.com" }
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
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta",  Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "XY003", ProjectTitle = "Gamma", Program = "P001", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"ParentProject\":\"PP\"}"
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
                new() { ParentProject = "PP001", ProjectTitle = "FMD Survey",     Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "TB Eradication", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", ProjectTitle = "FMD Outbreak",   Program = "P001", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"ProjectTitle\":\"FMD\"}"
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
                new() { ParentProject = "CC003", ProjectTitle = "Gamma", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "AA001", ProjectTitle = "Alpha", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta",  Program = "P001", UserEmail = "test@example.com" },
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
                new() { ParentProject = "AA001", ProjectTitle = "Alpha", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "CC003", ProjectTitle = "Gamma", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta",  Program = "P001", UserEmail = "test@example.com" },
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
                new() { ParentProject = "PP003", ProjectTitle = "Gamma Survey", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Survey", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Survey",  Program = "P001", UserEmail = "test@example.com" },
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
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", BudgetCvl = 500m,  UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta",  Program = "P001", BudgetCvl = 1500m, UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", ProjectTitle = "Gamma", Program = "P001", BudgetCvl = 1000m, UserEmail = "test@example.com" },
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
                new() { ParentProject = "AA001", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "BB002", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "CC003", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "DD004", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "EE005", Program = "P001", UserEmail = "test@example.com" },
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
                    UserEmail     = "test@example.com"
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

        #region GetAllPactProjectsAsync Tests

        [Fact]
        public async Task GetAllPactProjectsAsync_ReturnsAllViews_OrderedByParentProject()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "CC003", ProjectTitle = "Gamma Survey" },
                new() { ParentProject = "AA001", ProjectTitle = "Alpha Survey" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta Survey"  }
            };
            var repo = CreateRepository(pactProjectViews: pactViews);

            // Act
            var result = await repo.GetAllPactProjectsAsync();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(3, list.Count);
            Assert.Equal("AA001", list[0].ParentProject);
            Assert.Equal("BB002", list[1].ParentProject);
            Assert.Equal("CC003", list[2].ParentProject);
        }

        [Fact]
        public async Task GetAllPactProjectsAsync_ReturnsEmpty_WhenNoPactProjectsExist()
        {
            // Arrange
            var repo = CreateRepository(pactProjectViews: []);

            // Act
            var result = await repo.GetAllPactProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetPagedProjectsAsync Tests

        [Fact]
        public async Task GetPagedProjectsAsync_ReturnsAllProjects_WithDefaultSorting()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "BB002", ProjectTitle = "Beta Project",  Program = "P001", Customer = "APHA",  ProjectStatus = "Active", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1" },
                new() { ParentProject = "AA001", ProjectTitle = "Alpha Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1" }
            };
            var repo = CreateRepository(projects: projects);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.Equal("AA001", result.Data.First().ParentProject);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_FiltersResults_WhenSearchTermProvided()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "FMD Survey",     Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1" },
                new() { ParentProject = "PP002", ProjectTitle = "TB Eradication", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D2", Contract = "C2", IncomeAccountCode = "I2" },
                new() { ParentProject = "PP003", ProjectTitle = "FMD Outbreak",   Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D3", Contract = "C3", IncomeAccountCode = "I3" }
            };
            var repo = CreateRepository(projects: projects);
            var query = new PaginationParameters<string>(search: "fmd", page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("FMD", p.ProjectTitle, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedProjectsAsync_ReturnsEmpty_WhenNoProjectsMatchSearch()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1" }
            };
            var repo = CreateRepository(projects: projects);
            var query = new PaginationParameters<string>(search: "NOMATCH", page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_SortsByProjectTitle_Ascending()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "PP003", ProjectTitle = "Gamma Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1" },
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project",  Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1" }
            };
            var repo = CreateRepository(projects: projects);
            var query = new PaginationParameters<string>(sortBy: "projecttitle", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("PP001", items[0].ParentProject); // Alpha
            Assert.Equal("PP002", items[1].ParentProject); // Beta
            Assert.Equal("PP003", items[2].ParentProject); // Gamma
        }

        [Fact]
        public async Task GetPagedProjectsAsync_SortsByParentProject_Descending()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "AA001", ProjectTitle = "Alpha", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1" },
                new() { ParentProject = "CC003", ProjectTitle = "Gamma", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta",  Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1" }
            };
            var repo = CreateRepository(projects: projects);
            var query = new PaginationParameters<string>(sortBy: "parentproject", descending: true, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("CC003", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("AA001", items[2].ParentProject);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_AppliesPaging_ReturnsCorrectPage()
        {
            // Arrange
            var projects = Enumerable.Range(1, 10)
                .Select(i => new Project
                {
                    ParentProject     = $"PP{i:D3}",
                    ProjectTitle      = $"Project {i}",
                    Program           = "P001",
                    Customer          = "DEFRA",
                    ProjectStatus     = "Active",
                    Disease           = "D1",
                    Contract          = "C1",
                    IncomeAccountCode = "I1"
                }).ToList();
            var repo = CreateRepository(projects: projects);
            var query = new PaginationParameters<string>(page: 2, pageSize: 3);

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            Assert.Equal(3,     result.Data.Count());
            Assert.Equal(10,    result.PaginationData.TotalRecords);
            Assert.Equal(2,     result.PaginationData.PageNumber);
            Assert.Equal(4,     result.PaginationData.TotalPages);
            Assert.Equal("PP004", result.Data.First().ParentProject);
        }

        #endregion

        #region GetPagedPactProjectsAsync Tests

        [Fact]
        public async Task GetPagedPactProjectsAsync_ReturnsAllPactProjects_WithDefaultSorting()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "CC003", ProjectTitle = "Gamma" },
                new() { ParentProject = "AA001", ProjectTitle = "Alpha" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta"  }
            };
            var repo = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.Equal(3, result.PaginationData.TotalRecords);
            var items = result.Data.ToList();
            Assert.Equal("AA001", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("CC003", items[2].ParentProject);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_FiltersByParentProject_WhenFilterProvided()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta"  },
                new() { ParentProject = "XY003", ProjectTitle = "Gamma" }
            };
            var repo = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"ParentProject\":\"PP\"}"
            };

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("PP", p.ParentProject));
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_FiltersByProjectTitle_WhenFilterProvided()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "FMD Survey"     },
                new() { ParentProject = "PP002", ProjectTitle = "TB Eradication" },
                new() { ParentProject = "PP003", ProjectTitle = "FMD Outbreak"   }
            };
            var repo = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"ProjectTitle\":\"FMD\"}"
            };

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("FMD", p.ProjectTitle));
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_SortsByParentProject_Descending()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "AA001", ProjectTitle = "Alpha" },
                new() { ParentProject = "CC003", ProjectTitle = "Gamma" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta"  }
            };
            var repo = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(sortBy: "parentproject", descending: true, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("CC003", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("AA001", items[2].ParentProject);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_SortsByProjectTitle_Ascending()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP003", ProjectTitle = "Gamma" },
                new() { ParentProject = "PP001", ProjectTitle = "Alpha" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta"  }
            };
            var repo = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(sortBy: "projecttitle", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("PP001", items[0].ParentProject); // Alpha
            Assert.Equal("PP002", items[1].ParentProject); // Beta
            Assert.Equal("PP003", items[2].ParentProject); // Gamma
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_AppliesPaging_ReturnsCorrectPage()
        {
            // Arrange
            var pactViews = Enumerable.Range(1, 8)
                .Select(i => new PactProjectView
                {
                    ParentProject = $"PP{i:D3}",
                    ProjectTitle  = $"PACT Project {i}"
                }).ToList();
            var repo = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(page: 2, pageSize: 3);

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.Equal(3,      result.Data.Count());
            Assert.Equal(8,      result.PaginationData.TotalRecords);
            Assert.Equal(2,      result.PaginationData.PageNumber);
            Assert.Equal("PP004", result.Data.First().ParentProject);
        }

        #endregion

        #region CreateProjectAsync Tests

        [Fact]
        public async Task CreateProjectAsync_SetsCurrentFpsYear_OnProject()
        {
            // Arrange
            const int fpsYear = 2025;
            var repo = CreateRepository(projects: [], fpsYear: fpsYear);
            var project = new Project
            {
                ParentProject = "PP001", ProjectTitle = "New Project",
                Program = "P001", Customer = "DEFRA", ProjectStatus = "Active",
                Disease = "D1", Contract = "C1", IncomeAccountCode = "I1"
            };

            // Act
            var result = await repo.CreateProjectAsync(project);

            // Assert
            Assert.Equal(fpsYear, result.FpsYear);
        }

        [Fact]
        public async Task CreateProjectAsync_ReturnsSameProjectInstance_WithFpsYearSet()
        {
            // Arrange
            var repo = CreateRepository(projects: [], fpsYear: 2024);
            var project = new Project
            {
                ParentProject = "PP001", ProjectTitle = "New Project",
                Program = "P001", Customer = "DEFRA", ProjectStatus = "Active",
                Disease = "D1", Contract = "C1", IncomeAccountCode = "I1"
            };

            // Act
            var result = await repo.CreateProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.Same(project, result);
            Assert.Equal("PP001", result.ParentProject);
            Assert.Equal(2024, result.FpsYear);
        }

        #endregion

        #region UpdatePactProjectDetailsAsync Tests

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_UpdatesAllFields_WhenProjectFound()
        {
            // Arrange
            const int fpsYear = 2024;
            var existingProject = new Project
            {
                ParentProject = "PP001", ProjectTitle = "Original Title", Program = "P001",
                Customer = "DEFRA",  Manager = "Alice",  Contract = "C001", ProjectStatus = "Active",
                Disease = "FMD",    IsDefraProject = 1,  Finished = 0,     Comments = "Old comment",
                BudgetCvl = 1000m,  TransferIncome = 500m, PvsIncome = 200m,
                WipEoy = 100m,      WipLimit = 150m,    WipCurrent = 80m,  FecCost = 300m,
                IncomeAccountCode = "I1", FpsYear = fpsYear
            };
            var repo = CreateRepository(projects: [existingProject], fpsYear: fpsYear);

            var updateRequest = new Project
            {
                ParentProject = "PP001", ProjectTitle = "Updated Title", Program = "P002",
                Customer = "APHA",    Manager = "Bob",   Contract = "C002", ProjectStatus = "Closed",
                Disease = "TB",       IsDefraProject = 0, Finished = 1,    Comments = "New comment",
                BudgetCvl = 2000m,   TransferIncome = 600m, PvsIncome = 300m,
                WipEoy = 200m,       WipLimit = 250m,   WipCurrent = 180m, FecCost = 400m
            };

            // Act
            var result = await repo.UpdatePactProjectDetailsAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Title", result.ProjectTitle);
            Assert.Equal("P002",          result.Program);
            Assert.Equal("APHA",          result.Customer);
            Assert.Equal("Bob",           result.Manager);
            Assert.Equal("C002",          result.Contract);
            Assert.Equal("Closed",        result.ProjectStatus);
            Assert.Equal("TB",            result.Disease);
            Assert.Equal((short)0,        result.IsDefraProject);
            Assert.Equal((short)1,        result.Finished);
            Assert.Equal("New comment",   result.Comments);
            Assert.Equal(2000m,           result.BudgetCvl);
            Assert.Equal(600m,            result.TransferIncome);
            Assert.Equal(300m,            result.PvsIncome);
            Assert.Equal(200m,            result.WipEoy);
            Assert.Equal(250m,            result.WipLimit);
            Assert.Equal(180m,            result.WipCurrent);
            Assert.Equal(400m,            result.FecCost);
        }

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_ReturnsNull_WhenProjectNotFound()
        {
            // Arrange — existing project has different ParentProject than the update request
            const int fpsYear = 2024;
            var existingProject = new Project
            {
                ParentProject = "PP001", Program = "P001", Customer = "DEFRA",
                ProjectStatus = "Active", Disease = "D1", Contract = "C1",
                IncomeAccountCode = "I1", FpsYear = fpsYear
            };
            var repo = CreateRepository(projects: [existingProject], fpsYear: fpsYear);

            var updateRequest = new Project { ParentProject = "PP999" };

            // Act
            var result = await repo.UpdatePactProjectDetailsAsync(updateRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_ReturnsNull_WhenFpsYearDoesNotMatch()
        {
            // Arrange — project exists but for a different FPS year
            var existingProject = new Project
            {
                ParentProject = "PP001", Program = "P001", Customer = "DEFRA",
                ProjectStatus = "Active", Disease = "D1", Contract = "C1",
                IncomeAccountCode = "I1", FpsYear = 2023
            };
            var repo = CreateRepository(projects: [existingProject], fpsYear: 2024);

            var updateRequest = new Project { ParentProject = "PP001" };

            // Act
            var result = await repo.UpdatePactProjectDetailsAsync(updateRequest);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region DeleteProjectAsync Tests

        [Fact]
        public async Task DeleteProjectAsync_ReturnsTrue_WhenProjectExists()
        {
            // Arrange
            const int fpsYear = 2024;
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", Program = "P001", Customer = "DEFRA",
                        ProjectStatus = "Active", Disease = "D1", Contract = "C1",
                        IncomeAccountCode = "I1", FpsYear = fpsYear }
            };
            var repo = CreateRepository(projects: projects, fpsYear: fpsYear);

            // Act
            var result = await repo.DeleteProjectAsync("PP001");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteProjectAsync_ReturnsFalse_WhenProjectNotFound()
        {
            // Arrange — no project matching the requested ParentProject
            const int fpsYear = 2024;
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", Program = "P001", Customer = "DEFRA",
                        ProjectStatus = "Active", Disease = "D1", Contract = "C1",
                        IncomeAccountCode = "I1", FpsYear = fpsYear }
            };
            var repo = CreateRepository(projects: projects, fpsYear: fpsYear);

            // Act
            var result = await repo.DeleteProjectAsync("PP999");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProjectAsync_ReturnsFalse_WhenFpsYearDoesNotMatch()
        {
            // Arrange — project exists but for a different FPS year
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", Program = "P001", Customer = "DEFRA",
                        ProjectStatus = "Active", Disease = "D1", Contract = "C1",
                        IncomeAccountCode = "I1", FpsYear = 2023 }
            };
            var repo = CreateRepository(projects: projects, fpsYear: 2024);

            // Act
            var result = await repo.DeleteProjectAsync("PP001");

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}