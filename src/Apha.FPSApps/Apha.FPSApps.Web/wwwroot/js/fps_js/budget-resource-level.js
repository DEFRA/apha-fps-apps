// Budget Resource Level page JavaScript
// currentWorkgroup, currentAccount, currentProfitCentre and currentYear are
// declared in the Razor @section Scripts block because they are server-rendered.

// ─── Grid manager accessors ──────────────────────────────────────────────────
function getWorkGroupGridManager()  { return window['gridManager_workGroupGrid']; }
function getBudgetBidsGridManager() { return window['gridManager_budgetBidsGrid']; }
function getPurchasesGridManager()  { return window['gridManager_purchasesGrid']; }

// ─── Extra filter callbacks used by _DataGrid ────────────────────────────────
function getWorkGroupExtraFilters()  { return { profitCentre: currentProfitCentre }; }
function getBudgetBidExtraFilters()  { return { workgroup: currentWorkgroup }; }
function getPurchaseExtraFilters()   { return { workgroup: currentWorkgroup, account: currentAccount }; }

// ─── Cascade helpers ─────────────────────────────────────────────────────────
function onWorkGroupRowClick(WorkGroupName) {
    currentWorkgroup = WorkGroupName;
    currentAccount   = '';
    $('#fpsBrlSelectedWg').val(WorkGroupName);
    $('#fpsBrlPurchasesWg').val(WorkGroupName);
    $('#fpsBrlSelectedAccount').val('');

    var mgr = getBudgetBidsGridManager();
    if (mgr) mgr.reloadGrid({ page: 1 });

    var purchasesMgr = getPurchasesGridManager();
    if (purchasesMgr) purchasesMgr.reloadGrid({ page: 1 });
}

function onBudgetBidRowClick(account) {
    currentAccount = account;
    $('#fpsBrlSelectedAccount').val(account);
    $('#fpsBrlPurchasesWg').val(currentWorkgroup);

    var mgr = getPurchasesGridManager();
    if (mgr) mgr.reloadGrid({ page: 1 });
}

// ─── WorkGroup grid — row click ──────────────────────────────────────────────
$(document).on('click', '#gridContainer_workGroupGrid table tbody tr', function () {
    var WorkGroupName = $(this).data('id');
    if (!WorkGroupName) return;

    $('#gridContainer_workGroupGrid table tbody tr').removeClass('selected-row');
    $(this).addClass('selected-row');
    onWorkGroupRowClick(WorkGroupName);
});

// ─── Budget Bids grid — row click ────────────────────────────────────────────
$(document).on('click', '#gridContainer_budgetBidsGrid table tbody tr', function () {
    var account = $(this).find('td[data-property="Account"]').text().trim() || $(this).data('id');
    if (!account) return;

    $('#gridContainer_budgetBidsGrid table tbody tr').removeClass('selected-row');
    $(this).addClass('selected-row');
    onBudgetBidRowClick(account);
});

// ─── Budget Bid CRUD ─────────────────────────────────────────────────────────
function addBudgetBid() {
    if (!currentWorkgroup) { showAlertMessage('Please select a work group first.', AlertType.INFO); return; }
    openModal('/FPS/BudgetResourceLevel/CreateBudgetBid?WorkGroupName=' + encodeURIComponent(currentWorkgroup));
}

function editBudgetBid(btn) {
    var account = $(btn).data('id');
    openModal('/FPS/BudgetResourceLevel/EditBudgetBid?WorkGroupName=' + encodeURIComponent(currentWorkgroup) + '&account=' + encodeURIComponent(account));
}

function deleteBudgetBid(btn) {
    var account = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this budget bid?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/FPS/BudgetResourceLevel/DeleteBudgetBid?WorkGroupName=' + encodeURIComponent(currentWorkgroup) + '&account=' + encodeURIComponent(account),
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage(response.message, AlertType.SUCCESS).then(function () {
                        var mgr = getBudgetBidsGridManager(); if (mgr) mgr.reloadGrid({ page: 1 });
                    });
                } else {
                    showAlertMessage(response.message || 'Failed to delete budget bid.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting.', AlertType.ERROR);
            }
        });
    });
}

