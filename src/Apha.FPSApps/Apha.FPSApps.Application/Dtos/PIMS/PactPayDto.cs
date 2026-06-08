namespace Apha.FPSApps.Application.Dtos.PIMS
{
    public class PactPayDto
    {
        public short Year { get; set; }
        public string? Project { get; set; }
        public double Month { get; set; }
        public decimal Pay { get; set; }
        public decimal NonPay { get; set; }
        public decimal StaffCosts { get; set; }
        public decimal Overhead { get; set; }
    }
}
