using Apha.FPS.Application.Enums;
using Apha.FPS.Application.Services;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Contract for sending lifecycle notifications (release, rejection, completion, failure, cancellation).
    /// The stub (LogOnlyBulkRatesNotificationService) logs the event without sending any message.
    /// </summary>
    public interface IBulkRatesNotificationService
    {
        Task NotifyAsync(
            BulkRatesNotificationEvent notificationEvent,
            BulkRatesNotificationContext context,
            CancellationToken ct = default);
    }
}
