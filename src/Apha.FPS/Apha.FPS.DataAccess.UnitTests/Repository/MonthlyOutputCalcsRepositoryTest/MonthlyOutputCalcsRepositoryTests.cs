using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.MonthlyOutputCalcsRepositoryTest
{
    public class MonthlyOutputCalcsRepositoryTests
    {
        private const string ProjectCode   = "AH0033";
        private const string OtherProject  = "OTHER";
        private const string TestCodeTc01  = "TC01";
        private const string TestCodeTc02  = "TC02";
        private const string WorkGroupCsu  = "CSU";
        private const string WorkGroupLt5  = "LT5";
        private const int    DefaultYear   = 2024;

        private static MonthlyOutputCalcsRepository CreateRepository(
            IEnumerable<MonthlyOutput>? monthlyOutputs = null,
            int fpsYear = DefaultYear)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            var data   = monthlyOutputs ?? Array.Empty<MonthlyOutput>();
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            mockContext.Setup(x => x.MonthlyOutputs).Returns(mockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new MonthlyOutputCalcsRepository(mockContext.Object);
        }

        private static PaginationParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new() { Page = page, PageSize = pageSize };

        private static MonthlyOutput MakeOutput(
            string buyer, string testCode, double month, string workGroup, double? volume, int fpsYear = DefaultYear)
            => new() { Buyer = buyer, TestCode = testCode, Month = month, WorkGroup = workGroup, Volume = volume, FpsYear = fpsYear };

        #region GetByProjectAsync

        [Fact]
        public async Task GetByProjectAsync_WithMatchingRecords_ReturnsOnlyProjectRows()
        {
            var outputs = new List<MonthlyOutput>
            {
                MakeOutput(ProjectCode,  TestCodeTc01, 1, WorkGroupCsu, 5),
                MakeOutput(ProjectCode,  TestCodeTc02, 2, WorkGroupLt5, 3),
                MakeOutput(OtherProject, "TC03",       1, WorkGroupCsu, 2)
            };
            var repo = CreateRepository(monthlyOutputs: outputs);

            var result = await repo.GetByProjectAsync(DefaultQuery(), ProjectCode);

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Equal(ProjectCode, r.Buyer));
        }

        [Fact]
        public async Task GetByProjectAsync_WithNoMatchingProject_ReturnsEmpty()
        {
            var outputs = new List<MonthlyOutput> { MakeOutput(OtherProject, TestCodeTc01, 1, WorkGroupCsu, 5) };
            var repo = CreateRepository(monthlyOutputs: outputs);

            var result = await repo.GetByProjectAsync(DefaultQuery(), ProjectCode);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetByProjectAsync_WithEmptyDataSet_ReturnsEmpty()
        {
            var repo = CreateRepository(monthlyOutputs: Array.Empty<MonthlyOutput>());

            var result = await repo.GetByProjectAsync(DefaultQuery(), ProjectCode);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetByProjectAsync_MapsFieldsCorrectly()
        {
            var outputs = new List<MonthlyOutput> { MakeOutput(ProjectCode, TestCodeTc01, 3, WorkGroupCsu, 7) };
            var repo = CreateRepository(monthlyOutputs: outputs);

            var result = await repo.GetByProjectAsync(DefaultQuery(), ProjectCode);

            var row = Assert.Single(result.Data);
            Assert.Equal(ProjectCode,  row.Buyer);
            Assert.Equal(TestCodeTc01, row.TestCode);
            Assert.Equal(3,            row.Month);
            Assert.Equal(WorkGroupCsu, row.WorkGroup);
            Assert.Equal(7,            row.Volume);
        }

        [Fact]
        public async Task GetByProjectAsync_AppliesPaging_ReturnsCorrectPage()
        {
            var outputs = Enumerable.Range(1, 15)
                .Select(i => MakeOutput(ProjectCode, $"TC{i:D2}", i, WorkGroupCsu, i))
                .ToList();
            var repo = CreateRepository(monthlyOutputs: outputs);

            var result = await repo.GetByProjectAsync(DefaultQuery(page: 2, pageSize: 5), ProjectCode);

            Assert.Equal(5, result.Data.Count());
        }

        #endregion

        #region GetTotalActualByProjectAsync

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithMatchingRecords_ReturnsSumOfVolumes()
        {
            var outputs = new List<MonthlyOutput>
            {
                MakeOutput(ProjectCode,  TestCodeTc01, 1, WorkGroupCsu, 5),
                MakeOutput(ProjectCode,  TestCodeTc02, 2, WorkGroupCsu, 3),
                MakeOutput(OtherProject, "TC03",       1, WorkGroupCsu, 10)
            };
            var repo = CreateRepository(monthlyOutputs: outputs);

            var (totalVolume, totalCost) = await repo.GetTotalActualByProjectAsync(ProjectCode);

            Assert.Equal(8, totalVolume);
            Assert.Equal(0, totalCost);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithNoMatchingRecords_ReturnsZeroVolume()
        {
            var outputs = new List<MonthlyOutput> { MakeOutput(OtherProject, TestCodeTc01, 1, WorkGroupCsu, 10) };
            var repo = CreateRepository(monthlyOutputs: outputs);

            var (totalVolume, totalCost) = await repo.GetTotalActualByProjectAsync(ProjectCode);

            Assert.Equal(0, totalVolume);
            Assert.Equal(0, totalCost);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTotalActualByProjectAsync_WithEmptyProjectCode_ReturnsZero(string? projectCode)
        {
            var outputs = new List<MonthlyOutput> { MakeOutput(ProjectCode, TestCodeTc01, 1, WorkGroupCsu, 5) };
            var repo = CreateRepository(monthlyOutputs: outputs);

            var (totalVolume, totalCost) = await repo.GetTotalActualByProjectAsync(projectCode!);

            Assert.Equal(0, totalVolume);
            Assert.Equal(0, totalCost);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_TotalCostIsAlwaysZero()
        {
            // Price enrichment happens in the service layer — repository always returns 0 for TotalCost
            var outputs = new List<MonthlyOutput> { MakeOutput(ProjectCode, TestCodeTc01, 1, WorkGroupCsu, 5) };
            var repo = CreateRepository(monthlyOutputs: outputs);

            var (_, totalCost) = await repo.GetTotalActualByProjectAsync(ProjectCode);

            Assert.Equal(0, totalCost);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WhenRecordExists_DeletesAndReturnsTrue()
        {
            var outputs = new List<MonthlyOutput>
            {
                MakeOutput(ProjectCode, TestCodeTc01, 1, WorkGroupCsu, 5),
                MakeOutput(ProjectCode, TestCodeTc02, 2, WorkGroupLt5, 3)
            };
            var repo = CreateRepository(monthlyOutputs: outputs);

            var result = await repo.DeleteAsync(ProjectCode, TestCodeTc01, 1, WorkGroupCsu);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenRecordDoesNotExist_ReturnsFalse()
        {
            var outputs = new List<MonthlyOutput> { MakeOutput(ProjectCode, TestCodeTc01, 1, WorkGroupCsu, 5) };
            var repo = CreateRepository(monthlyOutputs: outputs);

            var result = await repo.DeleteAsync(ProjectCode, "TC99", 99, "NONE");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WithEmptyDataSet_ReturnsFalse()
        {
            var repo = CreateRepository(monthlyOutputs: Array.Empty<MonthlyOutput>());

            var result = await repo.DeleteAsync(ProjectCode, TestCodeTc01, 1, WorkGroupCsu);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenBuyerMismatch_ReturnsFalse()
        {
            var outputs = new List<MonthlyOutput> { MakeOutput(ProjectCode, TestCodeTc01, 1, WorkGroupCsu, 5) };
            var repo = CreateRepository(monthlyOutputs: outputs);

            var result = await repo.DeleteAsync(OtherProject, TestCodeTc01, 1, WorkGroupCsu);

            Assert.False(result);
        }

        #endregion
    }
}
