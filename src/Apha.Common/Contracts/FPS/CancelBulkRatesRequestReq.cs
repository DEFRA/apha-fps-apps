namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request body for cancelling a Bulk Rates request.
    /// </summary>
    public class CancelBulkRatesRequestReq
    {
        public string? Reason { get; set; }
    }
}
