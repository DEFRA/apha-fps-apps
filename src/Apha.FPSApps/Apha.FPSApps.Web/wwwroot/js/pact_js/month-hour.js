// Month Hour page JavaScript (read-only grid with year filter)

$(function () {

    // ── When Year dropdown changes: reload the grid via AJAX ─────────────
    $('#SelectedYear').on('change', function () {
        var year = $(this).val();

        $.ajax({
            url: '/PACT/MonthHour/LoadGrid',
            type: 'GET',
            data: { year: year },
            success: function (html) {
                $('#monthHourGridContainer').html(html);
            },
            error: function () {
                alert('Failed to load month hours for the selected year.');
            }
        });
    });
});
