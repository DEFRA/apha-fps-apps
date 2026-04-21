using Apha.FPSApps.Web.Handler;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Apha.FPSApps.Web.TagHelpers
{
    [HtmlTargetElement("fps-year-script")]
    public class FpsYearScriptTagHelper : TagHelper
    {
        private readonly IFpsYearContext _fy;

        public FpsYearScriptTagHelper(IFpsYearContext fy)
        {
            _fy = fy;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "script";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.SetHtmlContent($@"
        window.FPS_YEAR = {_fy.Year};
                jQuery(document).ajaxSend(function (e, xhr) {{
                    xhr.setRequestHeader('X-FPS-Year', window.FPS_YEAR);
                }});
                window.fpsNavigateTo = function (url) {{
                    var separator = url.indexOf('?') !== -1 ? '&' : '?';
                    window.location.href = url + separator + 'year=' + window.FPS_YEAR;
                }};
            ");
        }
    }
}

