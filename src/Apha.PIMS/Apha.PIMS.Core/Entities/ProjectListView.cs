using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Entities
{
    public partial class ProjectListView
    {
        public string Parentproject { get; set; } = null!;
        public string? Program { get; set; }
        public string? Customer { get; set; }
        public string? OnFps { get; set; }
    }
}
