// ── PIMS navigation menu initialisation ───────────────────────────────────
// Handles user-profile dropdown and main-nav dropdown toggles.
// This file is referenced by Areas/PIMS/Views/Shared/_Layout.cshtml.
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
    document.querySelectorAll('.nav-button.dropdown-toggle').forEach(function (toggle) {
        toggle.addEventListener('click', function (e) {
            e.stopPropagation();

            var dropdownId = this.getAttribute('data-dropdown');
            var dropdownMenu = document.getElementById(dropdownId);
            var parentDropdown = this.closest('.nav-item.dropdown');

            dropdownMenu.style.top = '';
            dropdownMenu.style.bottom = '';
            dropdownMenu.style.left = '';
            dropdownMenu.style.right = '';

            document.querySelectorAll('.dropdown-menu').forEach(function (m) {
                if (m !== dropdownMenu) m.classList.remove('show');
            });
            document.querySelectorAll('.nav-item.dropdown').forEach(function (d) {
                if (d !== parentDropdown) d.classList.remove('active');
            });

            dropdownMenu.classList.toggle('show');
            parentDropdown.classList.toggle('active');

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

    // ── Close everything when clicking outside the nav ────────────────────────
    document.addEventListener('click', function () {
        document.querySelectorAll('.dropdown-menu').forEach(function (menu) {
            menu.classList.remove('show');
        });
        document.querySelectorAll('.nav-item.dropdown').forEach(function (dropdown) {
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
