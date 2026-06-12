const popupOverlay = document.getElementById('govuk-popup-overlay');
const popupContainer = document.getElementById('govuk-popup');

/* ====================================
   CLOSE
==================================== */

function closePopup() {
    if (popupOverlay) {
        popupOverlay.classList.remove('active');
    }
    if (popupContainer) {
        popupContainer.classList.remove('active');
        popupContainer.innerHTML = '';
    }
}

/* ====================================
   SUCCESS POPUP
==================================== */

function showSuccessPopup() {
    popupContainer.innerHTML = `
        <div class="govuk-notification-banner govuk-notification-banner--success" role="alert">
            <div class="govuk-notification-banner__header">
                <h2 class="govuk-notification-banner__title">Success</h2>
            </div>
            <div class="govuk-notification-banner__content">
                <h3 class="govuk-notification-banner__heading">Data saved successfully</h3>
                <p class="govuk-body">Your changes have been updated.</p>
                <div class="govuk-button-group">
                    <button class="govuk-button" onclick="closePopup()">OK</button>
                </div>
            </div>
        </div>
    `;

    if (popupOverlay) {
        popupOverlay.classList.add('active');
    }
    if (popupContainer) {
        popupContainer.classList.add('active');
    }
}

/* ====================================
   CONFIRM POPUP
==================================== */

function showConfirmPopup() {
    popupContainer.innerHTML = `
        <div class="govuk-notification-banner" role="region">
            <div class="govuk-notification-banner__header">
                <h2 class="govuk-notification-banner__title">Important</h2>
            </div>
            <div class="govuk-notification-banner__content">
                <h3 class="govuk-notification-banner__heading">Are you sure you want to continue?</h3>
                <p class="govuk-body">Please confirm your action.</p>
                <div class="govuk-button-group">
                    <button class="govuk-button" onclick="confirmAction()">Confirm</button>
                    <button class="govuk-button govuk-button--secondary" onclick="closePopup()">Cancel</button>
                </div>
            </div>
        </div>
    `;

    if (popupOverlay) {
        popupOverlay.classList.add('active');
    }
    if (popupContainer) {
        popupContainer.classList.add('active');
    }
}

/* ====================================
   DELETE POPUP
==================================== */

function showDeletePopup() {
    popupContainer.innerHTML = `
        <div class="delete-popup">
            <div class="govuk-notification-banner" role="alert">
                <div class="govuk-notification-banner__header">
                    <h2 class="govuk-notification-banner__title">Delete Record</h2>
                </div>
                <div class="govuk-notification-banner__content">
                    <h3 class="govuk-notification-banner__heading">Are you sure you want to delete this record?</h3>
                    <p class="govuk-body">This action cannot be undone.</p>
                    <div class="govuk-button-group">
                        <button class="govuk-button govuk-button--secondary" onclick="closePopup()">Cancel</button>
                        <button class="govuk-button govuk-button--warning" onclick="deleteRecord()">Yes Delete</button>
                    </div>
                </div>
            </div>
        </div>
    `;

    if (popupOverlay) {
        popupOverlay.classList.add('active');
    }
    if (popupContainer) {
        popupContainer.classList.add('active');
    }
}

function showChangeLayoutPopup() {
    popupContainer.innerHTML = `
        <div class="delete-popup">
            <div class="govuk-notification-banner" role="alert">
                <div class="govuk-notification-banner__header">
                    <h2 class="govuk-notification-banner__title">Change Layout</h2>
                </div>
                <div class="govuk-notification-banner__content">
                    <h3 class="govuk-notification-banner__heading">Are you sure you want to Change Layout?</h3>
                    <p class="govuk-body">This action cannot be undone.</p>
                    <div class="govuk-button-group">
                        <button class="govuk-button govuk-button--secondary" onclick="closePopup()">Cancel</button>
                        <button class="govuk-button govuk-button--warning" onclick="deleteRecord()">Yes Delete</button>
                    </div>
                </div>
            </div>
        </div>
    `;

    if (popupOverlay) {
        popupOverlay.classList.add('active');
    }
    if (popupContainer) {
        popupContainer.classList.add('active');
    }
}

/* ====================================
   CALLBACKS
==================================== */

function confirmAction() {
    closePopup();
    // alert('Confirmed Successfully');
}

function deleteRecord() {
    closePopup();
    // alert('Record Deleted Successfully');
    showToast('delete');
}

function showChangeLayoutPopup(){
    closePopup();
}