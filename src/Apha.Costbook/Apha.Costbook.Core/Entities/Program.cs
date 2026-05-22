using System;
using System.Collections.Generic;

namespace Apha.Costbook.DataAccess;

public partial class Program
{
    public string ProgramNo { get; set; } = null!;

    public string? ProgramName { get; set; }

    public string? Directorate { get; set; }

    public string? Minim { get; set; }

    public string? SectorName { get; set; }

    public string? Customer { get; set; }

    public decimal? Target { get; set; }

    public string? Manager { get; set; }

    public int? FpScalYear { get; set; }
}
