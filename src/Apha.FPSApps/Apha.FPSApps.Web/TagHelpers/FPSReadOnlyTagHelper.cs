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
            // The UserPermission page is exempt: its buttons are never disabled by year read-only state.
            if (_fy.IsReadOnly && !AllowEdit && !IsUserPermissionPage())
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
    }
}
