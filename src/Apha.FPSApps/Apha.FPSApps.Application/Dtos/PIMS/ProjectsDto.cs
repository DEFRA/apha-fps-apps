using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    public class ProjectsDto
    {
        public int Year { get; set; }
        public string? Parentproject { get; set; }
        public string? Program { get; set; }
        public string? Customer { get; set; }
        public string? Manager { get; set; }
    }
}
