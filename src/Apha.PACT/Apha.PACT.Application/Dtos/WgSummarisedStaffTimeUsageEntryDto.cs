namespace Apha.PACT.Application.Dtos
{
    /// <summary>
    /// Flat DTO representing one raw row returned by the repository for the summarised
    /// staff time-usage query. Maps 1-to-1 from <c>WgSummarisedStaffTimeUsageView</c>
    /// so that the Application service layer is fully decoupled from the EF-mapped entity.
    /// </summary>
    public class WgSummarisedStaffTimeUsageEntryDto
    {
        public string? MonthName { get; set; }
        public string? Name { get; set; }
        public double? HrsPaid { get; set; }
        public string? ParentProject { get; set; }
        public string? JobCode { get; set; }
        public string? JobTitle { get; set; }
        public double? TotalTime { get; set; }
        public double? TotalCost { get; set; }
    }
}
