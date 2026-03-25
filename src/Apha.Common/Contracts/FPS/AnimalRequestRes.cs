namespace Apha.Common.Contracts.FPS
{
    public class AnimalRequestRes
    {
        public string JobCode { get; set; } = null!;

        public string AnimalType { get; set; } = null!;

        public double NumberOfDays { get; set; }

        public double NumberOfAnimals { get; set; }

        public int IndCounter { get; set; }

        public int? FpsCalYear { get; set; }
    }
}
