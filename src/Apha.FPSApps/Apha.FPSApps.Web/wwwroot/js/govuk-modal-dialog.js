(function () {
    if (window.showAlertMessage && window.showGovukConfirm) {
        return;
    }

    var AlertType = Object.freeze({
        ERROR: "E",
        INFO: "I",
        SUCCESS: "S",
        WARNING: "W"
    });

    window.AlertType = AlertType;

    var pending = Promise.resolve();

    function getFocusable(container) {
        return Array.prototype.slice.call(
            container.querySelectorAll(
                'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
            )
        ).filter(function (el) {
            return !el.hasAttribute("disabled") && el.getAttribute("aria-hidden") !== "true";
        });
    }

    function buildDialog(options) {
        var titleId = "govuk-dialog-title-" + Date.now() + "-" + Math.random().toString(16).slice(2);
        var descId = "govuk-dialog-desc-" + Date.now() + "-" + Math.random().toString(16).slice(2);
        var modalclassName = "";
        var modalTextHeader = "";

        switch (options.type) {
            case "confirm":
                modalclassName = "isInfo";
                modalTextHeader = "Please confirm";
                break;
            case AlertType.ERROR:
                modalclassName = "isDanger";
                modalTextHeader = "Error";
                break;
            case AlertType.INFO:
                modalclassName = "isInfo";
                modalTextHeader = "Information";
                break;
            case AlertType.SUCCESS:
                modalclassName = "isSuccess";
                modalTextHeader = "Message";
                break;
            case AlertType.WARNING:
                modalclassName = "isWarning";
                modalTextHeader = "Warning";
                break;
        }

        var backdrop = document.createElement("div");
        backdrop.setAttribute("data-govuk-modal", "backdrop");
        backdrop.style.position = "fixed";
        backdrop.style.inset = "0";
        backdrop.style.zIndex = "2000";
        backdrop.style.display = "flex";
        backdrop.style.alignItems = "center";
        backdrop.style.justifyContent = "center";
        backdrop.style.backgroundColor = "rgba(11, 12, 12, 0.6)";
        backdrop.style.padding = "20px";

        var dialog = document.createElement("div");
        dialog.setAttribute("role", "dialog");
        dialog.setAttribute("aria-modal", "true");
        dialog.setAttribute("aria-labelledby", titleId);
        dialog.setAttribute("aria-describedby", descId);
        dialog.setAttribute("tabindex", "-1");
        dialog.className = "govuk-body " + modalclassName + "";
        dialog.style.backgroundColor = "#ffffff";
        dialog.style.maxWidth = "620px";
        dialog.style.width = "100%";
        dialog.style.maxHeight = "calc(100vh - 40px)";
        dialog.style.overflowY = "auto";
        dialog.style.padding = "30px";
        dialog.style.boxShadow = "0 8px 24px rgba(11, 12, 12, 0.3)";
        dialog.style.border = "2px solid #0b0c0c";

        var heading = document.createElement("h2");
        heading.id = titleId;
        heading.className = "govuk-heading-m govuk-!-margin-bottom-3";
        heading.textContent = options.title || modalTextHeader;

        var message = document.createElement("p");
        message.id = descId;
        message.className = "govuk-body govuk-!-margin-bottom-5";
        message.textContent = String(options.message || "");

        var buttonGroup = document.createElement("div");
        buttonGroup.className = "govuk-button-group govuk-!-margin-bottom-0";
        buttonGroup.style.display = "flex";
        buttonGroup.style.justifyContent = "flex-end";
        buttonGroup.style.width = "100%";

        var okButton = document.createElement("button");
        okButton.type = "button";
        okButton.className = "govuk-button";
        okButton.setAttribute("data-module", "govuk-button");
        okButton.textContent = options.okText || "OK";

        buttonGroup.appendChild(okButton);

        var cancelButton = null;
        if (options.type === "confirm") {
            cancelButton = document.createElement("button");
            cancelButton.type = "button";
            cancelButton.className = "govuk-button govuk-button--secondary";
            cancelButton.setAttribute("data-module", "govuk-button");
            cancelButton.textContent = options.cancelText || "Cancel";
            buttonGroup.appendChild(cancelButton);
        }

        dialog.appendChild(heading);
        dialog.appendChild(message);
        dialog.appendChild(buttonGroup);
        backdrop.appendChild(dialog);

        return {
            backdrop: backdrop,
            dialog: dialog,
            okButton: okButton,
            cancelButton: cancelButton
        };
    }

    function openDialog(options) {
        return new Promise(function (resolve) {
            var previousActive = document.activeElement;
            var previousOverflow = document.body.style.overflow;
            var closed = false;
            var parts = buildDialog(options);

            function cleanup(result) {
                if (closed) {
                    return;
                }
                closed = true;

                document.removeEventListener("keydown", onKeyDown, true);
                if (parts.backdrop.parentNode) {
                    parts.backdrop.parentNode.removeChild(parts.backdrop);
                }

                document.body.style.overflow = previousOverflow;

                if (previousActive && typeof previousActive.focus === "function") {
                    previousActive.focus();
                }

                resolve(result);
            }

            function onKeyDown(event) {
                if (event.key === "Escape") {
                    event.preventDefault();
                    cleanup(options.type === "confirm" ? false : true);
                    return;
                }

                if (event.key === "Tab") {
                    var focusable = getFocusable(parts.dialog);
                    if (!focusable.length) {
                        event.preventDefault();
                        parts.dialog.focus();
                        return;
                    }

                    var first = focusable[0];
                    var last = focusable[focusable.length - 1];

                    if (event.shiftKey && document.activeElement === first) {
                        event.preventDefault();
                        last.focus();
                    } else if (!event.shiftKey && document.activeElement === last) {
                        event.preventDefault();
                        first.focus();
                    }
                }
            }

            parts.okButton.addEventListener("click", function () {
                cleanup(true);
            });

            if (parts.cancelButton) {
                parts.cancelButton.addEventListener("click", function () {
                    cleanup(false);
                });
            }

            document.body.appendChild(parts.backdrop);
            document.body.style.overflow = "hidden";

            document.addEventListener("keydown", onKeyDown, true);
            parts.okButton.focus();
        });
    }

    function stopPropagation(event) {
        event.stopPropagation();
    }

    // Isolates clicks inside the modal from the rest of the page, but lets
    // events on the header through so the drag handlers delegated on the
    // document can still receive them. Without this exception, any modal
    // container carrying the "modal" class (e.g. the CostBook YearlyDetails
    // #project1ModalContainer) becomes undraggable.
    function stopModalContentPropagation(event) {
        if (event.target.closest(".modal-header, .govuk-edit-modal__header")) {
            return;
        }

        stopPropagation(event);
    }

    function applySafeBootstrapConfig(modalElement) {
        if (!modalElement) {
            return;
        }

        modalElement.setAttribute("data-bs-backdrop", "static");
        modalElement.setAttribute("data-bs-keyboard", "false");

        if (window.bootstrap && window.bootstrap.Modal) {
            var instance = window.bootstrap.Modal.getInstance(modalElement);
            if (instance && instance._config) {
                instance._config.backdrop = "static";
                instance._config.keyboard = false;
            }
        }
    }

    function initializeSafeModal(modalOrId) {
        var modalElement = null;

        if (typeof modalOrId === "string") {
            var id = modalOrId.charAt(0) === "#" ? modalOrId.slice(1) : modalOrId;
            modalElement = document.getElementById(id);
        } else {
            modalElement = modalOrId;
        }

        if (!modalElement || !modalElement.classList || !modalElement.classList.contains("modal")) {
            return;
        }

        if (modalElement.dataset.safeModalInit === "true") {
            applySafeBootstrapConfig(modalElement);
            return;
        }

        modalElement.dataset.safeModalInit = "true";
        applySafeBootstrapConfig(modalElement);

        modalElement.addEventListener("show.bs.modal", function () {
            applySafeBootstrapConfig(modalElement);
        });

        var modalContent = modalElement.querySelector(".modal-content");
        if (modalContent) {
            ["click", "mousedown", "mouseup"].forEach(function (eventName) {
                modalContent.addEventListener(eventName, stopModalContentPropagation);
            });
        }

        var fields = modalElement.querySelectorAll("input, textarea, select, button");
        fields.forEach(function (field) {
            ["click", "keydown"].forEach(function (eventName) {
                field.addEventListener(eventName, stopPropagation);
            });
        });
    }

    function initializeAllSafeModals() {
        var modals = document.querySelectorAll(".modal");
        modals.forEach(function (modalElement) {
            initializeSafeModal(modalElement);
        });
    }

    function watchDynamicModals() {
        if (!window.MutationObserver || !document.body) {
            return;
        }

        var observer = new MutationObserver(function (mutations) {
            var hasChanges = mutations.some(function (mutation) {
                return mutation.type === "childList" && (mutation.addedNodes && mutation.addedNodes.length > 0);
            });

            if (hasChanges) {
                initializeAllSafeModals();
            }
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", function () {
            initializeAllSafeModals();
            watchDynamicModals();
        });
    } else {
        initializeAllSafeModals();
        watchDynamicModals();
    }

    window.initializeSafeModal = initializeSafeModal;

    window.showAlertMessage = function (message, type) {

        var validTypes = Object.values(AlertType);
        var resolvedType = type || AlertType.INFO;

        if (validTypes.indexOf(resolvedType) === -1) {
            resolvedType = AlertType.INFO;
        }
        pending = pending.then(function () {
            return openDialog({
                type: resolvedType,
                message: message,
                okText: "OK"
            });
        });

        return pending.then(function () {
            return undefined;
        });
    };

    window.showGovukConfirm = function (message) {
        pending = pending.then(function () {
            return openDialog({
                type: "confirm",
                message: message,
                okText: "OK",
                cancelText: "Cancel"
            });
        });

        return pending.then(function (result) {
            return result;
        });
    };

    window.showLoader = function () {
        $("#loader").show();
        // document.getElementById("loader").style.display = "block";
    }

    window.hideLoader = function () {
        $("#loader").hide();
        //document.getElementById("loader").style.display = "none";
    }

    // Centralised loader handling for the native fetch API.
    // Wraps window.fetch so every fetch call automatically shows the global
    // loader while the request is in flight and hides it once the response is
    // received (success or error). A counter keeps the loader visible while
    // multiple concurrent requests are pending. Individual pages can opt out by
    // passing { skipLoader: true } in the fetch init options.
    if (window.fetch && !window.fetch.__loaderWrapped) {
        var nativeFetch = window.fetch.bind(window);
        var pendingRequests = 0;

        var wrappedFetch = function (input, init) {
            if (init && init.skipLoader) {
                delete init.skipLoader;
                return nativeFetch(input, init);
            }

            pendingRequests++;
            showLoader();

            var done = function () {
                pendingRequests = Math.max(0, pendingRequests - 1);
                if (pendingRequests === 0) {
                    hideLoader();
                }
            };

            return nativeFetch(input, init).then(
                function (response) { done(); return response; },
                function (error) { done(); throw error; }
            );
        };

        wrappedFetch.__loaderWrapped = true;
        window.fetch = wrappedFetch;
    }

    // Centralised loader handling for jQuery AJAX requests.
    // Binds the global loader to jQuery's ajaxStart/ajaxStop so every $.ajax
    // call shows the loader while in flight and hides it once all requests
    // complete. Guarded so it only binds once and only when jQuery is present.
    if (window.jQuery && !window.jQuery.__loaderBound) {
        window.jQuery(function () {
            window.jQuery(document).ajaxStart(showLoader).ajaxStop(hideLoader);
        });
        window.jQuery.__loaderBound = true;
    }

    // Centralised loader handling for full-page navigation.
    // Shows the global loader whenever the user navigates to another page via a
    // standard link click or a form submission, giving visual feedback while the
    // next page loads. Elements can opt out with the data-no-loader attribute
    // (e.g. downloads, new-tab links, or in-page anchors). The loader is hidden
    // again if the user returns via the browser back/forward cache (pageshow).
    if (!window.__navigationLoaderBound) {
        window.__navigationLoaderBound = true;

        var shouldSkipNavigationLoader = function (el) {
            return !el || el.hasAttribute("data-no-loader") || el.closest("[data-no-loader]") !== null;
        };

        document.addEventListener("click", function (event) {
            if (event.defaultPrevented || event.button !== 0 ||
                event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
                return;
            }

            var link = event.target.closest ? event.target.closest("a[href]") : null;
            if (!link || shouldSkipNavigationLoader(link)) {
                return;
            }

            var href = link.getAttribute("href");
            if (!href || href.charAt(0) === "#" ||
                href.indexOf("javascript:") === 0 ||
                link.getAttribute("target") === "_blank" ||
                link.hasAttribute("download")) {
                return;
            }

            showLoader();
        }, true);

        document.addEventListener("submit", function (event) {
            var form = event.target;
            if (event.defaultPrevented || !form || shouldSkipNavigationLoader(form)) {
                return;
            }

            if (form.getAttribute("target") === "_blank") {
                return;
            }

            showLoader();
        }, true);

        // Hide the loader when the page is restored from the back/forward cache.
        window.addEventListener("pageshow", function (event) {
            if (event.persisted) {
                hideLoader();
            }
        });
    }

    // Downloads a file from the given URL, showing the global loader until the
    // download completes. Reusable for any Excel/PDF/CSV export endpoint.
    window.downloadFile = function (url, fileName) {
        showLoader();
        return fetch(url)
            .then(function (r) { return r.blob(); })
            .then(function (blob) {
                const link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = fileName || 'download';
                link.click();
                window.URL.revokeObjectURL(link.href);
            })
            .finally(hideLoader);
    }


    window.showGovukYesNo = function (message) {
        pending = pending.then(function () {
            return openDialog({
                type: "confirm",
                message: message,
                okText: "Yes",
                cancelText: "No"
            });
        });

        return pending.then(function (result) {
            return result;
        });
    };

    window.showGovukApproveReject = function (message) {
        pending = pending.then(function () {
            return openDialog({
                type: "confirm",
                message: message,
                okText: "Approve",
                cancelText: "Reject"
            });
        });

        return pending.then(function (result) {
            return result;
        });
    };

})();


