// ── Global keyboard-navigation support for custom "flyout" dropdowns ─────────

(function () {
    'use strict';

    function isVisible(el) {
        if (!el) return false;
        var style = window.getComputedStyle(el);
        return style.display !== 'none' && style.visibility !== 'hidden';
    }

    // Focus an element and force the visible focus ring to show, since some
    // browsers don't reliably apply ":focus-visible" styling to focus moved
    // programmatically via JavaScript (e.g. re-focusing a dropdown trigger
    // after a row is selected). The "js-force-focus-visible" class is styled
    // in main_style.css to match the existing :focus-visible outline, and is
    // removed as soon as the element naturally loses/gains focus again.
    function focusWithVisibleRing(el) {
        if (!el) return;
        el.classList.add('js-force-focus-visible');
        el.focus();
        rememberFocus(el);
        var cleanup = function () {
            el.classList.remove('js-force-focus-visible');
            el.removeEventListener('blur', cleanup);
        };
        el.addEventListener('blur', cleanup);
    }

    // ── Cross-page-refresh focus restoration ──────────────────────────────
    // Some pages trigger a full server-side refresh (or a partial re-render
    // that replaces the whole form) after a dropdown/multi-select selection.
    // On such reloads the previously focused element is destroyed, so focus
    // falls to <body>. To keep focus on the element the user last interacted
    // with, we persist its stable identifier to sessionStorage and restore
    // focus to the matching element (if it exists) on the next page load.
    var FOCUS_STORAGE_KEY = '__apha_last_focus_id';

    function elementIdentifier(el) {
        if (!el || el.nodeType !== 1) return null;
        if (el.id) return '#' + CSS.escape(el.id);
        if (el.name) return el.tagName.toLowerCase() + '[name="' + CSS.escape(el.name) + '"]';
        // data-* attributes commonly used for row/action identity across the apps
        // (e.g. data-id, data-key, data-value, data-pageno, data-tab-key, data-column).
        var dataAttrs = ['data-id', 'data-key', 'data-value', 'data-pageno', 'data-tab-key', 'data-column', 'data-filter', 'data-listdesc'];
        for (var i = 0; i < dataAttrs.length; i++) {
            var v = el.getAttribute(dataAttrs[i]);
            if (v) return el.tagName.toLowerCase() + '[' + dataAttrs[i] + '="' + CSS.escape(v) + '"]';
        }
        return null;
    }

    function isInteractiveElement(el) {
        if (!el || !el.tagName) return false;
        var tag = el.tagName;
        // Native focusable/clickable elements.
        if (tag === 'INPUT' || tag === 'SELECT' || tag === 'TEXTAREA' ||
            tag === 'BUTTON' || tag === 'A' || tag === 'SUMMARY') {
            return true;
        }
        // ARIA-driven interactives and explicitly focusable elements.
        var role = el.getAttribute && el.getAttribute('role');
        if (role === 'button' || role === 'link' || role === 'option' ||
            role === 'tab' || role === 'menuitem' || role === 'checkbox' ||
            role === 'radio' || role === 'row' || role === 'gridcell') {
            return true;
        }
        if (el.hasAttribute && el.hasAttribute('tabindex')) return true;
        return false;
    }

    function rememberFocus(el) {
        try {
            // Walk up to the nearest interactive ancestor if the target is
            // something like a <span>/<i> inside a button/link/row.
            var target = el;
            while (target && target !== document && !isInteractiveElement(target)) {
                target = target.parentElement;
            }
            if (!target || target === document) return;
            var id = elementIdentifier(target);
            if (id) sessionStorage.setItem(FOCUS_STORAGE_KEY, id);
        } catch (ignored) { /* private mode / disabled storage */ }
    }

    function restoreFocusAfterRefresh() {
        var id;
        try { id = sessionStorage.getItem(FOCUS_STORAGE_KEY); } catch (ignored) { return; }
        if (!id) return;

        var el = null;
        try { el = document.querySelector(id); } catch (ignored) { /* invalid selector */ }
        if (el && typeof el.focus === 'function' && isVisible(el)) {
            // Ensure the element can actually receive focus.
            if (!el.hasAttribute('tabindex') && !isInteractiveElement(el)) {
                el.setAttribute('tabindex', '-1');
            }
            focusWithVisibleRing(el);
        }
        try { sessionStorage.removeItem(FOCUS_STORAGE_KEY); } catch (ignored) { /* ignore */ }
    }

    // Track user-initiated focus AND clicks on any interactive element so a
    // subsequent page refresh can restore focus/context to the same element,
    // regardless of what triggered the refresh (dropdown selection, form
    // postback, action button, grid row action, tab switch, etc.).
    document.addEventListener('focusin', function (e) {
        if (isInteractiveElement(e.target)) rememberFocus(e.target);
    });
    document.addEventListener('click', function (e) {
        if (isInteractiveElement(e.target) ||
            (e.target && e.target.closest && e.target.closest('a, button, [role="button"], [role="link"], [role="option"], [role="tab"], [role="menuitem"], [role="row"], [tabindex]'))) {
            rememberFocus(e.target);
        }
    }, true);

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', restoreFocusAfterRefresh);
    } else {
        restoreFocusAfterRefresh();
    }

    // Resolve the flyout panel + row-container ("body") associated with a trigger input.
    function resolvePanelParts(trigger) {
        var panel = null;

        // Preferred: panel is the trigger's next sibling wrapper's child, or a
        // sibling element within the same "position: relative" wrapper.
        var container = trigger.parentElement;
        if (container) {
            panel = container.querySelector('[id$="DropdownPanel"]');
        }

        // Fallback: derive "<Prefix>DropdownPanel" from the trigger id
        // (e.g. "ProgramDisplay" → "ProgramDropdownPanel", "ManagerDisplay" → "ManagerDropdownPanel").
        if (!panel && trigger.id) {
            var prefix = trigger.id.replace(/Display$/, '');
            panel = document.getElementById(prefix + 'DropdownPanel');
        }

        if (!panel) return null;

        // Remember which trigger opened this panel so we can reliably refocus it
        // later (e.g. after a row selection), even for triggers whose id doesn't
        // follow the "<Prefix>Display" naming convention (e.g. "projectNameInput").
        panel.__kbdTrigger = trigger;

        // Ensure focus is restored to the trigger whenever this panel closes,
        // regardless of which action (mouse click, keyboard, click-outside)
        // caused it to close.
        ensurePanelFocusRestore(panel);

        var body = panel.querySelector('tbody[id$="DropdownBody"]') || panel.querySelector('tbody');
        var search = panel.querySelector('input[id$="SearchBox"]');

        return { panel: panel, body: body, search: search };
    }

    // Resolve the trigger input that opened a given dropdown panel, preferring
    // the reference captured in resolvePanelParts and falling back to the
    // "<Prefix>Display" id convention used by most (but not all) dropdowns.
    function resolveTriggerForPanel(panel) {
        if (!panel) return null;
        if (panel.__kbdTrigger) return panel.__kbdTrigger;
        var prefix = panel.id.replace(/DropdownPanel$/, '');
        return document.getElementById(prefix + 'Display');
    }

    // Watch a dropdown panel for becoming hidden (via any close/select path —
    // mouse click on a row, keyboard Enter/Escape, click-outside, etc.) and,
    // if focus has been lost to <body> as a result, restore it to the trigger
    // that opened the panel. Attached once per panel, so this applies
    // uniformly to every flyout/multi-column dropdown across all apps without
    // requiring any change to individual page scripts.
    function ensurePanelFocusRestore(panel) {
        if (!panel || panel.hasAttribute('data-kbd-focus-watched')) return;
        panel.setAttribute('data-kbd-focus-watched', 'true');

        var wasVisible = isVisible(panel);
        var observer = new MutationObserver(function () {
            var nowVisible = isVisible(panel);
            if (wasVisible && !nowVisible) {
                // Panel just closed. If focus fell through to <body> (or was
                // removed from the document entirely along with the row that
                // had it), bring focus back to the trigger.
                if (document.activeElement === document.body || !document.body.contains(document.activeElement)) {
                    var trigger = resolveTriggerForPanel(panel);
                    if (trigger) {
                        window.setTimeout(function () { focusWithVisibleRing(trigger); }, 0);
                    }
                }
            }
            wasVisible = nowVisible;
        });
        observer.observe(panel, { attributes: true, attributeFilter: ['style', 'class'] });
    }

    function getVisibleRows(body) {
        if (!body) return [];
        return Array.prototype.slice.call(body.querySelectorAll('tr')).filter(function (row) {
            return isVisible(row);
        });
    }

    function isFlyoutTrigger(el) {
        if (!el || el.tagName !== 'INPUT') return false;
        if (el.disabled) return false;
        var looksLikeTrigger = /Display$/.test(el.id || '') || el.classList.contains('down-arrow-img');
        if (!looksLikeTrigger) return false;
        return !!resolvePanelParts(el);
    }

    // Make each row keyboard-focusable/actionable the first time it is encountered.
    function ensureRowIsAccessible(row) {
        if (row.hasAttribute('data-kbd-enabled')) return;
        row.setAttribute('data-kbd-enabled', 'true');
        if (!row.hasAttribute('tabindex')) row.setAttribute('tabindex', '-1');
        if (!row.hasAttribute('role')) row.setAttribute('role', 'option');

        row.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar') {
                e.preventDefault();
                var panelEl = row.closest('[id$="DropdownPanel"]');
                row.click();
                // Selecting a row typically hides the panel (removing the row from
                // the accessibility tree), which drops focus to <body>. Re-focus the
                // trigger afterwards so keyboard focus isn't lost after selection.
                if (panelEl) {
                    window.setTimeout(function () {
                        if (!isVisible(panelEl)) {
                            var trigger = resolveTriggerForPanel(panelEl);
                            if (trigger) focusWithVisibleRing(trigger);
                        }
                    }, 0);
                }
            } else if (e.key === 'Escape') {
                e.preventDefault();
                closePanelAndRefocus(row);
            } else if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
                e.preventDefault();
                var body = row.closest('tbody');
                var rows = getVisibleRows(body);
                var idx = rows.indexOf(row);
                var delta = e.key === 'ArrowDown' ? 1 : -1;
                var nextIdx = idx + delta;
                if (nextIdx >= 0 && nextIdx < rows.length) {
                    rows[nextIdx].focus();
                } else if (nextIdx < 0) {
                    // Move back up to the search box (if present) or trigger.
                    var panelEl = row.closest('[id$="DropdownPanel"]');
                    var searchEl = panelEl ? panelEl.querySelector('input[id$="SearchBox"]') : null;
                    if (searchEl) searchEl.focus();
                }
            }
        });
    }

    function closePanelAndRefocus(fromEl) {
        var panel = fromEl.closest ? fromEl.closest('[id$="DropdownPanel"]') : null;
        if (!panel) return;
        var trigger = resolveTriggerForPanel(panel);
        if (isVisible(panel)) {
            // Reuse the page's own close behaviour: a click outside the panel/trigger
            // is what every existing page listens for to hide the panel.
            document.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
        }
        if (trigger) focusWithVisibleRing(trigger);
    }

    // Attach the Enter/Space/ArrowDown/Escape handling directly on a trigger
    // input the first time it is encountered. Attaching directly (rather than
    // relying solely on document-level delegation) means this keeps working
    // even if some other script further down the bubble chain calls
    // stopPropagation()/stopImmediatePropagation() on the keydown event.
    function ensureTriggerIsAccessible(trigger) {
        if (trigger.hasAttribute('data-kbd-enabled')) return;
        if (!isFlyoutTrigger(trigger)) return;
        trigger.setAttribute('data-kbd-enabled', 'true');

        trigger.addEventListener('keydown', function (event) {
            var parts = resolvePanelParts(trigger);
            if (!parts) return;

            if (event.key === 'Enter' || event.key === ' ' || event.key === 'Spacebar') {
                event.preventDefault();
                if (!isVisible(parts.panel)) {
                    trigger.click(); // Reuses each page's existing open logic unchanged.
                }
            } else if (event.key === 'ArrowDown') {
                event.preventDefault();
                if (!isVisible(parts.panel)) {
                    trigger.click();
                }
                // Focus moves to search box (if the page's own open-logic focuses it,
                // this is a no-op) or, failing that, straight to the first row.
                window.setTimeout(function () {
                    if (parts.search && isVisible(parts.panel)) {
                        parts.search.focus();
                    } else {
                        var rows = getVisibleRows(parts.body);
                        if (rows.length) rows[0].focus();
                    }
                }, 0);
            } else if (event.key === 'Escape' && isVisible(parts.panel)) {
                event.preventDefault();
                closePanelAndRefocus(trigger);
            }
        });
    }

    // Attach ArrowDown/Escape support directly on a search box the first time
    // it is encountered.
    function ensureSearchBoxIsAccessible(searchBox) {
        if (searchBox.hasAttribute('data-kbd-enabled')) return;
        searchBox.setAttribute('data-kbd-enabled', 'true');

        searchBox.addEventListener('keydown', function (event) {
            var panel = searchBox.closest('[id$="DropdownPanel"]');
            var body = panel ? (panel.querySelector('tbody[id$="DropdownBody"]') || panel.querySelector('tbody')) : null;

            if (event.key === 'ArrowDown') {
                event.preventDefault();
                var rows = getVisibleRows(body);
                if (rows.length) rows[0].focus();
            } else if (event.key === 'Escape') {
                event.preventDefault();
                closePanelAndRefocus(searchBox);
            }
        });
    }

    // Scan the given root for triggers, search boxes and rows that still need
    // keyboard wiring, and wire them up. Safe to call repeatedly (each element
    // is only ever wired once, via the 'data-kbd-enabled' guard).
    function scanForKeyboardSupport(root) {
        root.querySelectorAll('input[id$="Display"], input.down-arrow-img').forEach(ensureTriggerIsAccessible);
        root.querySelectorAll('input[id$="SearchBox"]').forEach(ensureSearchBoxIsAccessible);
        root.querySelectorAll('[id$="DropdownBody"] tr, tbody tr[data-value]').forEach(ensureRowIsAccessible);
    }

    // This script is loaded from <head> (before document.body exists), so
    // defer the initial scan + MutationObserver wiring until the DOM is ready.
    function init() {
        scanForKeyboardSupport(document);

        // Triggers/panels/rows for some pickers (e.g. AJAX-populated dropdowns,
        // partials rendered after page load) can appear after the initial scan,
        // so a MutationObserver keeps wiring up anything new.
        var observer = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType !== 1) return;
                    if (node.matches && (node.matches('input[id$="Display"]') || node.matches('input.down-arrow-img'))) {
                        ensureTriggerIsAccessible(node);
                    } else if (node.matches && node.matches('input[id$="SearchBox"]')) {
                        ensureSearchBoxIsAccessible(node);
                    } else if (node.matches && node.matches('tr[data-value]')) {
                        ensureRowIsAccessible(node);
                    } else if (node.tagName === 'TR' && node.closest && node.closest('[id$="DropdownBody"]')) {
                        // Some dropdowns (e.g. project-dropdown.js) append rows one at a
                        // time into a "<Prefix>DropdownBody" container without setting a
                        // data-value attribute. Recognise those rows too, matching the
                        // same "[id$='DropdownBody'] tr" selector used by the initial scan.
                        ensureRowIsAccessible(node);
                    } else if (node.querySelectorAll) {
                        scanForKeyboardSupport(node);
                    }
                });
            });
        });
        observer.observe(document.body, { childList: true, subtree: true });
    }

    if (document.body) {
        init();
    } else {
        document.addEventListener('DOMContentLoaded', init);
    }
})();

