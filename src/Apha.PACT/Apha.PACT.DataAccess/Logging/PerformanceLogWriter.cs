using Apha.PACT.Core.Entities;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Apha.PACT.DataAccess.Logging
{
    /// <summary>
    /// Background service that drains <see cref="IPerformanceLogQueue"/> and batch-inserts
    /// entries into the <c>performance_log</c> table using a short-lived scoped DbContext.
    /// </summary>
    public sealed class PerformanceLogWriter : BackgroundService
    {
        private readonly IPerformanceLogQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PerformanceLogWriter> _logger;

        public PerformanceLogWriter(
            IPerformanceLogQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<PerformanceLogWriter> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var batch = new List<PerformanceLog>(1);

            try
            {
                // Persist every entry immediately as it is dequeued.
                await foreach (var entry in _queue.DequeueAllAsync(stoppingToken))
                {
                    batch.Add(entry);
                    await FlushAsync(batch, stoppingToken);
                    batch.Clear();
                }
            }
            catch (OperationCanceledException)
            {
                // Shutdown requested.
            }
        }

        private async Task FlushAsync(List<PerformanceLog> batch, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FpsDbContext>();

                context.PerformanceLogs.AddRange(batch);
                await context.SaveChangesAsync(cancellationToken);

                foreach (var entry in batch)
                {
                    context.Entry(entry).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist {Count} performance-log entries.", batch.Count);
            }
        }
    }
}
