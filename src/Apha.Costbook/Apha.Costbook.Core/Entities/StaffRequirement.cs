using System;
using System.Collections.Generic;

namespace Apha.Costbook.Core.Entities;

public partial class StaffRequirement
{
    public int SrIdentity { get; set; }

    public string? Project { get; set; }

    public int? Year { get; set; }

    public string WgGrade { get; set; } = null!;

    public string? Name { get; set; }

    public double? Nohours { get; set; }

    public double? Nodays { get; set; }

    public double? Chargerate { get; set; }

    public double? Payrate { get; set; }

    public double? Npr { get; set; }

    public double? Ohr { get; set; }
}
