using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.UnitTests.Repository.Helpers
{
    /// <summary>
    /// Test implementation of IQueryProvider for supporting async LINQ operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class TestAsyncQueryProvider<T> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetGenericArguments().FirstOrDefault() ?? typeof(T);
            var enumerable = _inner.CreateQuery(expression);
            
            var asyncEnumerableType = typeof(TestAsyncEnumerable<>).MakeGenericType(elementType);
            var constructor = asyncEnumerableType.GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null,
                new[] { typeof(IQueryable<>).MakeGenericType(elementType) },
                null);
            
            return (IQueryable)constructor!.Invoke(new object[] { enumerable })!;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var query = _inner.CreateQuery<TElement>(expression);
            // NEW (uses reflection):
            var asyncEnumerableType = typeof(TestAsyncEnumerable<>).MakeGenericType(typeof(TElement));
            return (IQueryable<TElement>)Activator.CreateInstance(asyncEnumerableType, query)!;
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executeMethod = typeof(IQueryProvider)
                .GetMethods()
                .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(resultType);

            var result = executeMethod.Invoke(_inner, new object[] { expression });
            
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { result })!;
        }
    }
}