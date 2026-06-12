namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class StaffWorkgroupLookupDto
    {
        public string StaffID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string WorkGroupGrade { get; set; } = string.Empty;
        public double HrsAvail { get; set; }
        public double HrsPaid { get; set; }
        public double Leave { get; set; }
        public double SickSpecial { get; set; }
    }
}
