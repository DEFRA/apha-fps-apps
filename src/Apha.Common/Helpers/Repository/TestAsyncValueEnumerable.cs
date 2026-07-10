using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Apha.Common.Helpers.Repository
{
    /// <summary>
    /// Test implementation of IOrderedQueryable and IAsyncEnumerable that supports
    /// both reference types and value types (including nullable value types such as
    /// double? and decimal?) produced by intermediate LINQ projections.
    /// Unlike <see cref="TestAsyncEnumerable{T}"/>, this class has no T : class
    /// constraint and therefore cannot provide AsDbSetMock().
    /// </summary>
    /// <typeparam name="T">The element type, which may be a value type.</typeparam>
    internal class TestAsyncValueEnumerable<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
    {
        private readonly IQueryable<T> _inner;

        internal TestAsyncValueEnumerable(IQueryable<T> queryable)
        {
            _inner = queryable;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncValueEnumerator<T>(_inner.GetEnumerator());

        public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public Type ElementType => _inner.ElementType;

        public Expression Expression => _inner.Expression;

        public IQueryProvider Provider => new TestAsyncValueQueryProvider<T>(_inner.Provider);
    }

    internal class TestAsyncValueEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        internal TestAsyncValueEnumerator(IEnumerator<T> enumerator)
        {
            _inner = enumerator;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Query provider that wraps <see cref="TestAsyncValueEnumerable{T}"/> so that
    /// subsequent LINQ projections on value-type sequences continue to work.
    /// </summary>
    internal class TestAsyncValueQueryProvider<T> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncValueQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetGenericArguments().FirstOrDefault() ?? typeof(T);
            var query = _inner.CreateQuery(expression);
            var enumerableType = typeof(TestAsyncValueEnumerable<>).MakeGenericType(elementType);
            var constructor = enumerableType.GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null,
                [typeof(IQueryable<>).MakeGenericType(elementType)],
                null);
            return (IQueryable)constructor!.Invoke([query])!;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var query = _inner.CreateQuery<TElement>(expression);
            return new TestAsyncValueEnumerable<TElement>(query);
        }

        public object? Execute(Expression expression) => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executeMethod = typeof(IQueryProvider)
                .GetMethods()
                .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(resultType);

            var result = executeMethod.Invoke(_inner, [expression]);

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, [result])!;
        }
    }
}