// =============================================================
// DRAGGABLE MODAL DIALOGS
// =============================================================
//
// NOTE: this deliberately lives in its OWN IIFE.
//
// It used to sit inside the alert/confirm IIFE above, which
// begins with:
//
//     if (window.showAlertMessage && window.showGovukConfirm) {
//         return;
//     }
//
// Once those globals exist (they are assigned by that same
// IIFE), any later evaluation of this file returned early and
// the drag handlers below were NEVER registered. Alerts kept
// working because they were already on `window`, which made it
// look like "only dragging is broken".
//
// =============================================================

(function () {

    if (window.__govukModalDragInitialised) {
        return;
    }
    window.__govukModalDragInitialised = true;

    var HEADER_SELECTOR =
        ".modal-dialog .modal-header, " +
        ".govuk-edit-modal-dialog .govuk-edit-modal__header";

    var DIALOG_SELECTOR = ".modal-dialog, .govuk-edit-modal-dialog";

    // Minimum part of modal that must remain visible
    var minVisible = 50;

    var isDragging = false;

    // The dialog captured on mousedown. Previously mousemove
    // re-queried the DOM and took .first(), which could pick the
    // shared #modalPopup dialog rendered by _Layout.cshtml instead
    // of the page's own modal.
    var $activeDialog = null;

    var startX = 0;
    var startY = 0;
    var startLeft = 0;
    var startTop = 0;

    // Inline styles lose to rules like `max-width: 500px !important`
    // on the dialog and to `.modal.show { display:flex; align-items:center }`
    // in main_style.css, which keeps re-centring the dialog. Setting the
    // positional properties with priority is the only reliable way to win.
    function setImportant(el, prop, value) {
        el.style.setProperty(prop, value, "important");
    }

    function clearProps(el, props) {
        props.forEach(function (prop) {
            el.style.removeProperty(prop);
        });
    }

    function beginDrag($dialog) {
        var el = $dialog[0];

        $dialog.addClass("is-dragging");

        // Freeze the current size so the flex parent cannot resize it.
        setImportant(el, "width", $dialog.outerWidth() + "px");
        setImportant(el, "max-width", "none");
        setImportant(el, "position", "fixed");
        setImportant(el, "margin", "0");

        // Stop the flex container re-centring the dialog mid-drag.
        $dialog.closest(".modal").addClass("is-dragging-host");
    }

    function moveDialog($dialog, left, top) {
        var el = $dialog[0];
        setImportant(el, "left", left + "px");
        setImportant(el, "top", top + "px");
    }


    // =========================================================
    // DRAG START
    // =========================================================

    $(document).on("mousedown", HEADER_SELECTOR, function (e) {

        // Do not drag when clicking buttons/links
        if ($(e.target).closest("button, a, input, select, textarea").length) {
            return;
        }

        var $dialog = $(this).closest(DIALOG_SELECTOR);

        if (!$dialog.length) {
            return;
        }

        var offset = $dialog.offset();

        isDragging = true;
        $activeDialog = $dialog;

        startX = e.clientX;
        startY = e.clientY;

        startLeft = offset.left;
        startTop = offset.top;

        beginDrag($dialog);
        moveDialog($dialog, startLeft, startTop);

        $("body").css("user-select", "none");

        e.preventDefault();
    });


    // =========================================================
    // DRAGGING
    // =========================================================

    $(document).on("mousemove", function (e) {

        if (!isDragging || !$activeDialog || !$activeDialog.length) {
            return;
        }

        var $dialog = $activeDialog;

        var dialogWidth = $dialog.outerWidth();
        var dialogHeight = $dialog.outerHeight();

        var viewportWidth = $(window).width();
        var viewportHeight = $(window).height();

        var newLeft = startLeft + (e.clientX - startX);
        var newTop = startTop + (e.clientY - startY);


        // =====================================================
        // X LIMIT
        // =====================================================

        var minLeft = -(dialogWidth - minVisible);
        var maxLeft = viewportWidth - minVisible;

        newLeft = Math.max(
            minLeft,
            Math.min(newLeft, maxLeft)
        );


        // =====================================================
        // Y LIMIT
        // =====================================================

        var minTop = -(dialogHeight - minVisible);
        var maxTop = viewportHeight - minVisible;

        newTop = Math.max(
            minTop,
            Math.min(newTop, maxTop)
        );


        moveDialog($dialog, newLeft, newTop);
    });


    // =========================================================
    // DRAG END
    // =========================================================

    $(document).on("mouseup", function () {

        if (!isDragging) {
            return;
        }

        isDragging = false;

        $("body").css("user-select", "");
    });


    // =========================================================
    // CLICK FADED BACKGROUND
    // =========================================================
    //
    // This will NOT automatically reset the modal.
    //
    // Modal resets ONLY when the user clicks the faded
    // background outside the modal.
    //
    // =========================================================

    $(document).on("click", function (e) {

        if (!$activeDialog || !$activeDialog.length) {
            return;
        }

        // If click happened INSIDE modal, do nothing
        if ($(e.target).closest(DIALOG_SELECTOR).length) {
            return;
        }

        // Click happened outside modal = faded background
        resetModalPosition($activeDialog);
        $activeDialog = null;
    });


    // =========================================================
    // RESET MODAL TO ORIGINAL POSITION
    // =========================================================

    function resetModalPosition($dialog) {

        if (!$dialog || !$dialog.length) {
            return;
        }

        var el = $dialog[0];

        clearProps(el, [
            "position",
            "left",
            "top",
            "margin",
            "width",
            "max-width"
        ]);

        $dialog.removeClass("is-dragging");
        $dialog.closest(".modal").removeClass("is-dragging-host");
    }

    window.resetGovukModalPosition = resetModalPosition;

})();
