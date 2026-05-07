// Project Budget Profile page script

function goBack() {
    window.history.back();
}

function refreshGraph() {
    const project = document.getElementById('ParentProject').value;
    if (!project) return;
    loadProfileGraphData(project);
    loadCumulativeGraphData(project);
}

// ── Cost Profile Grid ─────────────────────────────────────────────────────

function loadCostProfileGrid(parentProject) {
    let url = '/PACT/ProjectProfile/LoadCostProfileGrid';
    if (parentProject) {
        url += '?parentProject=' + encodeURIComponent(parentProject);
    }

    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ filter: '{}' })
    })
        .then(response => response.text())
        .then(html => {
            const container = document.getElementById('gridContainer_costProfileGrid');
            container.innerHTML = '';
            container.appendChild(document.createRange().createContextualFragment(html));
            updateTotalCostProfile(parentProject);
        })
        .catch(() => console.error('Failed to load cost profile grid.'));
}

function updateTotalCostProfile(parentProject) {
    const input = document.getElementById('TotalCostProfile');
    if (!parentProject) {
        input.value = '';
        return;
    }

    fetch('/PACT/ProjectProfile/GetTotalCostProfile?parentProject=' + encodeURIComponent(parentProject))
        .then(response => response.json())
        .then(res => {
            if (res.success) {
                input.value = parseFloat(res.data).toFixed(2);
            }
        });
}

// ── Graph Data ────────────────────────────────────────────────────────────

let nonCumulativeChart = null;
let cumulativeChart = null;

function loadProfileGraphData(parentProject) {
    if (!parentProject) return;

    fetch('/PACT/ProjectProfile/GetProfileGraphData?parentProject=' + encodeURIComponent(parentProject))
        .then(response => response.json())
        .then(res => {
            if (!res.success || !res.data) return;

            const labels = res.data.map(d => 'Month ' + d.monthNo);
            const profileData = res.data.map(d => d.profile || 0);
            const costData = res.data.map(d => d.totalCost || 0);

            if (nonCumulativeChart) {
                nonCumulativeChart.destroy();
            }

            const ctx = document.getElementById('nonCumulativeChart').getContext('2d');
            nonCumulativeChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels,
                    datasets: [
                        {
                            label: 'Profile',
                            data: profileData,
                            backgroundColor: 'rgba(0, 112, 60, 0.6)',
                            borderColor: 'rgba(0, 112, 60, 1)',
                            borderWidth: 1
                        },
                        {
                            label: 'Actual Cost',
                            data: costData,
                            backgroundColor: 'rgba(29, 112, 184, 0.6)',
                            borderColor: 'rgba(29, 112, 184, 1)',
                            borderWidth: 1
                        }
                    ]
                },
                options: {
                    responsive: true,
                    plugins: { legend: { position: 'top' } },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: { maxTicksLimit: 7, callback: value => '£' + value.toLocaleString() }
                        }
                    }
                }
            });
        });
}

function loadCumulativeGraphData(parentProject) {
    if (!parentProject) return;

    fetch('/PACT/ProjectProfile/GetCumulativeGraphData?parentProject=' + encodeURIComponent(parentProject))
        .then(response => response.json())
        .then(res => {
            if (!res.success || !res.data) return;

            const labels = res.data.map(d => 'Month ' + d.monthNo);
            const cumProfileData = res.data.map(d => d.cumulativeProfile || 0);
            const cumCostData = res.data.map(d => d.cumulativeCost || 0);

            if (cumulativeChart) {
                cumulativeChart.destroy();
            }

            const ctx = document.getElementById('cumulativeChart').getContext('2d');
            cumulativeChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels,
                    datasets: [
                        {
                            label: 'Cumulative Profile',
                            data: cumProfileData,
                            borderColor: 'rgba(0, 112, 60, 1)',
                            backgroundColor: 'rgba(0, 112, 60, 0.1)',
                            fill: true,
                            tension: 0.3
                        },
                        {
                            label: 'Cumulative Cost',
                            data: cumCostData,
                            borderColor: 'rgba(29, 112, 184, 1)',
                            backgroundColor: 'rgba(29, 112, 184, 0.1)',
                            fill: true,
                            tension: 0.3
                        }
                    ]
                },
                options: {
                    responsive: true,
                    plugins: { legend: { position: 'top' } },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: { maxTicksLimit: 6, callback: value => '£' + value.toLocaleString() }
                        }
                    }
                }
            });
        });
}

