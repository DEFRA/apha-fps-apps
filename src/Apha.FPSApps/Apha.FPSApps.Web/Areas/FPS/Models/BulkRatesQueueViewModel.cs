using System.ComponentModel.DataAnnotations;
using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class BulkRatesQueueViewModel
    {
        /// <summary>Grid config built explicitly in BulkRatesController — never left as new().</summary>
        public DataGridConfig<BulkRatesQueueGridItem> Grid { get; set; } = new();
        public string? JobNameFilter { get; set; }
        public int? FpsYearFilter { get; set; }
        public string? StatusFilter { get; set; }
        public string CurrentUserEmail { get; set; } = string.Empty;

        /// <summary>
        /// True when a new JobNameFilter request may be created — false when a blocking request
        /// already exists, or when the check itself failed (fails closed; see ActiveRequestCheckError
        /// for which case it was). Only meaningful when JobNameFilter is set.
        /// </summary>
        public bool CanInitiateRequest { get; set; } = true;

        /// <summary>
        /// Set when the CanInitiateRequestAsync check itself failed (Success == false) — distinct
        /// from CanInitiateRequest == false meaning "a request genuinely exists". Never say "an
        /// active request already exists" when this is what actually happened.
        /// </summary>
        public string? ActiveRequestCheckError { get; set; }

        /// <summary>True when this page was reached via a rate-type-locked entry point — hides the "Job type" picker.</summary>
        public bool IsJobNameLocked { get; set; }

        /// <summary>
        /// True when the app-wide selected FPS year's status matches what JobNameFilter requires
        /// (Open for FEC Test Rates, Planned for Staff/Animal Rates) — false blocks "New Request".
        /// Always true when JobNameFilter is empty/unrecognised (no single job type to gate against).
        /// </summary>
        public bool CanCreateForYear { get; set; } = true;

        /// <summary>The yearstatus JobNameFilter requires (e.g. "Open"), for the blocked-state message.</summary>
        public string? RequiredYearStatus { get; set; }

        /// <summary>The app-wide selected FPS year's actual current status, for the blocked-state message.</summary>
        public string? CurrentYearStatus { get; set; }

        /// <summary>
        /// Set when the API call behind the grid failed (Success == false) — the grid then
        /// renders empty with no other indication, so this is shown as a banner instead of
        /// silently looking like "genuinely no requests".
        /// </summary>
        public string? GridLoadError { get; set; }
    }

    /// <summary>
    /// DataGrid row model for the Bulk Rates request queue.
    /// Property names must match BulkRatesQueueEntryDto for FpsViewModelMapper's AutoMapper profile.
    /// </summary>
    public class BulkRatesQueueGridItem
    {
        [Display(Name = "Request ID")]
        [GridColumn(Order = 1, Width = 320, Type = GridColumnType.Text, IsFilterable = false)]
        public Guid JobExecutionId { get; set; }

        [Display(Name = "Job Type")]
        [GridColumn(Order = 2, Width = 160, Type = GridColumnType.Text, IsFilterable = false)]
        public string JobName { get; set; } = string.Empty;

        [Display(Name = "Status")]
        [GridColumn(Order = 3, Width = 160, Type = GridColumnType.Text, IsFilterable = false)]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Requested By")]
        [GridColumn(Order = 4, Width = 220, Type = GridColumnType.Text, IsFilterable = false)]
        public string RequestedBy { get; set; } = string.Empty;

        [Display(Name = "Requested At (UTC)")]
        [GridColumn(Order = 5, Width = 170, Type = GridColumnType.DateTime, IsFilterable = false)]
        public DateTime RequestedAtUtc { get; set; }
    }
}
