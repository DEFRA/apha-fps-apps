// ── PACT navigation menu initialisation ──────────────────────────────────────
// Ported from nav_menu/script.js (prototype).
// Handles the user-profile dropdown and all main-nav dropdown toggles.
// This file is referenced by Areas/PACT/Views/Shared/_Layout.cshtml.
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

    // ── Main nav dropdowns ────────────────────────────────────────────────────
    document.querySelectorAll('.dropdown-toggle').forEach(function (toggle) {
        toggle.addEventListener('click', function (e) {
            e.stopPropagation();

            var dropdownId = this.getAttribute('data-dropdown');
            var dropdownMenu = document.getElementById(dropdownId);
            var parentDropdown = this.closest('.dropdown');

            // Reset inline position before toggling
            dropdownMenu.style.top = '';
            dropdownMenu.style.bottom = '';
            dropdownMenu.style.left = '';
            dropdownMenu.style.right = '';

            // Close every other open dropdown
            document.querySelectorAll('.dropdown-menu').forEach(function (m) {
                if (m !== dropdownMenu) m.classList.remove('show');
            });
            document.querySelectorAll('.dropdown').forEach(function (d) {
                if (d !== parentDropdown) d.classList.remove('active');
            });

            // Toggle the clicked dropdown
            dropdownMenu.classList.toggle('show');
            parentDropdown.classList.toggle('active');

            // Re-position AFTER show so getBoundingClientRect returns real dimensions
            if (dropdownMenu.classList.contains('show')) {
                var btnRect = toggle.getBoundingClientRect();
                var menuRect = dropdownMenu.getBoundingClientRect();
                var vw = window.innerWidth;
                var vh = window.innerHeight;

                // Vertical: open upward if insufficient space below
                if (btnRect.bottom + menuRect.height > vh) {
                    dropdownMenu.style.bottom = toggle.offsetHeight + 'px';
                } else {
                    dropdownMenu.style.top = toggle.offsetHeight + 'px';
                }

                // Horizontal: align right edge if menu would overflow viewport
                if (btnRect.left + menuRect.width > vw) {
                    dropdownMenu.style.right = '0';
                } else {
                    dropdownMenu.style.left = '0';
                }
            }
        });
    });

    // ── Close everything when clicking outside the nav ────────────────────────
    document.addEventListener('click', function () {
        document.querySelectorAll('.dropdown-menu').forEach(function (menu) {
            menu.classList.remove('show');
        });
        document.querySelectorAll('.dropdown').forEach(function (dropdown) {
            dropdown.classList.remove('active');
        });
        var udp = document.getElementById('userdropdowndp');
        if (udp) udp.classList.remove('show');
    });

    // ── Close dropdown when a nav item is clicked ─────────────────────────────
    document.querySelectorAll('.dropdown-item').forEach(function (item) {
        item.addEventListener('click', function () {
            var dropdownMenu = this.closest('.dropdown-menu');
            var parentDropdown = this.closest('.dropdown');
            if (dropdownMenu) dropdownMenu.classList.remove('show');
            if (parentDropdown) parentDropdown.classList.remove('active');
        });
    });
})();
