using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Moq;
using Newtonsoft.Json;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.TestActualBreakdownRepositoryTest
{
    public class TestActualBreakdownRepositoryTests
    {
        private static TestActualBreakdownRepository CreateRepository(
            IEnumerable<TestActualBreakdownView>? views = null)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            var mockContext       = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var mockDbSet         = RepositoryTestHelper.CreateMockDbSet(views ?? []);

            mockContext.Setup(x => x.TestActualBreakdownViews).Returns(mockDbSet.Object);

            return new TestActualBreakdownRepository(mockContext.Object);
        }

        private static List<TestActualBreakdownView> BuildViews() =>
        [
            new() { TestCode = "PT0047",  Buyer = "SV3300",   Program = "Viro",  Portfolio = "QAPTPORT1", WorkGroup = "QASB", Month = 4, PCPrice = 159.00m, PCCost = 319.00m, ShortDescription = "EVA serology",    ProfitCentre = "Comm", FpsYear = 2025 },
            new() { TestCode = "PT0049",  Buyer = "SB4600",   Program = "Bact",  Portfolio = "QAPTPORT1", WorkGroup = "QASB", Month = 4, PCPrice = 313.00m, PCCost = 313.00m, ShortDescription = "Glanders serology",ProfitCentre = "Comm", FpsYear = 2025 },
            new() { TestCode = "TC0001A", Buyer = "EDI300",   Program = "SIU",   Portfolio = "TG0100",    WorkGroup = "SVCA", Month = 2, PCPrice = 155.10m, PCCost = 155.10m, ShortDescription = "PM-Avian sngl",    ProfitCentre = "SLSD", FpsYear = 2025 },
            new() { TestCode = "TC0001A", Buyer = "EDI300",   Program = "SIU",   Portfolio = "TG0100",    WorkGroup = "SVCA", Month = 1, PCPrice = 155.10m, PCCost = 155.10m, ShortDescription = "PM-Avian sngl",    ProfitCentre = "SLSD", FpsYear = 2025 },
            new() { TestCode = "TC0001A", Buyer = "CSUT1313", Program = "LabT",  Portfolio = "TG0100",    WorkGroup = "SVSH", Month = 2, PCPrice = 155.10m, PCCost = 310.20m, ShortDescription = "PM-Avian sngl",    ProfitCentre = "SLSD", FpsYear = 2025 },
        ];

        private static string AsFilter(object obj) => JsonConvert.SerializeObject(obj);

        // ── Paging ────────────────────────────────────────────────────────────

        #region Paging

        [Fact]
        public async Task GetPagedAsync_NoFilter_ReturnsAllRecords()
        {
            var repo   = CreateRepository(BuildViews());
            var query  = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedAsync(query);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedAsync_RespectsPageSize()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 2 };

            var result = await repo.GetPagedAsync(query);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedAsync_SecondPage_ReturnsCorrectItems()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetPagedAsync(query);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedAsync_LastPage_ReturnsRemainingItems()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 3, PageSize = 2 };

            var result = await repo.GetPagedAsync(query);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedAsync_EmptySource_ReturnsEmptyData()
        {
            var repo  = CreateRepository([]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedAsync(query);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedAsync_PaginationData_IsCorrect()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 2 };

            var result = await repo.GetPagedAsync(query);

            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(2, result.PaginationData.PageSize);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        #endregion

        // ── Filter – early-exit paths ─────────────────────────────────────────

        #region ApplyFilter – null / empty / no-op paths

        [Fact]
        public async Task GetPagedAsync_NullFilter_ReturnsAllRecords()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetPagedAsync(query);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedAsync_EmptyStringFilter_ReturnsAllRecords()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "" };

            var result = await repo.GetPagedAsync(query);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedAsync_WhitespaceFilter_ReturnsAllRecords()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "   " };

            var result = await repo.GetPagedAsync(query);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedAsync_EmptyJsonObjectFilter_ReturnsAllRecords()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };

            var result = await repo.GetPagedAsync(query);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedAsync_FilterWithAllEmptyStringValues_ReturnsAllRecords()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = AsFilter(new { TestCode = "", Buyer = "", Program = "", Portfolio = "", WorkGroup = "", ProfitCentre = "", ShortDescription = "" })
            };

            var result = await repo.GetPagedAsync(query);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedAsync_NullDeserializedFilter_ReturnsAllRecords()
        {
            // "null" deserialises to a null Dictionary → early-exit path
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "null" };

            var result = await repo.GetPagedAsync(query);

            Assert.Equal(5, result.Data.Count);
        }

        #endregion

        // ── Sorting ───────────────────────────────────────────────────────────

        #region ApplySorting

        [Theory]
        [InlineData("testcode",         false)]
        [InlineData("testcode",         true)]
        [InlineData("shortdescription", false)]
        [InlineData("shortdescription", true)]
        [InlineData("program",          false)]
        [InlineData("program",          true)]
        [InlineData("buyer",            false)]
        [InlineData("buyer",            true)]
        [InlineData("portfolio",        false)]
        [InlineData("portfolio",        true)]
        [InlineData("workgroup",        false)]
        [InlineData("workgroup",        true)]
        [InlineData("month",            false)]
        [InlineData("month",            true)]
        [InlineData("pcprice",          false)]
        [InlineData("pcprice",          true)]
        [InlineData("pccost",           false)]
        [InlineData("pccost",           true)]
        [InlineData("profitcentre",     false)]
        [InlineData("profitcentre",     true)]
        public async Task GetPagedAsync_Sort_ReturnsAllRecords(string sortBy, bool descending)
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            var result = await repo.GetPagedAsync(query);

            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedAsync_SortByTestCode_Ascending_OrdersCorrectly()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "testcode", Descending = false };

            var result = await repo.GetPagedAsync(query);

            var codes = result.Data.Select(x => x.TestCode).ToList();
            Assert.Equal(codes.OrderBy(x => x).ToList(), codes);
        }

        [Fact]
        public async Task GetPagedAsync_SortByTestCode_Descending_OrdersCorrectly()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "testcode", Descending = true };

            var result = await repo.GetPagedAsync(query);

            var codes = result.Data.Select(x => x.TestCode).ToList();
            Assert.Equal(codes.OrderByDescending(x => x).ToList(), codes);
        }

        [Fact]
        public async Task GetPagedAsync_SortByMonth_Ascending_OrdersCorrectly()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "month", Descending = false };

            var result = await repo.GetPagedAsync(query);

            var months = result.Data.Select(x => x.Month).ToList();
            Assert.Equal(months.OrderBy(x => x).ToList(), months);
        }

        [Fact]
        public async Task GetPagedAsync_SortByMonth_Descending_OrdersCorrectly()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "month", Descending = true };

            var result = await repo.GetPagedAsync(query);

            var months = result.Data.Select(x => x.Month).ToList();
            Assert.Equal(months.OrderByDescending(x => x).ToList(), months);
        }

        [Fact]
        public async Task GetPagedAsync_SortByPCCost_Ascending_OrdersCorrectly()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "pccost", Descending = false };

            var result = await repo.GetPagedAsync(query);

            var vals = result.Data.Select(x => x.PCCost).ToList();
            Assert.Equal(vals.OrderBy(x => x).ToList(), vals);
        }

        [Fact]
        public async Task GetPagedAsync_SortByPCCost_Descending_OrdersCorrectly()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "pccost", Descending = true };

            var result = await repo.GetPagedAsync(query);

            var vals = result.Data.Select(x => x.PCCost).ToList();
            Assert.Equal(vals.OrderByDescending(x => x).ToList(), vals);
        }

        [Fact]
        public async Task GetPagedAsync_NullSortBy_DefaultsSortToTestCode_Ascending()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = null, Descending = false };

            var result = await repo.GetPagedAsync(query);

            var codes = result.Data.Select(x => x.TestCode).ToList();
            Assert.Equal(codes.OrderBy(x => x).ToList(), codes);
        }

        [Fact]
        public async Task GetPagedAsync_UnknownSortBy_DefaultsSortToTestCode_Ascending()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unknown_column", Descending = false };

            var result = await repo.GetPagedAsync(query);

            var codes = result.Data.Select(x => x.TestCode).ToList();
            Assert.Equal(codes.OrderBy(x => x).ToList(), codes);
        }

        [Fact]
        public async Task GetPagedAsync_UnknownSortBy_DefaultsSortToTestCode_Descending()
        {
            var repo  = CreateRepository(BuildViews());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unknown_column", Descending = true };

            var result = await repo.GetPagedAsync(query);

            var codes = result.Data.Select(x => x.TestCode).ToList();
            Assert.Equal(codes.OrderByDescending(x => x).ToList(), codes);
        }

        #endregion
    }
}
