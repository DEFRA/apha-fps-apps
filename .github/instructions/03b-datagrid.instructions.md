# Instruction: DataGrid — Grid Item, Controller Actions, Views & Modals

> **When to read this file:** Only when the **Grid Operations Profile** (step 2b in the prompt) confirms that at least one subform has `DefaultView = 1` or `2` AND a `<table>` exists in the HTML prototype. For forms with `DefaultView = 0` (Single Form) and no `<table>`, skip this file entirely.

> **Lock file — phase start:** Before writing any file, run `Get-Date -Format 'yyyy-MM-ddTHH:mm:ss'` and update `zPostRunValidationArtefacts/.codingagent-lock`:
> - Set `current-phase: Phase 2b — DataGrid Extensions`
> - Add a row: `| Phase 2b — DataGrid Extensions | <timestamp> | IN-PROGRESS | |`

---

## Pre-flight — Read a Reference DataGrid First

Before writing any `[FormName]Item.cs`, `Load*Grid` action, or modal partial, **read one existing working DataGrid view** in the same application:

- `Apha.FPSApps.Web/Areas/FPS/Views/StaffMaintenance/Index.cshtml` — canonical inline CRUD with shared modal
- `Apha.FPSApps.Web/Areas/FPS/Views/ProgramStaffPlan/Index.cshtml` — master-detail grid with AJAX reload
- `Apha.FPSApps.Web/wwwroot/js/fps_js/StaffJob.js` — canonical CRUD JavaScript pattern

Generate the new view and modal by following their exact patterns — do **not** adapt the HTML prototype's modal structure.

---

## Step 16b — Grid Item Model — `[FormName]Item.cs`

**Location:** `Apha.FPSApps.Web/Areas/[App]/Models/`

```csharp
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.[App].Models
{
    public class [FormName]Item
    {
        // PK — always hidden from the grid, used as KeyProperty
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public [KeyType] [KeyName] { get; set; }

        // One property per visible column in the HTML table.
        // Use exact field names from the entity/DTO (PascalCase).
        // Width is in pixels — match the column widths from the HTML prototype.
        [Required(ErrorMessage = "[FieldLabel] is required")]
        [Display(Name = "[FieldLabel]")]
        [GridColumn(Width = [px], Type = GridColumnType.[Type], IsFilterable = true)]
        public [Type] [FieldName] { get; set; }
    }
}
```

`GridColumnType` values: `Text`, `Number`, `DecimalNumber`, `GbpValue`, `Date`, `Dropdown`, `ReadOnly`

Rules:
- **Only editable fields should have `[Required]`** — read-only and computed fields must not carry `[Required]` because they are never part of the submitted model binding payload
- Grid columns that are **read-only** in the Grid Operations Profile use `GridColumnType.ReadOnly`
- The `[FormName]Item` class is used as the modal partial model (`_AddEdit[FormName].cshtml`) as well as the grid row shape — every field that appears in the modal must be a property here

---

## Step 17b — MVC Controller: DataGrid-Specific Endpoints

Add these endpoints to the `[FormName]Controller` (in addition to the standard `Index` GET in `03-frontend`):

### CRITICAL — `Index()` GET must build the full `DataGridConfig`

> **`DataGridConfig<T>` has dangerous defaults.** Its constructor sets `AllowAdd = true`, `GridId = ""`, `BindGridUrl = ""`, and `Columns = []`. If `Index()` returns a ViewModel whose `[GridName]Grid` property is left as `= new()`, the first server-side render of `_DataGrid` will show the Add button regardless of the `.frm` profile, the grid will render as empty and show "Action", and `window['gridManager_']` will never be found by `reloadGrid()`.
>
> **Rule:** `Index()` must **always** build the full `DataGridConfig` explicitly — same settings as `Get[FormName]GridConfigAsync`, but with `Data = new List<[FormName]Item>()` and `Pagination = new PaginationModel()` (no data on initial load).

