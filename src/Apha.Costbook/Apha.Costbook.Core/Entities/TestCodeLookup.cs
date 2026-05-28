using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.Core.Entities
{
    public class TestCodeLookup
    {
        public string ItemCode { get; set; } = null!;
        public string? ItemDescription { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? UnitPriceWithInflamation { get; set; }
    }
}
