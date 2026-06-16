namespace Apha.FPS.Application.Dtos
{
    public class AccountCategoryDto
    {
        public string AccShortName { get; set; } = null!;

        public string? AccountDescription { get; set; }

        public string? ConstituentAccountCodes { get; set; }

        public string? AccountType { get; set; }

        public int? ProjectSpecific { get; set; }

        public int? RcSpecific { get; set; }

        public int? FpsYear { get; set; }
    }
}
