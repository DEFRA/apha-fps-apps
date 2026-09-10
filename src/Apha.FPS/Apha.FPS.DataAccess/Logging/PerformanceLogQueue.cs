using System.Threading.Channels;
using Apha.FPS.Core.Entities;

namespace Apha.FPS.DataAccess.Logging
{
    /// <summary>
    /// Bounded, thread-safe queue used to hand performance-log entries off the request/query
    /// hot path to a background writer. Enqueue never blocks the caller: if the queue is full
    /// the entry is dropped (diagnostics data is best-effort, correctness of the app comes first).
    /// </summary>
    public interface IPerformanceLogQueue
    {
        void Enqueue(PerformanceLog entry);

        IAsyncEnumerable<PerformanceLog> DequeueAllAsync(CancellationToken cancellationToken);
    }

    public sealed class PerformanceLogQueue : IPerformanceLogQueue
    {
        private readonly Channel<PerformanceLog> _channel;

        public PerformanceLogQueue(int capacity = 10_000)
        {
            _channel = Channel.CreateBounded<PerformanceLog>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true,
                SingleWriter = false
            });
        }

        public void Enqueue(PerformanceLog entry)
        {
            _channel.Writer.TryWrite(entry);
        }

        public IAsyncEnumerable<PerformanceLog> DequeueAllAsync(CancellationToken cancellationToken)
            => _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