// ── Shared modal popup (#modalPopup) focus management ─────────────────────

(function () {
    'use strict';

    function init() {
        var modal = document.getElementById('modalPopup');
        if (!modal) return;

        var modalBody = document.getElementById('modaPopupBody');
        var previouslyFocused = null;
        var isOpen = false;

        function getFocusableElements() {
            var scope = modalBody || modal;
            return Array.prototype.slice.call(
                scope.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])')
            ).filter(function (el) {
                return !el.hasAttribute('disabled') && el.offsetParent !== null;
            });
        }

        // Keep focus trapped inside the modal while it's open. Buttons like
        // Save/Cancel/Close often trigger an AJAX call or DOM update that can
        // remove/disable/hide themselves once clicked; when that happens the
        // browser drops focus back to <body> (i.e. "the background") instead
        // of somewhere inside the still-open modal. Whenever focus lands
        // outside the modal while it's open, pull it back in immediately -
        // unless the modal is in the middle of closing (isOpen is set false
        // just before we restore focus to the trigger that opened it).
        document.addEventListener('focusin', function (e) {
            if (!isOpen) return;
            if (modal.contains(e.target)) return;

            var focusable = getFocusableElements();
            if (focusable.length) {
                focusable[0].focus();
            } else {
                modal.focus();
            }
        });

        // Belt-and-braces Tab trap: even if a control is removed from the DOM
        // between keydown and focusin (so the browser has nowhere obvious to
        // send focus), Tab/Shift+Tab still cycle only within the modal.
        modal.addEventListener('keydown', function (e) {
            if (e.key !== 'Tab') return;
            var focusable = getFocusableElements();
            if (!focusable.length) return;

            var first = focusable[0];
            var last = focusable[focusable.length - 1];
            var active = document.activeElement;

            if (e.shiftKey && (active === first || !modal.contains(active))) {
                e.preventDefault();
                last.focus();
            } else if (!e.shiftKey && (active === last || !modal.contains(active))) {
                e.preventDefault();
                first.focus();
            }
        });

        var observer = new MutationObserver(function () {
            if (modal.classList.contains('show')) {
                modal.style.display = 'flex';
                document.body.style.overflow = 'hidden';

                previouslyFocused = document.activeElement;
                isOpen = true;

                setTimeout(function () {
                    var focusable = getFocusableElements();
                    if (focusable.length) {
                        focusable[0].focus();
                    } else {
                        modal.focus();
                    }
                }, 0);
            } else {
                isOpen = false;
                modal.style.display = 'none';
                document.body.style.overflow = '';

                if (previouslyFocused && typeof previouslyFocused.focus === 'function') {
                    previouslyFocused.focus();
                }
                previouslyFocused = null;
            }
        });
        observer.observe(modal, { attributes: true, attributeFilter: ['class'] });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();

// ── Global key navigation for sidenav and menu bar  ──────────
(function () {
    'use strict';

    function isVisible(el) {
        if (!el) return false;
        var style = window.getComputedStyle(el);
        return style.display !== 'none' && style.visibility !== 'hidden';
    }

    function getSidenavLinks(sidenav) {
        return Array.prototype.slice.call(sidenav.querySelectorAll('ul a[href], ul button'))
            .filter(isVisible);
    }

    function getMainNavButtons(mainNav) {
        return Array.prototype.slice.call(mainNav.querySelectorAll(':scope > .nav-item > .nav-button'))
            .filter(isVisible);
    }

    document.addEventListener('keydown', function (e) {
        if (e.key !== 'ArrowDown' && e.key !== 'ArrowUp' && e.key !== 'ArrowLeft' && e.key !== 'ArrowRight') return;

        var target = e.target;
        if (!target) return;

        // ── Side navigation: ArrowDown/ArrowUp moves to the next/previous link ──
        var sidenav = target.closest('nav.sidenav');
        if (sidenav && (e.key === 'ArrowDown' || e.key === 'ArrowUp')) {
            var links = getSidenavLinks(sidenav);
            var index = links.indexOf(target);
            if (index === -1) return;

            e.preventDefault();
            var delta = e.key === 'ArrowDown' ? 1 : -1;
            var nextIndex = (index + delta + links.length) % links.length;
            links[nextIndex].focus();
            return;
        }

        // ── Top main menu: ArrowLeft/ArrowRight moves between L1 buttons ────────
        // (Kept here as a resilient fallback/complement to navmenu.js so it
        // keeps working even if a page loads without navmenu.js, or another
        // handler further down the bubble chain stops propagation.)
        var mainNav = target.closest('.main-nav');
        if (mainNav && target.classList.contains('nav-button') && (e.key === 'ArrowLeft' || e.key === 'ArrowRight')) {
            var buttons = getMainNavButtons(mainNav);
            var btnIndex = buttons.indexOf(target);
            if (btnIndex === -1) return;

            e.preventDefault();
            var btnDelta = e.key === 'ArrowRight' ? 1 : -1;
            var nextBtnIndex = (btnIndex + btnDelta + buttons.length) % buttons.length;
            buttons[nextBtnIndex].focus();
        }
    });
})();

// ── Global "Enter toggles checkbox" support ────────────────────────────────
(function () {
    'use strict';

    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter') return;

        var target = e.target;
        if (!target || target.tagName !== 'INPUT' || target.type !== 'checkbox') return;
        if (target.disabled || target.readOnly) return;

        e.preventDefault();
        target.checked = !target.checked;
        target.dispatchEvent(new Event('input', { bubbles: true }));
        target.dispatchEvent(new Event('change', { bubbles: true }));
    });
})();

