using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.WorkGroupGradeRepositoryTest
{
    public class WorkGroupGradeRepositoryTests
    {
        private const string DefaultPcGrade = "G001";

        private static WorkGroupGradeRepository CreateRepository(IEnumerable<WorkgroupGrade> grades)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var gradesMockSet = RepositoryTestHelper.CreateMockDbSet(grades);
            mockContext.Setup(x => x.WorkgroupGrades).Returns(gradesMockSet.Object);

            return new WorkGroupGradeRepository(mockContext.Object);
        }

        #region GetWorkGroupGradeAsync Tests

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithMatchingPcGrade_ReturnsPagedData()
        {
            // Arrange
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC1", Workgroup = "WG" },
                new() { WgGrade = "WG02", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC2", Workgroup = "WG" },
                new() { WgGrade = "WG03", ProfitCentreGrade = "OTHER",        GradeCode = "GC3", Workgroup = "WG" }
            };
            var repo  = CreateRepository(grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetWorkGroupGradeAsync(query, DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, g => Assert.Equal(DefaultPcGrade, g.ProfitCentreGrade));
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithNoMatchingPcGrade_ReturnsEmptyData()
        {
            // Arrange
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "OTHER", GradeCode = "GC1", Workgroup = "WG" }
            };
            var repo  = CreateRepository(grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetWorkGroupGradeAsync(query, DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_ReturnsOrderedByWgGrade()
        {
            // Arrange
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG03", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC3", Workgroup = "WG" },
                new() { WgGrade = "WG01", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC1", Workgroup = "WG" },
                new() { WgGrade = "WG02", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC2", Workgroup = "WG" }
            };
            var repo  = CreateRepository(grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetWorkGroupGradeAsync(query, DefaultPcGrade);

            // Assert
            var resultList = result.Data.ToList();
            Assert.Equal("WG01", resultList[0].WgGrade);
            Assert.Equal("WG02", resultList[1].WgGrade);
            Assert.Equal("WG03", resultList[2].WgGrade);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var grades = Enumerable.Range(1, 5).Select(i => new WorkgroupGrade
            {
                WgGrade           = $"WG0{i}",
                ProfitCentreGrade = DefaultPcGrade,
                GradeCode         = $"GC{i}",
                Workgroup         = "WG"
            }).ToList();
            var repo  = CreateRepository(grades);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            // Act
            var result = await repo.GetWorkGroupGradeAsync(query, DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithEmptyRepository_ReturnsEmptyData()
        {
            // Arrange
            var repo  = CreateRepository(new List<WorkgroupGrade>());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetWorkGroupGradeAsync(query, DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion
    }
}
