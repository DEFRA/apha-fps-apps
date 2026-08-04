/* ===================================================
    Year End Initiation – page logic
=================================================== */
(function () {
  "use strict";

  /* --------------------------------------------------
         Config Value table – Edit / Confirm
     -------------------------------------------------- */
  var activeConfigKey = null;

  function openConfigModal(configKey, label) {
    activeConfigKey = configKey;
    var currentVal = document.getElementById("cfg-val-" + configKey);
    var val = currentVal ? currentVal.textContent.trim() : "";
    document.getElementById("configModalLabel").textContent = label;

    var input = document.getElementById("configModalInput");
    var select = document.getElementById("configModalSelect");
    var isYesNo = val === "Yes" || val === "No";

    if (isYesNo) {
      input.style.display = "none";
      select.style.display = "";
      select.value = val;
      document
        .getElementById("configModalLabel")
        .setAttribute("for", "configModalSelect");
    } else {
      input.style.display = "";
      select.style.display = "none";
      input.value = val;
      document
        .getElementById("configModalLabel")
        .setAttribute("for", "configModalInput");
    }

    var modal = document.getElementById("configEditModal");
    modal.classList.add("active");
    modal.setAttribute("aria-hidden", "false");
    document.body.style.overflow = "hidden";
    (isYesNo ? select : input).focus();
  }

  function closeConfigModal() {
    var modal = document.getElementById("configEditModal");
    modal.classList.remove("active");
    modal.setAttribute("aria-hidden", "true");
    document.body.style.overflow = "";
    activeConfigKey = null;
  }

  function confirmConfigEdit() {
    if (!activeConfigKey) return;
    var select = document.getElementById("configModalSelect");
    var input = document.getElementById("configModalInput");
    var newVal =
      select.style.display !== "none" ? select.value : input.value.trim();
    var valCell = document.getElementById("cfg-val-" + activeConfigKey);
    if (valCell) valCell.textContent = newVal;

    var actionCell = document.getElementById("cfg-action-" + activeConfigKey);
    if (actionCell) {
      var confirmBtn = actionCell.querySelector(".cfg-confirm-link");
      if (confirmBtn) {
        confirmBtn.disabled = true;
        confirmBtn.setAttribute("aria-disabled", "true");
        confirmBtn.style.opacity = "0.4";
      }
    }

    closeConfigModal();
  }

  /* --------------------------------------------------
       Month Working Days grid
    -------------------------------------------------- */
  var monthWorkingDaysList = [];
  var monthWorkingDaysGrid = null;
  var confirmedMonthRows = {}; // rowId -> true once confirmed

  function loadMonthWorkingDaysData() {
    return fetch("../js/fps_js/data/month_working_days.json")
      .then(function (res) {
        if (!res.ok) throw new Error("Failed to load month working days data");
        return res.json();
      })
      .then(function (data) {
        monthWorkingDaysList = Array.isArray(data.monthWorkingDaysList)
          ? data.monthWorkingDaysList
          : [];
      })
      .catch(function (err) {
        console.error(err);
        monthWorkingDaysList = [];
      });
  }

  function renderMonthActionCell(row) {
    if (row.fMonth === 0) {
      return "";
    }
    return (
      '<button type="button" class="govuk-button govuk-button--secondary sup_margin_0 sup_p_8 mwd-edit-link" data-row-id="' +
      row.id +
      '" aria-label="Edit row ' +
      row.id +
      '">' +
      '<img src="../images/pen-to-square-regular-full.svg" alt="" aria-hidden="true" width="20" height="20">' +
      "</button>" +
      '<button type="button" class="govuk-button sup_margin_0 sup_p_8 mwd-confirm-link" data-row-id="' +
      row.id +
      '" aria-label="Confirm row ' +
      row.id +
      '">' +
      '<img src="../images/circle-check-regular-full.svg" alt="" aria-hidden="true" width="20" height="20" style="filter: brightness(0) saturate(100%) invert(26%) sepia(89%) saturate(1026%) hue-rotate(345deg) brightness(90%) contrast(102%);">' +
      "</button>"
    );
  }

  function initMonthWorkingDaysGrid() {
    var columns = [
      { field: "year", header: "Year", sortable: true, width: 80 },
      { field: "month", header: "Month", sortable: true, width: 70 },
      { field: "days", header: "Days", sortable: true, width: 70 },
      { field: "cvlHours", header: "CVLHours", sortable: true, width: 100 },
      { field: "vidHours", header: "VIDHours", sortable: true, width: 100 },
      { field: "fMonth", header: "FMonth", sortable: true, width: 80 },
      {
        field: "action",
        header: "Action",
        sortable: false,
        width: 140,
        render: function (val, row) {
          return renderMonthActionCell(row);
        },
      },
    ];

    monthWorkingDaysGrid = new DataGridComponent({
      gridId: "monthWorkingDaysGrid",
      containerSelector: "#gridContainer_monthWorkingDays",
      title: "Month Working Days, VID Hours and CVL Hours Details",
      columns: columns,
      data: monthWorkingDaysList,
      containerMinHeight: "550px",
      scrollContainerMaxHeight: "550px",
      scrollContainerHeight: "550px",
      pageSize: 25,
      enableSort: true,
      enableFilter: false,
      enableResize: true,
      enableSelection: false,
      enablePagination: false,
      showAddButton: false,
      pageSizeOptions: [10, 15, 20, 25, 30],
    });

    attachMonthGridEventHandlers();
  }

  function attachMonthGridEventHandlers() {
    var container = document.querySelector("#gridContainer_monthWorkingDays");
    if (!container || container.dataset.mwdBound === "true") return;

    container.addEventListener("click", function (e) {
      var editLink = e.target.closest(".mwd-edit-link");
      var confirmLink = e.target.closest(".mwd-confirm-link");

      if (editLink) {
        e.preventDefault();
        openMonthRowModal(editLink.dataset.rowId);
      }
      if (confirmLink) {
        e.preventDefault();
        confirmMonthRow(confirmLink.dataset.rowId);
      }
    });

    container.dataset.mwdBound = "true";
  }

  function openMonthRowModal(rowId) {
    var row = monthWorkingDaysList.find(function (r) {
      return String(r.id) === String(rowId);
    });
    if (!row) return;
    document.getElementById("monthModalRowId").value = rowId;
    document.getElementById("monthModalDays").value = row.days;
    document.getElementById("monthModalCvlHours").value = row.cvlHours;
    document.getElementById("monthModalVidHours").value = row.vidHours;
    var modal = document.getElementById("monthRowEditModal");
    modal.classList.add("active");
    modal.setAttribute("aria-hidden", "false");
    document.body.style.overflow = "hidden";
    document.getElementById("monthModalDays").focus();
  }

  function closeMonthRowModal() {
    var modal = document.getElementById("monthRowEditModal");
    modal.classList.remove("active");
    modal.setAttribute("aria-hidden", "true");
    document.body.style.overflow = "";
  }

  function saveMonthRowModal() {
    var rowId = document.getElementById("monthModalRowId").value;
    var row = monthWorkingDaysList.find(function (r) {
      return String(r.id) === String(rowId);
    });
    if (!row) return;
    row.days =
      parseFloat(document.getElementById("monthModalDays").value) || row.days;
    row.cvlHours =
      parseFloat(document.getElementById("monthModalCvlHours").value) ||
      row.cvlHours;
    row.vidHours =
      parseFloat(document.getElementById("monthModalVidHours").value) ||
      row.vidHours;
    monthWorkingDaysGrid.updateData(monthWorkingDaysList);
    attachMonthGridEventHandlers(); // re-attach after re-render
    closeMonthRowModal();
  }

  function confirmMonthRow(rowId) {
    var container = document.querySelector("#gridContainer_monthWorkingDays");
    if (!container) return;
    var row = container.querySelector('tr[data-id="' + rowId + '"]');
    if (!row) return;
    var confirmBtn = row.querySelector(".mwd-confirm-link");
    if (confirmBtn) {
      confirmBtn.disabled = true;
      confirmBtn.style.opacity = "0.4";
      confirmBtn.setAttribute("aria-disabled", "true");
    }
    confirmedMonthRows[rowId] = true;
  }

  /* --------------------------------------------------
       DOMContentLoaded – wire everything up
    -------------------------------------------------- */
  document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".cfg-edit-link").forEach(function (link) {
      link.addEventListener("click", function (e) {
        e.preventDefault();
        openConfigModal(this.dataset.config, this.dataset.label);
      });
    });

    document.querySelectorAll(".cfg-confirm-link").forEach(function (link) {
      link.addEventListener("click", function (e) {
        e.preventDefault();
        var configKey = this.dataset.config;
        var actionCell = document.getElementById("cfg-action-" + configKey);
        if (actionCell) {
          var confirmBtn = actionCell.querySelector(".cfg-confirm-link");
          if (confirmBtn) {
            confirmBtn.disabled = true;
            confirmBtn.style.opacity = "0.4";
            confirmBtn.setAttribute("aria-disabled", "true");
          }
        }
      });
    });

    document
      .getElementById("configModalSave")
      .addEventListener("click", confirmConfigEdit);
    document
      .getElementById("configModalCancel")
      .addEventListener("click", closeConfigModal);
    document
      .getElementById("configModalClose")
      .addEventListener("click", closeConfigModal);

    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape") {
        closeConfigModal();
        closeMonthRowModal();
      }
    });

    /* Month working days grid */
    loadMonthWorkingDaysData().then(function () {
      initMonthWorkingDaysGrid();
    });

    var monthModalSave = document.getElementById("monthModalSave");
    if (monthModalSave)
      monthModalSave.addEventListener("click", saveMonthRowModal);

    var monthModalCancel = document.getElementById("monthModalCancel");
    if (monthModalCancel)
      monthModalCancel.addEventListener("click", closeMonthRowModal);

    var monthModalClose = document.getElementById("monthModalClose");
    if (monthModalClose)
      monthModalClose.addEventListener("click", closeMonthRowModal);
  });
})();
