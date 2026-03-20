using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectRepositoryTest
{
    public class ProjectRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a ProjectRepository with in-memory Projects data.
        /// IFpsYearContext and IProgramRepository are substituted via NSubstitute.
        /// Get() / GetAllProjectsAsync() JOIN logic across Projects/Programs is covered
        /// by integration tests — only GetProjectByIdAsync is unit-testable here.
        /// </summary>
        private static ProjectRepository CreateRepository(IEnumerable<Project> projects)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects);
            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);

            var programRepo = Substitute.For<IProgramRepository>();
            programRepo.Get().Returns(Enumerable.Empty<Core.Entities.Program>().AsQueryable());

            return new ProjectRepository(mockContext.Object, programRepo);
        }

        #region GetProjectByIdAsync

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsProject_WhenFound()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "P001", ProjectTitle = "Project One",
                        Program = "PRG001", Customer = "Cust A",
                        ProjectStatus = "Active", Disease = "None",
                        Contract = "C001", IncomeAccountCode = "IAC001",
                        FpsCalYear = DefaultTestFpsYear },
                new() { ParentProject = "P002", ProjectTitle = "Project Two",
                        Program = "PRG002", Customer = "Cust B",
                        ProjectStatus = "Active", Disease = "None",
                        Contract = "C002", IncomeAccountCode = "IAC002",
                        FpsCalYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetProjectByIdAsync("P001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P001", result.ParentProject);
            Assert.Equal("Project One", result.ProjectTitle);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "P001", ProjectTitle = "Project One",
                        Program = "PRG001", Customer = "Cust A",
                        ProjectStatus = "Active", Disease = "None",
                        Contract = "C001", IncomeAccountCode = "IAC001",
                        FpsCalYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetProjectByIdAsync("P999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNull_WhenProjectsIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(new List<Project>());

            // Act
            var result = await repo.GetProjectByIdAsync("P001");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("p001")]  // case-sensitive — "p001" does not match "P001"
        [InlineData("P001 ")] // trailing space — does not match "P001"
        public async Task GetProjectByIdAsync_ReturnsNull_WhenIdDoesNotExactlyMatch(string parentProject)
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "P001", Program = "PRG001", Customer = "Cust A",
                        ProjectStatus = "Active", Disease = "None", Contract = "C001",
                        ProjectTitle = "Project One", IncomeAccountCode = "IAC001",
                        FpsCalYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetProjectByIdAsync(parentProject);

            // Assert
            Assert.Null(result);
        }

        #endregion
    }
}