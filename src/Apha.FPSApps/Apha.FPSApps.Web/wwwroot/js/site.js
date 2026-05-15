// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Strip leading zeros from all number inputs on change (e.g. 013 → 13)
$(document).on('change', 'input[type="number"]', function () {
    if (this.value !== '') this.value = +this.value;
});
