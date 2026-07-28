namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Flat row model for the Budget Bids cross-tab (BBQuery) DataGrid.
    /// Mirrors <c>BuildBudgetBidsCrosstabExcel</c>: a fixed account column, a row summary,
    /// and a dynamic set of workgroup columns whose values are resolved at runtime.
    /// The <see cref="Values"/> dictionary is read by GridHelpers.GetPropertyValue for the
    /// dynamic workgroup columns (keyed by column PropertyName).
    /// </summary>
    public class BBQueryCrosstabRow
    {
        public string AccShortName { get; set; } = string.Empty;

        public decimal RowSummary { get; set; }

        public IDictionary<string, object?> Values { get; } = new Dictionary<string, object?>();
    }
}
