using System;
using System.Collections.Generic;

namespace Apha.Costbook.Core.Entities;

public partial class TestRequirement
{
    public string Project { get; set; } = null!;

    public int Year { get; set; }

    public string TestCode { get; set; } = null!;

    public double? NumberOfTests { get; set; }

    public double? UnitPrice { get; set; }
}
