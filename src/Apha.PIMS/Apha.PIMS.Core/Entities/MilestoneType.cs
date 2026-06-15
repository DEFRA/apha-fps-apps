using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Entities
{
    public class MilestoneType
    {
        public char IdType { get; set; }

        public string? Type { get; set; }

        public char? MilestoneDeliverable { get; set; }
    }
}
