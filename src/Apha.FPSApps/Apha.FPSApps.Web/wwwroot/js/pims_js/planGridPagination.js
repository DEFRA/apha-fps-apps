// Plan Grids Pagination Module - Uses dummy data from payPact.js with GOV.UK pagination style
const planGrids = {
  staffPlans: {
    tableId: "staffPlansTable",
    bodyId: "staff-plans-body",
    paginationId: "staffPlansPagination",
    currentPage: 1,
    recordsPerPage: 5,
    data: [],
  },
  testPlans: {
    tableId: "testPlansTable",
    bodyId: "test-plans-body",
    paginationId: "testPlansPagination",
    currentPage: 1,
    recordsPerPage: 5,
    data: [],
  },
  animalPlansTab5: {
    tableId: "animalPlansTab5Table",
    bodyId: "AnimalPlansTab5Body",
    paginationId: "animalPlansTab5Pagination",
    currentPage: 1,
    recordsPerPage: 5,
    data: [],
  },
  animalPlans: {
    tableId: "animalPlansTable",
    bodyId: "AnimalPlansBody",
    paginationId: "animalPlansPagination",
    currentPage: 1,
    recordsPerPage: 5,
    data: [],
  },
  costPlans: {
    tableId: "costPlansTable",
    bodyId: "cost-plans-body",
    paginationId: "costPlansPagination",
    currentPage: 1,
    recordsPerPage: 5,
    data: [],
  },
};

// Initialize pagination for all grids
function initializePlanGridPagination() {
  // Populate data from payPact.js dummy data
  planGrids.staffPlans.data = staffPlansDummy || [];
  planGrids.testPlans.data = testPlansGridDummy || [];
  planGrids.animalPlansTab5.data = animalPlansDummy || [];
  planGrids.animalPlans.data = animalPlansDummy || [];
  planGrids.costPlans.data = costPlansDummy || [];

  console.log("Initializing Plan Grid Pagination");
  console.log("Animal Plans Data (Tab 5):", planGrids.animalPlansTab5.data);
  console.log("Animal Plans Data (Animal Panel):", planGrids.animalPlans.data);

  Object.keys(planGrids).forEach((gridKey) => {
    const grid = planGrids[gridKey];
    const tbody = document.getElementById(grid.bodyId);
    console.log(
      `Grid: ${gridKey}, Table Body ID: ${grid.bodyId}, Element Found:`,
      tbody !== null,
    );

    setupPagination(gridKey);
    renderGridPage(gridKey, 1);
  });
}

// Setup pagination controls for a specific grid
function setupPagination(gridKey) {
  const grid = planGrids[gridKey];
  const recordsSelect = document.getElementById(`${gridKey}RecordsPerPage`);

  if (recordsSelect) {
    recordsSelect.addEventListener("change", (e) => {
      grid.recordsPerPage = parseInt(e.target.value);
      grid.currentPage = 1;
      renderGridPage(gridKey, 1);
    });
  }
}

