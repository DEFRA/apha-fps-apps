namespace Apha.Common.Contracts.PACT
{
    public class WorkGroupTimeCodeRes
    {
        public string? PACTStaffID { get; set; }
        public string? ParentProject { get; set; }
        public string? WorkGroup { get; set; }
        public string? Name { get; set; }
        public string? TimeCode { get; set; } 
        public double Month { get; set; }
        public double Hours { get; set; }
    }
}