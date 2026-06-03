using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models;

public class ProposedProjectViewModel
{
    [Required(ErrorMessage = "Project is required")]
    [Display(Name = "Project")]
    public string? Parentproject { get; set; }

    [Display(Name = "Title")]
    public string? Projecttitle { get; set; }

    [Display(Name = "Status")]
    public string? Projectstatus { get; set; }

    [Display(Name = "Costbook No")]
    public string? Costbookno { get; set; }

    [Display(Name = "Disease")]
    public string? Disease { get; set; }

    [Display(Name = "Program")]
    public string? Program { get; set; }

    [Display(Name = "Customer")]
    public string? Customer { get; set; }

    [Display(Name = "Manager")]
    public string? Manager { get; set; }

    [Display(Name = "Reason for new proposal")]
    public string? Reason { get; set; }

    public List<SelectListItem> StatusOptions { get; set; } = [];
    public List<SelectListItem> ProgramOptions { get; set; } = [];
    public List<SelectListItem> CustomerOptions { get; set; } = [];
}
