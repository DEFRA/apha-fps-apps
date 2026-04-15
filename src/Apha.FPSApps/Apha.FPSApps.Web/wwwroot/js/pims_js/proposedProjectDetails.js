// JSON data for Proposed Project Details
const proposedProjectData = {
  project: "ABOG0508",
  transferTo: "AH0009",
  title:
    "Welsh Government for assistance with Bacterial Pathogens and Animal By-products",
  costbookNo: "CAC 2096 (1314A,44)",
  disease: "Bacterial Pathogens",
  program: "DoES",
  customer: "Welsh Government",
  manager: "Prendergast, Jeff",
};

// JSON data for table records
const tableRecordsData = [
  {
    year: "2023",
    topic: "A&F Report",
    comments:
      "This is about comments goes here, sample content, dynamic content",
  },
  {
    year: "2023",
    topic: "Contracts",
    comments:
      "This is about comments goes here, sample content, dynamic content",
  },
  {
    year: "2023",
    topic: "General Comment",
    comments:
      "This is about comments goes here, sample content, dynamic content",
  },
  {
    year: "2023",
    topic: "Invoicing",
    comments:
      "This is about comments goes here, sample content, dynamic content",
  },
];

// Function to populate form from JSON data
function populateProposedProjectForm(data) {
  if (document.getElementById("project-transfer")) {
    document.getElementById("project-transfer").value = data.project || "";
  }
  if (document.getElementById("transfer-to")) {
    document.getElementById("transfer-to").value = data.transferTo || "";
  }
  if (document.getElementById("title-transfer")) {
    document.getElementById("title-transfer").value = data.title || "";
  }
  if (document.getElementById("costbook-transfer")) {
    document.getElementById("costbook-transfer").value = data.costbookNo || "";
  }
  if (document.getElementById("disease-transfer")) {
    document.getElementById("disease-transfer").value = data.disease || "";
  }
  if (document.getElementById("program-transfer")) {
    document.getElementById("program-transfer").value = data.program || "";
  }
  if (document.getElementById("customer-transfer")) {
    document.getElementById("customer-transfer").value = data.customer || "";
  }
  if (document.getElementById("manager-transfer")) {
    document.getElementById("manager-transfer").value = data.manager || "";
  }
}

// Function to populate table rows from JSON data
function populateTableRecords(records) {
  const tableBody = document.getElementById("recordsTableBody");

  if (!tableBody) {
    console.error("Table body not found");
    return;
  }

  // Clear existing rows
  tableBody.innerHTML = "";

  // Create rows from JSON data
  records.forEach((record, index) => {
    const row = document.createElement("tr");
    row.className = "govuk-table__row";

    const yearCell = document.createElement("td");
    yearCell.className = "govuk-table__cell";
    yearCell.innerHTML = `<span class="govuk-visually-hidden">Year: </span>${record.year}`;

    const topicCell = document.createElement("td");
    topicCell.className = "govuk-table__cell";
    topicCell.innerHTML = `<span class="govuk-visually-hidden">Topic: </span>${record.topic}`;

    const commentsCell = document.createElement("td");
    commentsCell.className = "govuk-table__cell";
    commentsCell.innerHTML = `<span class="govuk-visually-hidden">Comments: </span>${record.comments}`;

    const editCell = document.createElement("td");
    editCell.className = "govuk-table__cell";
    editCell.style.textAlign = "center";
    console.log("RECORD: ", record);
    editCell.innerHTML = `<button type="button" class="action-btn edit-btn" aria-label="Edit record"
      onclick="openEditModal(${index}, '${record.year}', '${record.topic}', '${record.comments}')"
      style="border: none; background: #fff;">
      <img src="../images/pen-to-square-regular-full.svg" alt="Edit icon for selected record"
        class="editjobcode" width="20"></button>`;

    const deleteCell = document.createElement("td");
    deleteCell.className = "govuk-table__cell";
    deleteCell.style.textAlign = "center";
    deleteCell.innerHTML = `<button type="button" class="action-btn delete-btn" aria-label="Delete record"
      onclick="deleteRecord(${index})" style="border: none; background: #fff;">
      <img src="../images/trash-can-regular-full.svg" alt="Delete icon for selected record"
        width="20"></button>`;

    row.appendChild(yearCell);
    row.appendChild(topicCell);
    row.appendChild(commentsCell);
    row.appendChild(editCell);
    row.appendChild(deleteCell);

    tableBody.appendChild(row);
  });
}

