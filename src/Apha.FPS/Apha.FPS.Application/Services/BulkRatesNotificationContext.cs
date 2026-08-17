using Apha.FPS.Core.Entities;

namespace Apha.FPS.Application.Services
{
    public class BulkRatesNotificationContext
    {
        public Guid JobQueueId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public int FpsYear { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string? ApprovedBy { get; set; }
        public string? Reason { get; set; }
        public BulkRatesRowCounts? RowCounts { get; set; }
    }
}
