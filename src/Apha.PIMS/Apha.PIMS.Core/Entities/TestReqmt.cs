using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Entities
{
    public partial class TestReqmt
    {
        public short Year { get; set; }

        public string Testcode { get; set; } = null!;

        public string Buyer { get; set; } = null!;

        public decimal? Unitprice { get; set; }

        public double? Norequired { get; set; }

        public string? Projectbuyercode { get; set; }

        public string? Testbuyercode { get; set; }

        public string? Source { get; set; }
    }
}
