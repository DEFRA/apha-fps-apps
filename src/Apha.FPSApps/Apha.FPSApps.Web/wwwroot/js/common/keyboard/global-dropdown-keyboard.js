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
        // Some areas (e.g. PIMS) use a lower-level heading (h5) as the visual
        // page title instead of h1. Look for the first heading of any level
        // within the main content region so those pages are covered too,
        // falling back to any h1 on the page.
        var heading = document.querySelector('main h1, main h2, main h3, main h4, main h5, main h6, .content-wrapper h1, .content-wrapper h2, .content-wrapper h3, .content-wrapper h4, .content-wrapper h5, .content-wrapper h6, h1');
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
    // The shared AppFooter renders as <div class="govuk-footer"> with an inner
    // wrapper div carrying role="contentinfo". Landmark navigation can find
    // that role, but nothing inside the footer is a tab stop, so users tabbing
    // through the page never land on it. Also, a focused landmark with no
    // accessible name is only announced as "content information" - its
    // descendant text is not read - so we set aria-label to the visible
    // copyright text as well.
    // Additive: markup in Views/Shared/Components/AppFooter stays unchanged.
    function makeFooterReadable() {
        var footer = document.querySelector('.govuk-footer');
        if (!footer || !isVisible(footer)) return;

        var landmark = footer.tagName === 'FOOTER' || footer.getAttribute('role') === 'contentinfo'
            ? footer
            : footer.querySelector('[role="contentinfo"]');

        if (!landmark) {
            footer.setAttribute('role', 'contentinfo');
            landmark = footer;
        }

        if (!landmark.hasAttribute('tabindex')) {
            landmark.setAttribute('tabindex', '0');
        }

        if (!landmark.hasAttribute('aria-label') && !landmark.hasAttribute('aria-labelledby')) {
            var text = (landmark.textContent || '').replace(/\s+/g, ' ').trim();
            if (text) landmark.setAttribute('aria-label', text);
        }
    }

    // ── Announce message/confirm dialog content on open ───────────────────
    // The shared dialog builder (govuk-modal-dialog.js) moves focus straight
    // to the OK button when a message popup opens (e.g. after a successful
    // save/delete/edit), so screen readers announce only "OK, button" and the
    // actual message is never read out.
    //
    // Critically, the dialog carries aria-modal="true", which makes assistive
    // technology ignore ALL content outside the dialog - including any global
    // live region appended to document.body. The announcement must therefore
    // be injected INSIDE the dialog element itself to be spoken.
    //
    // Implemented here as a new, additive function so the common dialog file
    // is left untouched. Matching on the ARIA role means every dialog of this
    // shape is covered across all areas.
    // The heading is focused on open so the modal title is read out as soon as
    // the dialog appears, while the message text is made focusable (but never
    // auto-focused) so it is announced only when focus is actually on it.

    function announceDialogContent(dialog) {
        if (!dialog || dialog.getAttribute('data-sr-announced') === 'true') return;
        if (!isVisible(dialog)) return;
        dialog.setAttribute('data-sr-announced', 'true');

        var titleId = dialog.getAttribute('aria-labelledby');
        var titleEl = titleId
            ? document.getElementById(titleId)
            : dialog.querySelector('h1, h2, h3, h4, h5, h6');

        var descId = dialog.getAttribute('aria-describedby');
        var descEl = descId ? document.getElementById(descId) : dialog.querySelector('p');

        // Make the message text itself focusable and place it in the dialog's
        // Tab order, but never move focus to it programmatically. The message
        // is announced only when focus is actually on the message - i.e. when
        // the user tabs onto it. Auto-focusing it instead would announce the
        // message unprompted and stack a second announcement on top of the
        // screen reader's own dialog-open announcement, which is what caused
        // the title/message to be read repeatedly.
        if (descEl && (descEl.textContent || '').trim() && !descEl.hasAttribute('tabindex')) {
            descEl.setAttribute('tabindex', '0');
        }

        // Read the modal title on open by moving focus to the heading. It is
        // kept out of the Tab sequence (tabindex="-1") so the very next Tab
        // continues on to the message/buttons rather than returning here.
        if (!titleEl || !(titleEl.textContent || '').trim()) return;

        if (!titleEl.hasAttribute('tabindex')) {
            titleEl.setAttribute('tabindex', '-1');
        }

        // Deferred so the dialog is fully in the accessibility tree and the
        // dialog builder's own focus() call has already run, otherwise it
        // would immediately move focus away again.
        window.setTimeout(function () {
            if (!isVisible(dialog)) return;
            titleEl.focus();
        }, 100);
    }

    function watchForDialogs() {
        // Catch any dialog already present when this script initialises.
        Array.prototype.forEach.call(
            document.querySelectorAll('[role="dialog"], [role="alertdialog"]'),
            announceDialogContent
        );

        var observer = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                Array.prototype.forEach.call(mutation.addedNodes, function (node) {
                    if (!node || node.nodeType !== 1) return;
                    if (node.matches && node.matches('[role="dialog"], [role="alertdialog"]')) {
                        announceDialogContent(node);
                    }
                    if (node.querySelectorAll) {
                        Array.prototype.forEach.call(
                            node.querySelectorAll('[role="dialog"], [role="alertdialog"]'),
                            announceDialogContent
                        );
                    }
                });
            });
        });

        // Observe from the document element so this works even when the
        // script runs in <head> before <body> exists. Observation starts
        // immediately (not on DOMContentLoaded) because pages can open a
        // dialog inside jQuery's ready handler, which fires on that same
        // event and could otherwise be missed.
        observer.observe(document.documentElement, { childList: true, subtree: true });
    }

    watchForDialogs();

    // ── Make static data tables keyboard reachable and announced ──────────
    // Plain <table> markup is not in the tab order, so keyboard/screen-reader
    // users tab straight past read-only tables (e.g. the FPS Yearly Details
    // tab) and never hear their contents. Scrollable wrappers around such
    // tables are also unreachable by keyboard, which additionally fails
    // WCAG 2.1.1.
    //
    // This adds the required semantics at runtime (focusable scroll region,
    // accessible name, and column/row header scopes) so no existing view
    // markup has to change. Only tables that are not already handled - i.e.
    // those outside the shared editable DataGrid, which manages its own
    // keyboard behaviour - are processed.
    function makeStaticTablesAccessible(root) {
        var scope = root && root.querySelectorAll ? root : document;

        Array.prototype.forEach.call(scope.querySelectorAll('table'), function (table) {
            if (table.getAttribute('data-a11y-table') === 'true') return;
            // Skip grids handled by the shared DataGrid component.
            if (table.closest('.editable-grid-container')) return;
            table.setAttribute('data-a11y-table', 'true');

            // Give every header cell an explicit scope so screen readers can
            // associate data cells with their column heading.
            Array.prototype.forEach.call(table.querySelectorAll('thead th'), function (th) {
                if (!th.hasAttribute('scope')) th.setAttribute('scope', 'col');
            });

            // Derive an accessible name for the table from the nearest
            // preceding heading, or the tab that labels its panel.
            var name = '';
            var caption = table.querySelector('caption');
            if (caption) {
                name = (caption.textContent || '').trim();
            }
            if (!name) {
                var panel = table.closest('[role="tabpanel"]');
                var labelledBy = panel && panel.getAttribute('aria-labelledby');
                var labelEl = labelledBy ? document.getElementById(labelledBy) : null;
                if (labelEl) name = (labelEl.textContent || '').trim();
            }
            if (name && !table.hasAttribute('aria-label')) {
                table.setAttribute('aria-label', name);
            }

            // Make the scrollable wrapper (or the table itself) focusable so
            // the content can be reached and scrolled using the keyboard.
            var target = table.parentElement;
            var scrollable = false;
            if (target) {
                var style = window.getComputedStyle(target);
                scrollable = style.overflowY === 'auto' || style.overflowY === 'scroll' ||
                    style.overflowX === 'auto' || style.overflowX === 'scroll';
            }
            if (!scrollable) target = table;

            if (!target.hasAttribute('tabindex')) {
                target.setAttribute('tabindex', '0');
            }
            if (scrollable && !target.hasAttribute('role')) {
                target.setAttribute('role', 'region');
                if (name) target.setAttribute('aria-label', name);
            }

            makeTableCellsFocusable(table);
        });
    }

    // Make every data cell individually focusable and give it an accessible
    // name built from its column header paired with the cell's own value
    // (e.g. "Program: Surveillance"), so tabbing through the table announces
    // one column value at a time rather than the whole row at once. Per-cell
    // focus is used (rather than row-level) so a screen reader user hears
    // each field separately, matching the pattern already used by the
    // Invoice totals row (data-total-label).
    function makeTableCellsFocusable(table) {
        var headers = Array.prototype.map.call(
            table.querySelectorAll('thead th'),
            function (th) { return (th.textContent || '').trim(); }
        );

        var bodyRows = table.querySelectorAll('tbody tr');
        Array.prototype.forEach.call(bodyRows, function (row) {
            if (row.getAttribute('data-a11y-row') === 'true') return;
            row.setAttribute('data-a11y-row', 'true');

            // Skip rows that already opt into per-cell accessibility on their
            // own (e.g. the Invoice totals row, where each value cell already
            // carries its own "label: value" aria-label via data-total-label).
            if (row.querySelector('[data-total-label]')) return;

            var cells = row.querySelectorAll('th, td');
            if (!cells.length) return;

            // A single full-width cell is a placeholder such as
            // "No records found." - announce it as-is.
            if (cells.length === 1) {
                var onlyCell = cells[0];
                var onlyValue = (onlyCell.textContent || '').trim();
                if (!onlyValue) return;
                if (!onlyCell.hasAttribute('tabindex')) onlyCell.setAttribute('tabindex', '0');
                if (!onlyCell.hasAttribute('aria-label')) onlyCell.setAttribute('aria-label', onlyValue);
                return;
            }

            Array.prototype.forEach.call(cells, function (cell, index) {
                var value = (cell.textContent || '').trim();
                if (!value) return;
                var header = headers[index] || '';
                var label = header ? header + ': ' + value : value;

                if (!cell.hasAttribute('tabindex')) cell.setAttribute('tabindex', '0');
                if (!cell.hasAttribute('aria-label')) cell.setAttribute('aria-label', label);
            });
        });
    }

    // Re-apply to tables rendered later (AJAX grid refreshes, tab switches).
    function watchForStaticTables() {
        makeStaticTablesAccessible(document);

        var observer = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                Array.prototype.forEach.call(mutation.addedNodes, function (node) {
                    if (!node || node.nodeType !== 1) return;
                    if (node.tagName === 'TABLE' || (node.querySelector && node.querySelector('table'))) {
                        makeStaticTablesAccessible(node.parentElement || document);
                    }
                });
            });
        });

        observer.observe(document.documentElement, { childList: true, subtree: true });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', restoreFocusAfterRefresh);
        document.addEventListener('DOMContentLoaded', makePageHeadingFocusable);
        document.addEventListener('DOMContentLoaded', makeFooterReadable);
        document.addEventListener('DOMContentLoaded', watchForStaticTables);
    } else {
        restoreFocusAfterRefresh();
        makePageHeadingFocusable();
        makeFooterReadable();
        watchForStaticTables();
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

    var emptyDescriptionCounter = 0;

    // ── Stop the whole modal body being read out on open ──────────────────
    // These dialogs are marked up as role="dialog"/<dialog> with
    // aria-modal="true" and an aria-labelledby title. When such a dialog
    // becomes visible and focus moves into it, screen readers announce the
    // dialog's name AND read out its entire static contents - which is why
    // opening an Add/Edit modal reads every field label and value in one
    // burst, and then reads each field again as it is tabbed onto.
    //
    // Setting aria-describedby to an empty node is not enough on its own:
    // reading the contents on entry is the dialog's built-in behaviour, not a
    // description fallback. The reliable way to stop it - without changing any
    // markup or UI - is to take the modal BODY out of the accessibility tree
    // for the brief moment the dialog is being announced, then put it straight
    // back. The screen reader therefore finds only the title to announce on
    // open, and every field is still fully exposed (and announced normally)
    // by the time the user tabs to it.
    function prepareModalAnnouncement(modal) {
        if (!modal.hasAttribute('aria-describedby')) {
            var descId = 'js-modal-empty-desc-' + (++emptyDescriptionCounter);
            var desc = document.createElement('span');
            desc.id = descId;
            desc.className = 'govuk-visually-hidden';
            modal.appendChild(desc);
            modal.setAttribute('aria-describedby', descId);
        }
    }

    // Hides the modal's content region from assistive technology while the
    // open announcement happens, restoring it as soon as focus has settled on
    // the title. aria-hidden has no visual effect, so the UI is untouched.
    function muteModalBodyDuringOpen(modal) {
        var body = modal.querySelector('.govuk-edit-modal__body, .modal-body');
        if (!body || body.getAttribute('aria-hidden') === 'true') return;

        body.setAttribute('aria-hidden', 'true');

        // Restore after the announcement has been queued. Focus never lands
        // inside the body during this window (it goes to the title), so no
        // focused element is ever left inside an aria-hidden subtree.
        window.setTimeout(function () {
            body.removeAttribute('aria-hidden');
        }, 600);
    }

    function attachModal(modal, openClass, focusScopeSelector, toggleDisplay) {
        if (!modal || trackedModals.has(modal)) return;
        trackedModals.add(modal);

        prepareModalAnnouncement(modal);

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

        // When a modal opens, focus should land on its TITLE heading so the
        // screen reader announces only the dialog name (e.g. "Edit Yearly
        // Record, heading"). Focus used to go straight to the close ("X")
        // button; a bare button carries no text content of its own, so the
        // screen reader read the dialog's whole subtree as surrounding
        // context - i.e. every field label and value. Landing on the heading
        // (the WAI-ARIA dialog pattern's recommended target) gives a short,
        // predictable announcement instead.
        //
        // The heading is given tabindex="-1" so it is focusable
        // programmatically only and never joins the Tab sequence: the very
        // first Tab still moves to the close button exactly as before, so the
        // keyboard order and the UI are unchanged.
        function getModalTitle() {
            return modal.querySelector(
                '.govuk-edit-modal__title, .modal-title, ' +
                '.govuk-edit-modal__header h1, .govuk-edit-modal__header h2, .govuk-edit-modal__header h3, ' +
                '.modal-header h1, .modal-header h2, .modal-header h3'
            );
        }

        function getInitialFocusTarget(focusable) {
            var title = getModalTitle();
            if (title && title.offsetParent !== null) {
                if (!title.hasAttribute('tabindex')) {
                    title.setAttribute('tabindex', '-1');
                }
                return title;
            }

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
                muteModalBodyDuringOpen(modal);

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
            muteModalBodyDuringOpen(modal);

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
    // When the grid has a group header row above the column header row (e.g.
    // "Total Planned Time" spanning "PlanHrs, FEC, % Planned"), each column
    // name is prefixed with its group label so NVDA announces both levels,
    // e.g. "Total Planned Time PlanHrs, 1200.00" instead of just "PlanHrs,
    // 1200.00" with no indication of which group the column belongs to.
    function getColumnNames(table) {
        var headerRow = null;
        var groupRow = null;
        var headRows = table.querySelectorAll('thead tr');
        // Use the last header row that isn't the filter row - that's the one
        // holding the real column labels. The group row (if present) is the
        // one immediately above it, carrying wider "section" headings.
        for (var i = 0; i < headRows.length; i++) {
            if (headRows[i].classList.contains('grid-column-group-row')) {
                groupRow = headRows[i];
            } else if (!headRows[i].classList.contains('filter-row')) {
                headerRow = headRows[i];
            }
        }
        if (!headerRow) return [];

        var columnNames = Array.prototype.map.call(headerRow.children, function (th) {
            // Strip the sort-indicator glyph so it isn't announced.
            var clone = th.cloneNode(true);
            Array.prototype.forEach.call(clone.querySelectorAll('.sort-icon, .column-resizer'), function (n) {
                n.parentNode.removeChild(n);
            });
            return (clone.textContent || '').replace(/\s+/g, ' ').trim();
        });

        if (!groupRow) return columnNames;

        // Expand the group row (which uses colspan) into one group label per
        // column index, then prefix each column name with its group label.
        var groupNames = [];
        Array.prototype.forEach.call(groupRow.children, function (th) {
            var span = th.colSpan || 1;
            var label = (th.textContent || '').replace(/\s+/g, ' ').trim();
            for (var s = 0; s < span; s++) groupNames.push(label);
        });

        return columnNames.map(function (name, idx) {
            var group = groupNames[idx];
            if (!group) return name;
            return name ? group + ' ' + name : group;
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

// ── Global "announce disabled fields" support (NVDA / screen-reader friendly) ──
// Native disabled="disabled"/disabled inputs, selects and textareas are
// removed from the accessibility tree entirely - the browser strips them out
// regardless of any aria-* attributes placed on them, so NVDA (and every
// other screen reader) skips over them in both Tab order and browse-mode
// review. Sighted users still see the greyed-out field and its value, but
// screen reader users get no indication the field - or its value - exists
// at all.
//
// Fix: for every native disabled control, swap the semantics for
// aria-disabled="true" (announces "unavailable"/"dimmed" instead of hiding
// it) plus a mechanism that keeps the field inert:
//   * input/textarea: add readonly (keeps the value focusable/readable,
//     blocks typing) and drop the disabled attribute.
//   * select: cannot be made readonly, so the disabled attribute is kept
//     off the element itself; instead a tabindex="-1" avoids it being a
//     natural Tab stop while pointer-events/focus handling below re-block
//     interaction and keep it announced as "unavailable".
// The visual look is unaffected: existing CSS rules already style
// [readonly]/[disabled] the same way (see e.g. .fps-dg-summary-value,
// .govuk-input[readonly]), so removing the disabled attribute causes no
// visible change. Applied globally via this shared script - no per-page
// markup changes needed.
// Purely additive/behavioural, safe to re-run on every mutation.
(function () {
    'use strict';

    function isVisible(el) {
        if (!el) return false;
        var style = window.getComputedStyle(el);
        return style.display !== 'none' && style.visibility !== 'hidden';
    }

    // Keys that must always be allowed through even on an inert field, so
    // Tab/Shift+Tab moves focus to the next/previous control and Arrow/
    // Home/End/PageUp/PageDown/Escape keep working for keyboard and screen
    // reader navigation (browse-mode review, virtual cursor, etc.).
    var NAVIGATION_KEYS = [
        'Tab', 'ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight',
        'Home', 'End', 'PageUp', 'PageDown', 'Escape'
    ];

    function textOf(el) {
        return el ? (el.textContent || '').replace(/\s+/g, ' ').trim() : '';
    }

    function findLabelText(field) {
        var label = null;
        if (field.id) label = document.querySelector('label[for="' + CSS.escape(field.id) + '"]');
        if (!label) label = field.closest('label');
        if (label) return textOf(label);

        var ariaLabel = field.getAttribute('data-kbd-original-aria-label') || field.getAttribute('aria-label');
        if (ariaLabel) return ariaLabel;
        var labelledBy = field.getAttribute('aria-labelledby');
        if (labelledBy) {
            var byId = document.getElementById(labelledBy);
            if (byId) return textOf(byId);
        }
        return '';
    }

    function valueOf(field) {
        if (field.tagName === 'SELECT') {
            var selected = field.options[field.selectedIndex];
            return selected ? textOf(selected) : '';
        }
        return (field.value || '').replace(/\s+/g, ' ').trim();
    }

    // Build (or refresh) the accessible name announced for the disabled
    // field as "<label>: <value>". Screen readers announce a control's
    // aria-label instead of walking its label/value separately, so this is
    // the most reliable way to have NVDA read both together for a field
    // that remains reachable (readonly input/textarea, or a tabindex="-1"
    // select reached via browse-mode/virtual cursor review).
    function announceDisabledField(field) {
        // Preserve whatever aria-label was originally authored (if any) so
        // it can still contribute to the label text above, then overwrite
        // it with the combined "label: value" accessible name.
        if (!field.hasAttribute('data-kbd-original-aria-label') && field.hasAttribute('aria-label')) {
            field.setAttribute('data-kbd-original-aria-label', field.getAttribute('aria-label'));
        }

        var labelText = findLabelText(field);
        var valueText = valueOf(field);
        var name = labelText && valueText ? labelText + ': ' + valueText
                                          : (labelText || valueText);
        if (!name) return;

        field.setAttribute('aria-label', name);
    }

    // Re-block interaction natively blocked by the (removed) disabled
    // attribute: typing/editing is suppressed so the field still behaves as
    // inert, while remaining in the accessibility tree and Tab order for
    // screen reader users. Navigation keys are explicitly excluded so focus
    // can still move off the field via Tab/Shift+Tab/Arrow keys.
    function blockInteraction(el) {
        if (el.hasAttribute('data-kbd-disabled-guarded')) return;
        el.setAttribute('data-kbd-disabled-guarded', 'true');

        el.addEventListener('keydown', function (e) {
            if (el.getAttribute('aria-disabled') !== 'true') return;
            if (NAVIGATION_KEYS.indexOf(e.key) !== -1) return;
            if (e.ctrlKey || e.metaKey || e.altKey) return; // allow shortcuts (copy, browser nav, etc.)
            e.preventDefault();
        });
        ['keypress', 'paste', 'cut', 'drop'].forEach(function (evt) {
            el.addEventListener(evt, function (e) {
                if (el.getAttribute('aria-disabled') === 'true') e.preventDefault();
            });
        });
    }

    function convertDisabledField(el) {
        if (!el.disabled) return;

        el.removeAttribute('disabled');
        el.setAttribute('aria-disabled', 'true');

        if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {
            el.setAttribute('readonly', 'readonly');
        } else if (el.tagName === 'SELECT') {
            // A native <select> has no readonly equivalent; keep it out of the
            // natural Tab order but still reachable via arrow-key/virtual
            // cursor review so its current value is announced.
            if (!el.hasAttribute('tabindex')) el.setAttribute('tabindex', '-1');
            blockInteraction(el);
            el.addEventListener('mousedown', function (e) {
                if (el.getAttribute('aria-disabled') === 'true') e.preventDefault();
            });
        }

        if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {
            blockInteraction(el);
        }

        announceDisabledField(el);
    }

    function processAllDisabledFields(root) {
        var scope = root && root.querySelectorAll ? root : document;
        Array.prototype.forEach.call(
            scope.querySelectorAll('input:disabled, select:disabled, textarea:disabled'),
            function (el) {
                if (!isVisible(el)) return;
                convertDisabledField(el);
            }
        );
    }

    function init() {
        processAllDisabledFields(document);

        // Disabled state is frequently toggled at runtime (form validation,
        // conditional fields, grid row edit mode), so keep re-checking for
        // newly disabled controls.
        var observer = new MutationObserver(function (mutations) {
            var needsCheck = false;
            mutations.forEach(function (m) {
                if (m.type === 'attributes' && m.attributeName === 'disabled') {
                    needsCheck = true;
                } else if (m.type === 'childList' && m.addedNodes.length) {
                    needsCheck = true;
                }
            });
            if (needsCheck) processAllDisabledFields(document);
        });
        observer.observe(document.body, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: ['disabled']
        });

        // A field's value can change programmatically after it has already
        // been converted (e.g. AJAX refresh sets el.value = ...), so keep
        // the announced "label: value" text in sync.
        document.addEventListener('input', function (e) {
            if (e.target && e.target.getAttribute && e.target.getAttribute('aria-disabled') === 'true') {
                announceDisabledField(e.target);
            }
        }, true);
        document.addEventListener('change', function (e) {
            if (e.target && e.target.getAttribute && e.target.getAttribute('aria-disabled') === 'true') {
                announceDisabledField(e.target);
            }
        }, true);
    }

    if (document.body) {
        init();
    } else {
        document.addEventListener('DOMContentLoaded', init);
    }
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

    var MIRROR_ID_PREFIX = 'kbd-textarea-full-value-';

    function removeMirror(textarea) {
        var mirror = textarea.nextElementSibling;
        if (mirror && mirror.hasAttribute && mirror.hasAttribute('data-kbd-textarea-mirror')) {
            mirror.parentNode.removeChild(mirror);
        }
        var describedBy = textarea.getAttribute('aria-describedby');
        if (describedBy && describedBy.indexOf(MIRROR_ID_PREFIX) !== -1) {
            var remaining = describedBy.split(/\s+/).filter(function (id) {
                return id.indexOf(MIRROR_ID_PREFIX) !== 0;
            }).join(' ');
            if (remaining) {
                textarea.setAttribute('aria-describedby', remaining);
            } else {
                textarea.removeAttribute('aria-describedby');
            }
        }
    }

    // Editable multi-line textareas (e.g. "Report Help"/"Comments" fields) must
    // keep their native textbox semantics - typing, caret position, and
    // in-place value announcement all need to work as usual - so they
    // cannot use the read-only role="img" + aria-label swap above.
    // Editable textareas keep their native textbox semantics. The complete
    // value is now spoken once through the live region in announceFullValue()
    // below, so the older aria-describedby mirror is no longer attached - it
    // caused the text to be announced twice (partial value + description).
    // This function now only strips any mirror left over from a previous run
    // or from cached markup.
    function syncEditableMirror(textarea) {
        removeMirror(textarea);
    }

    // Some screen readers ignore (or are configured not to speak) description
    // text, and a multi-line textbox value is announced one line at a time, so
    // the aria-describedby mirror alone is not always enough. On focus we also
    // push the complete value into a polite live region so it is spoken in
    // full exactly once. The region is injected INSIDE the nearest
    // aria-modal="true" dialog when the field sits in a modal, because
    // assistive technology ignores everything outside such a dialog.
    var LIVE_REGION_ATTR = 'data-kbd-textarea-live';

    function liveRegionFor(textarea) {
        var host = textarea.closest('[role="dialog"], [role="alertdialog"]') || document.body;
        var region = host.querySelector(':scope > [' + LIVE_REGION_ATTR + ']');
        if (!region) {
            region = document.createElement('div');
            region.setAttribute(LIVE_REGION_ATTR, 'true');
            region.setAttribute('aria-live', 'polite');
            region.setAttribute('aria-atomic', 'true');
            region.className = 'govuk-visually-hidden';
            host.appendChild(region);
        }
        return region;
    }

    function announceFullValue(textarea) {
        if (textarea.readOnly || textarea.disabled) return;
        var raw = textarea.value || '';
        if (!raw.trim()) return;

        var value = raw.replace(/\s+/g, ' ').trim();
        // Only worth announcing when the native announcement would be partial:
        // the value wraps/overflows the visible rows or contains line breaks.
        var overflowing = textarea.scrollHeight > textarea.clientHeight + 1;
        if (!overflowing && !/\r|\n/.test(raw)) return;

        // Never repeat the same text for the same field while focus stays put.
        if (textarea.getAttribute('data-kbd-announced-value') === value) return;
        textarea.setAttribute('data-kbd-announced-value', value);

        var region = liveRegionFor(textarea);
        var labelText = findLabelText(textarea) ||
            textarea.getAttribute('aria-label') || '';
        var message = labelText ? labelText.replace(/[:*\s]+$/, '') + ': ' + value : value;

        region.textContent = '';
        window.setTimeout(function () {
            region.textContent = message;
        }, 120);
    }

    // Browsers place the caret at position 0 when a textarea is focused
    // (programmatically, e.g. modal open, or via Tab), with no selection.
    // With a multi-line value taller than the visible rows, NVDA/JAWS only
    // read the single line the caret sits on/scrolled into view - so only
    // the first line was ever announced, matching the reported bug.
    //
    // Selecting the ENTIRE value on focus (rather than just moving the
    // caret) is what makes the difference: when a field's whole content is
    // selected, the screen reader announces the complete selected text as
    // one block ("selected <full value>"), regardless of how many lines or
    // rows are visible. This mirrors the native browser behaviour already
    // seen on the "Description" field in this same modal (its text shows
    // fully highlighted on focus and is read out completely) - textareas do
    // not get that treatment natively, so we apply it explicitly here.
    function moveCaretToEnd(textarea) {
        try {
            textarea.select();
        } catch (ignored) { /* some input types don't support selection */ }
        textarea.scrollTop = textarea.scrollHeight;
    }

    function processAllTextareas(root) {
        var scope = root && root.querySelectorAll ? root : document;
        Array.prototype.forEach.call(scope.querySelectorAll('textarea'), function (textarea) {
            if (!isVisible(textarea)) return;
            syncAriaLabel(textarea);
            syncEditableMirror(textarea);
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
                        if (node.hasAttribute && node.hasAttribute('data-kbd-textarea-mirror')) return;
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

        // Overflow detection depends on layout (scrollHeight/clientHeight),
        // which can change on viewport resize (responsive column widths) even
        // without any value change, so re-check then too.
        window.addEventListener('resize', function () {
            processAllTextareas(document);
        });

        // Textarea values changed via script (e.g. el.value = '...') don't
        // fire childList/attribute mutations, so also catch the standard
        // input/change events.
        document.addEventListener('input', function (e) {
            if (e.target && e.target.tagName === 'TEXTAREA') {
                syncAriaLabel(e.target);
                syncEditableMirror(e.target);
            }
        }, true);
        document.addEventListener('change', function (e) {
            if (e.target && e.target.tagName === 'TEXTAREA') {
                syncAriaLabel(e.target);
                syncEditableMirror(e.target);
            }
        }, true);

        // Textareas inside modals/partials are frequently hidden (or not yet
        // laid out) when first processed, so re-sync as focus arrives on the
        // field - this is the point at which the announcement is composed -
        // and speak the complete value once via the live region.
        //
        // NVDA (and other screen readers) read a multi-line textbox starting
        // from wherever the caret sits, not the whole value. Browsers place
        // the caret at position 0 by default when a textarea receives focus,
        // so only the first line was ever announced natively even before any
        // scripted handling here. Moving the caret to the END of the value on
        // focus means the native "read current line/reading cursor" behaviour
        // starts from the last line - combined with the live-region
        // announcement above (which always speaks the full value up front),
        // this ensures every line is heard.
        document.addEventListener('focusin', function (e) {
            if (e.target && e.target.tagName === 'TEXTAREA') {
                syncAriaLabel(e.target);
                syncEditableMirror(e.target);
                announceFullValue(e.target);
                moveCaretToEnd(e.target);
            }
        }, true);

        // Allow the value to be announced again the next time the user
        // returns to the field (or after they edit it).
        document.addEventListener('focusout', function (e) {
            if (e.target && e.target.tagName === 'TEXTAREA') {
                e.target.removeAttribute('data-kbd-announced-value');
                var region = e.target.closest('[role="dialog"], [role="alertdialog"]') || document.body;
                var live = region.querySelector(':scope > [' + LIVE_REGION_ATTR + ']');
                if (live) live.textContent = '';
            }
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
                    try {
                        syncAriaLabel(this);
                        syncEditableMirror(this);
                    } catch (ignored) { /* never break assignment */ }
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

        // The shared GOV.UK message/confirm dialog (govuk-modal-dialog.js)
        // manages its own role/aria-labelledby/aria-describedby/focus and is
        // handled separately below - do not let the generic modal-header
        // logic re-point its accessible name or steal focus from it.
        if (modal.hasAttribute('data-govuk-modal') || modal.closest('[data-govuk-modal="backdrop"]')) {
            return;
        }

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

// ── Ensure alert (success / error / information) dialogs open AFTER the shared
//    add/edit modal has been closed ───────────────────────────────────────────
// Across the apps, add/edit forms are shown in the shared "#modalPopup" flyout,
// and on save the page scripts call showAlertMessage(...).then(closeModal). That
// sequence displays the success/error/info dialog while the add/edit modal is
// still open, stacking one modal on top of another. To keep a single modal on
// screen at a time (and correct focus/announcement behaviour), we transparently
// wrap the global showAlertMessage so the add/edit modal is dismissed first.
// This is additive and requires no changes to any existing call sites.
(function () {
    'use strict';

    var ADD_EDIT_MODAL_ID = 'modalPopup';

    function closeAddEditModalIfOpen() {
        var modal = document.getElementById(ADD_EDIT_MODAL_ID);
        if (!modal || !modal.classList.contains('show')) {
            return;
        }
        // Removing the "show" class is what the page scripts' own closeModal()
        // does; layouts watch this flip to hide the flyout and restore scroll.
        modal.classList.remove('show');
        // Defensive fallback for layouts without the class observer.
        modal.style.display = 'none';
        document.body.style.overflow = '';
    }

    function wrapAlert(original) {
        if (typeof original !== 'function' || original.__aphaAddEditAware) {
            return original;
        }
        var wrapped = function () {
            closeAddEditModalIfOpen();
            return original.apply(this, arguments);
        };
        wrapped.__aphaAddEditAware = true;
        return wrapped;
    }

    // showAlertMessage is defined by govuk-modal-dialog.js, which loads after
    // this script. Intercept the assignment so the wrapper is installed
    // regardless of script order.
    try {
        var _showAlertMessage = wrapAlert(window.showAlertMessage);
        Object.defineProperty(window, 'showAlertMessage', {
            configurable: true,
            enumerable: true,
            get: function () { return _showAlertMessage; },
            set: function (fn) { _showAlertMessage = wrapAlert(fn); }
        });
    } catch (ignored) {
        // If the property can't be redefined, fall back to wrapping in place
        // once the DOM is ready.
        document.addEventListener('DOMContentLoaded', function () {
            if (window.showAlertMessage && !window.showAlertMessage.__aphaAddEditAware) {
                window.showAlertMessage = wrapAlert(window.showAlertMessage);
            }
        });
    }
})();

// ── Ensure Enter/Space reliably toggle checkboxes (and their labels) ─────────
// GOV.UK Frontend checkboxes visually hide the native <input> and render the
// clickable box via the associated <label>. Some pages wire up their own
// keydown handlers on forms/modals which can end up calling
// preventDefault()/stopPropagation() on keydown before the browser's native
// "Space toggles a focused checkbox" behaviour applies, effectively locking
// the checkbox. This delegated, capture-phase handler guarantees Space/Enter
// always toggle a focused checkbox input (native or govuk-styled), and also
// lets Enter/Space activate a focused checkbox <label> whose "for" points at
// a checkbox, regardless of what other handlers do.
(function () {
    'use strict';

    function isCheckboxInput(el) {
        return !!el && el.tagName === 'INPUT' && el.type === 'checkbox' && !el.disabled;
    }

    function toggleCheckbox(checkbox) {
        checkbox.checked = !checkbox.checked;
        checkbox.dispatchEvent(new Event('input', { bubbles: true }));
        checkbox.dispatchEvent(new Event('change', { bubbles: true }));
    }

    function findLabelledCheckbox(label) {
        if (!label || label.tagName !== 'LABEL') return null;
        var forId = label.getAttribute('for');
        if (forId) {
            var target = document.getElementById(forId);
            if (isCheckboxInput(target)) return target;
            return null;
        }
        var nested = label.querySelector('input[type="checkbox"]');
        return isCheckboxInput(nested) ? nested : null;
    }

    document.addEventListener('keydown', function (e) {
        if (e.key !== ' ' && e.key !== 'Spacebar' && e.key !== 'Enter') return;

        var target = e.target;
        var checkbox = null;

        if (isCheckboxInput(target)) {
            checkbox = target;
        } else if (target && target.tagName === 'LABEL') {
            checkbox = findLabelledCheckbox(target);
        }

        if (!checkbox) return;

        // Prevent the browser's default (which may be blocked/inconsistent
        // once other handlers run) and drive the toggle ourselves, so the
        // outcome is consistent regardless of other keydown listeners.
        e.preventDefault();
        toggleCheckbox(checkbox);
    }, true);
})();

// ── Keyboard-accessible "List Description" items (PIMS Maintenance > Other tab) ─
// Rows are rendered by page script as plain <td class="other-list-item"> cells
// with only a click handler wired up (see maintenance.js), so they carry no
// tabindex and are skipped entirely when tabbing through the page - Tab jumps
// straight from the panel heading to whatever is focusable after the whole
// list. Make the list operable via the keyboard using the standard WAI-ARIA
// listbox "roving tabindex" pattern: the list itself is a single Tab stop
// (only one item at a time has tabindex="0", the rest are tabindex="-1"), and
// Up/Down/Home/End move focus *within* the list without adding extra Tab
// stops. Tab/Shift+Tab simply move into/out of the list as one stop, and
// Enter/Space activates the focused item (reusing each page's own click
// handler unchanged).
(function () {
    'use strict';

    var ITEM_SELECTOR = '.other-list-item';
    var LIST_SELECTOR = 'table.other-list-table';

    function getListItems(container) {
        return Array.prototype.slice.call(container.querySelectorAll(ITEM_SELECTOR));
    }

    function updateAriaSelected(item) {
        item.setAttribute('aria-selected', item.classList.contains('selected') ? 'true' : 'false');
    }

    function ensureListIsAccessible(list) {
        if (!list.hasAttribute('data-kbd-enabled')) {
            list.setAttribute('data-kbd-enabled', 'true');
            if (!list.hasAttribute('role')) list.setAttribute('role', 'listbox');
        }
    }

    // Exactly one item in the list is a Tab stop at any time: the selected
    // item if there is one, otherwise the first item. Every other item is
    // tabindex="-1" so Tab/Shift+Tab treat the whole list as a single stop.
    function syncRovingTabindex(list, focusedItem) {
        var items = getListItems(list);
        if (!items.length) return;

        var active = focusedItem ||
            items.filter(function (i) { return i.classList.contains('selected'); })[0] ||
            items[0];

        items.forEach(function (i) {
            i.setAttribute('tabindex', i === active ? '0' : '-1');
        });
    }

    function ensureListItemAccessible(item) {
        if (item.hasAttribute('data-kbd-enabled')) return;
        item.setAttribute('data-kbd-enabled', 'true');
        if (!item.hasAttribute('role')) item.setAttribute('role', 'option');
        updateAriaSelected(item);

        var list = item.closest(LIST_SELECTOR) || item.parentElement;
        if (list) {
            ensureListIsAccessible(list);
            syncRovingTabindex(list);
        }

        item.addEventListener('keydown', function (e) {
            var items;
            var currentList = item.closest(LIST_SELECTOR) || item.parentElement;

            if (e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar') {
                e.preventDefault();
                item.click();
            } else if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
                e.preventDefault();
                items = getListItems(currentList);
                var idx = items.indexOf(item);
                var nextIdx = idx + (e.key === 'ArrowDown' ? 1 : -1);
                if (nextIdx >= 0 && nextIdx < items.length) {
                    syncRovingTabindex(currentList, items[nextIdx]);
                    items[nextIdx].focus();
                }
            } else if (e.key === 'Home') {
                e.preventDefault();
                items = getListItems(currentList);
                if (items.length) {
                    syncRovingTabindex(currentList, items[0]);
                    items[0].focus();
                }
            } else if (e.key === 'End') {
                e.preventDefault();
                items = getListItems(currentList);
                if (items.length) {
                    syncRovingTabindex(currentList, items[items.length - 1]);
                    items[items.length - 1].focus();
                }
            }
        });

        // The page's own click handler toggles the "selected" class; reflect
        // that in aria-selected/roving tabindex for every item once that
        // class change lands.
        item.addEventListener('click', function () {
            window.setTimeout(function () {
                var currentList = item.closest(LIST_SELECTOR) || item.parentElement;
                Array.prototype.forEach.call(getListItems(currentList), updateAriaSelected);
                syncRovingTabindex(currentList, item);
            }, 0);
        });
    }

    function scanForListItems() {
        Array.prototype.forEach.call(document.querySelectorAll(ITEM_SELECTOR), ensureListItemAccessible);
    }

    function init() {
        var observer = new MutationObserver(function (mutations) {
            var recheck = false;
            mutations.forEach(function (m) {
                if (m.addedNodes && m.addedNodes.length) recheck = true;
            });
            if (recheck) scanForListItems();
        });
        observer.observe(document.body, { childList: true, subtree: true });
        scanForListItems();
    }

    if (document.body) {
        init();
    } else {
        document.addEventListener('DOMContentLoaded', init);
    }
})();
