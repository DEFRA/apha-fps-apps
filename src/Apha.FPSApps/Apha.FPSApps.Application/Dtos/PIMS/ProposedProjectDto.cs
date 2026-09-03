using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    public class ProposedProjectDto
    {
        public int Id { get; set; }
        public string? Parentproject { get; set; }
        public string? TransferTo { get; set; }
        public string? Projecttitle { get; set; }
        public string? Costbookno { get; set; }

        [StringLength(50)]
        public string? Disease { get; set; }

        [StringLength(10)]
        public string? Program { get; set; }

        [StringLength(50)]
        public string? Customer { get; set; }

        [StringLength(50)]
        public string? Manager { get; set; }
        public string? Projectstatus { get; set; }
        public string? Reason { get; set; }
    }
}
