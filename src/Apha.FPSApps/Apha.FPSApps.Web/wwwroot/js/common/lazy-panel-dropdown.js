// lazy-panel-dropdown.js
// Shared behaviour for the FPS "multi-column panel" dropdowns used inside the
// add/edit modals (Account, Staff, Test Code, ...).
//
// These panels used to render every lookup row server-side, which made the
// modals slow to open. This helper keeps the exact same markup/behaviour but:
//   * renders rows only when the panel is first opened, in chunks
//   * renders further chunks as the user scrolls
//   * debounces the search box and filters against a cached search key
//   * uses a single delegated click handler instead of one per row
//
// Usage:
//   var dd = createLazyPanelDropdown({
//       panelId: 'AccountDropdownPanel',
//       bodyId: 'AccountDropdownBody',
//       searchBoxId: 'AccountSearchBox',
//       displayId: 'AccountDisplay',
//       getOptions: function () { return window.accountPanelOptions; },
//       getSearchKey: function (o) { return o.n + '\u0000' + o.d; },
//       getRowAttributes: function (o) { return { value: o.v, display: o.n }; },
//       getCells: function (o) { return [o.n, o.d, o.c]; },
//       onSelect: function (row) { selectAccount(row.value, row.display); }
//   });
//   // dd.toggle() / dd.filter(query) are wired to the existing inline handlers.

var LAZY_PANEL_CHUNK_SIZE = 200;
var LAZY_PANEL_DEBOUNCE_MS = 150;

function escapeLazyPanelHtml(text) {
    return String(text === null || text === undefined ? '' : text)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

function createLazyPanelDropdown(config) {
    var chunkSize = config.chunkSize || LAZY_PANEL_CHUNK_SIZE;
    var debounceMs = config.debounceMs === undefined ? LAZY_PANEL_DEBOUNCE_MS : config.debounceMs;
    var cellStyle = ' style="padding:6px 8px; border-bottom:1px solid #f3f2f1;"';

    var visible = [];
    var rendered = 0;
    var searchTimer = null;

    function getOptions() {
        var options = config.getOptions() || [];
        // Search keys are computed once, on first use, and cached on the item.
        if (options.length && options[0].lazyKey === undefined) {
            options.forEach(function (o) {
                o.lazyKey = String(config.getSearchKey(o) || '').toLowerCase();
            });
        }
        return options;
    }

    function renderMore(count) {
        var body = document.getElementById(config.bodyId);
        if (!body) return;

        var start = rendered;
        var end = Math.min(start + (count || chunkSize), visible.length);
        if (end <= start) return;

        var html = '';
        for (var i = start; i < end; i++) {
            var option = visible[i];

            var attrs = config.getRowAttributes(option) || {};
            var rowAttrHtml = '';
            for (var name in attrs) {
                if (Object.prototype.hasOwnProperty.call(attrs, name)) {
                    rowAttrHtml += ' data-' + name + '="' + escapeLazyPanelHtml(attrs[name]) + '"';
                }
            }

            var cells = config.getCells(option) || [];
            var cellHtml = '';
            for (var c = 0; c < cells.length; c++) {
                cellHtml += '<td' + cellStyle + '>' + escapeLazyPanelHtml(cells[c]) + '</td>';
            }

            html += '<tr data-lazy-row="1"' + rowAttrHtml + ' style="cursor:pointer;">' + cellHtml + '</tr>';
        }

        body.insertAdjacentHTML('beforeend', html);
        rendered = end;
    }

    function render() {
        var body = document.getElementById(config.bodyId);
        if (!body) return;

        body.innerHTML = '';
        rendered = 0;
        renderMore(chunkSize);
    }

    function closePanel() {
        var panel = document.getElementById(config.panelId);
        if (panel) panel.style.display = 'none';
    }

    function filter(query) {
        if (searchTimer) {
            clearTimeout(searchTimer);
            searchTimer = null;
        }

        var apply = function () {
            var options = getOptions();
            var q = String(query || '').toLowerCase().trim();
            visible = q
                ? options.filter(function (o) { return o.lazyKey.indexOf(q) !== -1; })
                : options;
            render();
        };

        if (!query) {
            apply();
        } else {
            searchTimer = setTimeout(function () {
                searchTimer = null;
                apply();
            }, debounceMs);
        }
    }

    function toggle() {
        var panel = document.getElementById(config.panelId);
        if (!panel) return;

        // 'scroll' does not bubble, so the lazy-load listener is attached directly
        // to the panel (guarded so it is only bound once per rendered element).
        if (!panel.dataset.lazyScrollBound) {
            panel.dataset.lazyScrollBound = '1';
            panel.addEventListener('scroll', function () {
                if (rendered >= visible.length) return;
                if (this.scrollTop + this.clientHeight >= this.scrollHeight - 100) {
                    renderMore(chunkSize);
                }
            });
        }

        var isOpen = panel.style.display !== 'none';
        panel.style.display = isOpen ? 'none' : 'block';
        if (!isOpen) {
            var searchBox = document.getElementById(config.searchBoxId);
            if (searchBox) {
                searchBox.value = '';
                filter('');
                searchBox.focus();
            } else {
                filter('');
            }
        }
    }

    // Delegated row click, bound once at document level so it survives the modal
    // partial being replaced each time it is opened.
    $(document).on('click', '#' + config.bodyId + ' tr[data-lazy-row]', function () {
        var attrs = {};
        for (var i = 0; i < this.attributes.length; i++) {
            var attr = this.attributes[i];
            if (attr.name.indexOf('data-') === 0 && attr.name !== 'data-lazy-row') {
                attrs[attr.name.substring(5)] = attr.value;
            }
        }
        config.onSelect(attrs, this);
        closePanel();
    });

    // Close the panel when clicking outside of it or its display input.
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#' + config.panelId + ', #' + config.displayId).length) {
            closePanel();
        }
    });

    return {
        toggle: toggle,
        filter: filter,
        render: render,
        close: closePanel
    };
}
