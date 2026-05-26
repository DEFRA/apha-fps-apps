# Instruction: Code Standards, Naming Conventions & DataGrid Rules

---

## Code Quality Rules (SonarQube)

Apply these rules to **every** generated file. Rules are grouped by the layer where each is most likely to be violated.

### General — all files

| Sonar rule | Implementation |
|---|---|
| **S1192** No magic strings | Use `private const string` for repeated literals (especially error codes like `"INTERNAL_ERROR"`) |
| **S2139** No suppressed exceptions | Use `catch (Exception)` without variable if not logging; do **not** catch and ignore silently |
| **S8714** Nullable reference types | Enable `?` on all optional references; use `!` only on guaranteed non-null navigations |
| **S1128** No unused `using` directives | Include only the `using` statements required by the generated file |
| **S4462** Async all the way | All data access methods must be `async Task<T>` — no `.Result` or `.Wait()` |
| **S4457** No `var` for non-obvious types | Use explicit types for return values assigned from method calls |
| **S1481** No unused local variables | Every declared local variable must be read at least once; remove temporaries that are only assigned |
| **S1172** No unused method parameters | Every parameter must be used in the method body; use discards (`_`) only when required by an interface |
| **S109** No magic numbers | Use `private const int` for numeric literals that appear more than once (e.g., page sizes, counts) |
| **S2068** No hardcoded credentials | Connection strings, API keys, and passwords must come from injected configuration — never as literals |
| **S101** Naming follows conventions | Class, method, and property names follow the naming table below — never abbreviate or use Access VBA casing |

### Constructor-injected fields (repositories, services, clients)

| Sonar rule | Implementation |
|---|---|
| **S2933** Readonly fields | All `private` fields assigned **only** in the constructor must be declared `readonly`: `private readonly IMapper _mapper;` |
| **S4136** Single responsibility | One class per file; never add helper methods to a class they don't logically belong to |

### Service layer (Application Services)

| Sonar rule | Implementation |
|---|---|
| **S4457** Sync validation before async | Validate all non-nullable parameters **before** the first `await` — throw synchronously so callers get immediate feedback: `ArgumentNullException.ThrowIfNull(dto);` |
| **S3776** Cognitive complexity ≤ 15 | VBA subs with deeply nested `If`/`ElseIf` chains must be split into focused private helper methods rather than translated line-by-line into a single C# method |
| **S6966** No `await` in loops | Never call `await repo.SaveAsync()` inside a `foreach`; collect entities and call a batch method after the loop |
| **S107** Max 7 parameters | If a stored procedure has more than 7 parameters, group them into a typed request DTO rather than exposing each as a separate method parameter |

### Repository layer (DataAccess)

| Sonar rule | Implementation |
|---|---|
| **S2971** Use `.AnyAsync()` not `.CountAsync() > 0` | Guard checks in repositories must use `await _dbContext.T.AnyAsync(x => x.F == v)` — **not** `Count() != 0` or `Count() > 0` |
| **S3358** No nested ternaries | LINQ `select new` projections with conditional logic must use `if/else` helpers, not nested `? :` |
| **S2259** No null dereference | Always null-check results from `FirstOrDefaultAsync` before accessing properties |

### API controller layer

| Sonar rule | Implementation |
|---|---|
| **S1135** Track TODO stubs | All `// TODO: implement [sp name]` stubs generated for unavailable sub-SPs must be tracked — do **not** suppress Sonar warnings on them |
| **S6562** Anti-forgery | All `[HttpPost]` MVC actions must have `[ValidateAntiForgeryToken]` |
| **S2325** XML docs | All `public` API controller actions must have `<summary>` XML documentation |

### Infrastructure / HTTP client layer

| Sonar rule | Implementation |
|---|---|
| **S4456** Proper `IDisposable` | Do not instantiate `HttpClient` directly — always use `IHttpClientFactory` via `IHttpExecutor` |

---

## Naming Conventions

