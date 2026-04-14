namespace Apha.PACT.Core.Entities
{
    public partial class TestRequirementLog
    {
        public int SequenceNo { get; set; }

        public string? TestCode { get; set; }

        public string? Buyer { get; set; }

        public double? UnitPrice { get; set; }

        public int? NoRequired { get; set; }

        public string? ProjectBuyerCode { get; set; }

        public string? TestBuyerCode { get; set; }

        public short? Active { get; set; }

        public DateTime? DateTime { get; set; }

        public string? UserId { get; set; }

        public string? InsertDelete { get; set; }

        /// <summary>
        /// Generated column based on projectbuyercode
        /// </summary>
        public string? JobCode { get; set; }

        public int FpsYear { get; set; }
    }
}
