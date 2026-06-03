using System;
using System.Collections.Generic;

namespace Apha.PACT.Core.Entities;

public class TblUser
{
    public required string UserName { get; set; }
    public required string Comments { get; set; }

    public required virtual ICollection<RecreateSummariesLog> Logs { get; set; }
}
