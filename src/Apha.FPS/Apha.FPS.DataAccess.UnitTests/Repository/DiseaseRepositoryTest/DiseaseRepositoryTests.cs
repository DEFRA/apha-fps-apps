using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.DiseaseRepositoryTest
{
    public class DiseaseRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a DiseaseRepository with in-memory Diseases data.
        /// IFpsYearContext is substituted via NSubstitute purely for helper-signature symmetry —
        /// DiseaseRepository's constructor takes only FpsDbContext and does not consume IFpsRequestContext,
        /// since Disease has no FpsCalYear query filter and the year value is irrelevant.
        /// </summary>
        private static (DiseaseRepository Repository, Mock<FpsDbContext> Context, Mock<DbSet<Disease>> DbSet)
            CreateRepository(IEnumerable<Disease> diseases)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            fpsYearContext.FpsYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var diseasesMockSet = RepositoryTestHelper.CreateMockDbSet(diseases);
            RepositoryTestHelper.SetupDbSetOperations(diseasesMockSet);
            mockContext.Setup(x => x.Diseases).Returns(diseasesMockSet.Object);

            var repository = new DiseaseRepository(mockContext.Object);
            return (repository, mockContext, diseasesMockSet);
        }

        #region GetAllDiseasesAsync

        [Fact]
        public async Task GetAllDiseasesAsync_ReturnsAllDiseases_WhenDataExists()
        {
            // Arrange
            var diseases = new List<Disease>
            {
                new() { DiseaseName = "Disease A" },
                new() { DiseaseName = "Disease B" },
                new() { DiseaseName = "Disease C" }
            };
            var (repo, _, _) = CreateRepository(diseases);

            // Act
            var result = await repo.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetAllDiseasesAsync_ReturnsEmptyCollection_WhenNoDiseasesExist()
        {
            // Arrange
            var (repo, _, _) = CreateRepository(new List<Disease>());

            // Act
            var result = await repo.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_ReturnsCorrectData_WhenSingleDiseaseExists()
        {
            // Arrange
            var diseases = new List<Disease>
            {
                new() { DiseaseName = "Disease A" }
            };
            var (repo, _, _) = CreateRepository(diseases);

            // Act
            var result = await repo.GetAllDiseasesAsync();

            // Assert
            var single = Assert.Single(result);
            Assert.Equal("Disease A", single.DiseaseName);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_ReturnsIEnumerable_NotNull()
        {
            // Arrange — verifies the return type contract is always IEnumerable, never null
            var (repo, _, _) = CreateRepository(new List<Disease>());

            // Act
            var result = await repo.GetAllDiseasesAsync();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<Disease>>(result);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_AddsEntityAndReturnsIt()
        {
            // Arrange
            var (repo, mockContext, mockDbSet) = CreateRepository(new List<Disease>());
            var disease = new Disease { DiseaseName = "Disease A" };

            // Act
            var result = await repo.AddAsync(disease);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Disease A", result.DiseaseName);
            mockDbSet.Verify(x => x.Add(It.IsAny<Disease>()), Times.Once);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingName_RemovesEntityAndReturnsTrue()
        {
            // Arrange
            var diseases = new List<Disease>
            {
                new() { DiseaseName = "Disease A" }
            };
            var (repo, mockContext, mockDbSet) = CreateRepository(diseases);

            // Act
            var result = await repo.DeleteAsync("Disease A");

            // Assert
            Assert.True(result);
            mockDbSet.Verify(x => x.Remove(It.IsAny<Disease>()), Times.Once);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_UnknownName_ReturnsFalse()
        {
            // Arrange
            var diseases = new List<Disease>
            {
                new() { DiseaseName = "Disease A" }
            };
            var (repo, mockContext, mockDbSet) = CreateRepository(diseases);

            // Act
            var result = await repo.DeleteAsync("Unknown Disease");

            // Assert
            Assert.False(result);
            mockDbSet.Verify(x => x.Remove(It.IsAny<Disease>()), Times.Never);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_EmptyName_ReturnsFalse()
        {
            // Arrange
            var diseases = new List<Disease>
            {
                new() { DiseaseName = "Disease A" }
            };
            var (repo, mockContext, mockDbSet) = CreateRepository(diseases);

            // Act
            var result = await repo.DeleteAsync(string.Empty);

            // Assert
            Assert.False(result);
            mockDbSet.Verify(x => x.Remove(It.IsAny<Disease>()), Times.Never);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_WhenPresent_ReturnsTrue()
        {
            // Arrange
            var diseases = new List<Disease>
            {
                new() { DiseaseName = "Disease A" }
            };
            var (repo, _, _) = CreateRepository(diseases);

            // Act
            var result = await repo.ExistsAsync("Disease A");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WhenAbsent_ReturnsFalse()
        {
            // Arrange
            var diseases = new List<Disease>
            {
                new() { DiseaseName = "Disease A" }
            };
            var (repo, _, _) = CreateRepository(diseases);

            // Act
            var result = await repo.ExistsAsync("Disease B");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_CaseSensitiveMatch()
        {
            // Arrange — verifies string comparison is case-sensitive (no implicit case-insensitive matching)
            var diseases = new List<Disease>
            {
                new() { DiseaseName = "Disease A" }
            };
            var (repo, _, _) = CreateRepository(diseases);

            // Act
            var result = await repo.ExistsAsync("disease a");

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}