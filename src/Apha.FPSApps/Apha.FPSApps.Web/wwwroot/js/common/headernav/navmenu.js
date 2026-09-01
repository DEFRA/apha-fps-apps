// ── Shared navigation menu — all 4 apps (FPS, PACT, PIMS, CostBook) ──────────
// Handles: user-profile dropdown, L1 main-nav dropdowns, L2 sub-dropdowns (FPS).
// Referenced by Views/Shared/Components/AppNav/Default.cshtml.
(function () {
    'use strict';

    // ── User profile dropdown ─────────────────────────────────────────────────
    var userBtn = document.getElementById('userdropdownbtn');
    if (userBtn) {
        userBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            document.getElementById('userdropdowndp').classList.toggle('show');
        });
    }

    // ── L1 main nav dropdowns ─────────────────────────────────────────────────
    document.querySelectorAll('.nav-button.dropdown-toggle').forEach(function (toggle) {
        toggle.addEventListener('click', function (e) {
            e.stopPropagation();

            var dropdownId = this.getAttribute('data-dropdown');
            var dropdownMenu = document.getElementById(dropdownId);
            var parentDropdown = this.closest('.nav-item.dropdown');

            // Reset inline position before toggling
            dropdownMenu.style.top = '';
            dropdownMenu.style.bottom = '';
            dropdownMenu.style.left = '';
            dropdownMenu.style.right = '';

            // Close every other open L1 dropdown and all open sub-dropdowns
            document.querySelectorAll('.dropdown-menu').forEach(function (m) {
                if (m !== dropdownMenu) m.classList.remove('show');
            });
            document.querySelectorAll('.nav-item.dropdown').forEach(function (d) {
                if (d !== parentDropdown) d.classList.remove('active');
            });
            document.querySelectorAll('.sub-dropdown-menu').forEach(function (m) {
                m.classList.remove('show');
            });
            document.querySelectorAll('.sub-dropdown').forEach(function (d) {
                d.classList.remove('active');
            });

            // Toggle the clicked L1 dropdown
            dropdownMenu.classList.toggle('show');
            parentDropdown.classList.toggle('active');

            // Re-position AFTER show so getBoundingClientRect returns real dimensions
            if (dropdownMenu.classList.contains('show')) {
                var btnRect = toggle.getBoundingClientRect();
                var menuRect = dropdownMenu.getBoundingClientRect();
                var vw = window.innerWidth;
                var vh = window.innerHeight;

                if (btnRect.bottom + menuRect.height > vh) {
                    dropdownMenu.style.bottom = toggle.offsetHeight + 'px';
                } else {
                    dropdownMenu.style.top = toggle.offsetHeight + 'px';
                }

                if (btnRect.left + menuRect.width > vw) {
                    dropdownMenu.style.right = '0';
                } else {
                    dropdownMenu.style.left = '0';
                }
            }
        });
    });

    // ── L2 sub-dropdown toggles — open on hover (FPS only; ignored by others) ─
    document.querySelectorAll('.sub-dropdown').forEach(function (subDropdown) {
        subDropdown.addEventListener('mouseenter', function () {
            var subMenu = this.querySelector('.sub-dropdown-menu');
            if (!subMenu) return;

            // Close sibling sub-dropdowns within the same L1 panel
            var parentDropdownMenu = this.closest('.dropdown-menu');
            if (parentDropdownMenu) {
                parentDropdownMenu.querySelectorAll('.sub-dropdown-menu').forEach(function (m) {
                    if (m !== subMenu) m.classList.remove('show');
                });
                parentDropdownMenu.querySelectorAll('.sub-dropdown').forEach(function (d) {
                    if (d !== subDropdown) d.classList.remove('active');
                });
            }

            // Fly right; fall back to left if near viewport edge
            var itemRect = subDropdown.getBoundingClientRect();
            subMenu.style.left = '';
            subMenu.style.right = '';
            if (itemRect.right + 260 > window.innerWidth) {
                subMenu.style.left = 'auto';
                subMenu.style.right = '100%';
            } else {
                subMenu.style.left = '100%';
                subMenu.style.right = 'auto';
            }

            subMenu.classList.add('show');
            subDropdown.classList.add('active');
        });

        subDropdown.addEventListener('mouseleave', function () {
            var subMenu = this.querySelector('.sub-dropdown-menu');
            if (subMenu) subMenu.classList.remove('show');
            this.classList.remove('active');
        });
    });

    // ── Close everything when clicking outside the nav ────────────────────────
    document.addEventListener('click', function () {
        document.querySelectorAll('.dropdown-menu').forEach(function (menu) {
            menu.classList.remove('show');
        });
        document.querySelectorAll('.nav-item.dropdown').forEach(function (dropdown) {
            dropdown.classList.remove('active');
        });
        document.querySelectorAll('.sub-dropdown-menu').forEach(function (menu) {
            menu.classList.remove('show');
        });
        document.querySelectorAll('.sub-dropdown').forEach(function (d) {
            d.classList.remove('active');
        });
        var udp = document.getElementById('userdropdowndp');
        if (udp) udp.classList.remove('show');
    });

    // ── Close dropdown when a nav item is clicked ─────────────────────────────
    document.querySelectorAll('.dropdown-item').forEach(function (item) {
        item.addEventListener('click', function () {
            var dropdownMenu = this.closest('.dropdown-menu');
            var parentDropdown = this.closest('.nav-item.dropdown');
            if (dropdownMenu) dropdownMenu.classList.remove('show');
            if (parentDropdown) parentDropdown.classList.remove('active');
        });
    });

    // ── Keyboard (arrow key) navigation ────────────────────────────────────────
    // Tab/Shift+Tab already work via native browser focus order. Arrow-key
    // navigation between/within menus is not provided by the browser for
    // custom (non-<select>/<ul role="menu">) markup, so it is added here:
    //   - ArrowLeft / ArrowRight moves focus between top-level (L1) nav buttons
    //   - ArrowDown (on a L1 button) opens its dropdown and focuses first item
    //   - ArrowUp / ArrowDown moves focus between items inside an open dropdown
    //   - ArrowRight (on a sub-dropdown toggle) opens the sub-menu and focuses
    //     its first item; ArrowLeft closes it and returns focus to the toggle
    //   - Escape closes the current menu and returns focus to its L1 button
    (function () {
        var mainNav = document.querySelector('.main-nav');
        if (!mainNav) return;

        function getL1Buttons() {
            // Include plain nav links (PIMS/CostBook, which have no dropdown)
            // as well as dropdown-toggle buttons (FPS/PACT), so Left/Right
            // arrow-key navigation cycles through every top-level nav item.
            return Array.prototype.slice.call(mainNav.querySelectorAll(':scope > .nav-item > .nav-button'));
        }

        function getVisibleMenuItems(menu) {
            // Direct-child focusable entries within a dropdown/sub-dropdown menu:
            // either a plain link, a sub-dropdown's own toggle button, or (for
            // multi-column dropdowns, e.g. "MAB") a link nested one level
            // deeper inside a ".dropdown-col" wrapper.
            return Array.prototype.slice.call(
                menu.querySelectorAll(':scope > a.dropdown-item, :scope > .sub-dropdown > .sub-dropdown-toggle, :scope > .dropdown-col > a.dropdown-item')
            );
        }

        function closeAllMenus() {
            document.querySelectorAll('.dropdown-menu').forEach(function (m) { m.classList.remove('show'); });
            document.querySelectorAll('.nav-item.dropdown').forEach(function (d) { d.classList.remove('active'); });
            document.querySelectorAll('.sub-dropdown-menu').forEach(function (m) { m.classList.remove('show'); });
            document.querySelectorAll('.sub-dropdown').forEach(function (d) { d.classList.remove('active'); });
        }

        function openL1Dropdown(toggle) {
            var dropdownId = toggle.getAttribute('data-dropdown');
            var dropdownMenu = dropdownId ? document.getElementById(dropdownId) : null;
            if (!dropdownMenu) return null;

            var parentDropdown = toggle.closest('.nav-item.dropdown');
            closeAllMenus();
            dropdownMenu.classList.add('show');
            if (parentDropdown) parentDropdown.classList.add('active');
            return dropdownMenu;
        }

        function openSubDropdown(subToggle) {
            var subDropdown = subToggle.closest('.sub-dropdown');
            var subMenu = subDropdown ? subDropdown.querySelector(':scope > .sub-dropdown-menu') : null;
            if (!subMenu) return null;

            var parentMenu = subDropdown.closest('.dropdown-menu');
            if (parentMenu) {
                parentMenu.querySelectorAll(':scope > .sub-dropdown > .sub-dropdown-menu').forEach(function (m) {
                    if (m !== subMenu) m.classList.remove('show');
                });
                parentMenu.querySelectorAll(':scope > .sub-dropdown').forEach(function (d) {
                    if (d !== subDropdown) d.classList.remove('active');
                });
            }

            subMenu.style.left = '100%';
            subMenu.style.right = 'auto';
            subMenu.classList.add('show');
            subDropdown.classList.add('active');
            return subMenu;
        }

        mainNav.addEventListener('keydown', function (event) {
            var key = event.key;
            var target = event.target;

            // ── L1 nav buttons: Left/Right cycles between top-level menus, Down opens ──
            if (target.classList.contains('nav-button')) {
                var l1Buttons = getL1Buttons();
                var currentIndex = l1Buttons.indexOf(target);

                if (currentIndex === -1) {
                    // Not a recognised top-level nav item; let other handlers/native behaviour proceed.
                } else if (key === 'ArrowRight' || key === 'ArrowLeft') {
                    event.preventDefault();
                    var delta = key === 'ArrowRight' ? 1 : -1;
                    var nextIndex = (currentIndex + delta + l1Buttons.length) % l1Buttons.length;
                    closeAllMenus();
                    l1Buttons[nextIndex].focus();
                } else if (key === 'ArrowDown' && target.classList.contains('dropdown-toggle')) {
                    event.preventDefault();
                    var menu = openL1Dropdown(target);
                    var items = menu ? getVisibleMenuItems(menu) : [];
                    if (items.length) items[0].focus();
                } else if (key === 'Escape') {
                    closeAllMenus();
                }
                return;
            }

            // ── Items inside an open L1 dropdown menu ──────────────────────────────
            var dropdownMenu = target.closest('.dropdown-menu');
            var subDropdownMenu = target.closest('.sub-dropdown-menu');

            if (subDropdownMenu && (target.classList.contains('dropdown-item') || target.classList.contains('sub-dropdown-toggle'))) {
                var subItems = getVisibleMenuItems(subDropdownMenu);
                var subIndex = subItems.indexOf(target);

                if (key === 'ArrowDown' || key === 'ArrowUp') {
                    event.preventDefault();
                    var subDelta = key === 'ArrowDown' ? 1 : -1;
                    var nextSubIndex = (subIndex + subDelta + subItems.length) % subItems.length;
                    subItems[nextSubIndex].focus();
                } else if (key === 'ArrowRight' && target.classList.contains('sub-dropdown-toggle')) {
                    event.preventDefault();
                    var nestedMenu = openSubDropdown(target);
                    var nestedItems = nestedMenu ? getVisibleMenuItems(nestedMenu) : [];
                    if (nestedItems.length) nestedItems[0].focus();
                } else if (key === 'ArrowLeft' || key === 'Escape') {
                    event.preventDefault();
                    subDropdownMenu.classList.remove('show');
                    var parentSubDropdown = subDropdownMenu.closest('.sub-dropdown');
                    if (parentSubDropdown) {
                        parentSubDropdown.classList.remove('active');
                        var parentToggle = parentSubDropdown.querySelector(':scope > .sub-dropdown-toggle');
                        if (parentToggle) parentToggle.focus();
                    }
                }
                return;
            }

            if (dropdownMenu && (target.classList.contains('dropdown-item') || target.classList.contains('sub-dropdown-toggle'))) {
                var items = getVisibleMenuItems(dropdownMenu);
                var index = items.indexOf(target);

                if (key === 'ArrowDown' || key === 'ArrowUp') {
                    event.preventDefault();
                    var delta = key === 'ArrowDown' ? 1 : -1;
                    var nextIndex = (index + delta + items.length) % items.length;
                    items[nextIndex].focus();
                } else if (key === 'ArrowRight' && target.classList.contains('sub-dropdown-toggle')) {
                    event.preventDefault();
                    var subMenu = openSubDropdown(target);
                    var subMenuItems = subMenu ? getVisibleMenuItems(subMenu) : [];
                    if (subMenuItems.length) subMenuItems[0].focus();
                } else if (key === 'Escape') {
                    event.preventDefault();
                    closeAllMenus();
                    var l1Toggle = dropdownMenu.closest('.nav-item.dropdown').querySelector(':scope > .nav-button.dropdown-toggle');
                    if (l1Toggle) l1Toggle.focus();
                } else if (key === 'ArrowLeft') {
                    event.preventDefault();
                    closeAllMenus();
                    var currentL1Toggle = dropdownMenu.closest('.nav-item.dropdown').querySelector(':scope > .nav-button.dropdown-toggle');
                    if (currentL1Toggle) {
                        var l1List = getL1Buttons();
                        var l1Idx = l1List.indexOf(currentL1Toggle);
                        var prevIdx = (l1Idx - 1 + l1List.length) % l1List.length;
                        l1List[prevIdx].focus();
                    }
                }
            }
        });
    })();

    // ── Fix: submenu (sub-dropdown) arrow-key navigation ──────────────────────
    // Additive, standalone fix — does not modify any of the code above.
    // Arrow-key handling for submenu items is bound DIRECTLY to each submenu
    // item element (instead of relying on event delegation/bubbling through
    // `.main-nav`), so there is no ambiguity/ordering conflict with the
    // existing delegated `mainNav` keydown listener above. A MutationObserver
    // keeps this wired for any sub-dropdown menus added to the DOM later.
    (function () {
        function getSubMenuItems(menu) {
            return Array.prototype.slice.call(
                menu.querySelectorAll(':scope > a.dropdown-item, :scope > .sub-dropdown > .sub-dropdown-toggle')
            );
        }

        function openNestedSubDropdown(subToggle) {
            var subDropdown = subToggle.closest('.sub-dropdown');
            var subMenu = subDropdown ? subDropdown.querySelector(':scope > .sub-dropdown-menu') : null;
            if (!subMenu) return null;

            var parentMenu = subDropdown.closest('.dropdown-menu, .sub-dropdown-menu');
            if (parentMenu) {
                parentMenu.querySelectorAll(':scope > .sub-dropdown > .sub-dropdown-menu').forEach(function (m) {
                    if (m !== subMenu) m.classList.remove('show');
                });
                parentMenu.querySelectorAll(':scope > .sub-dropdown').forEach(function (d) {
                    if (d !== subDropdown) d.classList.remove('active');
                });
            }

            subMenu.style.left = '100%';
            subMenu.style.right = 'auto';
            subMenu.classList.add('show');
            subDropdown.classList.add('active');
            return subMenu;
        }

        function closeSubMenuAndFocusToggle(subMenu) {
            var ownerSubDropdown = subMenu.closest('.sub-dropdown');
            subMenu.classList.remove('show');
            if (ownerSubDropdown) {
                ownerSubDropdown.classList.remove('active');
                var ownerToggle = ownerSubDropdown.querySelector(':scope > .sub-dropdown-toggle');
                if (ownerToggle) ownerToggle.focus();
            }
        }

        function handleSubMenuItemKeydown(event) {
            var target = event.currentTarget;
            var subMenu = target.closest('.sub-dropdown-menu');
            if (!subMenu) return;

            var items = getSubMenuItems(subMenu);
            var index = items.indexOf(target);
            if (index === -1) return;

            switch (event.key) {
                case 'ArrowDown':
                    event.preventDefault();
                    event.stopPropagation();
                    items[(index + 1) % items.length].focus();
                    break;
                case 'ArrowUp':
                    event.preventDefault();
                    event.stopPropagation();
                    items[(index - 1 + items.length) % items.length].focus();
                    break;
                case 'ArrowRight':
                    if (target.classList.contains('sub-dropdown-toggle')) {
                        event.preventDefault();
                        event.stopPropagation();
                        var nestedMenu = openNestedSubDropdown(target);
                        var nestedItems = nestedMenu ? getSubMenuItems(nestedMenu) : [];
                        if (nestedItems.length) nestedItems[0].focus();
                    }
                    break;
                case 'ArrowLeft':
                    event.preventDefault();
                    event.stopPropagation();
                    closeSubMenuAndFocusToggle(subMenu);
                    break;
            }
        }

        function wireSubMenuItem(item) {
            if (item.hasAttribute('data-submenu-kbd-wired')) return;
            item.setAttribute('data-submenu-kbd-wired', 'true');
            item.addEventListener('keydown', handleSubMenuItemKeydown);
        }

        function wireAll(root) {
            (root.querySelectorAll ? root : document)
                .querySelectorAll('.sub-dropdown-menu > a.dropdown-item, .sub-dropdown-menu > .sub-dropdown > .sub-dropdown-toggle')
                .forEach(wireSubMenuItem);
        }

        wireAll(document);

        var observer = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType === 1) wireAll(node);
                });
            });
        });
        observer.observe(document.body, { childList: true, subtree: true });
    })();

    // ── Fix: ArrowDown/ArrowUp on the top-right "Home" user-profile dropdown ──
    // Additive, standalone fix. The `.userdropdown` menu only ever contains a
    // single "Home" link, so it has no arrow-key navigation of its own — but
    // with nothing handling/preventing the key, the browser's default
    // behaviour for ArrowDown/ArrowUp (scrolling the page) kicks in while the
    // link has focus, which visually looks like the whole header/button is
    // "moving downward". Prevent that default scroll while focus is on the
    // user dropdown button or inside its menu.
    (function () {
        var userBtn = document.getElementById('userdropdownbtn');
        var userMenu = document.getElementById('userdropdowndp');
        if (!userBtn && !userMenu) return;

        function preventArrowScroll(event) {
            if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
                event.preventDefault();
            }
        }

        if (userBtn) userBtn.addEventListener('keydown', preventArrowScroll);
        if (userMenu) {
            userMenu.querySelectorAll('a, button').forEach(function (el) {
                el.addEventListener('keydown', preventArrowScroll);
            });
        }
    })();
})();