```csharp
public async Task<IActionResult> Index()
{
    var viewModel = new [FormName]ViewModel();
    await PopulateDropdownsAsync(viewModel);

    // Build the full DataGridConfig — do NOT leave it as new() default.
    // DataGridConfig<T> constructor sets AllowAdd=true, GridId="", BindGridUrl="" — those defaults
    // cause the Add button to appear on initial load and gridManager to fail to register.
    viewModel.[GridName]Grid = new DataGridConfig<[FormName]Item>
    {
        GridId             = "[formNameCamel]Grid",
        Title              = "[Form Display Title]",
        ShowCheckboxColumn = true,
        ShowPagination     = true,
        KeyProperty        = "[KeyName]",
        AllowAdd           = false,   // must match Grid Operations Profile from .frm
        AddFunction        = null,
        AllowEdit          = true,    // must match Grid Operations Profile from .frm
        AllowDelete        = true,    // must match Grid Operations Profile from .frm
        EditFunction       = "edit[FormName]",
        DeleteFunction     = "delete[FormName]",
        ExtraFilterMethod  = "get[FormName]ExtraFilters",
        BindGridUrl        = "/[App]/[FormName]/Load[FormName]Grid",
        Data               = new List<[FormName]Item>(),
        Columns            = GridDataProvider.GetColumnsDefination<[FormName]Item>(),
        Pagination         = new PaginationModel()
    };

    return View(viewModel);
}
```

### AJAX reload endpoint — extra filter parameters

> **Extra filters are top-level POST params, not inside `request.Filter`.** `_DataGrid.cshtml` calls `getAjaxParams()` which merges `ExtraFilterMethod()` results (e.g. `{ testCode: "...", showRejected: true }`) as top-level form fields alongside `page`, `pageSize`, `filter`, etc. They are **never** serialised inside the `filter` JSON string. Define them as separate action parameters — the MVC model binder picks them up automatically.
>
> Pattern from `ProgramProjectController`: `Load[FormName]Grid(PaginationFilter<string> request, string? programNo = null)` — one extra `string?` or `bool` param per extra filter key.

```csharp
// No extra filters (simple grid — no master dropdown):
[HttpPost]
public async Task<IActionResult> Load[FormName]Grid(PaginationFilter<string> request)

// With master filter dropdown (e.g. "Choose a Test"):
[HttpPost]
public async Task<IActionResult> Load[FormName]Grid(
    PaginationFilter<string> request, string? [filterParam] = null)

// With master filter + boolean toggle (e.g. ShowRejected checkbox):
[HttpPost]
public async Task<IActionResult> Load[FormName]Grid(
    PaginationFilter<string> request, string? [filterParam] = null, bool [boolParam] = false)
```

Full signature with body:

```csharp
[HttpPost]
public async Task<IActionResult> Load[FormName]Grid(
    PaginationFilter<string> request, string? [filterParam] = null, bool [boolParam] = false)
{
    if (!ModelState.IsValid)
    {
        return Json(new
        {
            success = false,
            message = "Invalid request data",
            errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
        });
    }

    var gridConfig = await Get[FormName]GridConfigAsync(request, [filterParam], [boolParam]);
    return PartialView("_DataGrid", gridConfig);
}

private async Task<DataGridConfig<[FormName]Item>> Get[FormName]GridConfigAsync(
    PaginationFilter<string> request, string? [filterParam], bool [boolParam])
{
    var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
        ?? new Dictionary<string, string>();

    var queryParameters = _mapper.Map<QueryParameters<string>>(request);
    var pagedData = await _service.GetAll[FormName]Async(queryParameters, [filterParam] ?? string.Empty, [boolParam]);

    var items = pagedData.Data != null
        ? _mapper.Map<List<[FormName]Item>>(pagedData.Data.ToList())
        : new List<[FormName]Item>();

    var paginationModel = pagedData.Pagination == null
        ? new PaginationModel()
        : _mapper.Map<PaginationModel>(pagedData.Pagination);
    paginationModel.SortColumn = request.SortBy;
    paginationModel.SortDirection = request.Descending;

    // Grid Operations Profile (step 2b) — .frm file takes priority over HTML prototype.
    //   AllowAdd: true  → AllowAdd = true,  AddFunction = "add[FormName]"
    //   AllowAdd: false → AllowAdd = false, AddFunction = null   (Add button omitted from grid header)
    //   AllowEdit: true  → AllowEdit = true,  EditFunction = "edit[FormName]"
    //   AllowEdit: false → AllowEdit = false, EditFunction = null
    //   AllowDelete: true  → AllowDelete = true,  DeleteFunction = "delete[FormName]"
    //   AllowDelete: false → AllowDelete = false, DeleteFunction = null
    // The boolean AllowAdd/Edit/Delete controls rendering; the Function property controls the callback.
    // Setting AllowAdd = false is what actually suppresses the Add button — AddFunction = null alone is NOT sufficient.
    return new DataGridConfig<[FormName]Item>
    {
        GridId = "[formNameCamel]Grid",
        Title = "[Form Display Title]",
        ShowCheckboxColumn = true,
        ShowPagination = true,
        KeyProperty = "[KeyName]",           // row-level unique discriminator — NOT the master filter field
        AllowAdd = false,                    // set to true only when AllowAdd: true in the subform .frm
        AddFunction = null,                  // set to "add[FormName]" only when AllowAdd: true
        EditFunction = "edit[FormName]",     // set to null if AllowEdit: false
        DeleteFunction = "delete[FormName]", // set to null if AllowDelete: false
        ExtraFilterMethod = "get[FormName]ExtraFilters",
        BindGridUrl = "/[App]/[FormName]/Load[FormName]Grid",
        Data = items,
        Columns = GridDataProvider.GetColumnsDefination<[FormName]Item>(null),
        Pagination = paginationModel,
        CurrentFilters = filterDict
    };
}
```