// Render grid page
function renderGridPage(gridKey, page) {
  const grid = planGrids[gridKey];
  const startIndex = (page - 1) * grid.recordsPerPage;
  const endIndex = startIndex + grid.recordsPerPage;
  const pageData = grid.data.slice(startIndex, endIndex);

  const tbody = document.getElementById(grid.bodyId);
  if (!tbody) return;

  tbody.innerHTML = "";

  pageData.forEach((row) => {
    if (gridKey === "staffPlans") {
      const tr = document.createElement("tr");
      tr.className = "govuk-table__row";
      tr.innerHTML = `
        <td class="govuk-table__cell">${row.wgGrac || ""}</td>
        <td class="govuk-table__cell">${row.name || ""}</td>
        <td class="govuk-table__cell">${row.hour || ""}</td>
        <td class="govuk-table__cell">${row.ratt || ""}</td>
        <td class="govuk-table__cell govuk-!-text-align-right">${typeof row.cost === "number" ? formatMoney(row.cost) : row.cost || "£0.00"}</td>
      `;
      tbody.appendChild(tr);
    } else if (gridKey === "testPlans") {
      const tr = document.createElement("tr");
      tr.className = "govuk-table__row";
      tr.innerHTML = `
        <td class="govuk-table__cell">${row.test || ""}</td>
        <td class="govuk-table__cell">${row.unitPrice || ""}</td>
        <td class="govuk-table__cell">${row.noRequ || ""}</td>
        <td class="govuk-table__cell govuk-!-text-align-right">${typeof row.cost === "number" ? formatMoney(row.cost) : row.cost || "£0.00"}</td>
      `;
      tbody.appendChild(tr);
    } else if (gridKey === "animalPlansTab5") {
      const tr = document.createElement("tr");
      tr.className = "govuk-table__row";
      tr.innerHTML = `
        <td class="govuk-table__cell">${row.animalType || ""}</td>
        <td class="govuk-table__cell">${row.date || ""}</td>
        <td class="govuk-table__cell">${row.anim || ""}</td>
        <td class="govuk-table__cell">${row.rate || ""}</td>
        <td class="govuk-table__cell govuk-!-text-align-right">${typeof row.cost === "number" ? formatMoney(row.cost) : row.cost || "£0.00"}</td>
      `;
      tbody.appendChild(tr);
    } else if (gridKey === "animalPlans") {
      const tr = document.createElement("tr");
      tr.className = "govuk-table__row";
      tr.innerHTML = `
        <td class="govuk-table__cell">${row.animalType || ""}</td>
        <td class="govuk-table__cell">${row.days || row.date || ""}</td>
        <td class="govuk-table__cell">${row.anim || ""}</td>
        <td class="govuk-table__cell">${row.rate || ""}</td>
        <td class="govuk-table__cell govuk-!-text-align-right">${typeof row.cost === "number" ? formatMoney(row.cost) : row.cost || "£0.00"}</td>
      `;
      tbody.appendChild(tr);
    } else if (gridKey === "costPlans") {
      const tr = document.createElement("tr");
      tr.className = "govuk-table__row";
      tr.innerHTML = `
        <td class="govuk-table__cell">${row.account || ""}</td>
        <td class="govuk-table__cell">${row.description || ""}</td>
        <td class="govuk-table__cell govuk-!-text-align-right">${typeof row.cost === "number" ? formatMoney(row.cost) : row.cost || "£0.00"}</td>
      `;
      tbody.appendChild(tr);
    }
  });

  // Update outside total display with full-dataset total
  const totalDisplayMap = {
    staffPlans: "staffPlansPlanTotalDisplay",
    testPlans: "testPlansPlanTotalDisplay",
    animalPlansTab5: "animalPlansTab5TotalDisplay",
    animalPlans: "animalPlansTotalDisplay",
    costPlans: "costPlansPlanTotalDisplay",
  };
  const totalDisplayId = totalDisplayMap[gridKey];
  if (totalDisplayId) {
    const fullTotal = grid.data.reduce((sum, row) => {
      return sum + (typeof row.cost === "number" ? row.cost : 0);
    }, 0);
    const totalEl = document.getElementById(totalDisplayId);
    if (totalEl) totalEl.textContent = formatMoney(fullTotal);
  }

  updatePaginationControls(gridKey);
}

