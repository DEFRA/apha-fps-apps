using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.FPS.DataAccess.UnitTests.Repository.Helpers
{
    /// <summary>
    /// Test implementation of IAsyncEnumerator for mocking async enumeration.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }
}