### CRUD controller endpoints — conditional on Grid Operations Profile

Only generate the endpoints that the Grid Operations Profile permits:

**AllowAdd: true → generate:**
```csharp
[HttpGet]
public IActionResult Create()
{
    return PartialView("_AddEdit[FormName]", new [FormName]Item());
}

// AJAX POST — [FromBody] JSON. Do NOT use [ValidateAntiForgeryToken] on AJAX JSON endpoints.
[HttpPost]
public async Task<IActionResult> Create([FromBody] [Entity]Dto dto)
{
    if (dto is null)
        return Json(new { success = false, message = "Invalid data" });

    var result = await _service.CreateAsync(dto);
    return result.Success
        ? Json(new { success = true, message = "[FormName] created successfully" })
        : Json(new { success = false, errors = result.Errors });
}
```

**AllowEdit: true → generate:**
```csharp
[HttpGet]
public async Task<IActionResult> Edit([KeyType] id)
{
    var result = await _service.GetByIdAsync(id);
    if (!result.Success)
        return NotFound($"[FormName] with ID {id} not found.");
    var item = _mapper.Map<[FormName]Item>(result.Data);
    return PartialView("_AddEdit[FormName]", item);
}

// AJAX POST — [FromBody] JSON. Do NOT use [ValidateAntiForgeryToken].
[HttpPost]
public async Task<IActionResult> Edit([KeyType] id, [FromBody] [Entity]Dto dto)
{
    if (dto is null)
        return Json(new { success = false, message = "Invalid data" });

    var result = await _service.UpdateAsync(dto);
    return result.Success
        ? Json(new { success = true, message = "[FormName] updated successfully" })
        : Json(new { success = false, errors = result.Errors });
}
```

**AllowDelete: true → generate:**

> **DataGrid delete does not use a modal partial.** There is no `[HttpGet] Delete` action and no `_Delete[FormName].cshtml`. Delete is confirmed via a browser `confirm()` dialog in JavaScript; only the `[HttpDelete]` AJAX endpoint is needed on the controller.

```csharp
[HttpDelete]
public async Task<IActionResult> Delete([KeyType] id)
{
    var result = await _service.DeleteAsync(id);
    return result.Success
        ? Json(new { success = true, message = "[FormName] deleted successfully" })
        : Json(new { success = false, errors = result.Errors });
}
```

---

## `DataGridConfig` — Button visibility rules

> **The `.frm` file is the source of truth.** `AllowAdditions`, `AllowEdits`, `AllowDeletions` in the subform `.frm` always take precedence over anything in the HTML prototype.

