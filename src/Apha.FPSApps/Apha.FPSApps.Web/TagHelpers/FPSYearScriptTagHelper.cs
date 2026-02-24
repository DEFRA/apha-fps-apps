using Apha.FPSApps.Web.Handler;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Apha.FPSApps.Web.TagHelpers
{
    [HtmlTargetElement("fps-year-script")]
    public class FPSYearScriptTagHelper : ITagHelper
    {
        private readonly IFPSYearContext _fy;

        public FPSYearScriptTagHelper(IFPSYearContext fy)
        {
            _fy = fy;
        }
        public int Order => 0;

        public void Init(TagHelperContext context)
        {

        }

        public Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "script";
            output.Content.SetHtmlContent($@"
        window.FPS_YEAR = {_fy.Year};
                $(document).ajaxSend(function (e, xhr) {{
                    xhr.setRequestHeader('X-FPS-Year', window.FPS_YEAR);
                }});
            ");
            return Task.CompletedTask;
        }
    }
}
