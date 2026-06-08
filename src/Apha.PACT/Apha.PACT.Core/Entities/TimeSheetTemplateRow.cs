namespace Apha.PACT.Core.Entities
{   
    public class TimeSheetTemplateRow
    {
        public string StaffName { get; set; } = string.Empty;
        public string TimeCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ParentProject { get; set; } = string.Empty;
        public short Month { get; set; }
        public double? Hours { get; set; }
    }
}
