using System;
using System.Collections.Generic;

namespace Apha.Costbook.DataAccess;

public partial class FpsAnimals
{
    public string AnimalType { get; set; } = null!;

    public string? Species { get; set; }

    public string? SecurityLevel { get; set; }

    public decimal? DailyRate { get; set; }

    public bool PlanByWeek { get; set; }

    public decimal? DefraDailyRate { get; set; }
    public int FpsYear { get; set; }

    
}
