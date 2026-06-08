using System.Diagnostics.CodeAnalysis;

namespace Apha.FPSApps.Application.Dtos.FPS
{
    [ExcludeFromCodeCoverage]
    public class AccountCategoryDto
    {
        public string AccShortName { get; set; } = null!;

        public string? AccountDescription { get; set; }

        public string? ConstituentAccountCodes { get; set; }
    }
}
