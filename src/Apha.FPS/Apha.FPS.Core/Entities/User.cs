namespace Apha.FPS.Core.Entities    
{
    public partial class User
    {
        public int UserId { get; set; }

        public string? Username { get; set; }

        public int? AgencyId { get; set; }

        public bool FrmWarning { get; set; }

        public string? Comments { get; set; }

        public string? Dt2Username { get; set; }
    }
}


