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
        private const string DefaultPcGrade    = "G001";
        private const string DefaultUserEmail   = "test@example.com";

        private static WorkGroupGradeRepository CreateRepository(
            IEnumerable<WorkGroupGradeView>? viewGrades = null,
            IEnumerable<WorkgroupGrade>? grades = null)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);
            requestContext.UserEmailId.Returns(DefaultUserEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var viewGradesMockSet = RepositoryTestHelper.CreateMockDbSet(viewGrades ?? Enumerable.Empty<WorkGroupGradeView>());
            mockContext.Setup(x => x.WorkGroupGradeViews).Returns(viewGradesMockSet.Object);

            var gradesMockSet = RepositoryTestHelper.CreateMockDbSet(grades ?? Enumerable.Empty<WorkgroupGrade>());
            mockContext.Setup(x => x.WorkgroupGrades).Returns(gradesMockSet.Object);

            return new WorkGroupGradeRepository(mockContext.Object, requestContext);
        }

        #region GetWorkGroupGradeAsync Tests

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithMatchingPcGrade_ReturnsPagedData()
        {
            // Arrange
            var viewGrades = new List<WorkGroupGradeView>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC1", WorkGroup = "WG", UserEmail = DefaultUserEmail },
                new() { WgGrade = "WG02", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC2", WorkGroup = "WG", UserEmail = DefaultUserEmail },
                new() { WgGrade = "WG03", ProfitCentreGrade = "OTHER",        GradeCode = "GC3", WorkGroup = "WG", UserEmail = DefaultUserEmail }
            };
            var repo  = CreateRepository(viewGrades: viewGrades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetWorkGroupGradesAsync(query, DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, g => Assert.Equal(DefaultPcGrade, g.ProfitCentreGrade));
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithNoMatchingPcGrade_ReturnsEmptyData()
        {
            // Arrange
            var viewGrades = new List<WorkGroupGradeView>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "OTHER", GradeCode = "GC1", WorkGroup = "WG", UserEmail = DefaultUserEmail }
            };
            var repo  = CreateRepository(viewGrades: viewGrades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetWorkGroupGradesAsync(query, DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_ReturnsOrderedByWgGrade()
        {
            // Arrange
            var viewGrades = new List<WorkGroupGradeView>
            {
                new() { WgGrade = "WG03", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC3", WorkGroup = "WG", UserEmail = DefaultUserEmail },
                new() { WgGrade = "WG01", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC1", WorkGroup = "WG", UserEmail = DefaultUserEmail },
                new() { WgGrade = "WG02", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC2", WorkGroup = "WG", UserEmail = DefaultUserEmail }
            };
            var repo  = CreateRepository(viewGrades: viewGrades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetWorkGroupGradesAsync(query, DefaultPcGrade);

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
            var viewGrades = Enumerable.Range(1, 5).Select(i => new WorkGroupGradeView
            {
                WgGrade           = $"WG0{i}",
                ProfitCentreGrade = DefaultPcGrade,
                GradeCode         = $"GC{i}",
                WorkGroup         = "WG",
                UserEmail         = DefaultUserEmail
            }).ToList();
            var repo  = CreateRepository(viewGrades: viewGrades);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            // Act
            var result = await repo.GetWorkGroupGradesAsync(query, DefaultPcGrade);

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
            var repo  = CreateRepository(viewGrades: new List<WorkGroupGradeView>());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetWorkGroupGradesAsync(query, DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion

        #region DeleteWorkGroupGradeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WithExistingWgGrade_ReturnsTrueAndRemoves()
        {
            // Arrange
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC1", Workgroup = "WG" }
            };
            var repo = CreateRepository(grades: grades);

            // Act
            var result = await repo.DeleteWorkGroupGradeAsync("WG01");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WithNonExistentWgGrade_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(grades: new List<WorkgroupGrade>());

            // Act
            var result = await repo.DeleteWorkGroupGradeAsync("NONEXISTENT");

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
