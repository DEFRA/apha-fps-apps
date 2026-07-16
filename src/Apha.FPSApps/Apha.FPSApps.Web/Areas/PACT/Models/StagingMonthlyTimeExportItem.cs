using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class StagingMonthlyTimeExportItem
    {
        [Display(Name = "Work Group")]
        public string? WorkGroup { get; set; }
        public string? Name { get; set; }
        [Display(Name = "Time Code")]
        public string? TimeCode { get; set; }       
        
        [Display(Name = "Parent Project")]
        public string? ParentProject { get; set; }
        public double? Month { get; set; }       
        public double? Hours { get; set; }        
        public bool? Passed { get; set; }
        public string? FailureComments { get; set; }
        public string? Filename { get; set; }
    }
}
