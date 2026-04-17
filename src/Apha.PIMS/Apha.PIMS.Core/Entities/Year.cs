using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Entities
{
    public partial class Year
    {
        public int Value { get; set; }

        public int? Latestmonthreleased { get; set; }
    }
}