// Update pagination controls - GOV.UK pagination list style
function updatePaginationControls(gridKey) {
  const grid = planGrids[gridKey];
  const totalPages = Math.ceil(grid.data.length / grid.recordsPerPage);
  const paginationUl = document.getElementById(grid.paginationId);

  if (!paginationUl) return;

  // Clear pagination list
  paginationUl.innerHTML = "";

  // Previous button
  const prevLi = document.createElement("li");
  prevLi.className = `govuk-pagination__item${grid.currentPage === 1 ? " disabled" : ""}`;
  prevLi.setAttribute("aria-disabled", grid.currentPage === 1);
  const prevA = document.createElement("a");
  prevA.className = "govuk-link govuk-pagination__link";
  prevA.href = "#";
  prevA.onclick = (e) => {
    e.preventDefault();
    if (grid.currentPage > 1) {
      grid.currentPage--;
      renderGridPage(gridKey, grid.currentPage);
    }
  };
  prevA.innerHTML = `
    <svg class="govuk-pagination__icon govuk-pagination__icon--prev" xmlns="http://www.w3.org/2000/svg" height="13" width="15" aria-hidden="true" focusable="false" viewBox="0 0 15 13"></svg>
    <span class="govuk-pagination__link-title">Previous<span class="govuk-visually-hidden"> page</span></span>
  `;
  prevLi.appendChild(prevA);
  paginationUl.appendChild(prevLi);

  // Page numbers
  for (let i = 1; i <= totalPages; i++) {
    const pageLi = document.createElement("li");
    pageLi.className = `govuk-pagination__item${i === grid.currentPage ? " govuk-pagination__item--current" : ""}`;
    const pageA = document.createElement("a");
    pageA.className = "govuk-link govuk-pagination__link";
    pageA.href = "#";
    pageA.textContent = i;
    pageA.onclick = (e) => {
      e.preventDefault();
      if (i !== grid.currentPage) {
        grid.currentPage = i;
        renderGridPage(gridKey, i);
      }
    };
    pageLi.appendChild(pageA);
    paginationUl.appendChild(pageLi);
  }

  // Next button
  const nextLi = document.createElement("li");
  nextLi.className = `govuk-pagination__item govuk-pagination__next${grid.currentPage === totalPages ? " disabled" : ""}`;
  nextLi.setAttribute("aria-disabled", grid.currentPage === totalPages);
  const nextA = document.createElement("a");
  nextA.className = "govuk-link govuk-pagination__link";
  nextA.href = "#";
  nextA.setAttribute("rel", "next");
  nextA.onclick = (e) => {
    e.preventDefault();
    if (grid.currentPage < totalPages) {
      grid.currentPage++;
      renderGridPage(gridKey, grid.currentPage);
    }
  };
  nextA.innerHTML = `
    <span class="govuk-pagination__link-title">Next</span>
    <svg class="govuk-pagination__icon govuk-pagination__icon--next" xmlns="http://www.w3.org/2000/svg" height="13" width="15" aria-hidden="true" focusable="false" viewBox="0 0 15 13"></svg>
  `;
  nextLi.appendChild(nextA);
  paginationUl.appendChild(nextLi);
}

// Format money - convert number to £ format
function formatMoney(value) {
  if (typeof value === "string") return value;
  return `£${value.toLocaleString("en-GB", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

// ============ Helper Methods for Animal Plans Tables ============
// Method to update Animal Plans Tab 5 (Plan panel)
function updateAnimalPlansTab5(data) {
  planGrids.animalPlansTab5.data = data || [];
  planGrids.animalPlansTab5.currentPage = 1;
  setupPagination("animalPlansTab5");
  renderGridPage("animalPlansTab5", 1);
  console.log("Updated Animal Plans Tab 5 with", data?.length || 0, "records");
}

// Method to update Animal Plans Main (Animal Plans panel)
function updateAnimalPlans(data) {
  planGrids.animalPlans.data = data || [];
  planGrids.animalPlans.currentPage = 1;
  setupPagination("animalPlans");
  renderGridPage("animalPlans", 1);
  console.log("Updated Animal Plans with", data?.length || 0, "records");
}

// Method to get Animal Plans Tab 5 data
function getAnimalPlansTab5() {
  return planGrids.animalPlansTab5.data;
}

// Method to get Animal Plans Main data
function getAnimalPlans() {
  return planGrids.animalPlans.data;
}

// Method to refresh both animal plans tables
function refreshAllAnimalPlans(data) {
  updateAnimalPlansTab5(data);
  updateAnimalPlans(data);
  console.log("Refreshed all Animal Plans tables");
}

// Initialize on DOM ready
document.addEventListener("DOMContentLoaded", initializePlanGridPagination);
