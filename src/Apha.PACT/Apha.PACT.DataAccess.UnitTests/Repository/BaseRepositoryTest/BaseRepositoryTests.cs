using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.BaseRepositoryTest
{
    public class BaseRepositoryTests
    {
        /// <summary>
        /// Concrete subclass that exposes the protected ApplyPaging method for testing.
        /// </summary>
        private sealed class TestRepository : BaseRepository
        {
            public TestRepository(FpsDbContext context) : base(context) { }

            public PagedData<T> TestApplyPaging<T>(IEnumerable<T> source, int page, int pageSize)
                => ApplyPaging(source, page, pageSize);
        }

        private static TestRepository CreateRepository()
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            return new TestRepository(mockContext.Object);
        }

        #region Constructor

        [Fact]
        public void Constructor_NullContext_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TestRepository(null!));
        }

        #endregion

        #region ApplyPaging

        [Fact]
        public void ApplyPaging_FirstPage_ReturnsCorrectSlice()
        {
            var repo = CreateRepository();
            var source = Enumerable.Range(1, 25).ToList();

            var result = repo.TestApplyPaging(source, page: 1, pageSize: 10);

            Assert.Equal(10, result.Data.Count);
            Assert.Equal(1, result.Data.First());
            Assert.Equal(10, result.Data.Last());
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
            Assert.Equal(25, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public void ApplyPaging_LastPage_ReturnsRemainingItems()
        {
            var repo = CreateRepository();
            var source = Enumerable.Range(1, 25).ToList();

            var result = repo.TestApplyPaging(source, page: 3, pageSize: 10);

            Assert.Equal(5, result.Data.Count);
            Assert.Equal(21, result.Data.First());
            Assert.Equal(25, result.Data.Last());
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public void ApplyPaging_EmptySource_ReturnsEmptyWithZeroPagination()
        {
            var repo = CreateRepository();

            var result = repo.TestApplyPaging(new List<int>(), page: 1, pageSize: 10);

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
            Assert.Equal(0, result.PaginationData.TotalPages);
        }

        [Fact]
        public void ApplyPaging_PageSizeLargerThanSource_ReturnsAllItems()
        {
            var repo = CreateRepository();
            var source = Enumerable.Range(1, 5).ToList();

            var result = repo.TestApplyPaging(source, page: 1, pageSize: 100);

            Assert.Equal(5, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.TotalPages);
        }

        #endregion
    }
}
