namespace Apha.FPS.Application.Enums
{
    public enum BulkRatesNotificationEvent
    {
        ReleasedForApproval,
        Approved,
        Rejected,
        Cancelled,
        // Worker-owned
        Completed,
        Failed
    }
}
