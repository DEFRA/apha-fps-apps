using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq.Expressions;

namespace Apha.Common.Helpers.Repository
{

    /// <summary>
    /// Provides test infrastructure for mocking IQueryable and IAsyncEnumerable
    /// to enable Entity Framework Core DbSet mocking without database dependencies.
    /// </summary>
    /// <remarks>
    /// No <c>class</c> constraint is applied because LINQ projections inside repositories
    /// can produce value or nullable element types (for example <c>decimal?</c>), and the
    /// query provider re-creates this type for every intermediate projection.
    /// </remarks>
    /// <typeparam name="T">The element type.</typeparam>
    public class TestAsyncEnumerable<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
    {
        private readonly IQueryable<T> _inner;

        public TestAsyncEnumerable(IEnumerable<T> enumerable)
        {
            _inner = enumerable.AsQueryable();
        }

        internal TestAsyncEnumerable(IQueryable<T> queryable)
        {
            _inner = queryable;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(_inner.GetEnumerator());
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType => _inner.ElementType;

        public Expression Expression => _inner.Expression;

        public IQueryProvider Provider => new TestAsyncQueryProvider<T>(_inner.Provider);
    }

    /// <summary>
    /// DbSet mocking helpers for <see cref="TestAsyncEnumerable{T}"/>.
    /// </summary>
    public static class TestAsyncEnumerableExtensions
    {
        /// <summary>
        /// Creates a mocked DbSet from the supplied test data.
        /// </summary>
        public static Mock<DbSet<T>> AsDbSetMock<T>(this TestAsyncEnumerable<T> source) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(source.Provider);

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Expression)
                .Returns(source.Expression);

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.ElementType)
                .Returns(source.ElementType);

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.GetEnumerator())
                .Returns(source.GetEnumerator());

            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(source.GetAsyncEnumerator());

            return mockSet;
        }
    }
}