namespace Apha.PACT.Core.Entities
{
    /// <summary>
    /// Query projection for the blank time-sheet template sent to work group members.
    ///
    /// Flat-file (layout=1) — mirrors Access ldoMakeTimeSheet layout=1:
    ///   SELECT TimeCodeValid.WorkGroup, tblStaff.Name, TimeCodeValid.TimeCode,
    ///          TimeCodeValid.ParentProject, &lt;month&gt; AS Month, Null AS Hours
    ///   FROM (TimeCodeValid INNER JOIN WorkGroupGrade ...) INNER JOIN tblStaff ...
    ///   WHERE TimeCodeValid.WorkGroup = ? AND TimeCodeValid.Active &lt;&gt; 0
    ///   ORDER BY WorkGroup, Name, TimeCode, ParentProject
    ///
    /// Cross-tab (layout=2) — mirrors Access TRANSFORM Null AS Hours ... PIVOT tblStaff.Name:
    ///   Rows    = (TimeCode, Description, ParentProject)  grouped
    ///   Description = IIf(IsNull(JobCode), ItemDescription, JobCodeName)
    ///   StaffName   = comma-separated distinct staff names for the group (pivot column headers)
    /// </summary>
    public class TimeSheetTemplateRow
    {
        public string StaffName { get; set; } = string.Empty;
        public string TimeCode { get; set; } = string.Empty;
        /// <summary>Cross-tab only: IIf(IsNull(JobCode), ItemDescription, JobCodeName)</summary>
        public string? Description { get; set; }
        public string ParentProject { get; set; } = string.Empty;
        public short Month { get; set; }
        public double? Hours { get; set; }  // always null — recipient fills in
    }
}