function saveBudgetBid() {
    var form = $('#formAddBudgetBid');
    submitFormAsJson(form, '/FPS/BudgetResourceLevel/CreateBudgetBid', function (response) {
        if (response.success) {
            closeModal();
            showSuccess(response.message);
            var mgr = getBudgetBidsGridManager(); if (mgr) mgr.reloadGrid({ page: 1 });
        } else {
            showModalErrors(response.errors, response.message);
        }
    });
}

function updateBudgetBid() {
    var form = $('#formEditBudgetBid');
    submitFormAsJson(form, '/FPS/BudgetResourceLevel/EditBudgetBid', function (response) {
        if (response.success) {
            closeModal();
            showSuccess(response.message);
            var mgr = getBudgetBidsGridManager(); if (mgr) mgr.reloadGrid({ page: 1 });
        } else {
            showModalErrors(response.errors, response.message);
        }
    });
}

// ─── Purchase CRUD ───────────────────────────────────────────────────────────
function addPurchase() {
    if (!currentWorkgroup || !currentAccount) { showAlertMessage('Please select a work group and account first.', AlertType.INFO); return; }
    openModal('/FPS/BudgetResourceLevel/CreatePurchase?WorkGroupName=' + encodeURIComponent(currentWorkgroup) + '&account=' + encodeURIComponent(currentAccount));
}

function editPurchase(btn) {
    var itemDescription = $(btn).data('id');
    openModal('/FPS/BudgetResourceLevel/EditPurchase?WorkGroupName=' + encodeURIComponent(currentWorkgroup) + '&account=' + encodeURIComponent(currentAccount) + '&itemDescription=' + encodeURIComponent(itemDescription));
}

function deletePurchase(btn) {
    var itemDescription = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this purchase?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/FPS/BudgetResourceLevel/DeletePurchase?WorkGroupName=' + encodeURIComponent(currentWorkgroup) + '&account=' + encodeURIComponent(currentAccount) + '&itemDescription=' + encodeURIComponent(itemDescription),
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage(response.message, AlertType.SUCCESS).then(function () {
                        var mgr = getPurchasesGridManager(); if (mgr) mgr.reloadGrid({ page: 1 });
                    });
                } else {
                    showAlertMessage(response.message || 'Failed to delete purchase.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting.', AlertType.ERROR);
            }
        });
    });
}

function savePurchase() {
    var form = $('#formAddPurchase');
    submitFormAsJson(form, '/FPS/BudgetResourceLevel/CreatePurchase', function (response) {
        if (response.success) {
            closeModal();
            showSuccess(response.message);
            var mgr = getPurchasesGridManager(); if (mgr) mgr.reloadGrid({ page: 1 });
        } else {
            showModalErrors(response.errors, response.message);
        }
    });
}

function updatePurchase() {
    var form = $('#formEditPurchase');
    submitFormAsJson(form, '/FPS/BudgetResourceLevel/EditPurchase', function (response) {
        if (response.success) {
            closeModal();
            showSuccess(response.message);
            var mgr = getPurchasesGridManager(); if (mgr) mgr.reloadGrid({ page: 1 });
        } else {
            showModalErrors(response.errors, response.message);
        }
    });
}

// ─── WorkGroup action buttons ────────────────────────────────────────────────
function selectByAccount() {
    showAlertMessage('Select by Account feature coming soon.', AlertType.ERROR);
}

function viewReport() {
    showAlertMessage('View Report feature coming soon.', AlertType.ERROR);
}