| Grid Operations Profile | `DataGridConfig` properties | Effect on `_DataGrid.cshtml` |
|---|---|---|
| `AllowAdd: true` | `AllowAdd = true`, `AddFunction = "add[FormName]"` | Add button rendered in grid header |
| `AllowAdd: false` | `AllowAdd = false`, `AddFunction = null` | Add button **omitted entirely** — both properties are required; `AddFunction = null` alone is **not** sufficient |
| `AllowEdit: true` | `AllowEdit = true`, `EditFunction = "edit[FormName]"` | Edit column shown by `_DataGrid` |
| `AllowEdit: false` | `AllowEdit = false`, `EditFunction = null` | Edit column suppressed by `_DataGrid` |
| `AllowDelete: true` | `AllowDelete = true`, `DeleteFunction = "delete[FormName]"` | Delete column shown by `_DataGrid` |
| `AllowDelete: false` | `AllowDelete = false`, `DeleteFunction = null` | Delete column suppressed by `_DataGrid` |

`AllowAdd = false` is what controls whether the Add button HTML is emitted by `_DataGrid.cshtml` (via `@if (Model.AllowAdd)`). Setting only `AddFunction = null` leaves `AllowAdd` at its default of `true` and the button still appears.

---

## `KeyProperty` rule

`KeyProperty` must be the **row-level unique discriminator** — not the master filter field.

When a grid is filtered by a parent control (e.g. a "Choose a Test" dropdown that sets `testCode`), that parent value is **not** the row key. `KeyProperty` must identify a single row within the filtered set (e.g. `JobCode`). `_DataGrid.cshtml` puts this value in `data-id` on every row action button; JavaScript receives it via `$(btn).data('id')`.

---

## Step 18b — DataGrid Razor Views

### `Index.cshtml` — DataGrid section

> **The `.frm` file is the source of truth for Add/Edit/Delete.** Do NOT add a separate Add button in `Index.cshtml` — the Add button is rendered exclusively by `_DataGrid.cshtml` based on `Model.AllowAdd`. Only set `AllowAdd = true` when `AllowAdditions` is the default (enabled) in the subform `.frm`.

Inside `Index.cshtml`, the DataGrid block is simply:

```razor
<!-- DataGrid partial — Add/Edit/Delete buttons are controlled by AllowAdd/Edit/Delete in DataGridConfig -->
<div id="gridContainer_[formNameCamel]Grid">
    @await Html.PartialAsync("_DataGrid", Model.[GridName]Grid)
</div>
```

### JavaScript in `Index.cshtml` — `@section Scripts`

```javascript
// ALWAYS use gm.reloadGrid({ page: 1 }) — NOT gm.reload() (that method does not exist).
// Extra filter params (e.g. testCode, showRejected) are returned by ExtraFilterMethod automatically
// on every reload — do NOT pass them again inside reloadGrid({...}).
function reload[FormName]Grid() {
    var gm = window['gridManager_[formNameCamel]Grid'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

// ── Add — only include when AllowAdd: true ──────────────────────────────────
function add[FormName]() {
    // ALWAYS use AJAX GET into the shared #modaPopupBody — never replicate an inline modal.
    $.get('/[App]/[FormName]/Create', function(html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

// ── Edit — only include when AllowEdit: true ────────────────────────────────
// btn is the button ELEMENT passed by _DataGrid — use $(btn).data('id') for the row key.
function edit[FormName](btn) {
    var id = $(btn).data('id');
    $.get('/[App]/[FormName]/Edit', { id: id }, function(html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

// ── Delete — only include when AllowDelete: true ────────────────────────────
// btn is the button ELEMENT — use $(btn).data('id') for the row key.
// DataGrid delete uses confirm() — NOT a modal partial. No $.get to a Delete view.
function delete[FormName](btn) {
    var id = $(btn).data('id');
    if (confirm('Are you sure you want to delete this record?')) {
        $.ajax({
            url:  '/[App]/[FormName]/Delete?id=' + encodeURIComponent(id),
            type: 'DELETE',
            success: function(response) {
                if (response.success) {
                    alert('[FormName] deleted successfully');
                    reload[FormName]Grid();
                } else {
                    alert('Error: ' + (response.message || 'Delete failed.'));
                }
            },
            error: function() { alert('An error occurred while deleting.'); }
        });
    }
}

// ── Save (called from inside _AddEdit[FormName].cshtml) ─────────────────────
// Sends application/json — matches [FromBody] on the controller POST actions.
function save[FormName]() {
    var isEdit = $('#hdnIsEdit').val() === 'true';
    var dto = { /* collect field values from editable inputs in the partial */ };
    var url = isEdit
        ? '/[App]/[FormName]/Edit?id=' + encodeURIComponent($('#hdn[KeyName]').val())
        : '/[App]/[FormName]/Create';
    $.ajax({
        url:         url,
        type:        'POST',
        data:        JSON.stringify(dto),
        contentType: 'application/json; charset=utf-8',
        success: function(data) {
            if (data.success) { closeModal(); reload[FormName]Grid(); }
            else { alert('Save failed: ' + (data.message || '')); }
        },
        error: function() { alert('An error occurred while saving.'); }
    });
}

function closeModal() {
    $('#modaPopupBody').html('');
    $('#modalPopup').removeClass('show');
}
```

