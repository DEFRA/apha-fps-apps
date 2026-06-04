using System;

namespace Apha.PACT.Core.Models;

/// <summary>
/// Result model for recreate summary log queries that include user information.
/// This is used by the repository layer to return log data enriched with user details
/// without requiring a navigation property on the entity.
/// </summary>
public class RecreateSummaryLogWithUser
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public short? Period { get; set; }
    public DateTime? DateDone { get; set; }
    public int FpsYear { get; set; }
}
