using System;
using System.Collections.Generic;

namespace Apha.Costbook.DataAccess;

public partial class Settings
{
    public string Id { get; set; } = null!;

    public string? Setting { get; set; }

    public string? Notes { get; set; }

    public string? Testsetting { get; set; }

    public bool? Userupdateable { get; set; }
}
