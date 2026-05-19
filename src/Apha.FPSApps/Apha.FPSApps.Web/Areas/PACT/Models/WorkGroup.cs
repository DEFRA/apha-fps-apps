namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// View model for a single item in the Work Group selection dropdown.
    /// </summary>
    public class WorkGroup
    {
        public string WorkGroupName { get; set; } = null!;
        public string? ProfitCentre { get; set; }
    }
}
