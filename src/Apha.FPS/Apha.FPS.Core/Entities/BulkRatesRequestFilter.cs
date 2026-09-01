namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Filter fields for GetRequestsAsync's paged queue listing — the <c>TFilter</c> of
    /// <see cref="Apha.FPS.Core.Pagination.PaginationParameters{TFilter}"/> for Bulk Rates.
    /// </summary>
    public sealed record BulkRatesRequestFilter(
        string? JobName,
        int? FpsYear,
        string? Status);
}