---

## Modal partial `_AddEdit[FormName].cshtml` — field editability

### Editability rules — from the Grid Operations Profile

Before writing a single `<input>`, apply the Grid Operations Profile classification to every field:

| Classification (from subform `.frm`) | Add mode rendering | Edit mode rendering |
|---|---|---|
| **Editable** — neither `Enabled = NotDefault` nor `Locked = NotDefault` | `<input asp-for="..." />` (no `disabled`) | `<input asp-for="..." />` unless it is a PK field |
| **Read-only** — both `Enabled = NotDefault` AND `Locked = NotDefault` | `<input asp-for="..." disabled />` | `<input asp-for="..." disabled />` |
| **Computed** — `ControlSource` contains a formula (e.g. `=[A]*[B]`) | `<input asp-for="..." disabled />` + JS recalc listener | `<input asp-for="..." disabled />` + JS recalc listener |
| **PK field(s)** | `<input type="hidden" asp-for="..." />` or omitted | `<input type="hidden" asp-for="..." />` — always hidden |

Important rules:
- **Use `disabled` (not `readonly`) for all non-editable display fields.** GOV.UK Frontend applies a grey background (`#f3f2f1`) via its CSS to `[disabled]` inputs — this is the only way to visually distinguish read-only fields from editable ones in a modal. The `readonly` attribute alone gets no GOV.UK styling and the field looks identical to an editable one.
- **`disabled` does not prevent JS access.** `document.getElementById('Field').value` works on disabled elements — JS recalculation functions work unchanged.
- **`disabled` does not submit the value** — if the underlying key/value is needed for the save payload, store it in a `<input type="hidden">`. Derived/display-only fields (joins, computed totals) do not need hidden companions.
- A field the subform marks as **locked is always `disabled`** in the modal — it is derived from a join or another table and must never be submitted as a writable value. Do not rely on the HTML prototype (which is a static mockup and may show all fields as editable)
- **Computed fields** must have a JavaScript `input`/`change` event listener that recalculates the display value when any of its contributing inputs change; they are never included in the save payload
- **Do not apply `disabled` globally** — evaluate each control individually from the Grid Operations Profile

### `_AddEdit[FormName].cshtml` template

