using Apha.FPSApps.Web.Handler;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Apha.FPSApps.Web.TagHelpers
{
    [HtmlTargetElement("button")]
    [HtmlTargetElement("input", Attributes = "type=submit")]
    [HtmlTargetElement("input", Attributes = "type=button")]
    public class FPSReadOnlyTagHelper : TagHelper
    {
        // Controllers/pages on which the read-only disable logic should never apply
        private const string UserPermissionController = "UserPermission";
        private const string ProjectPlanningController = "ProjectPlanning";

        // Controllers whose grids/popups are only ever used from the Project Planning screen.
        // Their buttons should follow the same Planning(enabled)/Closed(disabled) rule as ProjectPlanning itself.
        private static readonly string[] ProjectPlanningRelatedControllers =
        {
            "StaffJob",
            "AnimalJob",
            "TestPlanJob",
            "AdditionalCostJob"
        };

        // ProjectTestPlanActual hosts its own "Planned Time (FPS)" grid (Index / LoadTestPlanGrid actions)
        // instead of delegating to TestPlanJobController. Only those specific actions should follow the
        // Planning(enabled)/Closed(disabled) rule; its "Actual Tests (PACT)" grid (LoadCompareTests2Grid)
        // must remain excluded and fall through to the default (always disabled in read-only) behavior.
        private const string ProjectTestPlanActualController = "ProjectTestPlanActual";
        private static readonly string[] ProjectTestPlanActualPlanningActions =
        {
            "Index",
            "LoadTestPlanGrid"
        };

        private readonly IFpsYearContext _fy;

        public FPSReadOnlyTagHelper(IFpsYearContext fy)
        {
            _fy = fy;
        }

        [HtmlAttributeName("allow-edit")]
        public bool AllowEdit { get; set; } = false;

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; } = default!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Disable button only when year is read-only AND editing is not explicitly allowed
            // This allows specific buttons to remain enabled even in read-only mode.
            if (_fy.IsReadOnly && !AllowEdit && ShouldDisableForCurrentPage())
            {
                output.Attributes.SetAttribute("disabled", "disabled");
            }

            // Remove the allow-edit attribute from final HTML
            output.Attributes.RemoveAll("allow-edit");
        }

        private bool IsUserPermissionPage()
        {
            var controller = ViewContext?.RouteData.Values["controller"]?.ToString();
            return string.Equals(controller, UserPermissionController, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsProjectPlanningPage()
        {
            var controller = ViewContext?.RouteData.Values["controller"]?.ToString();
            return string.Equals(controller, ProjectPlanningController, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsProjectPlanningRelatedController()
        {
            var controller = ViewContext?.RouteData.Values["controller"]?.ToString();
            if (controller is null)
            {
                return false;
            }

            foreach (var name in ProjectPlanningRelatedControllers)
            {
                if (string.Equals(controller, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsProjectTestPlanActualPlanningAction()
        {
            var controller = ViewContext?.RouteData.Values["controller"]?.ToString();
            if (!string.Equals(controller, ProjectTestPlanActualController, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var action = ViewContext?.RouteData.Values["action"]?.ToString();
            foreach (var name in ProjectTestPlanActualPlanningActions)
            {
                if (string.Equals(action, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ShouldDisableForCurrentPage()
        {
            if (IsUserPermissionPage())
            {
                return false;
            }

            if (IsProjectPlanningPage() || IsProjectPlanningRelatedController() || IsProjectTestPlanActualPlanningAction())
            {
                return string.Equals(_fy.YearStatus?.Trim(), "Closed", StringComparison.OrdinalIgnoreCase);
            }

            // default for all other pages
            return true;
        }
    }
}
