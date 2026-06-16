namespace Apha.Common.Contracts.FPS
{
    public class AccountCategoryReq
    {
        public string AccShortName { get; set; } = null!;

        public string? AccountDescription { get; set; }

        public string? ConstituentAccountCodes { get; set; }

        public string AccountType { get; set; } = null!;

        public int? ProjectSpecific { get; set; }

        public int? RcSpecific { get; set; }
    }
}
