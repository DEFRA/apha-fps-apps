using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Moq;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.ContractRepositoryTest
{
    public class ContractRepositoryTests
    {
        private static ContractRepository CreateRepository(IEnumerable<Project> projects)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects);
            mockContext.Setup(x => x.Set<Project>()).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ContractRepository(mockContext.Object);
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_ReturnsDistinctContractNumbers()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ContractNumber = "CON001" },
                new() { ProjectId = "2024/002", ContractNumber = "CON002" },
                new() { ProjectId = "2024/003", ContractNumber = "CON001" } // duplicate
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains("CON001", result);
            Assert.Contains("CON002", result);
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_ExcludesNullContractNumbers()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ContractNumber = "CON001" },
                new() { ProjectId = "2024/002", ContractNumber = null },
                new() { ProjectId = "2024/003", ContractNumber = "CON002" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(null, result);
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_ReturnsEmptyList_WhenNoProjects()
        {
            // Arrange
            var repo = CreateRepository(new List<Project>());

            // Act
            var result = await repo.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_ReturnsEmptyList_WhenAllContractNumbersAreNull()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ContractNumber = null },
                new() { ProjectId = "2024/002", ContractNumber = null }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_ReturnsList_WhenAllContractNumbersAreUnique()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ContractNumber = "CON001" },
                new() { ProjectId = "2024/002", ContractNumber = "CON002" },
                new() { ProjectId = "2024/003", ContractNumber = "CON003" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }
    }
}
