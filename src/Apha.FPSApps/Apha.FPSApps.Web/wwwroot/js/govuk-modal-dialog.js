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
        var statusText = document.getElementById("loader-status-text");
        if (statusText) {
            statusText.textContent = "";
            // Force a DOM change so screen readers announce the update even if
            // the previous message was identical.
            window.setTimeout(function () {
                statusText.textContent = "Loading page...";
            }, 50);
        }
    }

    window.hideLoader = function () {
        $("#loader").hide();
        //document.getElementById("loader").style.display = "none";
        var statusText = document.getElementById("loader-status-text");
        if (statusText) {
            statusText.textContent = "Content loaded.";
        }
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


    // =========================================================
    // DRAGGABLE MODAL DIALOGS
    // =========================================================
    //
    // Works for BOTH modal markup styles used in the app:
    //   1) .modal > .modal-dialog > .modal-content > .modal-header
    //   2) .govuk-edit-modal > .govuk-edit-modal-dialog > .govuk-edit-modal__header
    //
    // Key points:
    //  - The dragged dialog is remembered on mousedown, so pages that
    //    contain MORE THAN ONE modal in the DOM (Monthly Time,
    //    Monthly Output, Yearly Financial Data) always move the
    //    dialog the user actually grabbed.
    //  - Coordinates are taken from getBoundingClientRect(), which is
    //    viewport relative, so the page scroll position never shifts
    //    the dialog when the drag starts.
    //  - Any CSS transform / auto margins on the dialog are neutralised
    //    ONLY while dragging, then the exact original inline style is
    //    restored, so no page loses its own width / max-width styling.
    // =========================================================

    var isDragging = false;

    var $dragDialog = null;

    var startX = 0;
    var startY = 0;
    var startLeft = 0;
    var startTop = 0;

    // A transformed ancestor (e.g. PIMS `.modal-dialog` has
    // `transform: translateY(-20px)`) becomes the containing block for
    // `position: fixed` children. When that happens, `left`/`top` are NOT
    // viewport coordinates. These corrections convert a desired viewport
    // position into the value that must actually be written to left/top.
    // They are 0 when the viewport is the containing block, so pages that
    // already worked are completely unaffected.
    var correctionX = 0;
    var correctionY = 0;

    // Minimum part of modal that must remain visible
    var minVisible = 50;

    var DIALOG_SELECTOR = ".modal-dialog, .govuk-edit-modal-dialog";
    var HANDLE_SELECTOR =
        ".modal-dialog .modal-header, " +
        ".govuk-edit-modal-dialog .govuk-edit-modal__header";

    // Marker attribute so every dialog we have moved can be found again,
    // even after the page replaces modal markup.
    var DRAGGED_MARKER = "data-fps-dragged";

    // Remembers the untouched inline style of every dialog we move,
    // so the dialog can be put back exactly as the page authored it.
    function rememberOriginalStyle($dialog) {
        if (!$dialog[0].hasAttribute(DRAGGED_MARKER)) {
            $dialog[0].setAttribute(
                DRAGGED_MARKER,
                $dialog.attr("style") || ""
            );
        }
    }

    // Resets a dialog back to its authored position/size.
    function resetModalPosition($dialog) {

        if (!$dialog || !$dialog.length) {
            return;
        }

        $dialog.each(function () {

            if (!this.hasAttribute(DRAGGED_MARKER)) {
                return;
            }

            var original = this.getAttribute(DRAGGED_MARKER);

            if (original === "") {
                this.removeAttribute("style");
            } else {
                this.setAttribute("style", original);
            }

            this.removeAttribute(DRAGGED_MARKER);
        });
    }

    // Restores every dialog that has been dragged on this page.
    function resetAllDraggedDialogs() {
        resetModalPosition($("[" + DRAGGED_MARKER + "]"));
    }

    // A dialog is considered closed when neither it nor its host is visible.
    function isDialogOpen($dialog) {
        return $dialog.length > 0 && $dialog.is(":visible");
    }

    // Watches the modal host so the dialog is re-centred the next time
    // the modal is opened, without touching any other page behaviour.
    function watchForClose($dialog) {

        if ($dialog.data("fpsDragCloseWatcher")) {
            return;
        }

        // The host is the element that is shown/hidden. Most pages use
        // `.modal` / `.govuk-edit-modal`, but the PIMS layout uses a plain
        // `#modalPopup` wrapper, so fall back to the dialog's parent.
        var host =
            $dialog.closest(".modal, .govuk-edit-modal, #modalPopup")[0] ||
            $dialog.parent()[0];

        if (!host || typeof MutationObserver === "undefined") {
            return;
        }

        var observer = new MutationObserver(function () {
            if (!isDialogOpen($dialog)) {
                resetModalPosition($dialog);
            }
        });

        observer.observe(host, {
            attributes: true,
            attributeFilter: ["class", "style", "open", "hidden"]
        });

        $dialog.data("fpsDragCloseWatcher", observer);
    }


    // =========================================================
    // DRAG START
    // =========================================================

    $(document).on("mousedown", HANDLE_SELECTOR, function (e) {

        // Only respond to the primary mouse button
        if (e.which !== 1) {
            return;
        }

        // Do not drag when clicking buttons/links/fields
        if ($(e.target).closest("button, a, input, select, textarea, label").length) {
            return;
        }

        var $dialog = $(this).closest(DIALOG_SELECTOR);

        if (!$dialog.length) {
            return;
        }

        rememberOriginalStyle($dialog);
        watchForClose($dialog);

        // Viewport relative box - unaffected by page scroll position
        var rect = $dialog[0].getBoundingClientRect();

        $dragDialog = $dialog;
        isDragging = true;

        startX = e.clientX;
        startY = e.clientY;

        startLeft = rect.left;
        startTop = rect.top;

        correctionX = 0;
        correctionY = 0;

        // Pin the dialog exactly where it currently appears.
        // Transform/auto margins are cleared so left/top are authoritative.
        $dialog.css({
            position: "fixed",
            margin: 0,
            transform: "none",
            transition: "none",
            left: startLeft + "px",
            top: startTop + "px",
            width: rect.width + "px",
            maxWidth: "none",
            right: "auto",
            bottom: "auto"
        });

        // Measure again. If the dialog did not land where we asked, its
        // containing block is a transformed ancestor rather than the
        // viewport, so work out the constant offset and re-apply.
        var pinned = $dialog[0].getBoundingClientRect();

        correctionX = startLeft - pinned.left;
        correctionY = startTop - pinned.top;

        if (correctionX !== 0 || correctionY !== 0) {
            $dialog.css({
                left: (startLeft + correctionX) + "px",
                top: (startTop + correctionY) + "px"
            });
        }

        $("body").css("user-select", "none");

        e.preventDefault();
    });


    // =========================================================
    // DRAGGING
    // =========================================================

    $(document).on("mousemove", function (e) {

        if (!isDragging || !$dragDialog || !$dragDialog.length) {
            return;
        }

        var dialogWidth = $dragDialog.outerWidth();
        var dialogHeight = $dragDialog.outerHeight();

        var viewportWidth = $(window).width();
        var viewportHeight = $(window).height();

        var newLeft = startLeft + (e.clientX - startX);
        var newTop = startTop + (e.clientY - startY);

        // X LIMIT
        newLeft = Math.max(
            -(dialogWidth - minVisible),
            Math.min(newLeft, viewportWidth - minVisible)
        );

        // Y LIMIT
        newTop = Math.max(
            0,
            Math.min(newTop, viewportHeight - minVisible)
        );

        $dragDialog.css({
            left: (newLeft + correctionX) + "px",
            top: (newTop + correctionY) + "px"
        });
    });


    // =========================================================
    // DRAG END
    // =========================================================

    $(document).on("mouseup", function () {

        if (!isDragging) {
            return;
        }

        isDragging = false;
        $dragDialog = null;

        $("body").css("user-select", "");
    });

    // If the pointer leaves the window / focus is lost mid drag,
    // end the drag cleanly instead of leaving it stuck.
    $(window).on("blur", function () {

        if (!isDragging) {
            return;
        }

        isDragging = false;
        $dragDialog = null;

        $("body").css("user-select", "");
    });


    // =========================================================
    // CLICK FADED BACKGROUND
    // =========================================================
    //
    // Clicking the faded backdrop restores the dialog it belongs to
    // back to its authored (centred) position. Only dialogs that have
    // actually been dragged are touched.
    //
    // =========================================================

    $(document).on("mousedown", function (e) {

        // A drag is starting - never reset.
        if ($(e.target).closest(HANDLE_SELECTOR).length) {
            return;
        }

        // Ignore clicks inside any dialog itself
        if ($(e.target).closest(DIALOG_SELECTOR).length) {
            return;
        }

        // Click landed on the faded backdrop or anywhere else outside a
        // dialog, so put any moved dialog back where the page put it.
        resetAllDraggedDialogs();
    });

})();
