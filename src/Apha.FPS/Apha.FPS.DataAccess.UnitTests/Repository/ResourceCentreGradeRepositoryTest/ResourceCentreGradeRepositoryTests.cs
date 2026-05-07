using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ResourceCentreGradeRepositoryTest
{
    public class ResourceCentreGradeRepositoryTests
    {
        private const string DefaultProfitCentre = "PC01";

        private static ResourceCentreGradeRepository CreateRepository(
            IEnumerable<ProfitCentreGrade> grades)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var gradesMockSet = RepositoryTestHelper.CreateMockDbSet(grades);
            mockContext.Setup(x => x.ProfitcentreGrades).Returns(gradesMockSet.Object);

            return new ResourceCentreGradeRepository(mockContext.Object);
        }

        #region GetResourceCentreGradesAsync Tests

        [Fact]
        public async Task GetResourceCentreGradesAsync_WithMatchingProfitCentre_ReturnsPagedData()
        {
            // Arrange
            var grades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, ChargeRate = 100m },
                new() { PcGrade = "G002", ProfitCentre = DefaultProfitCentre, ChargeRate = 200m },
                new() { PcGrade = "G003", ProfitCentre = "OTHER",            ChargeRate = 300m }
            };
            var repo = CreateRepository(grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetResourceCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, g => Assert.Equal(DefaultProfitCentre, g.ProfitCentre));
        }

        [Fact]
        public async Task GetResourceCentreGradesAsync_WithNoMatchingProfitCentre_ReturnsEmptyData()
        {
            // Arrange
            var grades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "G001", ProfitCentre = "OTHER", ChargeRate = 100m }
            };
            var repo = CreateRepository(grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetResourceCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetResourceCentreGradesAsync_ReturnsOrderedByChargeRateDescending()
        {
            // Arrange
            var grades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, ChargeRate = 100m },
                new() { PcGrade = "G002", ProfitCentre = DefaultProfitCentre, ChargeRate = 300m },
                new() { PcGrade = "G003", ProfitCentre = DefaultProfitCentre, ChargeRate = 200m }
            };
            var repo = CreateRepository(grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetResourceCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            var resultList = result.Data.ToList();
            Assert.Equal(300m, resultList[0].ChargeRate);
            Assert.Equal(200m, resultList[1].ChargeRate);
            Assert.Equal(100m, resultList[2].ChargeRate);
        }

        [Fact]
        public async Task GetResourceCentreGradesAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var grades = Enumerable.Range(1, 5).Select(i => new ProfitCentreGrade
            {
                PcGrade      = $"G00{i}",
                ProfitCentre = DefaultProfitCentre,
                ChargeRate   = i * 100m
            }).ToList();
            var repo = CreateRepository(grades);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            // Act
            var result = await repo.GetResourceCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetResourceCentreGradesAsync_WithEmptyRepository_ReturnsEmptyData()
        {
            // Arrange
            var repo = CreateRepository(new List<ProfitCentreGrade>());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetResourceCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion
    }
}
