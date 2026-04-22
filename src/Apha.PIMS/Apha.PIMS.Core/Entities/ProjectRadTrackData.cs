using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Entities
{
    public partial class ProjectRadTrackData
    {
        public string Parentproject { get; set; } = null!;

        public string? Version { get; set; }

        public string? Fileref { get; set; }

        public string? Customerref { get; set; }

        public DateOnly? Startdate { get; set; }

        public DateOnly? Enddate { get; set; }

        public DateOnly? Finalreportreceived { get; set; }

        public DateOnly? Finalreportsent { get; set; }

        public short? Inflation { get; set; }

        public DateOnly? Closeddate { get; set; }

        public short Useprojectyear { get; set; }

        public string? Status { get; set; }

        public double? Pcforecastspend { get; set; }

        public int? Riskid { get; set; }

        public string? Costbooknumber { get; set; }

        public DateOnly? Revisedenddate { get; set; }

        public bool Formrequired { get; set; }

        public decimal? Overallcustincome { get; set; }
    }
}
