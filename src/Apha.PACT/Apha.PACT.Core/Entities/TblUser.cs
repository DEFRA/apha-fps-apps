using System;
using System.Collections.Generic;

namespace Apha.PACT.Core.Entities;

public class TblUser
{
    public string UserName { get; set; }
    public string Comments { get; set; }

    public virtual ICollection<RecreateSummariesLog> Logs { get; set; }
}
