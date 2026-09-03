namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request body for creating a new Bulk Rates request.
    /// </summary>
    public class CreateBulkRatesRequestReq
    {
        public string JobName { get; set; } = string.Empty;
        public int FpsYear { get; set; }
    }
}
