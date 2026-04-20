using System;
using System.Collections.Generic;

namespace Apha.PIMS.Core.Entities
{

    public partial class RadtrackProg
    {
        public string Program { get; set; } = null!;

        public bool Radtrackprog { get; set; }

        public string? Publicationprefix { get; set; }
    }
}