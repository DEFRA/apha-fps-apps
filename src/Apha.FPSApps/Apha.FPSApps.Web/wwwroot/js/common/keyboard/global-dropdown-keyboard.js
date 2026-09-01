// ── Global keyboard-navigation support for custom "flyout" dropdowns ─────────

(function () {
    'use strict';

    function isVisible(el) {
        if (!el) return false;
        var style = window.getComputedStyle(el);
        return style.display !== 'none' && style.visibility !== 'hidden';
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

        var body = panel.querySelector('tbody[id$="DropdownBody"]') || panel.querySelector('tbody');
        var search = panel.querySelector('input[id$="SearchBox"]');

        return { panel: panel, body: body, search: search };
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
                row.click();
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
        var prefix = panel.id.replace(/DropdownPanel$/, '');
        var trigger = document.getElementById(prefix + 'Display');
        if (isVisible(panel)) {
            // Reuse the page's own close behaviour: a click outside the panel/trigger
            // is what every existing page listens for to hide the panel.
            document.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
        }
        if (trigger) trigger.focus();
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

        function getFocusableElements() {
            var scope = modalBody || modal;
            return Array.prototype.slice.call(
                scope.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])')
            ).filter(function (el) {
                return !el.hasAttribute('disabled') && el.offsetParent !== null;
            });
        }

        var observer = new MutationObserver(function () {
            if (modal.classList.contains('show')) {
                modal.style.display = 'flex';
                document.body.style.overflow = 'hidden';

                previouslyFocused = document.activeElement;

                setTimeout(function () {
                    var focusable = getFocusableElements();
                    if (focusable.length) {
                        focusable[0].focus();
                    } else {
                        modal.focus();
                    }
                }, 0);
            } else {
                modal.style.display = 'none';
                document.body.style.overflow = '';

                if (previouslyFocused && typeof previouslyFocused.focus === 'function') {
                    previouslyFocused.focus();
                }
                previouslyFocused = null;
            }
        });
        observer.observe(modal, { attributes: true, attributeFilter: ['class'] });

        modal.addEventListener('keydown', function (e) {
            if (e.key !== 'Tab' || !modal.classList.contains('show')) return;

            var focusable = getFocusableElements();
            if (!focusable.length) return;

            var first = focusable[0];
            var last = focusable[focusable.length - 1];

            if (e.shiftKey && document.activeElement === first) {
                e.preventDefault();
                last.focus();
            } else if (!e.shiftKey && document.activeElement === last) {
                e.preventDefault();
                first.focus();
            }
        });

        modal.addEventListener('click', function (e) {
            if (e.target === modal) {
                return false;
            }
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && modal.classList.contains('show')) {
                modal.classList.remove('show');
            }
        });
    }


})();


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
