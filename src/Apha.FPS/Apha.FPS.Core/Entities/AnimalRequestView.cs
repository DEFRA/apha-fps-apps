namespace Apha.FPS.Core.Entities
{
    public partial class AnimalRequestView
    {
        public string JobCode { get; set; } = null!;

        public string AnimalType { get; set; } = null!;

        public double NumberOfDays { get; set; }

        public double NumberOfAnimals { get; set; }

        public int IndCounter { get; set; }

        public int? FpsCalYear { get; set; }
        public int? UserId { get; set; }

        public string? Dt2Username { get; set; }

        public string? UserEmail { get; set; }

    }
}


