/**
 * Reusable Searchable Select component.
 * Version: 1.0
 *
 * Progressively enhances an existing <select> into a type-and-search combo box.
 * The original <select> is kept in the DOM and its value/change event are still
 * the source of truth, so existing page scripts (and model binding) keep working
 * unchanged - they can continue to read `select.value` and listen for 'change'.
 *
 * Usage:
 *   // Explicit
 *   FpsSearchableSelect.enhance('#programSelect');
 *   FpsSearchableSelect.enhance(document.querySelectorAll('.js-searchable'));
 *
 *   // Declarative - add data-searchable-select to the <select> and it is
 *   // enhanced automatically on DOMContentLoaded.
 *   <select id="programSelect" data-searchable-select>...</select>
 *
 * Requires: /css/common/searchable-select.css
 */
(function (window, document) {
    'use strict';

    var INSTANCE_KEY = '__fpsSearchableSelect';
    var openInstance = null;

    function closeOpenInstance(except) {
        if (openInstance && openInstance !== except) {
            openInstance.close();
        }
    }

    document.addEventListener('click', function (event) {
        if (openInstance && !openInstance.wrapper.contains(event.target)) {
            openInstance.close();
        }
    });

    /**
     * @param {HTMLSelectElement} select the select element to enhance
     * @param {Object} [options] { placeholder, noResultsText }
     */
    function SearchableSelect(select, options) {
        this.select = select;
        this.options = Object.assign({
            placeholder: select.getAttribute('data-placeholder') || '-- select --',
            noResultsText: 'No matches found'
        }, options || {});

        this.activeIndex = -1;
        this.visibleOptions = [];

        this.build();
        this.attachEvents();
        this.syncFromSelect();
    }

    SearchableSelect.prototype.build = function () {
        var select = this.select;
        var id = select.id || ('searchableSelect_' + Math.random().toString(36).slice(2));

        var wrapper = document.createElement('div');
        wrapper.className = 'fps-searchable-select';

        select.parentNode.insertBefore(wrapper, select);
        wrapper.appendChild(select);
        select.classList.add('fps-searchable-select__source');

        var input = document.createElement('input');
        input.type = 'text';
        input.id = id + '_search';
        input.className = 'fps-searchable-select__input govuk-select govuk-!-font-size-16';
        input.setAttribute('autocomplete', 'off');
        input.setAttribute('role', 'combobox');
        input.setAttribute('aria-expanded', 'false');
        input.setAttribute('aria-autocomplete', 'list');
        input.setAttribute('aria-controls', id + '_panel');
        input.placeholder = this.options.placeholder;
        if (select.disabled) { input.disabled = true; }

        // Carry the accessible name across from the original select.
        var label = document.querySelector('label[for="' + id + '"]');
        if (label) {
            label.setAttribute('for', input.id);
        } else if (select.getAttribute('aria-label')) {
            input.setAttribute('aria-label', select.getAttribute('aria-label'));
        }

        var panel = document.createElement('ul');
        panel.id = id + '_panel';
        panel.className = 'fps-searchable-select__panel';
        panel.setAttribute('role', 'listbox');

        wrapper.appendChild(input);
        wrapper.appendChild(panel);

        this.wrapper = wrapper;
        this.input = input;
        this.panel = panel;
    };

    SearchableSelect.prototype.attachEvents = function () {
        var self = this;

        this.input.addEventListener('focus', function () {
            self.open('');
        });

        this.input.addEventListener('click', function () {
            self.open(self.input.value === self.selectedText() ? '' : self.input.value);
        });

        this.input.addEventListener('input', function () {
            self.open(self.input.value);
        });

        this.input.addEventListener('keydown', function (event) {
            self.onKeyDown(event);
        });

        this.input.addEventListener('blur', function () {
            // Re-display the committed selection; typed text that matched nothing is discarded.
            window.setTimeout(function () {
                if (!self.wrapper.contains(document.activeElement)) {
                    self.syncFromSelect();
                }
            }, 0);
        });

        this.panel.addEventListener('mousedown', function (event) {
            // mousedown (not click) so the option survives the input's blur.
            var option = event.target.closest('.fps-searchable-select__option');
            if (!option) { return; }
            event.preventDefault();
            self.commit(option.getAttribute('data-value'));
        });

        // Keep the component in step when other scripts change the select
        // programmatically (e.g. the mutually exclusive dropdown resets).
        this.select.addEventListener('change', function () {
            self.syncFromSelect();
        });
    };

    SearchableSelect.prototype.onKeyDown = function (event) {
        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                if (!this.isOpen()) { this.open(''); }
                this.moveActive(1);
                break;
            case 'ArrowUp':
                event.preventDefault();
                this.moveActive(-1);
                break;
            case 'Enter':
                if (this.isOpen() && this.activeIndex > -1) {
                    event.preventDefault();
                    this.commit(this.visibleOptions[this.activeIndex].value);
                }
                break;
            case 'Escape':
                this.close();
                this.syncFromSelect();
                break;
            case 'Tab':
                this.close();
                break;
            default:
                break;
        }
    };

    SearchableSelect.prototype.isOpen = function () {
        return this.wrapper.classList.contains('fps-searchable-select--open');
    };

    SearchableSelect.prototype.selectedText = function () {
        var option = this.select.options[this.select.selectedIndex];
        return option ? option.text : '';
    };

    /** Refresh the text box from the current <select> value. */
    SearchableSelect.prototype.syncFromSelect = function () {
        var option = this.select.options[this.select.selectedIndex];
        this.input.value = (option && option.value) ? option.text : '';
        this.input.disabled = this.select.disabled;
    };

    SearchableSelect.prototype.open = function (term) {
        closeOpenInstance(this);
        this.renderOptions(term || '');
        this.wrapper.classList.add('fps-searchable-select--open');
        this.input.setAttribute('aria-expanded', 'true');
        openInstance = this;
    };

    SearchableSelect.prototype.close = function () {
        this.wrapper.classList.remove('fps-searchable-select--open');
        this.input.setAttribute('aria-expanded', 'false');
        this.activeIndex = -1;
        if (openInstance === this) { openInstance = null; }
    };

    SearchableSelect.prototype.renderOptions = function (term) {
        var needle = (term || '').trim().toLowerCase();
        var currentValue = this.select.value;
        var html = '';

        this.visibleOptions = [];

        for (var i = 0; i < this.select.options.length; i++) {
            var option = this.select.options[i];
            var text = option.text || '';
            if (needle && text.toLowerCase().indexOf(needle) === -1) { continue; }

            this.visibleOptions.push({ value: option.value, text: text });

            var classes = 'fps-searchable-select__option';
            if (option.value && option.value === currentValue) {
                classes += ' fps-searchable-select__option--selected';
            }
            html += '<li class="' + classes + '" role="option"'
                + ' aria-selected="' + (option.value === currentValue) + '"'
                + ' data-value="' + escapeHtml(option.value) + '">'
                + escapeHtml(text) + '</li>';
        }

        if (!this.visibleOptions.length) {
            html = '<li class="fps-searchable-select__empty">' + escapeHtml(this.options.noResultsText) + '</li>';
        }

        this.panel.innerHTML = html;
        this.activeIndex = -1;
    };

    SearchableSelect.prototype.moveActive = function (delta) {
        if (!this.visibleOptions.length) { return; }

        this.activeIndex += delta;
        if (this.activeIndex < 0) { this.activeIndex = this.visibleOptions.length - 1; }
        if (this.activeIndex >= this.visibleOptions.length) { this.activeIndex = 0; }

        var items = this.panel.querySelectorAll('.fps-searchable-select__option');
        for (var i = 0; i < items.length; i++) {
            items[i].classList.toggle('fps-searchable-select__option--active', i === this.activeIndex);
        }

        var active = items[this.activeIndex];
        if (active && active.scrollIntoView) {
            active.scrollIntoView({ block: 'nearest' });
        }
    };

    /** Apply a value to the underlying select and notify listeners. */
    SearchableSelect.prototype.commit = function (value) {
        this.select.value = value;
        this.syncFromSelect();
        this.close();
        this.input.focus();
        this.select.dispatchEvent(new Event('change', { bubbles: true }));
    };

    function escapeHtml(value) {
        return String(value == null ? '' : value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function enhanceOne(select, options) {
        if (!select || select.tagName !== 'SELECT' || select[INSTANCE_KEY]) { return null; }
        var instance = new SearchableSelect(select, options);
        select[INSTANCE_KEY] = instance;
        return instance;
    }

    var FpsSearchableSelect = {
        /**
         * Enhance one or more selects.
         * @param {string|Element|NodeList|Array} target selector, element or collection
         * @param {Object} [options] { placeholder, noResultsText }
         */
        enhance: function (target, options) {
            var elements;
            if (typeof target === 'string') {
                elements = document.querySelectorAll(target);
            } else if (target instanceof Element) {
                elements = [target];
            } else {
                elements = target || [];
            }

            var created = [];
            Array.prototype.forEach.call(elements, function (element) {
                var instance = enhanceOne(element, options);
                if (instance) { created.push(instance); }
            });
            return created;
        },

        /** Get the instance attached to a select, if any. */
        getInstance: function (select) {
            var element = typeof select === 'string' ? document.querySelector(select) : select;
            return element ? element[INSTANCE_KEY] || null : null;
        },

        /** Re-read options from the underlying select (call after repopulating it). */
        refresh: function (select) {
            var instance = FpsSearchableSelect.getInstance(select);
            if (instance) { instance.syncFromSelect(); }
        }
    };

    document.addEventListener('DOMContentLoaded', function () {
        FpsSearchableSelect.enhance('select[data-searchable-select]');
    });

    window.FpsSearchableSelect = FpsSearchableSelect;
})(window, document);