function sendToExcel() {
    if (!currentProfitCentre) { showAlertMessage('Please select a Resource Centre first.', AlertType.INFO); return; }
    window.location.href = '/FPS/BudgetResourceLevel/ExportToExcel?profitCentre=' + encodeURIComponent(currentProfitCentre) + '&year=' + currentYear;
}

// ─── Submit form fields as JSON ──────────────────────────────────────────────
function submitFormAsJson(form, url, successCallback) {
    clearValidationErrors('#modalContent');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modalContent');
        return;
    }

    var data = {};
    form.serializeArray().forEach(function (item) {
        data[item.name] = item.value;
    });

    $.ajax({
        url: url,
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: successCallback,
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modalContent');
            } else {
                showAlertMessage('An error occurred while saving.', AlertType.ERROR);
            }
        }
    });
}

// ─── Close modal on backdrop click ──────────────────────────────────────────
$(document).on('click', '#modalContainer', function (e) {
    if ($(e.target).is('#modalContainer')) { closeModal(); }
});

// ─── Modal open / close ──────────────────────────────────────────────────────
function openModal(url) {
    $.get(url, function (html) {
        $('#modalContent').html(html);
        $('#modalContainer').css('display', 'flex');
    }).fail(function () {
        showAlertMessage('An error occurred while loading the form.', AlertType.ERROR);
    });
}

function closeModal() {
    $('#modalContent').html('');
    $('#modalContainer').hide();
}

function showModalErrors(errors, message) {
    displayServerValidationErrors(errors, message || null, '#modalContent');
}

function showSuccess(message) {
    showAlertMessage(message, AlertType.SUCCESS);
}

function showError(message) {
    showAlertMessage(message, AlertType.ERROR);
}

// ─── Total calculation helpers ────────────────────────────────────────────────
function parseGovukAmount(text) {
    return parseFloat((text || '0').replace(/[£,\s]/g, '')) || 0;
}

function recalcTotalBid() {
    var total = 0;
    $('#gridContainer_budgetBidsGrid table tbody tr').each(function () {
        var cell = $(this).find('td[data-property="GenBid"]');
        if (cell.length) total += parseGovukAmount(cell.text().trim());
    });
    $('#fpsBrlTotalBid').val(total.toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
}

function recalcTotalPurchases() {
    var total = 0;
    $('#gridContainer_purchasesGrid table tbody tr').each(function () {
        var cell = $(this).find('td[data-property="Amount"]');
        if (cell.length) total += parseGovukAmount(cell.text().trim());
    });
    $('#fpsBrlTotalPurchases').val(total.toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
}

// ─── DOM-ready: observers and grid height constraint ─────────────────────────
$(document).ready(function () {
    var bidObserver = new MutationObserver(function () { recalcTotalBid(); });
    var budgetBidsNode = document.getElementById('gridContainer_budgetBidsGrid');
    if (budgetBidsNode) bidObserver.observe(budgetBidsNode, { childList: true, subtree: true });

    var purchaseObserver = new MutationObserver(function () { recalcTotalPurchases(); });
    var purchasesNode = document.getElementById('gridContainer_purchasesGrid');
    if (purchasesNode) purchaseObserver.observe(purchasesNode, { childList: true, subtree: true });

    function applyWorkgroupGridHeight() {
        var wgScroll = document.querySelector('#gridContainer_workGroupGrid .grid-scroll-container');
        if (wgScroll) {
            wgScroll.style.height = '300px';
            wgScroll.style.maxHeight = '300px';
        }
        var wgContainer = document.querySelector('#gridContainer_workGroupGrid .editable-grid-container');
        if (wgContainer) {
            wgContainer.style.minHeight = 'unset';
        }
    }
    applyWorkgroupGridHeight();

    var wgNode = document.getElementById('gridContainer_workGroupGrid');
    if (wgNode) {
        var wgObserver = new MutationObserver(applyWorkgroupGridHeight);
        wgObserver.observe(wgNode, { childList: true, subtree: false });
    }
});
