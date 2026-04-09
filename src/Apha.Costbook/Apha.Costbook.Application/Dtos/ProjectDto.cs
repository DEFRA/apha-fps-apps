using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Application.Dtos
{
    public class ProjectDto
    {
        public string ProjectId { get; set; } = null!;

        public string? Plancat { get; set; }

        public string? Projecttitle { get; set; }

        public string? Programme { get; set; }

        public string? Projectworkgroup { get; set; }

        public double? Contractprice { get; set; }

        public DateOnly? Startdate { get; set; }

        public string? Disease { get; set; }

        public double? Startfyear { get; set; }

        public string? CustomerName { get; set; }

        public string? ContractNumber { get; set; }

        public string? Submittedbyfname { get; set; }

        public string? Submittedbylname { get; set; }

        public DateOnly? DateOfSubmission { get; set; }

        public string? PreparedBy { get; set; }

        public int? Inflation { get; set; }

        public int? Financialyears { get; set; }

        public string? Notes { get; set; }

        public double? Euroconvrate { get; set; }

        public short? Isdefraproject { get; set; }
    }
}
