namespace Apha.FPS.Core.Entities
{
    public partial class AccountCategory
    {
        public string AccShortName { get; set; } = null!;

        public string? AccountDescription { get; set; }

        public string? AccountType { get; set; }

        public string? ConstituentAccountCodes { get; set; }

        public string? Csg7Group { get; set; }

        public int? ProjectSpecific { get; set; }

        public int? RcSpecific { get; set; }

        public int? FpsYear { get; set; }
    }
}