| Element | Convention | Example |
|---|---|---|
| Entity class | PascalCase, singular | `StaffJob` |
| Entity view class | PascalCase + `View` suffix | `StaffJobView` |
| Repository interface | `I` + EntityName + `Repository` | `IStaffJobRepository` |
| Service interface (API) | `I` + FormName + `Service` | `IStaffJobService` |
| Service interface (Web) | `I` + FormName + `Service` | `IStaffJobService` |
| API client interface | `I[App]` + FormName + `ApiClient` | `IFps[FormName]ApiClient` |
| DTO (API layer) | EntityName + `Dto` | `StaffJobDto` |
| DTO (Web layer) | EntityName + `Dto` (in `Dtos/[App]/`) | `StaffJobDto` |
| Request contract | EntityName + `Req` | `StaffJobReq` |
| Response contract | EntityName + `Res` | `StaffJobRes` |
| Grid item model | FormName + `Item` | `StaffJobItem` |
| Page ViewModel | FormName + `ViewModel` | `StaffJobViewModel` |
| API Controller | FormName + `Controller` | `StaffJobController` |
| MVC Controller | FormName + `Controller` | `StaffJobController` |
| API route | `api/[formname]` (lowercase) | `api/staffjob` |
| MVC area route | `/[App]/[FormName]/[Action]` | `/FPS/StaffJob/Index` |
| Grid ID | `[formNameCamel]Grid` | `staffJobGrid` |
| Grid load URL | `/[App]/[FormName]/Load[FormName]Grid` | `/FPS/StaffJob/LoadStaffJobGrid` |

---

## Field Name Preservation

- **Never rename** fields, labels, CSS classes, or IDs from the HTML prototype
- Map MS Access `ControlSource` property to the ViewModel property with the **same display name**
- Map MS Access column names to C# properties using PascalCase translation only (e.g., `staff_id` → `StaffId`), but keep `[Display(Name = "...")]` matching the original HTML label text exactly
- **`HasColumnName` in `OnModelCreating` must always use lowercase** — all column names in the PostgreSQL database are lowercase. Always pass a lowercase string literal: `entity.Property(e => e.FpsYear).HasColumnName("fpsyear")`. Passing PascalCase or mixed-case values will silently break queries at runtime because PostgreSQL treats unquoted identifiers as lowercase

### ViewModel / View / Controller Field Naming — Zero-Tolerance Rules

These rules are enforced across ViewModel, Razor View, and MVC controller. Violations cause silent data loss or mislabelled fields that are hard to detect in testing.

| Rule | Correct | Wrong |
|---|---|---|
| ViewModel property name matches the entity/DTO field name exactly | `public string ProjectStatus { get; set; }` | `public string Lk_Status { get; set; }` |
| Access VBA `Lk_` prefix must never appear in C# | `Disease`, `Contract` | `Lk_Disease`, `Lk_Contract` |
| Each distinct entity field gets its own ViewModel property | `TransferIncome` + `BudgetCvl` as two properties | `BudgetCvl` reused for both |
| `asp-for` binds to the property whose name matches the entity field | `asp-for="TransferIncome"` on the Transfer Income row | `asp-for="BudgetCvl"` on the Transfer Income row |
| Label text in the View matches the HTML prototype label, not the property name | Label `Transfer Income:` + `asp-for="TransferIncome"` | Label `Transfer Income:` + `asp-for="BudgetCvl"` |
| Dropdown list property named `[FieldName]List` | `StatusList` bound to `ProjectStatus` | `Lk_StatusList` or `StatusOptions` |
| Fields stay in the HTML column they came from | Budget in right column if HTML shows it right | Budget moved to left column arbitrarily |

**Verification step — mandatory before writing ViewModel/View code:**  
Build this mapping for every form field and confirm all four columns are consistent before writing a single property or `asp-for`:

| HTML `id` / label | Entity field name | ViewModel property | `asp-for` value |
|---|---|---|---|
| `CustomerIncome` / Customer Income | `CustIncome` | `CustIncome` | `CustIncome` |
| `TransferIncome` / Transfer Income | `TransferIncome` | `TransferIncome` | `TransferIncome` |
| `Budget` / Budget | `BudgetCvl` | `BudgetCvl` | `BudgetCvl` |
| `Status` / Status | `ProjectStatus` | `ProjectStatus` | `ProjectStatus` |

Failure to complete this mapping before writing output files is the root cause of label/input mismatches.

---

## DataGrid Usage Rules

### When to include a DataGrid — MANDATORY two-condition check

Before adding any DataGrid component, **both** of the following conditions must be true. If either condition fails, do **not** include a DataGrid.

**Condition 1 — FRM `DefaultView` check:**

Read the `DefaultView` property from the `.frm` file:

| `DefaultView` value | MS Access view | Include DataGrid? |
|---|---|---|
| `0` | Single Form (one record at a time) | **NO** — form is a create/edit form only |
| `1` | Continuous Forms (scrollable list of records) | **YES** |
| `2` | Datasheet (grid/table layout) | **YES** |
| *(absent / not set)* | Defaults to Single Form | **NO** |

**Condition 2 — HTML prototype `<table>` check:**

Scan the HTML prototype for a `<table>` or `<thead>`/`<tbody>` element that contains data rows (not just a layout table). If no such element exists, do **not** include a DataGrid regardless of the FRM value.

