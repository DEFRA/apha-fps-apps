// ── FPS navigation menu initialisation ──────────────────────────────────────
// Handles user-profile dropdown, L1 main-nav dropdowns, and L2 sub-dropdowns.
// This file is referenced by Areas/FPS/Views/Shared/_Layout.cshtml.
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

    // ── Main nav L1 dropdowns ─────────────────────────────────────────────────
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

    // ── L2 sub-dropdown toggles — open on hover ────────────────────────────────
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
        document.querySelectorAll('.sub-dropdown').forEach(function (dropdown) {
            dropdown.classList.remove('active');
        });
        var udp = document.getElementById('userdropdowndp');
        if (udp) udp.classList.remove('show');
    });

    // ── Close all on terminal dropdown-item click ─────────────────────────────
    document.querySelectorAll('.dropdown-item').forEach(function (item) {
        item.addEventListener('click', function () {
            document.querySelectorAll('.dropdown-menu').forEach(function (m) {
                m.classList.remove('show');
            });
            document.querySelectorAll('.nav-item.dropdown').forEach(function (d) {
                d.classList.remove('active');
            });
            document.querySelectorAll('.sub-dropdown-menu').forEach(function (m) {
                m.classList.remove('show');
            });
            document.querySelectorAll('.sub-dropdown').forEach(function (d) {
                d.classList.remove('active');
            });
        });
    });
})();
