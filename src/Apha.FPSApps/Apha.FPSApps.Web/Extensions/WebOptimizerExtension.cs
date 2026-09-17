namespace Apha.FPSApps.Web.Extensions
{
    public static class WebOptimizerExtension
    {
        public static IServiceCollection AddAssetBundling(this IServiceCollection services)
        {
            services.AddWebOptimizer(pipeline =>
            {
                // ── Shared common component assets ─────────────────────────
                // Minify the multicolumn-dropdown component in place so the
                // existing ~/js/common/multicolumn-dropdown.component.js and
                // ~/css/common/multicolumn-dropdown.css references (used across
                // ~22 views) are served minified without changing those views.
                pipeline.MinifyJsFiles("js/common/multicolumn-dropdown.component.js");
                pipeline.MinifyCssFiles("css/common/multicolumn-dropdown.css");

                // Combined, minified bundle for views that prefer a single asset.
                pipeline.AddCssBundle("/css/bundles/multicolumn-dropdown.css",
                    "css/common/multicolumn-dropdown.css");

                pipeline.AddJavaScriptBundle("/js/bundles/multicolumn-dropdown.js",
                    "js/common/multicolumn-dropdown.component.js");

                // ── Root layout (Views/Shared/_Layout.cshtml) ──────────────
                pipeline.AddCssBundle("/css/bundles/root.css",
                    "lib/bootstrap/dist/css/bootstrap.min.css",
                    "css/govuk-frontend-6.0.0.min.css",
                    "css/main_style.css",
                    "css/supplement_style_for_gov.uk_style.css",
                    "css/site.css");

                pipeline.AddJavaScriptBundle("/js/bundles/root.js",
                    "lib/jquery/dist/jquery.min.js",
                    "lib/bootstrap/dist/js/bootstrap.bundle.min.js",
                    "js/site.js");

                // ── FPS area layout ────────────────────────────────────────
                pipeline.AddCssBundle("/css/bundles/fps.css",
                    "lib/bootstrap/dist/css/bootstrap.min.css",
                    "css/govuk-frontend-6.0.0.min.css",
                    "css/supplement_style_for_gov.uk_style.css",
                    "css/fps_styles/styles.css",
                    "DataGrid/datagrid.css",
                    "css/main_style.css",
                    "DataGrid/editable-grid.css",
                    "css/common/headernav/navstyle.css");

                pipeline.AddJavaScriptBundle("/js/bundles/fps-scripts.js",
                    "lib/bootstrap/dist/js/bootstrap.bundle.min.js",
                    "lib/jquery-validation/dist/jquery.validate.js",
                    "js/common/keyboard/global-dropdown-keyboard.js",
                    "lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js",
                    "js/govuk-modal-dialog.js");

                pipeline.AddJavaScriptBundle("/js/bundles/fps-common.js",
                    "js/common/headernav/navmenu.js",
                    "js/common/numeric-decimal-input.js",
                    "js/common/js-alphanumeric-field.js");

                // ── PACT area layout ───────────────────────────────────────
                pipeline.AddCssBundle("/css/bundles/pact.css",
                    "css/govuk-frontend-6.0.0.min.css",
                    "css/supplement_style_for_gov.uk_style.css",
                    "css/pact_styles/styles.css",
                    "css/main_style.css",
                    "DataGrid/editable-grid.css",
                    "css/common/headernav/navstyle.css");

                pipeline.AddJavaScriptBundle("/js/bundles/pact-common.js",
                    "js/common/headernav/navmenu.js",
                    "js/number-validation.js",
                    "js/site.js");

                // ── PIMS area layout ───────────────────────────────────────
                pipeline.AddCssBundle("/css/bundles/pims.css",
                    "lib/bootstrap/dist/css/bootstrap.min.css",
                    "css/common/_govuk_tabs.css",
                    "css/govuk-frontend-6.0.0.min.css",
                    "css/supplement_style_for_gov.uk_style.css",
                    "css/pims_styles/styles.css",
                    "css/main_style.css",
                    "DataGrid/editable-grid.css",
                    "css/common/headernav/navstyle.css");

                pipeline.AddJavaScriptBundle("/js/bundles/pims-common.js",
                    "js/common/headernav/navmenu.js",
                    "js/common/numeric-decimal-input.js");

                // ── CostBook area layout ───────────────────────────────────
                pipeline.AddCssBundle("/css/bundles/costbook.css",
                    "lib/bootstrap/dist/css/bootstrap.min.css",
                    "css/govuk-frontend-6.0.0.min.css",
                    "css/supplement_style_for_gov.uk_style.css",
                    "css/costbook_styles/styles.css",
                    "css/main_style.css",
                    "DataGrid/editable-grid.css",
                    "css/common/headernav/navstyle.css");
            });

            return services;
        }
    }
}
