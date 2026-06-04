using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Entities
{
    public partial class MonthlyOutput
    {
        public short Year { get; set; }

        public string Testcode { get; set; } = null!;

        public string Buyer { get; set; } = null!;

        public double Month { get; set; }

        public string Workgroup { get; set; } = null!;

        public double? Volume { get; set; }

        public string? Wgbuyer { get; set; }
    }
}
