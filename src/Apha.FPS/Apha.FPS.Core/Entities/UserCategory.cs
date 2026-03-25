namespace Apha.FPS.Core.Entities
{
    public partial class UserCategory
    {
        public int UserId { get; set; }

        public string Category { get; set; } = null!;

        public int? FpsCalYear { get; set; }
    }
}


