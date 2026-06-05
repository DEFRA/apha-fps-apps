using System;

namespace Apha.PACT.Core.Entities;

public class RecreateSummaryLogWithComment
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public short? Period { get; set; }
    public DateTime? DateDone { get; set; }
    public string? Comments { get; set; }

}
