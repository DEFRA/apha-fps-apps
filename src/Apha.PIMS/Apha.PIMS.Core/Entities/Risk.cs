using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Entities
{
    public partial class Risk
    {
        public int Riskid { get; set; }

        public string Riskrating { get; set; } = null!;
    }
}
