/**
 * header-builder.js  —  Shared header builder for PACT, PIMS, and Costbook
 *
 * Reads a headerConfig object (defined in each app's header-config.js) and
 * builds the Defra logo / app-name / financial-year-selector banner, then
 * injects it into the element identified by containerId.
 *
 * headerConfig shape:
 *   {
 *     appName:        string,   // e.g. "PACT", "PIMS", "Costbook"
 *     showYearSelect: boolean,  // whether to render the financial-year dropdown
 *     yearSelectId:   string    // optional id attribute for the <select>
 *   }
 *
 * Usage (in every HTML page):
 *   <header class="govuk-header" role="banner" id="app-header"></header>
 *   <script src="<path>/header-config.js"></script>
 *   <script src="<path>/header-builder.js"></script>
 *   <script>
 *       buildHeader('app-header', headerConfig);
 *   </script>
 */

const _HEADER_YEARS = [
    '2017-18', '2018-19', '2019-20', '2020-21',
    '2021-22', '2022-23', '2023-24', '2024-25', '2025-26'
];

const _HEADER_CURRENT_YEAR = '2025-26';

/** Change this once to update the copyright year across all apps. */
const footerConfig = {
    year: 2026
};

function buildHeader(containerId, config) {
    const container = document.getElementById(containerId);
    if (!container) return;

    // Outer wrapper
    const nav = document.createElement('div');
    nav.className = 'header-nav';

    const logWrapper = document.createElement('div');
    logWrapper.className = 'app-log-wrapper';

    // Defra logo
    const logoDiv = document.createElement('div');
    const img = document.createElement('img');
    img.src = '../images/defra_logo.png';
    img.alt = 'Defra logo';
    img.width = 80;
    img.style.display = 'block';
    logoDiv.appendChild(img);
    logWrapper.appendChild(logoDiv);

    // App name pill — fixed width to keep consistent across PACT / PIMS / Costbook,
    // flex-centred so text stays centred regardless of box height.
    const nameDiv = document.createElement('div');
    nameDiv.className = 'app-log';
    nameDiv.style.cssText = 'min-width:120px;height:32px;display:flex;align-items:center;justify-content:center;padding:0;';
    const span = document.createElement('span');
    span.textContent = config.appName;
    nameDiv.appendChild(span);
    logWrapper.appendChild(nameDiv);

    // Financial year selector (PACT & PIMS only)
    // The select itself carries the .app-log visual styling; no outer wrapper div
    // is needed, which previously caused double-padding and an unequal height.
    if (config.showYearSelect) {
        const sel = document.createElement('select');
        sel.className = 'app-log';
        sel.style.cssText = 'margin-left:10px;width:160px;height:32px;outline:0;font-size:20px;cursor:pointer;font-family:"GDS Transport",arial,sans-serif;';
        sel.setAttribute('aria-label', 'Select financial year');
        if (config.yearSelectId) sel.id = config.yearSelectId;

        _HEADER_YEARS.forEach(y => {
            const opt = document.createElement('option');
            opt.textContent = y;
            if (y === _HEADER_CURRENT_YEAR) opt.selected = true;
            sel.appendChild(opt);
        });

        logWrapper.appendChild(sel);
    }

    nav.appendChild(logWrapper);
    container.appendChild(nav);
}

/**
 * buildFooter  —  Shared footer builder for all apps.
 *
 * Renders the Defra copyright footer and injects it into the element
 * identified by containerId.
 *
 * footerConfig shape:
 *   {
 *     year: number   // copyright year, e.g. 2025
 *   }
 *
 * Usage (in every HTML page):
 *   <div id="app-footer"></div>
 *   <script>
 *       buildFooter('app-footer', footerConfig);
 *   </script>
 */
function buildFooter(containerId, config) {
    const container = document.getElementById(containerId);
    if (!container) return;

    container.className = 'govuk-footer sup_p_0 sup_margin_left_right_0';

    const inner = document.createElement('div');
    inner.className = 'govuk-width-container sup_p_8 sup_text_center app-footer-inner';

    const text = document.createElement('span');
    text.className = 'app-footer-text';
    text.textContent = '\u00A9 copyright Defra ' + (config.year || new Date().getFullYear());

    inner.appendChild(text);
    container.appendChild(inner);
}
