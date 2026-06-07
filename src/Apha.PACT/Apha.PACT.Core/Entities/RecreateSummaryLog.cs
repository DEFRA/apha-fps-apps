using System;
using System.Collections.Generic;

namespace Apha.PACT.Core.Entities;

public partial class RecreateSummaryLog
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public short? Period { get; set; }
    public DateTime? DateDone { get; set; }
    public int FpsYear { get; set; }
}
