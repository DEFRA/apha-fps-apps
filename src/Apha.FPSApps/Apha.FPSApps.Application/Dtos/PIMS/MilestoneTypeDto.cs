using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    public class MilestoneTypeDto
    {
        public char IdType { get; set; }

        public string? Type { get; set; }

        public char? MilestoneDeliverable { get; set; }
    }
}
