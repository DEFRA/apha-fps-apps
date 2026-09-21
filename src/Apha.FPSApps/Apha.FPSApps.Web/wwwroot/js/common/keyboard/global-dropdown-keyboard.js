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

    // Make the page's top heading reachable and announced by keyboard/screen
    // readers AFTER the top navigation menus, rather than grabbing focus on
    // load (which would skip past the menu options). The heading appears after
    // the nav in the DOM, so giving it tabindex="0" places it in the natural
    // Tab order right after the top menus: once the user tabs past the menus,
    // focus lands on the heading and it is read out. Applied globally via this
    // shared script, which is loaded on every area layout.
    // Additive: does not modify the existing restoreFocusAfterRefresh logic.
    function makePageHeadingFocusable() {
        var heading = document.querySelector('main h1, .content-wrapper h1, h1');
        if (!heading || !isVisible(heading)) return;
        if (!heading.hasAttribute('tabindex')) {
            heading.setAttribute('tabindex', '0');
        }
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

    // Make the app footer reachable and announced by keyboard/screen readers.
    // The shared AppFooter renders as a plain <div class="govuk-footer">, which
    // has no landmark semantics, so NVDA skips it entirely and the copyright
    // text is never read. Promoting it to a "contentinfo" landmark and adding
    // tabindex="0" places it at the natural end of the Tab order, where its
    // visible text is read once. Applied globally via this shared script.
    // No aria-label is set: the footer already contains visible text, and an
    // accessible name would make NVDA announce the copyright twice (once as
    // the name, once as the contained text).
    // Additive: markup in Views/Shared/Components/AppFooter stays unchanged.
    function makeFooterReadable() {
        var footer = document.querySelector('.govuk-footer');
        if (!footer || !isVisible(footer)) return;
        if (!footer.hasAttribute('role') && footer.tagName !== 'FOOTER') {
            footer.setAttribute('role', 'contentinfo');
        }
        if (!footer.hasAttribute('tabindex')) {
            footer.setAttribute('tabindex', '0');
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', restoreFocusAfterRefresh);
        document.addEventListener('DOMContentLoaded', makePageHeadingFocusable);
        document.addEventListener('DOMContentLoaded', makeFooterReadable);
    } else {
        restoreFocusAfterRefresh();
        makePageHeadingFocusable();
        makeFooterReadable();
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

    // Build the column header texts for the table a dropdown row belongs to, so
    // each cell value can be announced together with the column it belongs to
    // (e.g. "Project: FP1234, Description: Some title").
    function getColumnHeaders(row) {
        var table = row.closest('table');
        if (!table) return [];
        var headerRow = table.querySelector('thead tr');
        if (!headerRow) return [];
        return Array.prototype.map.call(headerRow.querySelectorAll('th, td'), function (th) {
            return (th.textContent || '').replace(/\s+/g, ' ').trim();
        });
    }

    // Give a multi-column dropdown row an accessible name describing every
    // column it contains. Applying role="option" to a <tr> removes its native
    // table semantics, so the individual <td> cells are no longer exposed to
    // screen readers and NVDA announces only "row" with no data. Composing an
    // aria-label from the cell values (prefixed with their column headers when
    // available) restores the full record announcement.
    function ensureRowIsLabelled(row) {
        if (row.hasAttribute('aria-label')) return;

        var cells = row.querySelectorAll('td, th');
        if (!cells.length) return;

        var headers = getColumnHeaders(row);
        var parts = [];
        Array.prototype.forEach.call(cells, function (cell, i) {
            if (cell.getAttribute('aria-hidden') === 'true') return;
            var text = (cell.textContent || '').replace(/\s+/g, ' ').trim();
            if (!text) return;
            var header = headers[i];
            parts.push(header ? header + ': ' + text : text);
        });

        if (parts.length) row.setAttribute('aria-label', parts.join(', '));
    }

    // Make each row keyboard-focusable/actionable the first time it is encountered.
    function ensureRowIsAccessible(row) {
        if (row.hasAttribute('data-kbd-enabled')) return;
        row.setAttribute('data-kbd-enabled', 'true');
        if (!row.hasAttribute('tabindex')) row.setAttribute('tabindex', '-1');
        if (!row.hasAttribute('role')) row.setAttribute('role', 'option');
        ensureRowIsLabelled(row);

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

// ── Shared modal popup focus management ────────────────────────────────────
// Covers both the legacy #modalPopup (toggles a "show" class) and every
// GOV.UK edit-record dialog using class="govuk-edit-modal" (toggles an
// "open" class, e.g. #editRecordModal, #editFormDatesModal,
// #editInvoiceModal, #importEditModal, #editYFDModal, #timeRecordModal).
// Many of the .govuk-edit-modal dialogs are loaded/replaced dynamically via
// AJAX into a container (e.g. $('#milestoneModalContainer').html(html)), so
// this also watches the DOM for ones added after page load.
(function () {
    'use strict';

    var trackedModals = new WeakSet();

    // When openClass is falsy, the modal doesn't toggle a class to show/hide
    // (e.g. #modalContainer in BudgetResourceLevel/Index.cshtml, which is
    // shown/hidden purely via inline style - $(...).css('display','flex') /
    // $(...).hide()). In that case fall back to checking computed visibility
    // so the same focus-management logic still applies.
    function isModalVisible(modal) {
        var style = window.getComputedStyle(modal);
        return style.display !== 'none' && style.visibility !== 'hidden';
    }

    function isModalOpen(modal, openClass) {
        if (openClass) return modal.classList.contains(openClass);
        return isModalVisible(modal);
    }

    function attachModal(modal, openClass, focusScopeSelector, toggleDisplay) {
        if (!modal || trackedModals.has(modal)) return;
        trackedModals.add(modal);

        var focusScope = focusScopeSelector ? modal.querySelector(focusScopeSelector) : null;
        var previouslyFocused = null;
        var isOpen = isModalOpen(modal, openClass);

        function getFocusableElements() {
            var scope = focusScope || modal;
            return Array.prototype.slice.call(
                scope.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])')
            ).filter(function (el) {
                return !el.hasAttribute('disabled') && el.offsetParent !== null;
            });
        }

        // When a modal opens, focus should land on its close ("X") button
        // rather than the first form field, so keyboard/screen-reader users
        // land on a predictable, always-available control (form fields can be
        // disabled/hidden depending on modal state, the close button is not).
        // The close button often lives in the modal header, outside the
        // narrower focusScope used for Tab-trapping, so it is looked up
        // against the whole modal rather than the getFocusableElements() list.
        function getInitialFocusTarget(focusable) {
            var closeButton = modal.querySelector(
                '.btn-close, [data-bs-dismiss="modal"], [aria-label="Close"], [aria-label="close"]'
            );
            if (closeButton && !closeButton.hasAttribute('disabled') && closeButton.offsetParent !== null) {
                return closeButton;
            }
            return focusable.length ? focusable[0] : null;
        }

        // Keep focus trapped inside the modal while it's open. Buttons like
        // Save/Cancel/Close often trigger an AJAX call or DOM update that can
        // remove/disable/hide themselves once clicked; when that happens the
        // browser drops focus back to <body> (i.e. "the background") instead
        // of somewhere inside the still-open modal. Whenever focus lands
        // outside the modal while it's open, pull it back in immediately -
        // unless the modal is in the middle of closing (isOpen is set false
        // just before we restore focus to the trigger that opened it).
        // Some multi-column dropdown panels are re-parented to <body> while open
        // (a transformed .modal-dialog ancestor would otherwise break their fixed
        // positioning). They are still logically part of the modal, so focus must
        // be allowed to stay inside them - otherwise the trap below immediately
        // yanks focus back and the panel's search box cannot be typed into.
        function isFloatingModalPanel(target) {
            return !!(target && target.closest && target.closest('[data-floating-dropdown-panel]'));
        }

        document.addEventListener('focusin', function (e) {
            if (!isOpen) return;
            if (modal.contains(e.target)) return;
            if (isFloatingModalPanel(e.target)) return;

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
            var nowOpen = isModalOpen(modal, openClass);
            if (nowOpen && !isOpen) {
                if (toggleDisplay) {
                    modal.style.display = 'flex';
                    document.body.style.overflow = 'hidden';
                }

                isOpen = true;
                previouslyFocused = document.activeElement;

                setTimeout(function () {
                    var focusable = getFocusableElements();
                    var initialTarget = getInitialFocusTarget(focusable);
                    if (initialTarget) {
                        initialTarget.focus();
                    } else {
                        modal.focus();
                    }
                }, 0);
            } else if (!nowOpen && isOpen) {
                isOpen = false;

                if (toggleDisplay) {
                    modal.style.display = 'none';
                    document.body.style.overflow = '';
                }

                if (previouslyFocused && typeof previouslyFocused.focus === 'function') {
                    previouslyFocused.focus();
                }
                previouslyFocused = null;
            }
        });
        observer.observe(modal, { attributes: true, attributeFilter: openClass ? ['class'] : ['class', 'style'] });

        // Handle the case where the modal is already "open" the moment we
        // first attach to it. This happens for dynamically-injected dialogs
        // (e.g. container.html(html) followed synchronously by
        // modal.classList.add('open')): by the time the MutationObserver
        // above starts observing, the "open" class is already present, so
        // the false->true transition it looks for never fires and focus is
        // never moved into the modal (it stays wherever it was - e.g. the
        // "Add"/"Edit" button - which looks like "focus stuck in the
        // background"). Detect that up front and move focus in immediately.
        if (isOpen) {
            previouslyFocused = document.activeElement;

            setTimeout(function () {
                var focusable = getFocusableElements();
                var initialTarget = getInitialFocusTarget(focusable);
                if (initialTarget) {
                    initialTarget.focus();
                } else {
                    modal.focus();
                }
            }, 0);
        }
    }

    function scanEditModals(root) {
        var scope = root || document;
        if (!scope.querySelectorAll) return;
        Array.prototype.forEach.call(scope.querySelectorAll('.govuk-edit-modal'), function (modal) {
            attachModal(modal, 'open', '.govuk-edit-modal-dialog', false);
        });
    }

    // Attaches the legacy "show"-class modal handling to every matching
    // popup on the page, not just the original #modalPopup. Several pages
    // (e.g. DepartmentIncome's #modalPopupDeptIncome / #modaPopupBodyDeptIncome)
    // define their own uniquely-suffixed modal/body id pair following the
    // same "modalPopup*" / "modaPopupBody*" naming convention, so they were
    // previously never picked up here and focus stayed in the background.
    function scanLegacyModalPopups(root) {
        var scope = root || document;
        if (!scope.querySelectorAll) return;
        Array.prototype.forEach.call(scope.querySelectorAll('[id^="modalPopup"]'), function (modal) {
            var body = modal.querySelector('[id^="modaPopupBody"]');
            attachModal(modal, 'show', body ? '#' + body.id : null, true);
        });
    }

    // Attaches focus management to modals that show/hide purely via inline
    // style (e.g. $('#modalContainer').css('display', 'flex') / .hide()) and
    // never toggle a class - such as #modalContainer in
    // BudgetResourceLevel/Index.cshtml. openClass is omitted so attachModal
    // tracks visibility instead of a class.
    function scanStyleToggledModals(root) {
        var scope = root || document;
        if (!scope.querySelectorAll) return;
        Array.prototype.forEach.call(scope.querySelectorAll('.brl-modal-container'), function (modal) {
            attachModal(modal, null, '.modal-dialog', false);
        });
    }

    function init() {
        scanLegacyModalPopups(document);

        scanEditModals(document);

        scanStyleToggledModals(document);

        // Watch for .govuk-edit-modal dialogs injected/replaced dynamically
        // (AJAX-loaded partials such as _AddEditMilestone.cshtml,
        // _AddEditInvoice.cshtml, etc. are typically inserted via
        // container.html(html)).
        var bodyObserver = new MutationObserver(function (mutations) {
            for (var i = 0; i < mutations.length; i++) {
                var added = mutations[i].addedNodes;
                for (var j = 0; j < added.length; j++) {
                    var node = added[j];
                    if (node.nodeType !== 1) continue;
                    if (node.classList && node.classList.contains('govuk-edit-modal')) {
                        attachModal(node, 'open', '.govuk-edit-modal-dialog', false);
                    }
                    if (node.querySelectorAll) {
                        scanEditModals(node);
                    }
                }
            }
        });
        bodyObserver.observe(document.body, { childList: true, subtree: true });
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
// Also enforces consistency with mouse behaviour: if a checkbox is visually
// non-interactive because it (or an ancestor) has `pointer-events:none` —
// as used by the datagrid GridColumnType.Checkbox cells — then Enter and
// Space must NOT toggle it either, so keyboard and mouse behave the same.
(function () {
    'use strict';

    // Returns true if this element (or any ancestor) has a computed
    // pointer-events value of "none", meaning mouse clicks can't reach it.
    function isPointerEventsBlocked(el) {
        for (var node = el; node && node.nodeType === 1; node = node.parentElement) {
            var pe = window.getComputedStyle(node).pointerEvents;
            if (pe === 'none') return true;
        }
        return false;
    }

    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter' && e.key !== ' ' && e.key !== 'Spacebar') return;

        var target = e.target;
        if (!target || target.tagName !== 'INPUT' || target.type !== 'checkbox') return;
        if (target.disabled || target.readOnly) return;

        // Match mouse behaviour: if the checkbox isn't clickable via mouse
        // (pointer-events:none on it or an ancestor), don't let keyboard
        // toggle it either. Block both Enter and Space (Space is the native
        // browser toggle key for checkboxes, so it must be prevented too).
        if (isPointerEventsBlocked(target)) {
            e.preventDefault();
            return;
        }

        // Only Enter needs a manual toggle; Space is handled natively by
        // the browser for checkbox inputs.
        if (e.key === 'Enter') {
            e.preventDefault();
            target.checked = !target.checked;
            target.dispatchEvent(new Event('input', { bubbles: true }));
            target.dispatchEvent(new Event('change', { bubbles: true }));
        }
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

// ── Global GOV.UK tabs: hand off focus to next tab after panel content ──────
// When a user Tabs through the focusable elements inside the currently
// active tab panel, focus would naturally leave the whole tabs component
// once it reaches the last focusable element in that panel. Instead, we
// want focus to move to the *next* tab header so keyboard users continue
// to move through the tabs component (unless the active tab is the last
// one, in which case default browser behaviour — moving focus out of the
// component — is preserved).
(function () {
    'use strict';

    function isFocusable(el) {
        if (!el || el.disabled || el.hidden) {
            return false;
        }
        if (el.tabIndex < 0) {
            return false;
        }
        var style = window.getComputedStyle(el);
        if (style.display === 'none' || style.visibility === 'hidden') {
            return false;
        }
        return true;
    }

    function getFocusableElements(container) {
        var selector = 'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';
        return Array.prototype.slice
            .call(container.querySelectorAll(selector))
            .filter(isFocusable);
    }

    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Tab' || e.shiftKey) {
            return;
        }

        var target = e.target;
        var panel = target && target.closest ? target.closest('.govuk-tabs__panel') : null;
        if (!panel) {
            return;
        }

        // Ignore panels that are hidden/inactive.
        if (panel.classList.contains('govuk-tabs__panel--hidden') || panel.style.display === 'none') {
            return;
        }

        var tabsWrapper = panel.closest('.govuk-tabs');
        if (!tabsWrapper) {
            return;
        }

        var focusable = getFocusableElements(panel);
        if (!focusable.length || target !== focusable[focusable.length - 1]) {
            return;
        }

        var tabs = Array.prototype.slice.call(tabsWrapper.querySelectorAll('.govuk-tabs__tab'));
        var selectedTab = tabsWrapper.querySelector('.govuk-tabs__list-item--selected .govuk-tabs__tab');
        var currentIndex = tabs.indexOf(selectedTab);
        if (currentIndex === -1) {
            return;
        }

        var nextIndex = currentIndex + 1;
        if (nextIndex >= tabs.length) {
            // Last tab's content finished — allow focus to leave the component naturally.
            return;
        }

        e.preventDefault();
        tabs[nextIndex].focus();
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

        // Screen readers run pages in "browse mode", where they capture the
        // arrow keys for their own virtual cursor - the keydown never reaches
        // the page, so the arrow-key navigation below appears dead whenever
        // NVDA is running. Exposing the grid's wrapper as an application
        // region makes the screen reader switch to focus mode while focus is
        // inside the grid, so arrow keys are passed straight through. The
        // table keeps its grid/row/gridcell semantics, so rows, columns and
        // cell values are still announced.
        var appRegion = table.closest('.grid-scroll-container') || table.parentElement;
        if (appRegion && appRegion.getAttribute('role') !== 'application') {
            appRegion.setAttribute('role', 'application');
            appRegion.setAttribute('aria-roledescription', 'data grid');
        }

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
                var menu = document.getElementById(dropdownId);
                // An empty popup is not a menu, so the button must not advertise
                // one - aria-haspopup / aria-controls / aria-expanded are dropped
                // and the item is exposed as unavailable instead.
                if (!menu || !menu.querySelector('a.dropdown-item, .sub-dropdown-toggle')) {
                    btn.removeAttribute('aria-haspopup');
                    btn.removeAttribute('aria-controls');
                    btn.removeAttribute('aria-expanded');
                    btn.setAttribute('aria-disabled', 'true');
                    return;
                }
                btn.removeAttribute('aria-disabled');
                btn.setAttribute('aria-haspopup', 'true');
                btn.setAttribute('aria-controls', dropdownId);
                btn.setAttribute('aria-expanded', menu.classList.contains('show') ? 'true' : 'false');
            }
        });

        // Level-2 / level-3 menus and their entries.
        // A role="menu" with no menuitem descendants is an invalid ARIA menu, so
        // containers that render empty (config- or role-gated links) are left as
        // plain markup rather than being given menu semantics.
        document.querySelectorAll('.dropdown-menu, .sub-dropdown-menu').forEach(function (menu) {
            if (!menu.querySelector('a.dropdown-item, .sub-dropdown-toggle')) {
                menu.removeAttribute('role');
                menu.removeAttribute('aria-orientation');
                return;
            }
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
            var subDropdown = toggle.closest('.sub-dropdown');
            var subMenu = subDropdown ? subDropdown.querySelector(':scope > .sub-dropdown-menu') : null;
            if (!subMenu || !subMenu.querySelector('a.dropdown-item, .sub-dropdown-toggle')) {
                toggle.removeAttribute('aria-haspopup');
                toggle.removeAttribute('aria-controls');
                toggle.removeAttribute('aria-expanded');
                toggle.setAttribute('aria-disabled', 'true');
                return;
            }
            toggle.removeAttribute('aria-disabled');
            toggle.setAttribute('aria-haspopup', 'true');
            if (subMenu.id) {
                toggle.setAttribute('aria-controls', subMenu.id);
            }
            toggle.setAttribute('aria-expanded', subMenu.classList.contains('show') ? 'true' : 'false');
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

// ── Focus trap for the GOV.UK alert/confirm dialog (govuk-modal-dialog.js) ──
// Separate, additive module - does not modify govuk-modal-dialog.js or any
// other existing code. showAlertMessage()/showGovukConfirm() build a dialog
// on the fly (a <div data-govuk-modal="backdrop"> containing the actual
// dialog with role="dialog"/aria-modal="true") and already trap Tab/Escape,
// but other global listeners in this app (e.g. rememberFocus() above, or
// scripts on the page reacting to an AJAX response) can still call .focus()
// on an element in the page "background" while the dialog is open, moving
// real focus out from under it. This watches for that and pulls focus back
// into the dialog immediately, until OK/Cancel/Escape closes it.
(function () {
    'use strict';

    var DIALOG_BACKDROP_SELECTOR = '[data-govuk-modal="backdrop"]';

    function getOpenDialog() {
        // openDialog() always appends the backdrop as the last matching node;
        // querySelector returns the first in document order, but there is
        // normally only ever one open at a time (a "pending" promise chain
        // serialises them in govuk-modal-dialog.js).
        return document.querySelector(DIALOG_BACKDROP_SELECTOR + ' [role="dialog"]');
    }

    function getFocusable(dialog) {
        return Array.prototype.slice.call(
            dialog.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])')
        ).filter(function (el) {
            return !el.hasAttribute('disabled') && el.getAttribute('aria-hidden') !== 'true';
        });
    }

    document.addEventListener('focusin', function (e) {
        var dialog = getOpenDialog();
        if (!dialog) return;
        if (dialog.contains(e.target)) return;

        // Focus escaped the open GOV.UK dialog to the background - pull it
        // back in without touching the dialog's own open/close logic.
        var focusable = getFocusable(dialog);
        if (focusable.length) {
            focusable[0].focus();
        } else {
            dialog.focus();
        }
    });
})();

// ── Reset focus to the top of the page after Back/Forward navigation ───────
// Going Back (browser button, a govuk-back-link or history.back()) restores
// the previous page along with its scroll position, and the browser leaves
// focus on <body> wherever the user was. Screen readers therefore carry on
// reading from the middle of the restored page instead of announcing it from
// the start. Moving focus to the application logo/title in the header puts
// the reading position back at the top of the page, exactly as it is on a
// fresh page load.
(function () {
    'use strict';

    var HEADER_TARGETS = ['.app-log', 'header .app-log-wrapper', 'header'];

    function getHeaderTarget() {
        for (var i = 0; i < HEADER_TARGETS.length; i++) {
            var el = document.querySelector(HEADER_TARGETS[i]);
            if (el) return el;
        }
        return null;
    }

    function isBackForwardNavigation(persisted) {
        // Restored from the back/forward cache.
        if (persisted) return true;

        if (window.performance && typeof window.performance.getEntriesByType === 'function') {
            var entries = window.performance.getEntriesByType('navigation');
            if (entries && entries.length) return entries[0].type === 'back_forward';
        }
        // Legacy fallback: 2 === TYPE_BACK_FORWARD.
        return !!(window.performance && window.performance.navigation &&
                  window.performance.navigation.type === 2);
    }

    function focusPageTop() {
        var target = getHeaderTarget();
        if (!target) return;

        if (!target.hasAttribute('tabindex')) {
            target.setAttribute('tabindex', '-1');
        }
        window.scrollTo(0, 0);
        target.focus();
    }

    window.addEventListener('pageshow', function (e) {
        if (!isBackForwardNavigation(e.persisted)) return;
        // Let the browser finish restoring scroll/focus first, then override.
        window.setTimeout(focusPageTop, 0);
    });
})();

// ── Global "read full textarea value" support (NVDA / screen-reader friendly) ──
// A <textarea rows="1"> (or any textarea shorter than its content) clips the
// value visually to one line, and only reveals the rest via internal
// scrolling. Sighted users can't see the hidden text either, and screen
// reader users have to know to arrow-down inside the field to discover more
// content exists. This is common across the app for compact, read-only
// summary fields (e.g. Project Description cards).
//
// Fix: keep the existing visual size/rows exactly as authored (no UI change),
// and for READ-ONLY textareas expose the label plus the complete value as a
// single accessible name. The element is also given role="img", which makes
// screen readers announce only that name and ignore the textbox's own value.
// Without this, NVDA announces twice: once for the aria-label (full text) and
// again for the control's value (clipped to the current visible line).
// Editable textareas are left completely untouched so typing/caret reporting
// still behaves natively. No hidden/duplicate DOM elements are added.
// Purely additive, safe to re-run.
(function () {
    'use strict';

    function isVisible(el) {
        if (!el) return false;
        var style = window.getComputedStyle(el);
        return style.display !== 'none' && style.visibility !== 'hidden';
    }

    function findLabelText(textarea) {
        var label = null;
        if (textarea.id) label = document.querySelector('label[for="' + CSS.escape(textarea.id) + '"]');
        if (!label) label = textarea.closest('label');
        if (!label) return '';
        return (label.textContent || '').replace(/\s+/g, ' ').trim();
    }

    function syncAriaLabel(textarea) {
        // Only read-only/disabled summary fields are re-presented; editable
        // fields must keep their native textbox semantics.
        if (!textarea.readOnly && !textarea.disabled) return;

        var labelText = findLabelText(textarea);

        // Cleared value: drop the stale accessible name rather than leaving
        // the previously announced text in place.
        if (!textarea.value) {
            textarea.removeAttribute('aria-label');
            return;
        }

        var value = textarea.value.replace(/\s+/g, ' ').trim();
        textarea.setAttribute('aria-label', labelText ? labelText + ': ' + value : value);
        // Announce the name once, as a single static item, instead of
        // name + (partial) textbox value.
        textarea.setAttribute('role', 'img');
        textarea.setAttribute('aria-readonly', 'true');
    }

    function processAllTextareas(root) {
        var scope = root && root.querySelectorAll ? root : document;
        Array.prototype.forEach.call(scope.querySelectorAll('textarea'), function (textarea) {
            if (!isVisible(textarea)) return;
            syncAriaLabel(textarea);
        });
    }

    function init() {
        processAllTextareas(document);

        // Summary/comment textareas are frequently populated or replaced
        // after the initial page load (AJAX partials, grid reloads, dynamic
        // selection changes), so keep re-checking for new/changed textareas.
        var observer = new MutationObserver(function (mutations) {
            var needsCheck = false;
            mutations.forEach(function (m) {
                if (m.type === 'childList' && m.addedNodes.length) {
                    Array.prototype.forEach.call(m.addedNodes, function (node) {
                        if (node.nodeType !== 1) return;
                        if (node.tagName === 'TEXTAREA' || (node.querySelector && node.querySelector('textarea'))) {
                            needsCheck = true;
                        }
                    });
                } else if (m.type === 'characterData' || (m.type === 'attributes' && m.attributeName === 'value')) {
                    needsCheck = true;
                }
            });
            if (needsCheck) processAllTextareas(document);
        });
        observer.observe(document.body, { childList: true, subtree: true, characterData: true });

        // Textarea values changed via script (e.g. el.value = '...') don't
        // fire childList/attribute mutations, so also catch the standard
        // input/change events.
        document.addEventListener('input', function (e) {
            if (e.target && e.target.tagName === 'TEXTAREA') syncAriaLabel(e.target);
        }, true);
        document.addEventListener('change', function (e) {
            if (e.target && e.target.tagName === 'TEXTAREA') syncAriaLabel(e.target);
        }, true);

        // Dropdown/multi-select pages update these summary fields with a plain
        // assignment ("textarea.value = selectedTitle"). That fires no input or
        // change event and mutates no DOM node, so neither the listeners above
        // nor the MutationObserver would ever see it - leaving the aria-label
        // stuck on whichever option was selected first. Wrapping the native
        // "value" setter keeps the accessible name in sync with every
        // programmatic assignment, whatever page script performs it.
        var valueDescriptor = Object.getOwnPropertyDescriptor(HTMLTextAreaElement.prototype, 'value');
        if (valueDescriptor && valueDescriptor.configurable && valueDescriptor.set) {
            Object.defineProperty(HTMLTextAreaElement.prototype, 'value', {
                configurable: true,
                enumerable: valueDescriptor.enumerable,
                get: valueDescriptor.get,
                set: function (newValue) {
                    valueDescriptor.set.call(this, newValue);
                    try { syncAriaLabel(this); } catch (ignored) { /* never break assignment */ }
                }
            });
        }
    }

    if (document.body) {
        init();
    } else {
        document.addEventListener('DOMContentLoaded', init);
    }
})();

// ── Global summary-list / summary-card readability (NVDA friendly) ───────────
// Summary cards across the apps render as a <dl class="govuk-summary-list">
// with each row split into a <dt> key and a <dd> value (e.g. the "Time Summary"
// card: HrsPaid / Leave / SickSpecial / HrsAvail / ...). Two problems follow:
//   1. None of it is in the Tab order, so keyboard users never reach the card
//      and NVDA only exposes it if the user happens to browse into that region.
//   2. The key and the value are separate nodes, so they are announced
//      disconnected - the number is read without the label it belongs to.
// Fix: make each row a single focusable item whose accessible name combines the
// key and its value ("HrsPaid: 37.5"), and expose the card itself as a labelled
// group so its title is announced once on entry. Works for any <dl>-based
// summary structure anywhere in the apps - no per-page markup changes needed.
(function () {
    'use strict';

    function isVisible(el) {
        if (!el) return false;
        var style = window.getComputedStyle(el);
        return style.display !== 'none' && style.visibility !== 'hidden';
    }

    function textOf(el) {
        return el ? (el.textContent || '').replace(/\s+/g, ' ').trim() : '';
    }

    // Label a single key/value row so both halves are announced together.
    function enhanceRow(row) {
        var key = row.querySelector('dt');
        var value = row.querySelector('dd');
        if (!key || !value) return;

        var keyText = textOf(key);
        var valueText = textOf(value);
        if (!keyText && !valueText) return;

        var name = keyText && valueText ? keyText + ': ' + valueText
                                        : (keyText || valueText);

        row.setAttribute('aria-label', name);
        // The <dt>/<dd> children are already covered by the row's accessible
        // name, so hide them to avoid the value being announced a second time.
        key.setAttribute('aria-hidden', 'true');
        value.setAttribute('aria-hidden', 'true');
        if (!row.hasAttribute('role')) row.setAttribute('role', 'group');
        if (!row.hasAttribute('tabindex')) row.setAttribute('tabindex', '0');
    }

    // A <dl> whose rows are plain <dt>/<dd> pairs without a wrapping row
    // element still needs the pairs joining, so fall back to pairing siblings.
    function enhanceBareList(list) {
        var children = Array.prototype.slice.call(list.children);
        for (var i = 0; i < children.length - 1; i++) {
            var key = children[i];
            var value = children[i + 1];
            if (key.tagName !== 'DT' || value.tagName !== 'DD') continue;
            if (value.hasAttribute('data-kbd-summary-labelled')) continue;

            var keyText = textOf(key);
            var valueText = textOf(value);
            if (!keyText && !valueText) continue;

            value.setAttribute('data-kbd-summary-labelled', 'true');
            value.setAttribute('aria-label', keyText && valueText ? keyText + ': ' + valueText : (keyText || valueText));
            if (!value.hasAttribute('role')) value.setAttribute('role', 'group');
            if (!value.hasAttribute('tabindex')) value.setAttribute('tabindex', '0');
        }
    }

    // Expose the surrounding card as a named region so its heading (e.g.
    // "Time Summary") is announced once when focus enters the card.
    function enhanceCard(list) {
        var card = list.closest('.govuk-summary-card');
        if (!card || card.hasAttribute('role')) return;

        var title = card.querySelector('.govuk-summary-card__title, h1, h2, h3, h4');
        if (!title) return;

        if (!title.id) {
            title.id = 'kbd-summary-card-title-' + Math.random().toString(36).slice(2, 10);
        }
        card.setAttribute('role', 'region');
        card.setAttribute('aria-labelledby', title.id);
    }

    // Inline "label: value" header pairs shown above grids (e.g. the
    // "Staff Name: <name>  Grade: <grade>" list above the ZT code grid) are
    // rendered as two sibling elements - a bold label and a plain value. They
    // are not focusable and the two halves are announced as disconnected
    // fragments, so the value is read without the label it belongs to.
    // Joining them into one focusable item fixes both issues. Recognised by
    // the label ending in ":" followed by a sibling holding the value.
    function enhanceInlinePair(container) {
        if (container.hasAttribute('data-kbd-summary-labelled')) return;

        var parts = container.querySelectorAll(':scope > span, :scope > strong, :scope > b');
        if (parts.length !== 2) return;

        var keyText = textOf(parts[0]);
        var valueText = textOf(parts[1]);
        if (!/:\s*$/.test(keyText) || !valueText) return;

        container.setAttribute('data-kbd-summary-labelled', 'true');
        container.setAttribute('aria-label', keyText.replace(/\s*:\s*$/, '') + ': ' + valueText);
        // Children are covered by the container's accessible name; hide them so
        // the value isn't announced twice.
        parts[0].setAttribute('aria-hidden', 'true');
        parts[1].setAttribute('aria-hidden', 'true');
        if (!container.hasAttribute('role')) container.setAttribute('role', 'group');
        if (!container.hasAttribute('tabindex')) container.setAttribute('tabindex', '0');
    }

    function processInlinePairs(root) {
        var scope = root && root.querySelectorAll ? root : document;
        Array.prototype.forEach.call(scope.querySelectorAll('li, p, div'), function (container) {
            if (!isVisible(container)) return;
            enhanceInlinePair(container);
        });
    }

    function processSummaryLists(root) {
        var scope = root && root.querySelectorAll ? root : document;
        Array.prototype.forEach.call(scope.querySelectorAll('dl'), function (list) {
            if (!isVisible(list)) return;

            var rows = list.querySelectorAll('.govuk-summary-list__row');
            if (rows.length) {
                Array.prototype.forEach.call(rows, function (row) {
                    if (row.hasAttribute('data-kbd-summary-labelled')) return;
                    row.setAttribute('data-kbd-summary-labelled', 'true');
                    enhanceRow(row);
                });
            } else {
                enhanceBareList(list);
            }

            enhanceCard(list);
        });

        processInlinePairs(scope);
    }

    function init() {
        processSummaryLists(document);

        // Summary values are frequently recalculated or re-rendered after the
        // initial load (AJAX saves, grid edits, dropdown selections), so keep
        // the accessible names in sync with the visible figures.
        var observer = new MutationObserver(function (mutations) {
            var needsCheck = false;
            mutations.forEach(function (m) {
                if (m.type === 'characterData') {
                    needsCheck = true;
                    return;
                }
                Array.prototype.forEach.call(m.addedNodes || [], function (node) {
                    if (node.nodeType !== 1) return;
                    // Any added element may bring a summary list or an inline
                    // "label: value" header pair with it.
                    needsCheck = true;
                });
            });
            if (!needsCheck) return;

            // Values changed in place: clear the cached marker so rows are
            // re-labelled with the new figures rather than the stale ones.
            Array.prototype.forEach.call(
                document.querySelectorAll('[data-kbd-summary-labelled]'),
                function (el) { el.removeAttribute('data-kbd-summary-labelled'); });
            processSummaryLists(document);        });
        observer.observe(document.body, { childList: true, subtree: true, characterData: true });
    }

    if (document.body) {
        init();
    } else {
        document.addEventListener('DOMContentLoaded', init);
    }
})();

// ──────────────────────────────────────────────────────────────────────────
// Initial page focus
// After a full page navigation (e.g. clicking a side-nav link) browsers leave
// focus on the document body. NVDA then resumes reading from wherever its
// virtual cursor happened to be instead of the top of the page. Explicitly
// moving focus to the application title in the header puts both the tab order
// and the screen-reader review cursor back at the top of every page.
// ──────────────────────────────────────────────────────────────────────────
(function () {
    'use strict';

    // The app logo/title lives in the AppHeader view component. Anchor the
    // lookup on .app-log-wrapper so we never accidentally match a page-level
    // <header> (e.g. a card or grid toolbar header) further down the document.
    function findLogo() {
        return document.querySelector('.app-log-wrapper .app-log') ||
               document.querySelector('.app-log-wrapper') ||
               document.querySelector('header .app-log') ||
               null;
    }

    // Focus is only considered "intentionally claimed" by the page when it sits
    // on real page content. Focus parked on the chrome (main menu buttons, the
    // side nav, the user dropdown) is exactly the bug we are fixing, so it
    // must not block us from moving back to the logo.
    function isChromeOrUnfocused(el) {
        if (!el || el === document.body || el === document.documentElement) return true;
        return !!(el.closest &&
            el.closest('#header, .main-nav, .header-nav, .navmenu, .userdropdown, .userdropdownbtn, ' +
                       '.sidenav, #shortnav, .side-nav, .project-side-nav'));
    }

    // A bare "#" (or any invalid fragment) is not a real in-page target.
    // querySelector('#') throws a SyntaxError, which previously aborted the
    // whole routine on pages whose nav links use href="#".
    function hasRealHashTarget() {
        var hash = window.location.hash;
        if (!hash || hash === '#') return false;
        try {
            return !!document.querySelector(hash);
        } catch (e) {
            return false;
        }
    }

    function applyFocus() {
        var target = findLogo();
        if (!target) return;

        if (!target.hasAttribute('tabindex')) {
            target.setAttribute('tabindex', '-1');
        }

        // Don't fight genuine page-content focus (autofocus field, validation
        // summary, or a dialog opened on load).
        if (!isChromeOrUnfocused(document.activeElement)) return;

        try {
            target.focus({ preventScroll: true });
        } catch (e) {
            target.focus();
        }
        window.scrollTo(0, 0);
    }

    function focusPageStart() {
        // Respect in-page anchors (#section links) - the browser target wins.
        if (hasRealHashTarget()) return;

        // Run once as soon as the DOM is usable, then again after 'load' and on
        // the next frame. This file is included before navmenu.js and before any
        // page-level scripts, so a single early pass can be overridden by nav
        // code that restores/highlights the active menu item and focuses it.
        applyFocus();
        window.requestAnimationFrame(function () { applyFocus(); });
        window.setTimeout(applyFocus, 0);
        window.setTimeout(applyFocus, 150);
    }

    // A side-nav item that only expands/collapses its own sub-list is NOT a
    // navigation: the user stays on the same page, so focus must remain on the
    // menu option they just operated (otherwise it jumps to the app logo and
    // they lose their place). Detected via the ARIA disclosure contract
    // (aria-expanded / aria-controls) or a nested sub-list.
    function isSubmenuDisclosure(link) {
        if (!link) return false;
        if (link.hasAttribute('aria-expanded') || link.hasAttribute('aria-controls')) return true;
        var parent = link.parentElement;
        return !!(parent && parent.querySelector(':scope > ul, :scope > .sidenav-children, :scope > .dropdown-menu, :scope > .sub-dropdown-menu'));
    }

    // Some screens (e.g. FPS Master Lookup) use side-nav links with href="#"
    // that swap the content pane over AJAX instead of navigating. No page load
    // fires, so focus would otherwise stay on the clicked link. Treat those the
    // same as a navigation and send focus back to the logo once the new content
    // has been injected.
    document.addEventListener('click', function (e) {
        var link = e.target && e.target.closest
            ? e.target.closest('a[href="#"], a[href=""], a:not([href])')
            : null;
        if (!link) return;
        if (!link.closest('.sidenav, #shortnav, .side-nav, .project-side-nav, .main-nav')) return;
        if (isSubmenuDisclosure(link)) {
            // Expanding a sub-list keeps the user on the same page, so re-assert
            // focus on the option itself in case a later pass tries to move it.
            window.setTimeout(function () {
                try {
                    link.focus({ preventScroll: true });
                } catch (err) {
                    link.focus();
                }
            }, 0);
            return;
        }

        // Let the page's own handler run and render first.
        window.setTimeout(applyFocus, 0);
        window.setTimeout(applyFocus, 250);
    }, true);

    // Expose so page scripts can re-assert top-of-page focus after their own
    // AJAX content swaps if the timings above aren't sufficient.
    window.focusAppLogo = applyFocus;

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', focusPageStart);
    } else {
        focusPageStart();
    }

    // Late-running scripts and slow partials can still shift focus after
    // DOMContentLoaded, so take a final pass once everything has loaded.
    window.addEventListener('load', function () {
        if (hasRealHashTarget()) return;
        window.setTimeout(applyFocus, 0);
    });

    // Back/forward navigation restored from the bfcache re-runs no scripts,
    // so hook pageshow as well.
    window.addEventListener('pageshow', function (e) {
        if (e.persisted) focusPageStart();
    });
})();

// ──────────────────────────────────────────────────────────────────────────
// Modal header announcement
// The shared #modalPopup container in every area layout is a plain <div> with
// a static aria-label ("Dialog" / "Modal dialog"). Partials injected into
// #modaPopupBody supply their own ".modal-header > h3" title, but nothing
// gives that heading focus or ties it to the dialog, so NVDA announces only
// the generic container label and the modal's real title is never read.
//
// This module watches for a modal becoming visible and then:
//   1) promotes the container to a proper role="dialog" + aria-modal
//   2) points aria-labelledby at the injected heading, so the real title
//      becomes the dialog's accessible name
//   3) moves focus to the heading, so NVDA starts reading from the title
//      rather than from the first input or the close button
// Applied globally; no markup changes needed in the many modal partials.
// ──────────────────────────────────────────────────────────────────────────
(function () {
    'use strict';

    var HEADER_SELECTOR = '.modal-header, .govuk-edit-modal__header';
    var TITLE_SELECTOR = 'h1, h2, h3, h4, .modal-title, .govuk-heading-s, .govuk-heading-m';
    var titleSeq = 0;

    function isShown(el) {
        if (!el) return false;
        if (el.classList.contains('show')) return true;
        var style = window.getComputedStyle(el);
        return style.display !== 'none' && style.visibility !== 'hidden';
    }

    function findHeading(modal) {
        var header = modal.querySelector(HEADER_SELECTOR);
        if (header) {
            var inHeader = header.querySelector(TITLE_SELECTOR);
            if (inHeader && inHeader.textContent.trim()) return inHeader;
            // Header with no heading element but visible text of its own.
            if (header.textContent.trim()) return header;
        }
        var anyTitle = modal.querySelector(TITLE_SELECTOR);
        return (anyTitle && anyTitle.textContent.trim()) ? anyTitle : null;
    }

    function announceModal(modal) {
        if (!modal || !isShown(modal)) return;

        var heading = findHeading(modal);
        if (!heading) return;

        // Skip if we've already wired up this exact heading for this modal.
        if (modal.getAttribute('data-modal-titled') === heading.id && heading.id) return;

        if (!heading.id) {
            heading.id = 'modalTitle_' + (++titleSeq);
        }

        // Give the container real dialog semantics so the name is announced
        // as a dialog title rather than stray text.
        if (modal.getAttribute('role') !== 'dialog') {
            modal.setAttribute('role', 'dialog');
        }
        modal.setAttribute('aria-modal', 'true');

        // aria-labelledby must win over the layout's generic aria-label.
        modal.removeAttribute('aria-label');
        modal.setAttribute('aria-labelledby', heading.id);

        modal.setAttribute('data-modal-titled', heading.id);

        // Never let the whole dialog body become the accessible description -
        // that is what makes NVDA read the entire form the moment it opens.
        modal.removeAttribute('aria-describedby');

        // Make the heading programmatically focusable (but keep it out of the
        // Tab sequence) so NVDA reads ONLY the title, and the very next Tab
        // lands on the close (cross) button that follows it in the header.
        if (!heading.hasAttribute('tabindex')) {
            heading.setAttribute('tabindex', '-1');
        }


        focusHeading(modal, heading);
    }

    // Other scripts (modal focus traps, autofocus attributes, page-level
    // "focus the first input" code) run at unpredictable times just after a
    // modal opens and will happily steal focus to an input or the close
    // button - which makes NVDA read that control instead of the title.
    // Re-assert focus on the heading over a short window, but only while
    // focus is still somewhere the user did not deliberately put it.
    function focusHeading(modal, heading) {
        var attempts = 0;

        function settle() {
            if (!isShown(modal)) return;

            var active = document.activeElement;
            // The user has started interacting (typed/tabbed somewhere) -
            // stop fighting them.
            if (active && active !== document.body &&
                active !== document.documentElement &&
                active !== heading &&
                active !== modal &&
                modal.contains(active) &&
                modal.getAttribute('data-modal-settled') === '1') {
                return;
            }

            if (active !== heading) {
                try {
                    heading.focus({ preventScroll: true });
                } catch (e) {
                    heading.focus();
                }
            }

            if (++attempts < 4) {
                window.setTimeout(settle, 60);
            } else {
                // Hand control back to the user / focus trap.
                modal.setAttribute('data-modal-settled', '1');
            }
        }

        modal.removeAttribute('data-modal-settled');
        window.setTimeout(settle, 0);
    }

    function scanForOpenModals() {
        var modals = document.querySelectorAll(
            '#modalPopup, .modal.show, [role="dialog"], [data-govuk-modal="backdrop"]');
        Array.prototype.forEach.call(modals, function (m) {
            if (isShown(m)) {
                announceModal(m);
            } else if (m.hasAttribute('data-modal-titled')) {
                // Closed again - clear the markers so the next open re-runs
                // the announcement (the injected content/title changes).
                m.removeAttribute('data-modal-titled');
                m.removeAttribute('data-modal-settled');
            }
        });
    }

    function init() {
        // Modals are shown by toggling a class and are filled over AJAX, so
        // watch both attribute flips and injected content.
        var observer = new MutationObserver(function (mutations) {
            var recheck = false;
            mutations.forEach(function (m) {
                if (m.type === 'attributes' || (m.addedNodes && m.addedNodes.length)) {
                    recheck = true;
                }
            });
            if (recheck) scanForOpenModals();
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: ['class', 'style']
        });

        scanForOpenModals();
    }

    // Allow page scripts to re-assert the title after replacing modal content.
    window.announceModalHeader = scanForOpenModals;

    if (document.body) {
        init();
    } else {
        document.addEventListener('DOMContentLoaded', init);
    }
})();
