/**
 * menu-builder.js  —  Shared navigation builder for PACT & PIMS
 *
 * Reads a menuConfig array (defined in each app's menu-config.js) and
 * builds the full navbar HTML + wires up all event listeners.
 *
 * Config item shapes:
 *   Direct link:    { label, href }
 *   Level-1 drop:   { label, id, items: [ ...Level-2 items ] }
 *   Level-2 drop:   { label, id, items: [ ...Level-3 items ] }  (sub-dropdown)
 *   Level-2 link:   { label, href }
 *   Level-3 link:   { label, href }
 *
 * Usage (in every HTML page):
 *   <div id="header"></div>
 *   <script src="<path>/menu-config.js"></script>
 *   <script src="<path>/menu-builder.js"></script>
 *   <script>
 *       buildNav('header', menuConfig, userConfig);
 *   </script>
 *
 * userConfig shape: { name: "Meg Ved", homeHref: "../index.html" }
 */

// ---------------------------------------------------------------------------
// HTML builders
// ---------------------------------------------------------------------------

function _buildLevel3Item(item) {
    const a = document.createElement('a');
    a.className = 'dropdown-item';
    a.href = item.href || '#';
    a.textContent = item.label;
    return a;
}

function _buildLevel2Item(item) {
    // Level-2 item that has Level-3 children → sub-dropdown
    if (item.items && item.items.length) {
        const wrapper = document.createElement('div');
        wrapper.className = 'sub-dropdown';

        const btn = document.createElement('button');
        btn.className = 'dropdown-item sub-dropdown-toggle';
        btn.setAttribute('data-subdropdown', item.id);
        btn.innerHTML = `<span>${item.label}</span><img class="sub-right-arrow" src="${_imgPath}right-arrow.png" alt="" aria-hidden="true" width="14">`;

        const panel = document.createElement('div');
        panel.className = 'sub-dropdown-menu';
        panel.id = item.id;

        item.items.forEach(child => panel.appendChild(_buildLevel3Item(child)));

        wrapper.appendChild(btn);
        wrapper.appendChild(panel);
        return wrapper;
    }

    // Plain Level-2 link
    const a = document.createElement('a');
    a.className = 'dropdown-item';
    a.href = item.href || '#';
    a.textContent = item.label;
    return a;
}

function _buildLevel1Dropdown(item) {
    const wrapper = document.createElement('div');
    wrapper.className = 'nav-item dropdown';

    const btn = document.createElement('button');
    btn.className = 'nav-button dropdown-toggle';
    btn.setAttribute('data-dropdown', item.id);
    btn.setAttribute('aria-expanded', 'false');
    btn.innerHTML = `<div class="align-menu-text"><span>${item.label}</span></div><span class="arrow"><img src="${_imgPath}arrow-down.svg" alt="" aria-hidden="true" width="12"></span>`;

    const panel = document.createElement('div');
    panel.className = 'dropdown-menu';
    panel.id = item.id;

    if (item.columns && item.columns > 1) {
        // Multi-column layout.
        // Items with a `column` (1-based) property are pinned to that column.
        // Remaining items are distributed evenly across all columns, column by column.
        // Optional `colWidths` array sets min-width per column, e.g. ['200px','300px','200px'].
        panel.classList.add('dropdown-menu--columns');

        const colBuckets = Array.from({ length: item.columns }, () => []);
        const unpinned = [];

        item.items.forEach(child => {
            const col = child.column;
            if (col && col >= 1 && col <= item.columns) {
                colBuckets[col - 1].push(child);
            } else {
                unpinned.push(child);
            }
        });

        // Distribute unpinned items evenly, filling each column in turn
        const colSize = Math.ceil(unpinned.length / item.columns);
        unpinned.forEach((child, i) => {
            const idx = Math.min(Math.floor(i / colSize), item.columns - 1);
            colBuckets[idx].push(child);
        });

        colBuckets.forEach((colItems, colIdx) => {
            const col = document.createElement('div');
            col.className = 'dropdown-col';
            if (item.colWidths && item.colWidths[colIdx]) {
                col.style.minWidth = item.colWidths[colIdx];
            }
            colItems.forEach(child => col.appendChild(_buildLevel2Item(child)));
            panel.appendChild(col);
        });
    } else {
        item.items.forEach(child => panel.appendChild(_buildLevel2Item(child)));
    }

    wrapper.appendChild(btn);
    wrapper.appendChild(panel);
    return wrapper;
}

