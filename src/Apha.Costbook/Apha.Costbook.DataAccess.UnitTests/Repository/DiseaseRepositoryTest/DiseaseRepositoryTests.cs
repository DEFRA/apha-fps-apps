using System;
using System.Collections.Generic;
using System.Text;
using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Moq;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.DiseaseRepositoryTest
{
    public class DiseaseRepositoryTests
    {
        private static DiseaseRepository CreateRepository(IEnumerable<Disease> diseases)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var diseasesMockSet = RepositoryTestHelper.CreateMockDbSet(diseases);
            mockContext.Setup(x => x.Set<Disease>()).Returns(diseasesMockSet.Object);
            mockContext.Setup(x => x.Diseases).Returns(diseasesMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new DiseaseRepository(mockContext.Object);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_ReturnsAllDiseases()
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
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_ReturnsEmptyList_WhenNoDiseases()
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
        public async Task GetAllDiseasesAsync_ReturnsCorrectDiseaseNames()
        {
            // Arrange
            var diseases = new List<Disease>
            {
                new() { DiseaseName = "Foot and Mouth" },
                new() { DiseaseName = "Avian Influenza" }
            };
            var repo = CreateRepository(diseases);

            // Act
            var result = await repo.GetAllDiseasesAsync();

            // Assert
            Assert.Contains(result, d => d.DiseaseName == "Foot and Mouth");
            Assert.Contains(result, d => d.DiseaseName == "Avian Influenza");
        }

        [Fact]
        public async Task GetAllDiseasesAsync_ReturnsSingleDisease_WhenOnlyOneExists()
        {
            // Arrange
            var diseases = new List<Disease>
            {
                new() { DiseaseName = "Only Disease" }
            };
            var repo = CreateRepository(diseases);

            // Act
            var result = await repo.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Only Disease", result[0].DiseaseName);
        }
    }
}
