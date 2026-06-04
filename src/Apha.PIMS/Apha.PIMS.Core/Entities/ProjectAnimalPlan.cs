namespace Apha.PIMS.Core.Entities
{
    public partial class ProjectAnimalPlan
    {
        public short? Year { get; set; }

        public string? Parentproject { get; set; }

        public string? Animaltype { get; set; }

        public double? Numberofdays { get; set; }

        public double? Numberofanimals { get; set; }

        public decimal? Rate { get; set; }

        public decimal? Cost { get; set; }
    }
}
