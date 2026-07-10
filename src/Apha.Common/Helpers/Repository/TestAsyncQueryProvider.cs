using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Apha.Common.Helpers.Repository
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
            var rewritten = LikeRewriter.Rewrite(expression);
            var elementType = rewritten.Type.GetGenericArguments().FirstOrDefault() ?? typeof(T);
            var enumerable = _inner.CreateQuery(rewritten);

            if (elementType.IsValueType || (Nullable.GetUnderlyingType(elementType) != null))
            {
                var valueEnumerableType = typeof(TestAsyncValueEnumerable<>).MakeGenericType(elementType);
                var ctor = valueEnumerableType.GetConstructor(
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                    null,
                    new[] { typeof(IQueryable<>).MakeGenericType(elementType) },
                    null);
                return (IQueryable)ctor!.Invoke(new object[] { enumerable })!;
            }

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
            var rewritten = LikeRewriter.Rewrite(expression);
            var query = _inner.CreateQuery<TElement>(rewritten);

            if (typeof(TElement).IsValueType || Nullable.GetUnderlyingType(typeof(TElement)) != null)
                return new TestAsyncValueEnumerable<TElement>(query);

            var asyncEnumerableType = typeof(TestAsyncEnumerable<>).MakeGenericType(typeof(TElement));
            return (IQueryable<TElement>)Activator.CreateInstance(asyncEnumerableType, query)!;
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(LikeRewriter.Rewrite(expression));
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(LikeRewriter.Rewrite(expression));
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var rewritten = LikeRewriter.Rewrite(expression);
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executeMethod = typeof(IQueryProvider)
                .GetMethods()
                .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(resultType);

            var result = executeMethod.Invoke(_inner, new object[] { rewritten });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { result })!;
        }

        /// <summary>
        /// Rewrites EF.Functions.ILike(col, "%pattern%") calls into
        /// col.ToLower().Contains("pattern") for client-side LINQ evaluation in tests.
        /// </summary>
        private sealed class LikeRewriter : ExpressionVisitor
        {
            private const string LikeMethodName = "ILike";

            public static Expression Rewrite(Expression expression)
                => new LikeRewriter().Visit(expression);

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == LikeMethodName &&
                    node.Arguments.Count == 3) // (DbFunctions, matchExpression, pattern)
                {
                    // Extract the column expression and the pattern string
                    var matchExpression = Visit(node.Arguments[1]); // e.g. staff.Name
                    // The pattern may be a constant or a closure-captured variable (e.g. $"%{value}%").
                    // Evaluate it to a runtime string either way.
                    var patternArg = Visit(node.Arguments[2]);
                    string? pattern = patternArg is ConstantExpression constExpr
                        ? constExpr.Value as string
                        : Expression.Lambda<Func<string>>(patternArg).Compile()();

                    if (pattern != null)
                    {
                        // Strip leading/trailing % wildcards → "%general%" becomes "general"
                        var keyword = pattern.Trim('%').ToLower();

                        // Build: matchExpression.ToLower().Contains(keyword)
                        var toLower = Expression.Call(
                            matchExpression,
                            typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!);

                        var contains = Expression.Call(
                            toLower,
                            typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                            Expression.Constant(keyword));

                        return contains;
                    }
                }

                return base.VisitMethodCall(node);
            }
        }
    }
}