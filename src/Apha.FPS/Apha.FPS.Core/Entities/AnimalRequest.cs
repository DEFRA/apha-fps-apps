namespace Apha.FPS.Core.Entities
{
    public partial class AnimalRequest
    {
        public string JobCode { get; set; } = null!;

        public string AnimalType { get; set; } = null!;

        public double NumberOfDays { get; set; }

        public double NumberOfAnimals { get; set; }

        public int IndCounter { get; set; }

        public int? FpsYear { get; set; }

    }
}


