namespace Apha.FPS.Core.Entities
{
    public partial class UserTestOwner
    {
        public int UserId { get; set; }

        public string TestOwner { get; set; } = null!;

        public int? FpsYear { get; set; }
    }
}
