using System;
using System.Collections.Generic;

namespace Apha.PIMS.Core.Entities { 

public partial class Projects
{
    public short Year { get; set; }

    public string Parentproject { get; set; } = null!;

    public string? Program { get; set; }

    public string? Customer { get; set; }

    public string? Manager { get; set; }

    public decimal? Transferincome { get; set; }

    public decimal? Custincome { get; set; }

    public decimal? WipEoy { get; set; }

    public decimal? WipLimit { get; set; }

    public decimal? WipCurrent { get; set; }

    public string? Projectstatus { get; set; }

    public DateTime? Datecreated { get; set; }

    public decimal? Feccost { get; set; }

    public decimal? Profit { get; set; }

    public decimal? BudgetCvl { get; set; }

    public decimal? Caseworksub { get; set; }

    public decimal? Pvsincome { get; set; }

    public decimal? Plancaseworkdebit { get; set; }

    public string? Source { get; set; }

    public string? Disease { get; set; }

    public string? Contract { get; set; }

    public short? Finished { get; set; }

    public string? Comments { get; set; }

    public decimal? Carryover { get; set; }

    public short? Isdefraproject { get; set; }

    public double? Costcentre { get; set; }

    public string? Oracleprojectcode { get; set; }

    public string? Subaccountcode { get; set; }

    public string? Projectgroup { get; set; }

    public string? Incomeaccountcode { get; set; }
}
}