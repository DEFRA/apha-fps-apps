using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Common.Contracts.PIMS
{
    public class ProjectDetailsMilestoneRes
    {
        public string Parentproject { get; set; } = null!;
        public string? Program { get; set; }
        public string? Customer { get; set; }
        public string? ProjectGroup { get; set; }
        public bool Formrequired { get; set; }

        public char TypeLookUp { get; set; }
    }
}