```razor
@model Apha.FPSApps.Web.Areas.[App].Models.[FormName]Item

@{
    var isEditMode = Model.[KeyName] != default([KeyType]) && Model.[KeyName] is not null;
    // For string keys: var isEditMode = !string.IsNullOrEmpty(Model.[KeyName]);
    var title     = isEditMode ? "Edit [FormName]" : "Add [FormName]";
    var saveLabel = isEditMode ? "Update" : "Save";
}

<div class="modal-header flex-space-center">
    <h3 class="modal-title" style="margin: 0;">@title</h3>
    <button type="button" class="btn-close btn-red" onclick="closeModal()">X</button>
</div>
<div class="modal-body" style="padding-top: 5px;">
    <form class="row" id="form[FormName]">
        @Html.AntiForgeryToken()

        @* Hidden fields that identify the record *@
        <input type="hidden" asp-for="[KeyName]" id="hdn[KeyName]" />
        <input type="hidden" id="hdnIsEdit" value="@isEditMode.ToString().ToLower()" />

        @* ── Editable field (from Grid Operations Profile) ── *@
        <div class="col-12">
            <div class="govuk-form-group sup_margin_bottom_10">
                <label class="govuk-label govuk-!-font-weight-bold" asp-for="[EditableField]">
                    [Label from HTML]:
                </label>
                <input class="govuk-input govuk-!-font-size-16"
                       asp-for="[EditableField]"
                       id="[EditableField]"
                       type="text" />
                <span asp-validation-for="[EditableField]" class="govuk-error-message" style="display:none;"></span>
            </div>
        </div>

        @* ── Read-only field (Enabled = NotDefault AND Locked = NotDefault in subform .frm) ──
             Use disabled (not readonly) so GOV.UK Frontend renders a grey background          ── *@
        <div class="col-12">
            <div class="govuk-form-group sup_margin_bottom_10">
                <label class="govuk-label govuk-!-font-weight-bold" asp-for="[ReadOnlyField]">
                    [Label from HTML]:
                </label>
                <input class="govuk-input govuk-!-font-size-16"
                       asp-for="[ReadOnlyField]"
                       id="[ReadOnlyField]"
                       type="text"
                       disabled />
            </div>
        </div>

        @* ── Computed field (ControlSource is a formula) — disabled + JS recalc ──
             disabled keeps the GOV.UK grey styling; JS can still read/set .value  ── *@
        <div class="col-12">
            <div class="govuk-form-group sup_margin_bottom_10">
                <label class="govuk-label govuk-!-font-weight-bold" asp-for="[ComputedField]">
                    [Label from HTML]:
                </label>
                <input class="govuk-input govuk-!-font-size-16"
                       asp-for="[ComputedField]"
                       id="[ComputedField]"
                       type="text"
                       disabled />
            </div>
        </div>

        <div class="col-12" style="text-align: right; margin-top: 10px;">
            <button type="button"
                    class="govuk-button govuk-button--secondary sup_margin_0"
                    onclick="closeModal()">
                Cancel
            </button>
            <button type="button"
                    class="govuk-button sup_margin_0"
                    onclick="save[FormName]()">
                @saveLabel
            </button>
        </div>
    </form>
</div>

@section Scripts {
    <script>
        /* Recalculate computed field(s) whenever contributing inputs change.
           Only needed when at least one field has a formula ControlSource. */
        function recalc[FormName]() {
            var a = parseFloat(document.getElementById('[FieldA]').value) || 0;
            var b = parseFloat(document.getElementById('[FieldB]').value) || 0;
            document.getElementById('[ComputedField]').value = (a * b).toFixed(2);
        }
        document.getElementById('[FieldA]').addEventListener('input', recalc[FormName]);
        document.getElementById('[FieldB]').addEventListener('input', recalc[FormName]);
    </script>
}
```

---

## `_Delete[FormName].cshtml` — Delete confirmation partial

> Only generate when `AllowDelete: true` in the Grid Operations Profile.

```razor
@model Apha.FPSApps.Web.Areas.[App].Models.[FormName]Item

<div class="modal-header flex-space-center">
    <h3 class="modal-title" style="margin: 0;">Delete [FormName]</h3>
    <button type="button" class="btn-close btn-red" onclick="closeModal()">X</button>
</div>
<div class="modal-body" style="padding-top: 5px;">
    <p class="govuk-body">Are you sure you want to delete this record?</p>

    @* Hidden key field for the delete confirmation call *@
    <input type="hidden" id="hdn[KeyName]Delete" value="@Model.[KeyName]" />

    @* All fields are disabled — display only *@
    <div class="col-12">
        <div class="govuk-form-group sup_margin_bottom_10">
            <label class="govuk-label govuk-!-font-weight-bold" asp-for="[FieldName]">
                [Label from HTML]:
            </label>
            <input class="govuk-input govuk-!-font-size-16"
                   asp-for="[FieldName]"
                   type="text"
                   disabled />
        </div>
    </div>

    <div class="col-12" style="text-align: right; margin-top: 10px;">
        <button type="button"
                class="govuk-button govuk-button--secondary sup_margin_0"
                onclick="closeModal()">
            Cancel
        </button>
        <button type="button"
                class="govuk-button govuk-button--warning sup_margin_0"
                onclick="confirmDelete[FormName]()">
            Confirm Delete
        </button>
    </div>
</div>
```

---

## Subform master-detail DataGrid

When the DataGrid is a **subform** with `LinkChildFields` / `LinkMasterFields`:

