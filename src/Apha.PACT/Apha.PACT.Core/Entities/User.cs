using System;
using System.Collections.Generic;

namespace Apha.PACT.Core.Entities;

public class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public int? AgencyId { get; set; }

    public bool FrmWarning { get; set; }

    public string? Comments { get; set; }

    public string? Dt2UserName { get; set; }
    public string? UserEmail { get; set; }
}
