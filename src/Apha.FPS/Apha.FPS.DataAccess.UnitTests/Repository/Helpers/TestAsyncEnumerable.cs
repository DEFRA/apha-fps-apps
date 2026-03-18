using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Apha.FPS.DataAccess.UnitTests.Repository.Helpers
{
    /// <summary>
    /// Provides test infrastructure for mocking IQueryable and IAsyncEnumerable
    /// to enable Entity Framework Core DbSet mocking without database dependencies.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class TestAsyncEnumerable<T> : IOrderedQueryable<T>, IAsyncEnumerable<T> where T : class
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

        /// <summary>
        /// Creates a mocked DbSet from the current test data.
        /// </summary>
        public Mock<DbSet<T>> AsDbSetMock()
        {
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(Provider);

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Expression)
                .Returns(Expression);

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.ElementType)
                .Returns(ElementType);

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.GetEnumerator())
                .Returns(GetEnumerator());

            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(GetAsyncEnumerator());

            return mockSet;
        }
    }
}