using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Entities
{
    public partial class ProjectStatus
    {
        public string Projectstatus { get; set; } = null!;

        public bool IsFps { get; set; }

        public bool IsPims { get; set; }
    }
}
