namespace Apha.PIMS.Core.Entities
{
    /// <summary>Projection result of qryProjectTimeCostCalcs (grouped by Year/Project/Month).</summary>
    public class PactPayCalc
    {
        public short Year { get; set; }
        public string Project { get; set; } = null!;
        public double Month { get; set; }
        public decimal Pay { get; set; }
        public decimal NonPay { get; set; }
        public decimal StaffCosts { get; set; }
        public decimal Overhead { get; set; }
    }
}
