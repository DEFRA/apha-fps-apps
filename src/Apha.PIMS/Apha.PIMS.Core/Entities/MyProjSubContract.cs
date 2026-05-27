namespace Apha.PIMS.Core.Entities
{
    public partial class MyProjSubContract
    {
        public short Year { get; set; }

        public int Subcontcounter { get; set; }

        public string? Project { get; set; }

        public string? Testjob { get; set; }

        public double? Month { get; set; }

        public decimal? Amount { get; set; }

        public string? Workgroup { get; set; }

        public string? Acctcode { get; set; }

        public string? Supplier { get; set; }

        public string? Description { get; set; }

        public int? Suppliernumber { get; set; }

        public decimal? Dailyrate { get; set; }

        public int? Animaldays { get; set; }
    }
}