// ── Global GOV.UK tabs keyboard navigation (ArrowLeft/ArrowRight) ──────────

(function () {
    'use strict';

    // Capture phase on document: guarantees this runs before any other
    // keydown listener elsewhere in the page (e.g. grid/pagination handlers
    // that call stopPropagation()/stopImmediatePropagation()) can prevent
    // the tab navigation from being processed.
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'ArrowLeft' && e.key !== 'ArrowRight') {
            return;
        }

        var tab = e.target && e.target.closest ? e.target.closest('.govuk-tabs__tab') : null;
        if (!tab) {
            return;
        }

        var tabsWrapper = tab.closest('.govuk-tabs');
        if (!tabsWrapper) {
            return;
        }

        var tabs = Array.prototype.slice.call(
            tabsWrapper.querySelectorAll('.govuk-tabs__tab')
        );
        var currentIndex = tabs.indexOf(tab);
        if (currentIndex === -1) {
            return;
        }

        e.preventDefault();

        var delta = e.key === 'ArrowRight' ? 1 : -1;
        var nextIndex = (currentIndex + delta + tabs.length) % tabs.length;
        var nextTab = tabs[nextIndex];

        nextTab.focus();
        nextTab.click();

        // Ensure focus stays on the newly-selected tab even if the click
        // handler/native anchor navigation would otherwise move it away.
        window.setTimeout(function () {
            if (document.activeElement !== nextTab) {
                nextTab.focus();
            }
        }, 0);
    }, true);
})();

