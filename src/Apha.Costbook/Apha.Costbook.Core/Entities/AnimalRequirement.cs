using System;
using System.Collections.Generic;

namespace Apha.Costbook.DataAccess;


public partial class AnimalRequirement
{
    public int ArIdentity { get; set; }

    public string? Project { get; set; }

    public int? Year { get; set; }

    public string AnimalType { get; set; } = null!;

    public double? NumberOfDays { get; set; }

    public double? NumberOfAnimals { get; set; }

    public double? DailyRate { get; set; }
}
