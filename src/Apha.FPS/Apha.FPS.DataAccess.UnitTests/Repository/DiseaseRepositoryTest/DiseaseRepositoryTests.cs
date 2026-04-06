using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.DiseaseRepositoryTest
{
    public class DiseaseRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a DiseaseRepository with in-memory Diseases data.
        /// IFpsYearContext is substituted via NSubstitute.
        /// Disease has no FpsCalYear query filter, so year value is irrelevant.
        /// </summary>
        private static DiseaseRepository CreateRepository(IEnumerable<Disease> diseases)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            fpsYearContext.FpsYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var diseasesMockSet = RepositoryTestHelper.CreateMockDbSet(diseases);
            mockContext.Setup(x => x.Diseases).Returns(diseasesMockSet.Object);

            return new DiseaseRepository(mockContext.Object);
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
            var repo = CreateRepository(diseases);

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
            var repo = CreateRepository(new List<Disease>());

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
            var repo = CreateRepository(diseases);

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
            var repo = CreateRepository(new List<Disease>());

            // Act
            var result = await repo.GetAllDiseasesAsync();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<Disease>>(result);
        }

        #endregion
    }
}