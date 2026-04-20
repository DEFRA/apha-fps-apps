using System;
using System.Collections.Generic;

namespace Apha.Costbook.DataAccess;

public partial class ProjectYear
{
    public string Project { get; set; } = null!;

    public int YearValue { get; set; }

    public double? MarkupTime { get; set; }

    public double? MarkupTests { get; set; }

    public double? MarkupAnimals { get; set; }

    public double? MarkupAdditional { get; set; }

    public double? ProfitTime { get; set; }

    public double? ProfitTests { get; set; }

    public double? ProfitAnimals { get; set; }

    public double? ProfitAdditional { get; set; }
}
