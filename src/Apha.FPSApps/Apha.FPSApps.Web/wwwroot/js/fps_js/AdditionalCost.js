var AdditionalCostConfig = {
    getJobCode: function () { return ''; },
    requireJobCodeForAdd: false,
    onSaved: function () { },
    onUpdated: function () { },
    onDeleted: function () { }
};

function addAdditionalCost() {
    var jobCode = AdditionalCostConfig.getJobCode();
    if (AdditionalCostConfig.requireJobCodeForAdd && !jobCode) {
        showGovukAlert('Please select a project first.');
        return;
    }

    $.ajax({
        url: '/FPS/AdditionalCostJob/Create?jobCode=' + encodeURIComponent(jobCode),
        type: 'GET',
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        },
        error: function () {
            showGovukAlert('An error occurred while loading the form.');
        }
    });
}

function saveAdditionalCost() {
    var jobCode = AdditionalCostConfig.getJobCode();    
    var data = {
        JobCode: jobCode,
        Description: $('#Description').val(),
        Account: $('#Account').val(),
        ItemCost: parseFloat($('#ItemCost').val()) || 0,
        Freq: $('#Freq').val(),
        Supplier: $('#Supplier').val()
    };

    $.ajax({
        url: '/FPS/AdditionalCostJob/Create',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                showGovukAlert(result.message).then(function () {
                    closeModal();
                    AdditionalCostConfig.onSaved();
                });
            } else {
                displayServerValidationErrors(result.errors, result.message, '#modaPopupBody');
            }
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showGovukAlert('An error occurred while saving.');
            }
        }
    });
}

function editAdditionalCost(btn) {
    var description = $(btn).data('id');
    var row = $(btn).closest('tr');
    var jobCode = AdditionalCostConfig.getJobCode();
    var account = row.find('td[data-property="Account"] span').text().trim();

    $.ajax({
        url: '/FPS/AdditionalCostJob/Edit',
        type: 'GET',
        data: { jobCode: jobCode, account: account, description: description },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        },
        error: function () {
            showGovukAlert('An error occurred while loading the form.');
        }
    });
}

function updateAdditionalCost() {
    var jobCode = AdditionalCostConfig.getJobCode();    
    var originalAccount = $('#OriginalAccount').val();
    var data = {
        JobCode: jobCode,
        Description: $('#Description').val(),
        Account: $('#Account').val() || $('#OriginalAccount').val(),
        ItemCost: parseFloat($('#ItemCost').val()) || 0,
        Freq: $('#Freq').val(),
        Supplier: $('#Supplier').val()
    };

    $.ajax({
        url: '/FPS/AdditionalCostJob/Edit',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                showGovukAlert(result.message).then(function () {
                    closeModal();
                    AdditionalCostConfig.onUpdated();
                });
            } else {
                displayServerValidationErrors(result.errors, result.message, '#modaPopupBody');
            }
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showGovukAlert('An error occurred while updating.');
            }
        }
    });
}

function deleteAdditionalCost(btn) {
    var description = $(btn).data('id');
    var row = $(btn).closest('tr');
    var jobCode = AdditionalCostConfig.getJobCode();
    var account = row.find('td[data-property="Account"] span').text().trim();

    showGovukConfirm('Are you sure you want to delete this record?').then(function (confirmed) {
        if (!confirmed) { return; }
        $.ajax({
            url: '/FPS/AdditionalCostJob/Delete',
            type: 'DELETE',
            data: { jobCode: jobCode, account: account, description: description },
            success: function (response) {
                if (response.success) {
                    showGovukAlert('Deleted successfully.').then(function () {
                        AdditionalCostConfig.onDeleted();
                    });
                } else {
                    showGovukAlert(response.message);
                }
            },
            error: function () {
                showGovukAlert('An error occurred while deleting.');
            }
        });
    });
}

function getAdditionalCostExtraFilters() {
    return { jobCode: AdditionalCostConfig.getJobCode() };
}

function closeModal() {
    clearValidationErrors('#modaPopupBody');
    $('#modaPopupBody').html('');
    $('#modalPopup').removeClass('show');
}