function _buildDirectLink(item) {
    const wrapper = document.createElement('div');
    wrapper.className = 'nav-item';

    const a = document.createElement('a');
    a.className = 'nav-button';
    a.href = item.href || '#';
    a.style.textDecoration = 'none';
    a.textContent = item.label;

    wrapper.appendChild(a);
    return wrapper;
}

// ---------------------------------------------------------------------------
// Event wiring
// ---------------------------------------------------------------------------

function _wireEvents() {
    // Level-1 dropdowns
    document.querySelectorAll('.nav-button.dropdown-toggle').forEach(toggle => {
        toggle.addEventListener('click', function (e) {
            e.stopPropagation();

            const dropdownId = this.getAttribute('data-dropdown');
            const dropdownMenu = document.getElementById(dropdownId);
            const parentDropdown = this.closest('.dropdown');

            if (!dropdownMenu) return;

            // Viewport-aware vertical positioning
            const btnRect = this.getBoundingClientRect();
            const vw = window.innerWidth;
            const vh = window.innerHeight;

            dropdownMenu.style.top = '';
            dropdownMenu.style.bottom = '';
            dropdownMenu.style.left = '';
            dropdownMenu.style.right = '';

            // Vertical
            if (btnRect.bottom + dropdownMenu.offsetHeight > vh) {
                dropdownMenu.style.bottom = this.offsetHeight + 'px';
            } else {
                dropdownMenu.style.top = this.offsetHeight + 'px';
            }

            // Horizontal
            if (btnRect.left + dropdownMenu.offsetWidth > vw) {
                dropdownMenu.style.right = '0';
            } else {
                dropdownMenu.style.left = '0';
            }

            // Close all other Level-1 menus
            document.querySelectorAll('.dropdown-menu').forEach(m => {
                if (m !== dropdownMenu) m.classList.remove('show');
            });
            document.querySelectorAll('.nav-item.dropdown').forEach(d => {
                if (d !== parentDropdown) d.classList.remove('active');
            });
            // Close any open sub-menus
            document.querySelectorAll('.sub-dropdown-menu').forEach(m => m.classList.remove('show'));
            document.querySelectorAll('.sub-dropdown').forEach(d => d.classList.remove('active'));

            dropdownMenu.classList.toggle('show');
            parentDropdown.classList.toggle('active');
            this.setAttribute('aria-expanded', dropdownMenu.classList.contains('show') ? 'true' : 'false');
        });
    });

    // Level-2 sub-dropdown — opens on hover
    document.querySelectorAll('.sub-dropdown').forEach(subDropdown => {
        subDropdown.addEventListener('mouseenter', function () {
            const subMenu = this.querySelector('.sub-dropdown-menu');
            if (!subMenu) return;

            // Close any other open sub-menus at this level
            document.querySelectorAll('.sub-dropdown-menu').forEach(m => {
                if (m !== subMenu) m.classList.remove('show');
            });
            document.querySelectorAll('.sub-dropdown').forEach(d => {
                if (d !== this) d.classList.remove('active');
            });

            // Fly right, fall back to left if near viewport edge
            const itemRect = this.getBoundingClientRect();
            const vw = window.innerWidth;
            subMenu.style.left = '';
            subMenu.style.right = '';
            if (itemRect.right + 260 > vw) {
                subMenu.style.left = 'auto';
                subMenu.style.right = '100%';
            } else {
                subMenu.style.left = '100%';
                subMenu.style.right = 'auto';
            }

            subMenu.classList.add('show');
            this.classList.add('active');
        });

        subDropdown.addEventListener('mouseleave', function () {
            const subMenu = this.querySelector('.sub-dropdown-menu');
            if (subMenu) subMenu.classList.remove('show');
            this.classList.remove('active');
        });
    });

    // Close everything on outside click
    document.addEventListener('click', () => {
        document.querySelectorAll('.dropdown-menu').forEach(m => m.classList.remove('show'));
        document.querySelectorAll('.nav-item.dropdown').forEach(d => d.classList.remove('active'));
        document.querySelectorAll('.sub-dropdown-menu').forEach(m => m.classList.remove('show'));
        document.querySelectorAll('.sub-dropdown').forEach(d => d.classList.remove('active'));
        document.querySelectorAll('.nav-button.dropdown-toggle').forEach(t => t.setAttribute('aria-expanded', 'false'));
    });

    // Clicking a final link closes its parent panel
    document.querySelectorAll('.dropdown-item:not(.sub-dropdown-toggle)').forEach(item => {
        item.addEventListener('click', function () {
            const panel = this.closest('.dropdown-menu, .sub-dropdown-menu');
            const parentL1 = this.closest('.nav-item.dropdown');
            if (panel) panel.classList.remove('show');
            if (parentL1) parentL1.classList.remove('active');
        });
    });
}

