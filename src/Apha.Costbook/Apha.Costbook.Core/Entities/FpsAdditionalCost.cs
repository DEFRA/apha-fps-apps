using System;
using System.Collections.Generic;

namespace Apha.Costbook.Core.Entities;

public partial class FpsAdditionalCost
{
    public string JobCode { get; set; } = null!;

    public string Account { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal ItemCost { get; set; }

    public string? Frequency { get; set; }

    public string? Supplier { get; set; }

    public int FpsYear { get; set; }

    
}
