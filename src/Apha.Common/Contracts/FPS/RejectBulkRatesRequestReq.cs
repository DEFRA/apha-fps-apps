namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request body for rejecting a Bulk Rates request.
    /// </summary>
    public class RejectBulkRatesRequestReq
    {
        public string Reason { get; set; } = string.Empty;
    }
}
