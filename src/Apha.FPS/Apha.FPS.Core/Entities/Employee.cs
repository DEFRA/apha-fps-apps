namespace Apha.FPS.Core.Entities
{
    public partial class Employee
    {
        public string SPNumber { get; set; } = null!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Title { get; set; }

        public int? FpsYear { get; set; }
    }
}