// ── CRUD helpers ──────────────────────────────────────────────────────────

// Store modal state outside of jQuery .data()
let _modalProject = '';

function addProjectMonth() {
    const project = document.getElementById('ParentProject').value;
    if (!project) { alert('Please select a project first.'); return; }
    openCostProfileModal(project, 0);
}

function editProjectMonth(btn) {
    const monthNo = parseInt(btn.getAttribute('data-id')) || 0;
    const project = document.getElementById('ParentProject').value;
    openCostProfileModal(project, monthNo);
}

function openCostProfileModal(project, monthNo) {
    _modalProject = project;

    const url = '/PACT/ProjectProfile/GetProjectMonth?project=' + encodeURIComponent(project) + '&monthNo=' + monthNo;

    fetch(url)
        .then(response => {
            if (!response.ok) throw new Error('HTTP ' + response.status + ' – ' + url);
            return response.text();
        })
        .then(html => {
            const content = document.getElementById('costProfileModalContent');
            content.innerHTML = '';
            content.appendChild(document.createRange().createContextualFragment(html));
            document.getElementById('costProfileModal').classList.add('show');
        })
        .catch(err => console.error('Failed to load cost profile form:', err));
}

function saveProjectMonth() {
    const form = document.getElementById('projectMonthForm');
    if (!form) return;

    const payload = {
        project:     form.querySelector('[name="Project"]')?.value,
        monthNo:     parseInt(form.querySelector('[name="MonthNo"]')?.value) || 0,
        costProfile: parseFloat(form.querySelector('[name="CostProfile"]')?.value) || null
    };

    if (!payload.monthNo) { alert('Please enter a month number.'); return; }

    fetch('/PACT/ProjectProfile/SaveProjectMonth', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    })
        .then(response => response.json())
        .then(res => {
            if (res.success) {
                document.getElementById('costProfileModal').classList.remove('show');
                loadCostProfileGrid(_modalProject);
                loadProfileGraphData(_modalProject);
                loadCumulativeGraphData(_modalProject);
            } else {
                alert(res.message || 'Failed to save.');
            }
        });
}

function deleteProjectMonth(btn) {
    const monthNo = parseInt(btn.getAttribute('data-id')) || 0;
    const project = document.getElementById('ParentProject').value;
    if (!confirm('Delete month ' + monthNo + '?')) return;

    fetch('/PACT/ProjectProfile/DeleteProjectMonth?project=' + encodeURIComponent(project) + '&monthNo=' + monthNo, {
        method: 'DELETE'
    })
        .then(response => response.json())
        .then(res => {
            if (res.success) {
                loadCostProfileGrid(project);
            } else {
                alert(res.message || 'Failed to delete.');
            }
        });
}

// ── Document Ready ────────────────────────────────────────────────────────

function loadProjectDetails(project) {
    const titleInput  = document.getElementById('ProjectTitle');
    const budgetInput = document.getElementById('BudgetCvl');

    if (!project) {
        if (titleInput)  titleInput.value  = '';
        if (budgetInput) budgetInput.value = '';
        return;
    }

    fetch('/PACT/ProjectProfile/GetProjectDetailsAsync?parentProject=' + encodeURIComponent(project))
        .then(response => response.json())
        .then(res => {
            if (res.success) {
                if (titleInput)  titleInput.value  = res.projectTitle  ?? '';
                if (budgetInput) budgetInput.value = res.budgetCvl     ?? '';
            }
        })
        .catch(err => console.error('Failed to load project details:', err));
}

document.addEventListener('DOMContentLoaded', () => {

    // Bind grid on dropdown change
    document.getElementById('ParentProject').addEventListener('change', function () {
        const project = this.value;
        loadProjectDetails(project);
        loadCostProfileGrid(project);
        loadProfileGraphData(project);
        loadCumulativeGraphData(project);
    });

    // Bind grid on page load if a project is already selected
    const initialProject = document.getElementById('ParentProject').value;
    if (initialProject) {
        loadProjectDetails(initialProject);
        loadCostProfileGrid(initialProject);
        loadProfileGraphData(initialProject);
        loadCumulativeGraphData(initialProject);
    }


    // Close modal when clicking outside the dialog
    document.getElementById('costProfileModal').addEventListener('click', function (e) {
        if (e.target === this) closeCostProfileModal();
    });
});

function closeCostProfileModal() {
    document.getElementById('costProfileModal').classList.remove('show');
}