// ── Global data-grid arrow-key navigation (NVDA / screen-reader friendly) ───
// Makes every ".editable-grid-table" behave like a proper ARIA grid:
//   * The whole grid is a SINGLE tab stop (roving tabindex). Tab moves into
//     and out of the grid instead of stepping through every cell.
//   * Arrow keys move the focused cell: Left/Right within a row, Up/Down
//     between rows. Home/End jump to first/last cell in the row,
//     Ctrl+Home/Ctrl+End to the first/last cell of the grid.
//   * Each cell is given an accessible name of "<Column name>, <value>", so
//     NVDA announces both the column heading and the data when focus lands
//     on it (e.g. "Project Name, FZ2000").
//   * Enter/Space on a cell activates the row's select/edit behaviour by
//     reusing the row's existing click handler; interactive controls inside
//     a cell (buttons, checkboxes, inputs, links) keep their own behaviour.
(function () {
    'use strict';

    var GRID_SELECTOR = 'table.editable-grid-table';

    function isVisible(el) {
        if (!el) return false;
        var style = window.getComputedStyle(el);
        return style.display !== 'none' && style.visibility !== 'hidden';
    }

    // Column headings for a grid, indexed to match the cell position in a row.
    function getColumnNames(table) {
        var headerRow = null;
        var headRows = table.querySelectorAll('thead tr');
        // Use the last header row that isn't the filter row - that's the one
        // holding the real column labels.
        for (var i = 0; i < headRows.length; i++) {
            if (!headRows[i].classList.contains('filter-row') &&
                !headRows[i].classList.contains('grid-column-group-row')) {
                headerRow = headRows[i];
            }
        }
        if (!headerRow) return [];

        return Array.prototype.map.call(headerRow.children, function (th) {
            // Strip the sort-indicator glyph so it isn't announced.
            var clone = th.cloneNode(true);
            Array.prototype.forEach.call(clone.querySelectorAll('.sort-icon, .column-resizer'), function (n) {
                n.parentNode.removeChild(n);
            });
            return (clone.textContent || '').replace(/\s+/g, ' ').trim();
        });
    }

    // Navigable data cells of a row (skips nothing - action/checkbox cells are
    // reachable too, so their controls stay usable from the keyboard).
    function getRowCells(row) {
        return Array.prototype.filter.call(row.children, function (cell) {
            return (cell.tagName === 'TD' || cell.tagName === 'TH') && isVisible(cell);
        });
    }

    function getDataRows(table) {
        return Array.prototype.filter.call(
            table.querySelectorAll('tbody tr'),
            function (row) {
                // Skip the "No records found" placeholder row.
                if (row.querySelector('td[colspan]') && !row.hasAttribute('data-row-index')) return false;
                return isVisible(row);
            }
        );
    }

    // Give a cell an accessible name combining its column heading and value so
    // screen readers announce "<Column>, <value>" when it receives focus.
    function labelCell(cell, columnName) {
        var value = (cell.textContent || '').replace(/\s+/g, ' ').trim();

        // Don't override the label of cells whose content is an interactive
        // control that already carries its own accessible name.
        var control = cell.querySelector('input, button, select, textarea, a');
        if (control) return;

        var label = columnName ? (value ? columnName + ', ' + value : columnName)
                               : value;
        if (label) {
            cell.setAttribute('aria-label', label);
        }
    }

    // The navigable "stops" in a row. Data cells are single stops, but a cell
    // containing several controls (e.g. the Edit/Copy/Delete action cell) is
    // expanded so each button is its own arrow-key stop.
    function getRowNavigables(row) {
        var stops = [];
        getRowCells(row).forEach(function (cell) {
            var controls = Array.prototype.filter.call(
                cell.querySelectorAll('button, a[href], input:not([type="hidden"]), select, textarea'),
                isVisible
            );
            if (controls.length) {
                controls.forEach(function (c) { stops.push(c); });
            } else {
                stops.push(cell);
            }
        });
        return stops;
    }

    // Set up roles, labels and a single roving tab stop for one grid. Tab
    // moves from the toolbar (e.g. the Add button) straight into the grid,
    // landing on one focusable stop; from there every other cell/action
    // button is reached with the arrow keys only. Safe to call repeatedly -
    // re-runs cheaply after AJAX reloads replace the table body.
    function initGrid(table) {
        if (!table) return;

        table.setAttribute('role', 'grid');

        var columnNames = getColumnNames(table);
        var rows = getDataRows(table);

        rows.forEach(function (row) {
            row.setAttribute('role', 'row');
            var cells = getRowCells(row);
            cells.forEach(function (cell, colIdx) {
                cell.setAttribute('role', 'gridcell');
                cell.setAttribute('tabindex', '-1');
                labelCell(cell, columnNames[colIdx]);
            });

            // Every control inside the row (action buttons, row checkboxes,
            // inline inputs) is also out of the Tab order - reached with the
            // arrow keys instead.
            Array.prototype.forEach.call(
                row.querySelectorAll('button, a[href], input:not([type="hidden"]), select, textarea'),
                function (control) { control.setAttribute('tabindex', '-1'); }
            );
        });

        // Exactly one stop in the whole grid is a real Tab stop, so Tab from
        // the toolbar lands here and Shift+Tab/Tab out continues to the next
        // control after the grid (e.g. "Records per page").
        if (!table.querySelector('[tabindex="0"]')) {
            var firstRow = rows[0];
            if (firstRow) {
                var firstStop = getRowNavigables(firstRow)[0];
                if (firstStop) firstStop.setAttribute('tabindex', '0');
            }
        }
    }

    function setActiveCell(table, stop) {
        if (!stop) return;
        Array.prototype.forEach.call(
            table.querySelectorAll('[tabindex="0"]'),
            function (s) { s.setAttribute('tabindex', '-1'); }
        );
        stop.setAttribute('tabindex', '0');
        stop.focus();
    }

    function moveFocus(table, currentStop, rowDelta, colDelta) {
        var currentRow = currentStop.closest('tr');
        var rows = getDataRows(table);
        var rowIdx = rows.indexOf(currentRow);
        if (rowIdx === -1) return;

        var stops = getRowNavigables(currentRow);
        var colIdx = stops.indexOf(currentStop);
        if (colIdx === -1) return;

        var targetRowIdx = rowIdx + rowDelta;
        var targetColIdx = colIdx + colDelta;

        if (targetRowIdx < 0 || targetRowIdx >= rows.length) return;

        var targetStops = getRowNavigables(rows[targetRowIdx]);
        if (targetColIdx < 0) targetColIdx = 0;
        if (targetColIdx >= targetStops.length) targetColIdx = targetStops.length - 1;

        setActiveCell(table, targetStops[targetColIdx]);
    }

    document.addEventListener('keydown', function (e) {
        var key = e.key;
        if (key !== 'ArrowUp' && key !== 'ArrowDown' && key !== 'ArrowLeft' &&
            key !== 'ArrowRight' && key !== 'Home' && key !== 'End' &&
            key !== 'Enter' && key !== ' ' && key !== 'Spacebar') {
            return;
        }

        var target = e.target;
        if (!target || !target.closest) return;

        var cell = target.closest('[role="gridcell"]');
        if (!cell) return;

        var table = cell.closest(GRID_SELECTOR);
        if (!table) return;

        // The current arrow-navigation stop is either the focused control
        // (action button, checkbox, inline input) or the cell itself.
        var tag = target.tagName;
        var isControl = tag === 'INPUT' || tag === 'SELECT' || tag === 'TEXTAREA' ||
                        tag === 'BUTTON' || tag === 'A';
        var currentStop = isControl ? target : cell;

        // Let text entry / dropdowns keep their own Left/Right/Home/End keys
        // for caret movement; arrows still move between rows.
        var isTextEntry = (tag === 'INPUT' && !/^(checkbox|radio|button|submit)$/i.test(target.type)) ||
                          tag === 'TEXTAREA' || tag === 'SELECT';
        if (isTextEntry && key !== 'ArrowUp' && key !== 'ArrowDown') return;

        if (key === 'Enter' || key === ' ' || key === 'Spacebar') {
            // Activate the focused control itself (Edit/Copy/Delete button,
            // checkbox), otherwise fall back to the row's select behaviour.
            if (isControl) return; // native activation handles this
            var row = cell.parentElement;
            if (row && (row.classList.contains('selectable-row') || row.hasAttribute('data-select-function'))) {
                e.preventDefault();
                row.click();
            }
            return;
        }

        e.preventDefault();

        var stops = getRowNavigables(cell.parentElement);

        if (key === 'ArrowRight') {
            moveFocus(table, currentStop, 0, 1);
        } else if (key === 'ArrowLeft') {
            moveFocus(table, currentStop, 0, -1);
        } else if (key === 'ArrowDown') {
            moveFocus(table, currentStop, 1, 0);
        } else if (key === 'ArrowUp') {
            moveFocus(table, currentStop, -1, 0);
        } else if (key === 'Home') {
            if (e.ctrlKey) {
                var firstRow = getDataRows(table)[0];
                if (firstRow) setActiveCell(table, getRowNavigables(firstRow)[0]);
            } else {
                setActiveCell(table, stops[0]);
            }
        } else if (key === 'End') {
            if (e.ctrlKey) {
                var allRows = getDataRows(table);
                var lastRow = allRows[allRows.length - 1];
                if (lastRow) {
                    var lastStops = getRowNavigables(lastRow);
                    setActiveCell(table, lastStops[lastStops.length - 1]);
                }
            } else {
                setActiveCell(table, stops[stops.length - 1]);
            }
        }
    }, true);

    // When the user clicks/tabs into a cell or action button, make it the new
    // roving Tab stop so returning to the grid later (Shift+Tab back in, or a
    // fresh Tab from the toolbar) resumes from the last position used.
    document.addEventListener('focusin', function (e) {
        var target = e.target;
        if (!target || !target.closest) return;
        var cell = target.closest('[role="gridcell"]');
        if (!cell) return;
        var table = cell.closest(GRID_SELECTOR);
        if (!table) return;

        var tag = target.tagName;
        var isControl = tag === 'INPUT' || tag === 'SELECT' || tag === 'TEXTAREA' ||
                        tag === 'BUTTON' || tag === 'A';
        var stop = isControl ? target : cell;

        if (stop.getAttribute('tabindex') !== '0') {
            Array.prototype.forEach.call(
                table.querySelectorAll('[tabindex="0"]'),
                function (s) { s.setAttribute('tabindex', '-1'); }
            );
            stop.setAttribute('tabindex', '0');
        }
    });

    function initAllGrids(root) {
        var scope = root && root.querySelectorAll ? root : document;
        Array.prototype.forEach.call(scope.querySelectorAll(GRID_SELECTOR), initGrid);
        // The root itself may be a grid (AJAX responses often return the table).
        if (scope.matches && scope.matches(GRID_SELECTOR)) initGrid(scope);
    }

    function init() {
        initAllGrids(document);

        // Grid bodies are replaced wholesale on sort/page/filter/reload, so
        // re-apply roles, labels and the roving tabindex whenever that happens.
        var observer = new MutationObserver(function (mutations) {
            var needsInit = false;
            mutations.forEach(function (m) {
                if (m.type !== 'childList' || !m.addedNodes.length) return;
                Array.prototype.forEach.call(m.addedNodes, function (node) {
                    if (node.nodeType !== 1) return;
                    if ((node.matches && node.matches(GRID_SELECTOR)) ||
                        (node.querySelector && node.querySelector(GRID_SELECTOR)) ||
                        node.closest && node.closest(GRID_SELECTOR)) {
                        needsInit = true;
                    }
                });
            });
            if (needsInit) initAllGrids(document);
        });
        observer.observe(document.body, { childList: true, subtree: true });
    }

    if (document.body) {
        init();
    } else {
        document.addEventListener('DOMContentLoaded', init);
    }
})();

