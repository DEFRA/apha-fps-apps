using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using System.Linq.Expressions;

namespace Apha.PACT.DataAccess.UnitTests.Repository.BaseRepositoryTest
{
    public class BaseRepositoryTests
    {
        /// <summary>
        /// Async-capable queryable wrapper for value types (e.g. int). Unlike TestAsyncEnumerable&lt;T&gt;
        /// this has no 'where T : class' constraint, so it works with EF Core async operators
        /// (CountAsync / ToListAsync) in tests that use primitive sequences.
        /// </summary>
        private sealed class AsyncQueryable<T> : IQueryable<T>, IAsyncEnumerable<T>
        {
            private readonly IQueryable<T> _inner;

            public AsyncQueryable(IEnumerable<T> source) : this(source.AsQueryable()) { }

            private AsyncQueryable(IQueryable<T> inner)
            {
                _inner = inner;
                Provider = new AsyncProvider(inner.Provider);
                Expression = inner.Expression;
            }

            public Type ElementType => typeof(T);
            public Expression Expression { get; }
            public IQueryProvider Provider { get; }

            public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _inner.GetEnumerator();

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new AsyncEnumerator(_inner.GetEnumerator());

            private sealed class AsyncProvider : IAsyncQueryProvider
            {
                private readonly IQueryProvider _inner;
                public AsyncProvider(IQueryProvider inner) => _inner = inner;

                public IQueryable CreateQuery(Expression expression) => _inner.CreateQuery(expression);

                public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
                {
                    var q = _inner.CreateQuery<TElement>(expression);
                    if (q is IQueryable<T> sameType)
                        return (IQueryable<TElement>)(object)new AsyncQueryable<T>(sameType);
                    var ctor = typeof(AsyncQueryable<>)
                        .MakeGenericType(typeof(TElement))
                        .GetConstructor(
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                            null, new[] { typeof(IQueryable<>).MakeGenericType(typeof(TElement)) }, null);
                    return (IQueryable<TElement>)ctor!.Invoke(new object[] { q });
                }

                public object? Execute(Expression expression) => _inner.Execute(expression);
                public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

                public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
                {
                    var resultType = typeof(TResult).GetGenericArguments()[0];
                    var result = typeof(IQueryProvider)
                        .GetMethods()
                        .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethodDefinition)
                        .MakeGenericMethod(resultType)
                        .Invoke(_inner, new object[] { expression });
                    return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                        .MakeGenericMethod(resultType)
                        .Invoke(null, new[] { result })!;
                }
            }

            private sealed class AsyncEnumerator : IAsyncEnumerator<T>
            {
                private readonly IEnumerator<T> _inner;
                public AsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
                public T Current => _inner.Current;
                public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
                public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
            }
        }

        /// <summary>
        /// Concrete subclass that exposes the protected ApplyPaging method for testing.
        /// </summary>
        private sealed class TestRepository : BaseRepository
        {
            public TestRepository(FpsDbContext context) : base(context) { }

            public async Task<PagedData<T>> TestApplyPaging<T>(IQueryable<T> source, int page, int pageSize)
                => await ApplyPaging(source, page, pageSize);
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
        public async Task ApplyPaging_FirstPage_ReturnsCorrectSlice()
        {
            var repo = CreateRepository();
            var source = new AsyncQueryable<int>(Enumerable.Range(1, 25));

            var result = await repo.TestApplyPaging(source, page: 1, pageSize: 10);

            Assert.Equal(10, result.Data.Count);
            Assert.Equal(1, result.Data.First());
            Assert.Equal(10, result.Data.Last());
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
            Assert.Equal(25, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task ApplyPaging_LastPage_ReturnsRemainingItems()
        {
            var repo = CreateRepository();
            var source = new AsyncQueryable<int>(Enumerable.Range(1, 25));

            var result =await repo.TestApplyPaging(source, page: 3, pageSize: 10);

            Assert.Equal(5, result.Data.Count);
            Assert.Equal(21, result.Data.First());
            Assert.Equal(25, result.Data.Last());
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task ApplyPaging_EmptySource_ReturnsEmptyWithZeroPagination()
        {
            var repo = CreateRepository();

            var result = await repo.TestApplyPaging(new AsyncQueryable<int>(Enumerable.Empty<int>()), page: 1, pageSize: 10);

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
            Assert.Equal(0, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task ApplyPaging_PageSizeLargerThanSource_ReturnsAllItems()
        {
            var repo = CreateRepository();
            var source = new AsyncQueryable<int>(Enumerable.Range(1, 5));

            var result = await repo.TestApplyPaging(source, page: 1, pageSize: 100);

            Assert.Equal(5, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.TotalPages);
        }

        #endregion
    }
}
