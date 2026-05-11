using System;
using System.Collections.Generic;

namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Master table of fiscal / FPS years. Defines which years exist, their display codes, and their lifecycle status (Open, Closed, Planned).
    /// </summary>
    public partial class YearMaster
    {
        /// <summary>
        /// Four-digit calendar year that starts the fiscal period (e.g. 2025 for Apr 2025 – Mar 2026). Primary key.
        /// </summary>
        public int FpsYear { get; set; }

        /// <summary>
        /// Human-readable fiscal-year label, e.g. FPS2025-26.
        /// </summary>
        public string FpsYearCode { get; set; } = null!;

        /// <summary>
        /// Lifecycle state: Open (transactions allowed), Closed (read-only), or Planned (configuration only).
        /// </summary>
        public string YearStatus { get; set; } = null!;

        /// <summary>
        /// Free-text note explaining the status or special conditions.
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// Soft-delete flag. TRUE = visible to the application.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Timestamp when the row was first inserted (auto-set).
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// User or service account that created the row.
        /// </summary>
        public string? CreatedBy { get; set; }


    }
}