- Add the link-child field as a second parameter on `Load[SubformName]Grid`:
  ```csharp
  [HttpPost]
  public async Task<IActionResult> Load[SubformName]Grid(
      PaginationFilter<string> request,
      string [linkChildField])  // e.g. string testCode
  ```
- In the repository, add `.Where(x => x.[LinkChildField] == [linkChildField])` before `ApplySorting`
- In `Index.cshtml`, pass the master filter value via `ExtraFilterMethod` — the callback is registered on `DataGridConfig.ExtraFilterMethod` and is called automatically on every `gm.reloadGrid()`. Do **not** pass the extra parameter inside the `reloadGrid({...})` call itself.

---

## Checklist — before marking DataGrid work done

- [ ] `[FormName]Item.cs` properties match Grid Operations Profile (editable fields have no `readonly` attribute; read-only fields do)
- [ ] `DataGridConfig.AddFunction` / `EditFunction` / `DeleteFunction` are `null` where the profile says disabled
- [ ] Add button in `Index.cshtml` grid header is **absent** when `AllowAdd: false`
- [ ] `Create` GET/POST endpoints only present when `AllowAdd: true`
- [ ] `Delete` GET/DELETE endpoints and `_Delete[FormName].cshtml` only present when `AllowDelete: true`
- [ ] `_AddEdit[FormName].cshtml` — every read-only field has `readonly` attribute; every computed field has `readonly` + JS recalc listener; only editable fields are in the save payload
- [ ] `gm.reloadGrid({ page: 1 })` used — NOT `gm.reload()`
- [ ] JavaScript `edit[FormName](btn)` and `delete[FormName](btn)` use `$(btn).data('id')` to get the row key — NOT a hardcoded value
- [ ] Composite key forms pass both key parts into the Edit/Delete URL

---

### Publish Phase 2b File Changes

Append a `## File Changes` section to the **existing** `zPostRunValidationArtefacts/[App]-[FormName]-DataGrid.md` (created in the prompt's step 2b).

> **Complete file structure at end of Phase 2b** — `[App]-[FormName]-DataGrid.md` must contain all three sections:
> 1. `## Grid Operations Profile` — full named profile block per subform DataGrid (created in prompt step 2b)
> 2. `## Column Editability` — per-column editability table per subform (created in prompt step 2b)
> 3. `## File Changes — Phase 2b DataGrid` — every file created/modified/removed this phase **(appended now)**
>
> If sections 1–2 are missing, create them before appending section 3.

```markdown
## File Changes — Phase 2b DataGrid

| # | Action | File path (relative to `src/`) | Reason |
|---|--------|-------------------------------|--------|
| 1 | CREATED | `Apha.FPSApps.Web/Areas/[App]/Models/[FormName]Item.cs` | Grid Item class for [SubformName] DataGrid |
| 2 | MODIFIED | `Apha.FPSApps.Web/Areas/[App]/Controllers/[FormName]Controller.cs` | Added Load[FormName]Grid, Create, Edit, Delete DataGrid endpoints |
| 3 | CREATED | `Apha.FPSApps.Web/Areas/[App]/Views/[FormName]/_AddEdit[FormName].cshtml` | Add/Edit modal partial (AllowAdd or AllowEdit: true) |
| 4 | MODIFIED | `Apha.FPSApps.Web/Areas/[App]/Views/[FormName]/Index.cshtml` | Added DataGrid container, JS functions, Add button |
```

Rules:
- **Action**: `CREATED` | `MODIFIED` | `REMOVED`
- One row per subform DataGrid if there are multiple — prefix the **Reason** with the subform name
- For `MODIFIED` files, state exactly which methods, sections, or blocks were added
- If `_Delete[FormName].cshtml` was **not** generated (DataGrid delete uses `confirm()` dialog), add a row: `SKIPPED | … | AllowDelete: true but DataGrid form — JS confirm() used instead`
- For `REMOVED` files, state why removal was necessary

---

> **Lock file — phase end:** Run `Get-Date -Format 'yyyy-MM-ddTHH:mm:ss'` and update `zPostRunValidationArtefacts/.codingagent-lock`:
> - Update the `Phase 2b — DataGrid Extensions` row: set `Status = COMPLETED` and fill in the `End` timestamp.
