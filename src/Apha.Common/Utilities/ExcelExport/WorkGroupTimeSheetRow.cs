namespace Apha.Common.Utilities.ExcelExport
{
    public class WorkGroupTimeSheetRow
    {
        public string StaffName { get; set; } = string.Empty;
        public string TimeCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ParentProject { get; set; } = string.Empty;
        public short Month { get; set; }
    }
}
