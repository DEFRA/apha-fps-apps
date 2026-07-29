using System.Linq.Expressions;

namespace Apha.Common.Helpers.Repository
{
    internal sealed class TestAsyncQueryable<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
    {
        private readonly IQueryable<T> _inner;

        public TestAsyncQueryable(IEnumerable<T> enumerable)
        {
            _inner = enumerable.AsQueryable();
        }

        internal TestAsyncQueryable(IQueryable<T> queryable)
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
}