// ---------------------------------------------------------------------------
// User dropdown (top-right avatar)
// ---------------------------------------------------------------------------

function _buildUserDropdown(userConfig) {
    const wrapper = document.createElement('div');
    wrapper.style.cssText = 'display:flex;flex-direction:row;';

    const btn = document.createElement('button');
    btn.className = 'userdropdownbtn';
    btn.id = 'userdropdownbtn';
    btn.style.cssText = 'border:0;margin-right:20px;';
    btn.innerHTML = `${userConfig.name} &nbsp; <img src="${_imgPath}circle-user-regular-full.svg" alt="" aria-hidden="true" width="24"/>`;
    wrapper.appendChild(btn);

    const dp = document.createElement('div');
    dp.className = 'userdropdown';
    dp.id = 'userdropdowndp';
    dp.innerHTML = `<ul><li><a href="${userConfig.homeHref}" style="display:flex;align-items:center;">
        <img src="${_imgPath}house-user-solid-full.svg" alt="" aria-hidden="true" width="22px"> Home</a></li></ul>`;

    btn.addEventListener('click', function () {
        const container = this.closest('.userdropdownbtn');
        container.parentElement.nextElementSibling.classList.toggle('show');
    });

    return { wrapper, dp };
}

// ---------------------------------------------------------------------------
// Public entry point
// ---------------------------------------------------------------------------

// Resolved at build time based on the hosting page's location
let _imgPath = '../images/';

function buildNav(containerId, config, userConfig) {
    // Allow caller to override image path if needed
    if (userConfig && userConfig.imgPath) _imgPath = userConfig.imgPath;

    const container = document.getElementById(containerId);
    if (!container) return;

    // User dropdown row
    const { wrapper: userRow, dp: userDp } = _buildUserDropdown(
        userConfig || { name: 'User', homeHref: '../index.html' }
    );

    // Match page structure: keep user controls in the top header row
    // (after logo/year block), and keep #header for main nav only.
    const headerNav = document.querySelector('#app-header .header-nav') || document.querySelector('.header-nav');
    if (headerNav) {
        headerNav.appendChild(userRow);
        headerNav.appendChild(userDp);
    }

    // Main nav
    const nav = document.createElement('nav');
    nav.className = 'main-nav';

    config.forEach(item => {
        if (item.items && item.items.length) {
            nav.appendChild(_buildLevel1Dropdown(item));
        } else {
            nav.appendChild(_buildDirectLink(item));
        }
    });

    container.appendChild(nav);

    // Wire all events after DOM is built
    _wireEvents();
}
