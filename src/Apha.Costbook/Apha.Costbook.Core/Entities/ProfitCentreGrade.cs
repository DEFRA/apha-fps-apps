using System;
using System.Collections.Generic;

namespace Apha.Costbook.Core.Entities;

public partial class ProfitCentreGrade
{
    public string PcGrade { get; set; } = null!;

    public string DivisionGrade { get; set; } = null!;

    public string GradeCode { get; set; } = null!;

    public string ProfitCentre { get; set; } = null!;

    public decimal? ChargeRate { get; set; }

    public decimal? DirectRate { get; set; }

    public decimal? PayRate { get; set; }

    public decimal? Npr { get; set; }

    public decimal? Ohr { get; set; }

    public double? HrsAvailable { get; set; }

    public decimal? OldChargeRate { get; set; }

    public decimal? DefraChargeRate { get; set; }

    public int FpsYear { get; set; }
}