// Store current record index for edit operations
let currentEditingIndex = null;

// Open edit modal with record data
function openEditModal(index, year, title, comment) {
  currentEditingIndex = index;

  // Populate form with existing data
  document.getElementById("edit-year").value = year;
  document.getElementById("edit-title").value = title;
  document.getElementById("edit-comment").value = comment;

  // Clear any previous error messages
  document.getElementById("edit-year-error").style.display = "none";
  document.getElementById("edit-year").classList.remove("govuk-input--error");

  // Show modal using CSS class
  document.getElementById("editRecordModal").classList.add("open");
}

// Validate year field (numeric only)
function validateYear(year) {
  const yearRegex = /^\d+$/;
  return yearRegex.test(year.trim());
}

// Save edited record
function saveRecord() {
  const yearInput = document.getElementById("edit-year");
  const titleInput = document.getElementById("edit-title");
  const commentInput = document.getElementById("edit-comment");
  const yearError = document.getElementById("edit-year-error");

  // Get values
  const year = yearInput.value;
  const title = titleInput.value;
  const comment = commentInput.value;

  // Validate year
  if (!validateYear(year)) {
    yearError.style.display = "block";
    yearInput.classList.add("govuk-input--error");
    return;
  }

  // Clear error if valid
  yearError.style.display = "none";
  yearInput.classList.remove("govuk-input--error");

  // Get table rows
  const tableRows = document.querySelectorAll(
    "table.custom-table tbody tr.govuk-table__row",
  );

  if (currentEditingIndex !== null && tableRows[currentEditingIndex]) {
    const cells = tableRows[currentEditingIndex].querySelectorAll(
      "td.govuk-table__cell",
    );

    // Update table cells with new data
    if (cells[0]) {
      cells[0].querySelector("span.govuk-visually-hidden").textContent =
        "Year: ";
      cells[0].textContent = year;
      cells[0].innerHTML =
        '<span class="govuk-visually-hidden">Year: </span>' + year;
    }

    if (cells[1]) {
      cells[1].querySelector("span.govuk-visually-hidden").textContent =
        "Topic: ";
      cells[1].textContent = title;
      cells[1].innerHTML =
        '<span class="govuk-visually-hidden">Topic: </span>' + title;
    }

    if (cells[2]) {
      cells[2].querySelector("span.govuk-visually-hidden").textContent =
        "Comments: ";
      cells[2].textContent = comment;
      cells[2].innerHTML =
        '<span class="govuk-visually-hidden">Comments: </span>' + comment;
    }

    // Update onclick attributes for the buttons
    if (cells[3]) {
      const editBtn = cells[3].querySelector("button.edit-btn");
      if (editBtn) {
        editBtn.setAttribute(
          "onclick",
          `openEditModal(${currentEditingIndex}, '${year}', '${title}', '${comment}')`,
        );
      }
    }

    // Close modal
    closeEditModal();

    // Show success message
    showNotification("Record updated successfully", "success");
  }
}

// Delete record
function deleteRecord(index) {
  if (confirm("Are you sure you want to delete this record?")) {
    const tableRows = document.querySelectorAll(
      "table.custom-table tbody tr.govuk-table__row",
    );

    if (tableRows[index]) {
      tableRows[index].remove();
      showNotification("Record deleted successfully", "danger");
    }
  }
}

// Show notification message
function showNotification(message, type) {
  // Create alert element
  const alertDiv = document.createElement("div");
  alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
  alertDiv.role = "alert";
  alertDiv.innerHTML = `
    ${message}
    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
  `;

  // Insert at top of main content
  const mainContent = document.querySelector("main.main-content");
  if (mainContent) {
    mainContent.insertBefore(alertDiv, mainContent.firstChild);

    // Auto-dismiss after 4 seconds
    setTimeout(() => {
      alertDiv.remove();
    }, 4000);
  }
}

// Close edit modal
function closeEditModal() {
  document.getElementById("editRecordModal").classList.remove("open");
}

// Load data when page loads
document.addEventListener("DOMContentLoaded", function () {
  populateProposedProjectForm(proposedProjectData);
  populateTableRecords(tableRecordsData);
});