> **Example of this rule in practice:** `frmProgrammeNewProject` has `DefaultView = 0` (Single Form) and the HTML prototype contains no `<table>` element — therefore no DataGrid is included.

### DataGrid implementation (only when both conditions above are met)

> **BEFORE writing any DataGrid view or modal dialog, read an existing working DataGrid view in the same application.** The reference views are:
> - `Apha.FPSApps.Web/Areas/FPS/Views/StaffMaintenance/Index.cshtml` — inline CRUD with shared modal
> - `Apha.FPSApps.Web/Areas/FPS/Views/ProgramStaffPlan/Index.cshtml` — master-detail grid with AJAX
> - `Apha.FPSApps.Web/wwwroot/js/fps_js/StaffJob.js` — canonical CRUD JS pattern
>
> These files are the authoritative implementation references. Generate the new view and modal by following their exact patterns — do **not** adapt the HTML prototype's modal structure.

Every `<table>` from the HTML prototype that displays data from a `RecordSource` query **must** use the `_DataGrid.cshtml` partial view.

1. Create a `[Name]Item.cs` class with `[GridColumn]` and `[Display]` attributes per column
2. Configure a `DataGridConfig<[Name]Item>` in the controller (private helper method pattern)
3. Wrap partial in `<div id="gridContainer_[gridId]">...</div>`
4. Provide a `[HttpPost] Load[Name]Grid(PaginationFilter<string> request)` action returning `PartialView("_DataGrid", config)`
5. Add matching API endpoint that returns `PaginationRes<[EntityView]Res>`
6. `BindGridUrl` must point to the controller's `Load[Name]Grid` action path
7. `GridDataProvider.GetColumnsDefination<[Name]Item>(filterOptionsSource)` generates columns from attributes
8. **Grid manager API — always use `gm.reloadGrid({ page: 1 })`** to trigger a grid refresh from JavaScript. The method is named `reloadGrid`, **not** `reload` (which does not exist and will silently do nothing). The grid manager is retrieved via `window['gridManager_' + gridId]`. Extra filter parameters such as a master dropdown value or a checkbox state must be wired through the `ExtraFilterMethod` callback on `DataGridConfig` — **do not pass them as extra properties inside the `reloadGrid({...})` call**; they are picked up automatically on every reload.

9. **`KeyProperty` must be the row-level unique discriminator — not the master filter field.** When a grid is filtered by a parent control (e.g. a "Choose a Test" dropdown setting `testCode`), that parent value is **not** the row key. `KeyProperty` must be the field that uniquely identifies a single row within the filtered set (e.g. `JobCode`). `_DataGrid.cshtml` puts this value in `data-id` on every row button, so the JS functions receive the correct per-row identifier via `$(btn).data('id')`.

> **DataGrid CRUD operations and field editability** (button visibility, `AddFunction`/`EditFunction`/`DeleteFunction` null rules, modal field `readonly`/computed rendering) are documented in
> 📄 [03b — DataGrid Rules](./../instructions/03b-datagrid.instructions.md).

---

## Multiple Entities / Lookup Forms

When the `.frm` file references multiple tables or subforms:
- Create a separate Repository and Service per entity
- Create a separate API client interface and implementation per logical data group
- Use **one** MVC controller for the entire form
- Use **one** API controller for the main form; add separate controllers only if the lookup data belongs to a different `[App]` domain
- Populate lookup `SelectListItem` lists in `Index()` by calling dedicated lookup service methods

### Subform DataGrid identification

A `Begin Subform` block in the main `.frm` with `SourceObject = "Form.fsubXxx"` produces data displayed as a scrollable grid in the HTML prototype area. Apply the standard two-condition DataGrid check to the **subform's own `.frm`**:
- Check the subform `.frm`'s own `DefaultView` value (not the parent form's)
- Confirm a `<table>` element exists in the subform area of the HTML prototype

When both conditions are met, create `[SubformName]Item.cs` (not `[FormName]Item.cs`) for the grid columns. Add a `DataGridConfig<[SubformName]Item>` property to the parent `[FormName]ViewModel`. Add a `Load[SubformName]Grid(PaginationFilter<string> request)` action to the MVC controller.

### Subform master-detail link

`LinkChildFields` / `LinkMasterFields` in the subform control define the master-detail join:
- `LinkChildFields` = the field on the subform's entity used as the filter key
- `LinkMasterFields` = the parent form control whose selected value is passed as the filter

Translate to: an additional mandatory parameter on `Load[SubformName]Grid(PaginationFilter<string> request, [KeyType] [linkChildField])`, added as a `.Where(x => x.[LinkChildField] == linkChildField)` clause in the repository LINQ query. Pass the parameter value from the parent filter control (e.g. a `<select>`) via the JavaScript grid reload call.