// ── Global nav menubar semantics (fixes arrow keys under NVDA / JAWS) ───────
// Screen readers run web pages in "browse mode" by default, where they
// capture the arrow keys for their own virtual cursor - the keydown event
// never reaches the page, so the arrow-key navigation implemented in
// navmenu.js appears dead whenever NVDA is running. Screen readers only
// switch to "focus mode" (passing keys straight through to the page) when
// focus lands on an element exposed as a recognised interactive widget.
// The header nav is built from plain <div>/<button>/<a> markup with no menu
// semantics, so nothing triggers that switch.
//
// Applying the WAI-ARIA menubar pattern fixes it: menubar / menu / menuitem
// are widget roles screen readers auto-switch to focus mode for. This adds
// markup only - the existing navmenu.js keyboard handlers are untouched,
// they simply start receiving the events again.
(function () {
    'use strict';

    function applyMenuRoles() {
        var mainNav = document.querySelector('.main-nav');
        if (!mainNav) return;

        // Level-1 bar and its top-level items.
        if (mainNav.getAttribute('role') !== 'menubar') {
            mainNav.setAttribute('role', 'menubar');
            mainNav.setAttribute('aria-orientation', 'horizontal');
        }
        mainNav.querySelectorAll(':scope > .nav-item').forEach(function (item) {
            item.setAttribute('role', 'none');
        });
        mainNav.querySelectorAll(':scope > .nav-item > .nav-button').forEach(function (btn) {
            btn.setAttribute('role', 'menuitem');
            var dropdownId = btn.getAttribute('data-dropdown');
            if (dropdownId) {
                btn.setAttribute('aria-haspopup', 'true');
                btn.setAttribute('aria-controls', dropdownId);
                var menu = document.getElementById(dropdownId);
                btn.setAttribute('aria-expanded', menu && menu.classList.contains('show') ? 'true' : 'false');
            }
        });

        // Level-2 / level-3 menus and their entries.
        document.querySelectorAll('.dropdown-menu, .sub-dropdown-menu').forEach(function (menu) {
            menu.setAttribute('role', 'menu');
            menu.setAttribute('aria-orientation', 'vertical');
        });
        // Presentational wrappers must not break the menubar > menuitem chain.
        document.querySelectorAll('.dropdown-col, .sub-dropdown').forEach(function (wrapper) {
            wrapper.setAttribute('role', 'none');
        });
        document.querySelectorAll('a.dropdown-item').forEach(function (link) {
            link.setAttribute('role', 'menuitem');
        });
        document.querySelectorAll('.sub-dropdown-toggle').forEach(function (toggle) {
            toggle.setAttribute('role', 'menuitem');
            toggle.setAttribute('aria-haspopup', 'true');
            var subDropdown = toggle.closest('.sub-dropdown');
            var subMenu = subDropdown ? subDropdown.querySelector(':scope > .sub-dropdown-menu') : null;
            toggle.setAttribute('aria-expanded', subMenu && subMenu.classList.contains('show') ? 'true' : 'false');
        });

        // The user-profile dropdown is the same kind of widget.
        var userBtn = document.getElementById('userdropdownbtn');
        var userMenu = document.getElementById('userdropdowndp');
        if (userBtn && userMenu) {
            userBtn.setAttribute('aria-haspopup', 'true');
            userBtn.setAttribute('aria-expanded', userMenu.classList.contains('show') ? 'true' : 'false');
            userMenu.setAttribute('role', 'menu');
            userMenu.querySelectorAll('a, button').forEach(function (el) {
                el.setAttribute('role', 'menuitem');
            });
        }
    }

    function initMenuRoles() {
        var mainNav = document.querySelector('.main-nav');
        if (!mainNav) return;

        applyMenuRoles();

        // Menus are shown/hidden by toggling the "show"/"active" classes, so
        // keep aria-expanded (and roles on any late-injected markup) in sync.
        var observer = new MutationObserver(applyMenuRoles);
        observer.observe(mainNav, {
            attributes: true, attributeFilter: ['class'], subtree: true, childList: true
        });

        var userMenu = document.getElementById('userdropdowndp');
        if (userMenu) {
            observer.observe(userMenu, {
                attributes: true, attributeFilter: ['class'], subtree: true, childList: true
            });
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initMenuRoles);
    } else {
        initMenuRoles();
    }
})();
