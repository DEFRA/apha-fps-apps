/* ===================================================
    Year End Cutoff – page logic
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
    document.getElementById("configModalLabel").textContent = label;
    document.getElementById("configModalInput").value = currentVal
      ? currentVal.textContent.trim()
      : "";
    var modal = document.getElementById("configEditModal");
    modal.classList.add("active");
    modal.setAttribute("aria-hidden", "false");
    document.body.style.overflow = "hidden";
    document.getElementById("configModalInput").focus();
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
    var newVal = document.getElementById("configModalInput").value.trim();
    var valCell = document.getElementById("cfg-val-" + activeConfigKey);
    if (valCell) valCell.textContent = newVal;

    var actionCell = document.getElementById("cfg-action-" + activeConfigKey);
    if (actionCell) {
      actionCell.querySelectorAll(".cfg-action-btn").forEach(function (btn) {
        btn.disabled = true;
        btn.setAttribute("aria-disabled", "true");
        btn.style.opacity = "0.4";
      });
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
      title: "Month Working Days",
      columns: columns,
      data: monthWorkingDaysList,
      pageSize: 5,
      enableSort: true,
      enableFilter: false,
      enableResize: true,
      enableSelection: false,
      enablePagination: true,
      showAddButton: false,
      pageSizeOptions: [5, 10, 15, 20],
    });
  }

  function attachMonthWorkingDaysHandlers() {
    var gridContainer = document.querySelector(
      "#gridContainer_monthWorkingDays",
    );
    if (!gridContainer || gridContainer.dataset.mwdBound === "true") return;

    gridContainer.addEventListener("click", function (event) {
      var editButton = event.target.closest(".mwd-edit-link");
      var confirmButton = event.target.closest(".mwd-confirm-link");

      if (editButton) {
        var rowId = editButton.getAttribute("data-row-id");
        openMonthRowEditModal(rowId);
      }

      if (confirmButton) {
        var rowId = confirmButton.getAttribute("data-row-id");
        confirmMonthRowEdit(rowId);
      }
    });

    gridContainer.dataset.mwdBound = "true";
  }

  function openMonthRowEditModal(rowId) {
    var row = monthWorkingDaysList.find(function (r) {
      return String(r.id) === String(rowId);
    });

    if (!row) return;

    document.getElementById("monthModalRowId").value = rowId;
    document.getElementById("monthModalDays").value = row.days || "";
    document.getElementById("monthModalCvlHours").value = row.cvlHours || "";
    document.getElementById("monthModalVidHours").value = row.vidHours || "";

    var modal = document.getElementById("monthRowEditModal");
    modal.classList.add("active");
    modal.setAttribute("aria-hidden", "false");
    document.body.style.overflow = "hidden";
    document.getElementById("monthModalDays").focus();
  }

  function closeMonthRowEditModal() {
    var modal = document.getElementById("monthRowEditModal");
    modal.classList.remove("active");
    modal.setAttribute("aria-hidden", "true");
    document.body.style.overflow = "";
  }

  function confirmMonthRowEdit(rowId) {
    var days = document.getElementById("monthModalDays").value.trim();
    var cvlHours = document.getElementById("monthModalCvlHours").value.trim();
    var vidHours = document.getElementById("monthModalVidHours").value.trim();

    var row = monthWorkingDaysList.find(function (r) {
      return String(r.id) === String(rowId);
    });

    if (row) {
      row.days = days;
      row.cvlHours = cvlHours;
      row.vidHours = vidHours;

      if (monthWorkingDaysGrid) {
        monthWorkingDaysGrid.updateData(monthWorkingDaysList);
      }
    }

    closeMonthRowEditModal();
    disableConfirmButton(rowId); // Call to disable the confirm button
  }

  function disableConfirmButton(rowId) {
    var gridContainer = document.querySelector(
      "#gridContainer_monthWorkingDays",
    );
    if (gridContainer) {
      var confirmButton = gridContainer.querySelector(
        '.mwd-confirm-link[data-row-id="' + rowId + '"]',
      );
      if (confirmButton) {
        confirmButton.disabled = true;
        confirmButton.setAttribute("aria-disabled", "true");
        confirmButton.style.opacity = "0.5";
        confirmButton.style.cursor = "not-allowed";
      }
    }
  }

  /* --------------------------------------------------
        Modal Button Handlers
    -------------------------------------------------- */
  function attachMonthModalHandlers() {
    var monthModalClose = document.getElementById("monthModalClose");
    var monthModalCancel = document.getElementById("monthModalCancel");
    var monthModalSave = document.getElementById("monthModalSave");

    if (monthModalClose) {
      monthModalClose.addEventListener("click", closeMonthRowEditModal);
    }

    if (monthModalCancel) {
      monthModalCancel.addEventListener("click", closeMonthRowEditModal);
    }

    if (monthModalSave) {
      monthModalSave.addEventListener("click", function () {
        var rowId = document.getElementById("monthModalRowId").value;
        confirmMonthRowEdit(rowId);
      });
    }
  }

  /* --------------------------------------------------
        Button Handlers - Initiate & Approve
    -------------------------------------------------- */
  function attachApprovalButtonHandlers() {
    var btnInitiate = document.getElementById("btnInitiateDataSetupRequest");
    var btnApprove = document.getElementById("btnApproveDataSetupRequest");

    if (btnInitiate) {
      btnInitiate.addEventListener("click", function () {
        console.log("Initiate DataSetup Request clicked");
        // Add initiation logic here
      });
    }

    if (btnApprove) {
      btnApprove.addEventListener("click", function () {
        console.log("Approve DataSetup Request clicked");
        // Add approval logic here
      });
    }
  }

  /* --------------------------------------------------
        Initialize Page
    -------------------------------------------------- */
  document.addEventListener("DOMContentLoaded", function () {
    attachApprovalButtonHandlers();
  });
})();
