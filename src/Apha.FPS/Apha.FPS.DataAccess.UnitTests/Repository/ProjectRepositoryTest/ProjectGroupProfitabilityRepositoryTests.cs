using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectRepositoryTest
{
    public class ProjectGroupProfitabilityRepositoryTests
    {
        private static ProjectRepository CreateRepository(
            IEnumerable<ProjectGroupView>? projectGroupViews = null,
            IEnumerable<Project>? projects = null,
            IEnumerable<Program>? programs = null,
            IEnumerable<StaffJob>? staffJobs = null,
            IEnumerable<AdditionalCost>? additionalCosts = null,
            IEnumerable<TestRequirement>? testRequirements = null,
            IEnumerable<AnimalRequest>? animalRequests = null,
            IEnumerable<Animal>? animals = null,
            IEnumerable<WorkGroupEmployee>? workGroupEmployees = null,
            IEnumerable<WorkgroupGrade>? workgroupGrades = null,
            IEnumerable<ProfitCentreGrade>? profitCentreGrades = null,
            string userEmailId = "test@example.com",
            int fpsYear = 2024)
        // ComputeProfitabilityAsync always joins WorkGroupEmployees/WorkgroupGrades/ProfitCentreGrades;
        // callers that reach it must supply at least empty lists for these three sets.
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.UserEmailId).Returns(userEmailId);
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            if (projectGroupViews != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(projectGroupViews);
                mockContext.Setup(x => x.ProjectGroupViews).Returns(mockSet.Object);
            }

            if (projects != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(projects);
                mockContext.Setup(x => x.Projects).Returns(mockSet.Object);
            }

            if (programs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(programs);
                mockContext.Setup(x => x.Programs).Returns(mockSet.Object);
            }

            if (staffJobs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(staffJobs);
                mockContext.Setup(x => x.StaffJobs).Returns(mockSet.Object);
            }

            if (additionalCosts != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(additionalCosts);
                mockContext.Setup(x => x.AdditionalCosts).Returns(mockSet.Object);
            }

            if (testRequirements != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(testRequirements);
                mockContext.Setup(x => x.TestRequirements).Returns(mockSet.Object);
            }

            if (animalRequests != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(animalRequests);
                mockContext.Setup(x => x.AnimalRequests).Returns(mockSet.Object);
            }

            if (animals != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(animals);
                mockContext.Setup(x => x.Animals).Returns(mockSet.Object);
            }

            var wgeMockSet = RepositoryTestHelper.CreateMockDbSet(workGroupEmployees ?? Enumerable.Empty<WorkGroupEmployee>());
            mockContext.Setup(x => x.WorkGroupEmployees).Returns(wgeMockSet.Object);

            var wggMockSet = RepositoryTestHelper.CreateMockDbSet(workgroupGrades ?? Enumerable.Empty<WorkgroupGrade>());
            mockContext.Setup(x => x.WorkgroupGrades).Returns(wggMockSet.Object);

            var pcgMockSet = RepositoryTestHelper.CreateMockDbSet(profitCentreGrades ?? Enumerable.Empty<ProfitCentreGrade>());
            mockContext.Setup(x => x.ProfitCentreGrades).Returns(pcgMockSet.Object);

            return new ProjectRepository(mockContext.Object, mockRequestContext.Object);
        }

        // ── GetProjectGroupProfitabilityAsync ─────────────────────────────────

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WithNoMatchingProjectGroup_ReturnsEmptyPage()
        {
            // Arrange
            var repo = CreateRepository(
                projectGroupViews: new List<ProjectGroupView>(),
                projects: new List<Project>(),
                programs: new List<Program>(),
                staffJobs: new List<StaffJob>(),
                additionalCosts: new List<AdditionalCost>(),
                testRequirements: new List<TestRequirement>(),
                animalRequests: new List<AnimalRequest>(),
                animals: new List<Animal>());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectGroupProfitabilityAsync(query, "NonExistentGroup", "all");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WithValidProjectGroup_ReturnsProjectsForGroup()
        {
            // Arrange
            var projectGroupViews = new List<ProjectGroupView>
            {
                new() { ProjectGroupName = "Group1", UserEmail = "test@example.com" },
                new() { ProjectGroupName = "Group2", UserEmail = "test@example.com" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectGroup = "Group1", Program = "P001", BudgetCvl = 5000m, Profit = 500m, ProjectStatus = "Approved" },
                new() { ParentProject = "PP002", ProjectGroup = "Group1", Program = "P001", BudgetCvl = 6000m, Profit = 600m, ProjectStatus = "Approved" },
                new() { ParentProject = "PP003", ProjectGroup = "Group2", Program = "P002", BudgetCvl = 7000m, Profit = 700m, ProjectStatus = "Approved" }
            };
            var programs = new List<Program>
            {
                new() { ProgramNo = "P001", Target = 10000m },
                new() { ProgramNo = "P002", Target = 15000m }
            };

            var repo = CreateRepository(
                projectGroupViews: projectGroupViews,
                projects: projects,
                programs: programs,
                staffJobs: new List<StaffJob>(),
                additionalCosts: new List<AdditionalCost>(),
                testRequirements: new List<TestRequirement>(),
                animalRequests: new List<AnimalRequest>(),
                animals: new List<Animal>());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectGroupProfitabilityAsync(query, "Group1", "all");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, item => Assert.True(item.JobCode == "PP001" || item.JobCode == "PP002"));
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WorkTypeFilter_Approved_FiltersCorrectly()
        {
            // Arrange
            var projectGroupViews = new List<ProjectGroupView>
            {
                new() { ProjectGroupName = "Group1", UserEmail = "test@example.com" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectGroup = "Group1", Program = "P001", BudgetCvl = 5000m, Profit = 500m, ProjectStatus = "Approved" },
                new() { ParentProject = "PP002", ProjectGroup = "Group1", Program = "P001", BudgetCvl = 6000m, Profit = 600m, ProjectStatus = "Not Approved" }
            };
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };

            var repo = CreateRepository(
                projectGroupViews: projectGroupViews,
                projects: projects,
                programs: programs,
                staffJobs: new List<StaffJob>(),
                additionalCosts: new List<AdditionalCost>(),
                testRequirements: new List<TestRequirement>(),
                animalRequests: new List<AnimalRequest>(),
                animals: new List<Animal>());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectGroupProfitabilityAsync(query, "Group1", "approved");

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("PP001", result.Data.First().JobCode);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WorkTypeFilter_NotApproved_FiltersCorrectly()
        {
            // Arrange
            var projectGroupViews = new List<ProjectGroupView>
            {
                new() { ProjectGroupName = "Group1", UserEmail = "test@example.com" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectGroup = "Group1", Program = "P001", BudgetCvl = 5000m, Profit = 500m, ProjectStatus = "Approved" },
                new() { ParentProject = "PP002", ProjectGroup = "Group1", Program = "P001", BudgetCvl = 6000m, Profit = 600m, ProjectStatus = "Not Approved" }
            };
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };

            var repo = CreateRepository(
                projectGroupViews: projectGroupViews,
                projects: projects,
                programs: programs,
                staffJobs: new List<StaffJob>(),
                additionalCosts: new List<AdditionalCost>(),
                testRequirements: new List<TestRequirement>(),
                animalRequests: new List<AnimalRequest>(),
                animals: new List<Animal>());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectGroupProfitabilityAsync(query, "Group1", "not-approved");

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("PP002", result.Data.First().JobCode);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_ProgrammeTargetIsResolvedPerProject()
        {
            // Arrange — projects belong to two different programmes within same group
            var projectGroupViews = new List<ProjectGroupView>
            {
                new() { ProjectGroupName = "MultiProg", UserEmail = "test@example.com" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectGroup = "MultiProg", Program = "P001", BudgetCvl = 5000m, Profit = 500m, ProjectStatus = "Approved" },
                new() { ParentProject = "PP002", ProjectGroup = "MultiProg", Program = "P002", BudgetCvl = 7000m, Profit = 700m, ProjectStatus = "Approved" }
            };
            var programs = new List<Program>
            {
                new() { ProgramNo = "P001", Target = 10000m },
                new() { ProgramNo = "P002", Target = 20000m }
            };

            var repo = CreateRepository(
                projectGroupViews: projectGroupViews,
                projects: projects,
                programs: programs,
                staffJobs: new List<StaffJob>(),
                additionalCosts: new List<AdditionalCost>(),
                testRequirements: new List<TestRequirement>(),
                animalRequests: new List<AnimalRequest>(),
                animals: new List<Animal>());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectGroupProfitabilityAsync(query, "MultiProg", "all");

            // Assert
            Assert.Equal(2, result.Data.Count());
            var pp001 = result.Data.First(x => x.JobCode == "PP001");
            var pp002 = result.Data.First(x => x.JobCode == "PP002");
            Assert.Equal(10000m, pp001.ProgrammeTarget);
            Assert.Equal(20000m, pp002.ProgrammeTarget);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_PagingIsApplied()
        {
            // Arrange
            var projectGroupViews = new List<ProjectGroupView>
            {
                new() { ProjectGroupName = "Group1", UserEmail = "test@example.com" }
            };
            var projects = Enumerable.Range(1, 5).Select(i => new Project
            {
                ParentProject = $"PP00{i}",
                ProjectGroup = "Group1",
                Program = "P001",
                BudgetCvl = 5000m,
                Profit = 500m,
                ProjectStatus = "Approved"
            }).ToList();
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };

            var repo = CreateRepository(
                projectGroupViews: projectGroupViews,
                projects: projects,
                programs: programs,
                staffJobs: new List<StaffJob>(),
                additionalCosts: new List<AdditionalCost>(),
                testRequirements: new List<TestRequirement>(),
                animalRequests: new List<AnimalRequest>(),
                animals: new List<Animal>());

            var query = new PaginationParameters<string> { Page = 1, PageSize = 2 };

            // Act
            var result = await repo.GetProjectGroupProfitabilityAsync(query, "Group1", "all");

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_UserEmailFilter_ExcludesOtherUsersGroups()
        {
            // Arrange — two groups, only one belongs to "test@example.com"
            var projectGroupViews = new List<ProjectGroupView>
            {
                new() { ProjectGroupName = "MyGroup",    UserEmail = "test@example.com" },
                new() { ProjectGroupName = "OtherGroup", UserEmail = "other@example.com" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectGroup = "MyGroup",    Program = "P001", BudgetCvl = 5000m, Profit = 0m, ProjectStatus = "Approved" },
                new() { ParentProject = "PP002", ProjectGroup = "OtherGroup", Program = "P001", BudgetCvl = 6000m, Profit = 0m, ProjectStatus = "Approved" }
            };
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };

            var repo = CreateRepository(
                projectGroupViews: projectGroupViews,
                projects: projects,
                programs: programs,
                staffJobs: new List<StaffJob>(),
                additionalCosts: new List<AdditionalCost>(),
                testRequirements: new List<TestRequirement>(),
                animalRequests: new List<AnimalRequest>(),
                animals: new List<Animal>(),
                userEmailId: "test@example.com");

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectGroupProfitabilityAsync(query, "MyGroup", "all");

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("PP001", result.Data.First().JobCode);
        }
    }
}
