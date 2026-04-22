using System;
using System.Collections.Generic;

namespace Apha.Costbook.DataAccess;

public partial class FpsAccountCategory
{
    public string AccShortName { get; set; } = null!;

    public string? AccountDescription { get; set; }

    public string? ConstituentAccountCodes { get; set; }

    public string AccountType { get; set; } = null!;

    public int? ProjectSpecific { get; set; }
    public int? RcSpecific { get; set; }

    public string? Csg7Group { get; set; }

    public int FpsYear { get; set; }
    
}
