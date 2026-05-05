using System;
using System.Collections.Generic;


namespace Apha.PACT.Core.Entities
{
    public partial class Month
    {
        public required short MonthNumber { get; set; }

        public required string  MonthName { get; set; }
    }
}