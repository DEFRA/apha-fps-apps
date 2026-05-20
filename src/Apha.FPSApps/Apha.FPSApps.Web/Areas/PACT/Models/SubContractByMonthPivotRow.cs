namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// Flat row model for the monthly invoices pivot DataGrid.
    /// M1–M12 correspond to calendar months Jan–Dec.
    /// Reflection in GridHelpers.GetPropertyValue binds column PropertyName ("M1"…"M12") to these properties.
    /// </summary>
    public class SubContractByMonthPivotRow
    {
        public string Program { get; set; } = string.Empty;
        public string ParentProject { get; set; } = string.Empty;
        public decimal M1 { get; set; }
        public decimal M2 { get; set; }
        public decimal M3 { get; set; }
        public decimal M4 { get; set; }
        public decimal M5 { get; set; }
        public decimal M6 { get; set; }
        public decimal M7 { get; set; }
        public decimal M8 { get; set; }
        public decimal M9 { get; set; }
        public decimal M10 { get; set; }
        public decimal M11 { get; set; }
        public decimal M12 { get; set; }
    }
}
