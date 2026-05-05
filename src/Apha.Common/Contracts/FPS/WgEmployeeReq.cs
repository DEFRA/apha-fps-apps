namespace Apha.Common.Contracts.FPS
{
    public class WgEmployeeReq
    {
        public string PactId { get; set; } = null!;
        public double HrsPaid { get; set; }
        public double Leave { get; set; }
        public double SickSpecial { get; set; }
        public string PersonStatus { get; set; } = null!;
        public string? PersonClass { get; set; }
        public int MakeAvailable { get; set; }
    }
}
