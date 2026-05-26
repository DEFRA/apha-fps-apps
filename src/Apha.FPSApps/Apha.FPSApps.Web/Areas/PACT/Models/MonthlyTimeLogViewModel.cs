using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class MonthlyTimeLogViewModel
    {
        public DataGridConfig<MonthlyTimeLogItem> LogGrid { get; set; } = new();
        public List<SelectListItem> WorkGroupOptions { get; set; } = new();
        public List<SelectListItem> TestCodeOptions { get; set; } = new();

        // Multi-column dropdown data
        public List<ProjectDropdownItem> ProjectOptions { get; set; } = new();
        public List<JobCodeDropdownItem> JobCodeOptions { get; set; } = new();
        public List<StaffDropdownItem> StaffOptions { get; set; } = new();

        public List<SelectListItem> ActionOptions { get; set; } = new()
        {
            new SelectListItem("Inserted", "I"),
            new SelectListItem("Deleted", "D"),
            new SelectListItem("Updated", "U")
        };
    }

    public class ProjectDropdownItem
    {
        public string Value { get; set; } = string.Empty;       // ParentProject (submitted)
        public string Code { get; set; } = string.Empty;        // ParentProject (column 1)
        public string Title { get; set; } = string.Empty;       // ProjectTitle (column 2)
    }

    public class JobCodeDropdownItem
    {
        public string Value { get; set; } = string.Empty;       // JobCodeId (submitted)
        public string Code { get; set; } = string.Empty;        // JobCodeId (column 1)
        public string Project { get; set; } = string.Empty;     // ParentProject (column 2)
    }

    public class StaffDropdownItem
    {
        public string Value { get; set; } = string.Empty;       // PactId (submitted)
        public string PactId { get; set; } = string.Empty;      // PactId (column 1)
        public string SpNumber { get; set; } = string.Empty;    // SpNumber (column 2)
        public string Name { get; set; } = string.Empty;        // Name (column 3)
    }
}

