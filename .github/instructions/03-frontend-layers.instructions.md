# Instruction: Frontend Layers (Steps 10–18)

> **Lock file — phase start:** Before writing any file, run `Get-Date -Format 'yyyy-MM-ddTHH:mm:ss'` and update `zPostRunValidationArtefacts/.codingagent-lock`:
> - Set `current-phase: Phase 2 — Frontend Layers`
> - Add a row: `| Phase 2 — Frontend Layers | <timestamp> | IN-PROGRESS | |`

> **Phase 2 gate — run this command immediately after updating the lock file:**
> ```powershell
> Test-Path "zPostRunValidationArtefacts/[App]-[FormName]-Frontend.md"
> ```
> - **`False`** → create the skeleton file now using the template below, then proceed to Step 10.
>   File-change rows are appended after each step as you go. The field mapping table is filled in at Step 17.
> - **`True`** → already exists from a previous run — verify both sections are present, then proceed.
>
> **Skeleton — create this file now if the result was `False`:**
> ```markdown
> # Frontend Analysis — [App] [FormName]
>
> ## Field Mapping
>
> *To be completed at Step 17 — fill in this table before writing any controller or view code.*
>
> ## File Changes — Phase 2 Frontend
>
> | # | Action | File path (relative to `src/`) | Reason |
> |---|--------|-------------------------------|--------|
> ```

Generate all frontend layers in order.

---

## Step 10 — Apha.FPSApps.Application: Frontend DTOs

**Location:** `Apha.FPSApps.Application/Dtos/[App]/`

Mirror the backend DTOs for use in the frontend application and infrastructure layers.

```csharp
namespace Apha.FPSApps.Application.Dtos.[App]
{
    public class [EntityName]Dto
    {
        // Same shape as Apha.[App].Application.Dtos.[EntityName]Dto
    }
}
```

> **→ Frontend.md:** Append a row to `## File Changes` for each file created in this step.

---

## Step 11 — Apha.FPSApps.Application: Frontend API Client Interface

**Location:** `Apha.FPSApps.Application/Interfaces/[App]ApiClients/`

**Pattern:**
```csharp
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.[App];
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.[App]ApiClients
{
    public interface I[App][FormName]ApiClient
    {
        Task<ApiResponseDto<List<[EntityView]Dto>>> GetAll[FormName]Async(QueryParameters<string> query);
        Task<ApiResponseDto<[Entity]Dto>> GetByIdAsync([KeyType] id);
        Task<ApiResponseDto<[Entity]Dto>> CreateAsync([Entity]Dto dto);
        Task<ApiResponseDto<[Entity]Dto>> UpdateAsync([Entity]Dto dto);
        Task<ApiResponseDto<bool>> DeleteAsync([KeyType] id);
        // Add lookup methods matching the API controller endpoints
    }
}
```

Register the new client on the aggregate API client interface:
```csharp
// In Apha.FPSApps.Application/Interfaces/[App]ApiClients/I[App]ApiClient.cs
public interface I[App]ApiClient
{
    // ... existing properties
    I[App][FormName]ApiClient [App][FormName] { get; }
}
```

> **→ Frontend.md:** Append rows to `## File Changes` for each file created or modified in this step.

---

## Step 12 — Apha.FPSApps.Application: Frontend Service Interface

**Location:** `Apha.FPSApps.Application/Interfaces/`

```csharp
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.[App];
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces
{
    public interface I[FormName]Service
    {
        Task<ApiResponseDto<List<[EntityView]Dto>>> GetAll[FormName]Async(QueryParameters<string> query);
        Task<ApiResponseDto<[Entity]Dto>> GetByIdAsync([KeyType] id);
        Task<ApiResponseDto<[Entity]Dto>> CreateAsync([Entity]Dto dto);
        Task<ApiResponseDto<[Entity]Dto>> UpdateAsync([Entity]Dto dto);
        Task<ApiResponseDto<bool>> DeleteAsync([KeyType] id);
    }
}
```

> **→ Frontend.md:** Append a row to `## File Changes` for each file created in this step.

---

## Step 13 — Apha.FPSApps.Application: Frontend Service Implementation

**Location:** `Apha.FPSApps.Application/Services/`

**Pattern:**
```csharp
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.[App];
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Interfaces.[App]ApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services
{
    public class [FormName]Service : I[FormName]Service
    {
        private readonly I[App]ApiClient _client;

        public [FormName]Service(I[App]ApiClient client)
        {
            _client = client;
        }

        public async Task<ApiResponseDto<List<[EntityView]Dto>>> GetAll[FormName]Async(QueryParameters<string> query)
        {
            return await _client.[App][FormName].GetAll[FormName]Async(query);
        }
        // ... delegate all methods to the API client
    }
}
```

Rules:
- Frontend services **only** delegate to `I[App]ApiClient` — no business logic here
- Business logic lives exclusively in `Apha.[App].Application.Services`

**Sonar compliance:**
- **S2933** — `_client` must be `private readonly`
- **S4144** — all methods are thin delegates; this is intentional and not a duplicate-implementation violation — do **not** collapse them

> **→ Frontend.md:** Append a row to `## File Changes` for each file created in this step.

---

## Step 14 — Apha.FPSApps.Infrastructure: API Client Implementation

**Location:** `Apha.FPSApps.Infrastructure/Integrations/[App]Apis/Clients/`

**Pattern:**
```csharp
using Apha.Common.Contracts.[App];
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.[App];
using Apha.FPSApps.Application.Interfaces.[App]ApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.[App]Apis.Clients
{
    public class [App][FormName]ApiClient : I[App][FormName]ApiClient
    {
        private readonly I[App]HttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public [App][FormName]ApiClient(I[App]HttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<[EntityView]Dto>>> GetAll[FormName]Async(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString("api/[formname]", query);
                var response = await _http.GetAsync<List<[EntityView]Res>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<[EntityView]Dto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<[EntityView]Dto>>>(response);
                return ApiResponseDto<List<[EntityView]Dto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<[EntityView]Dto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve [FormName] data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
        // ... implement all interface methods following the same try/catch pattern
    }
}
```

**Register on the aggregate client — update `[App]ApiClient.cs`:**
```csharp
public class [App]ApiClient : I[App]ApiClient
{
    public I[App][FormName]ApiClient [App][FormName] { get; }
    // existing properties...

    public [App]ApiClient(I[App]HttpExecutor http, IMapper mapper)
    {
        // existing assignments...
        [App][FormName] = new [App][FormName]ApiClient(http, mapper);
    }
}
```

Rules:
- Always wrap HTTP calls in `try/catch (Exception)` — return `FailureResponse` on error
- Use `_mapper.Map<ApiResponseDto<...>>(response)` for success mapping
- API URL path must **exactly match** the API controller's `[Route]` attribute
- Do **not** suppress the caught exception variable — use discard `catch (Exception)` to satisfy SonarQube S2139

**Sonar compliance:**
- **S2933** — `_http` and `_mapper` must be `private readonly`
- **S1192** — `InternalCodeError` is already declared as `private const string`; apply the same pattern to **every** literal error code and URL fragment in the class
- **S6966** — never call `await` inside a loop; if batching multiple API calls, use `Task.WhenAll` instead
- **S1481** — do not declare intermediate variables from VBA-style translations that are only assigned and never read

> **→ Frontend.md:** Append rows to `## File Changes` for each file created or modified in this step (including the updated aggregate client).

---

## Step 15 — Frontend AutoMapper Profiles and DI Registration

### 15a — `Apha.FPSApps.Infrastructure/Mappings/ApiDtoMapper.cs`

Add:
```csharp
CreateMap<[EntityView]Dto, [EntityView]Res>().ReverseMap();
CreateMap<[Entity]Dto, [Entity]Req>().ReverseMap();
CreateMap<[Entity]Dto, [Entity]Res>().ReverseMap();
// One entry per lookup entity (dropdown endpoints):
CreateMap<[LookupEntity]Res, [LookupEntity]Dto>();
```

> **Lookup entities must have their own mapper entry.** Every distinct entity returned by an API endpoint — including dropdown/lookup types (e.g. `TestOrProductRes` → `TestOrProductDto`) — must have an explicit `CreateMap` entry. Do **not** rely on the CRUD entity's mapper registration to handle lookup types; they are unrelated shapes and will throw a `Missing type map configuration` exception at runtime.

### 15b — `Apha.FPSApps.Web/Mappings/ViewModelMapper.cs`

Add:
```csharp
CreateMap<[FormName]Item, [EntityView]Dto>().ReverseMap();
CreateMap<[FormName]ViewModel, [Entity]Dto>().ReverseMap();
```

### 15c — `Apha.FPSApps.Web/Extensions/ServiceCollectionExtension.cs` — `AddServices()`

```csharp
services.AddScoped<I[FormName]Service, [FormName]Service>();
```

> **→ Frontend.md:** Append rows to `## File Changes` for each mapper profile and DI file modified in this step.

---

## Step 16 — Apha.FPSApps.Web: ViewModels

**Location:** `Apha.FPSApps.Web/Areas/[App]/Models/`

> **DataGrid forms only:** If the Grid Operations Profile confirms a DataGrid exists, also follow
> 📄 [03b — DataGrid Rules](./../instructions/03b-datagrid.instructions.md) for the `[FormName]Item.cs` class
> and all DataGrid-specific ViewModel, controller, and view content.

### Page ViewModel — `[FormName]ViewModel.cs`

```csharp
using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.[App].Models
{
    public class [FormName]ViewModel
    {
        // Scalar fields bound to form controls (ControlSource-bound fields)
        public [Type] [FieldName] { get; set; }

        // One DataGridConfig<T> per table/subform in the MS Access form
        // ONLY include if DefaultView = 1 or 2 in the .frm file AND a <table> exists in the HTML prototype
        // If DefaultView = 0 (Single Form) or no <table> in HTML — omit this property entirely
        public DataGridConfig<[FormName]Item> [GridName]Grid { get; set; } = new();

        // Dropdowns — populated from RowSource queries
        public List<SelectListItem> [LookupName]Options { get; set; } = new();
    }
}
```

### ViewModel Field Naming Rules — STRICTLY ENFORCED

> These rules exist to prevent mismatched `asp-for` bindings and mislabelled form fields (e.g. a "Transfer Income" label bound to a `BudgetCvl` property).

1. **Every ViewModel property name must exactly match the corresponding `[Entity]Dto` / C# entity property name.** Never invent aliases, prefix with `Lk_`, or rename a field to match the Access control name. The only exception is navigation metadata (e.g. `OldJobCode`, `SelectedProgramme`) that has no entity counterpart.

2. **One field in the entity = one property in the ViewModel.** If the entity has `TransferIncome` and `BudgetCvl` as separate fields, the ViewModel must have both as separate properties — never merge or alias them.

3. **Lookup fields that resolve to a string/code value in the entity must use the entity field name directly.** For example, if the entity stores the status as `ProjectStatus string`, the ViewModel property is `ProjectStatus`, not `Lk_Status` or `StatusCode`. The Access VBA `Lk_` prefix is an Access-internal convention that must not appear in C#.

4. **Each dropdown list property must be named `[FieldName]List`** — where `[FieldName]` is the exact name of the bound scalar property. For example, `ProjectStatus` is bound to `StatusList`, `Disease` to `DiseaseList`, `Contract` to `ContractList`.

5. **Right-column fields come from the HTML right column only.** Cross-check every field placement against the HTML prototype before writing the ViewModel. Do not move a field to a different column because it "seems" to belong there.

> **→ Frontend.md:** Append a row to `## File Changes` for each ViewModel file created in this step.

---

## Step 17 — Apha.FPSApps.Web: MVC Controller

**Location:** `Apha.FPSApps.Web/Areas/[App]/Controllers/`

> ⚠️ **Fill in the `## Field Mapping` section of `[App]-[FormName]-Frontend.md`** (created at phase start) before writing any controller, ViewModel, or `.cshtml` file. Complete every row of the HTML → entity mapping table in the section below, then proceed to writing the controller.

### Controller Field-Mapping Rules — STRICTLY ENFORCED

1. **Complete the HTML → entity field mapping table before writing `PopulateDropdownsAsync` or any model initialisation.** Every `<input>`, `<select>`, and `<textarea>` in the HTML prototype must have a confirmed entity field or be explicitly marked as unmapped.

   **Create `zPostRunValidationArtefacts/[App]-[FormName]-Frontend.md`** immediately after completing this table and before writing any controller or view code. The file must contain:

   | HTML `id` / label text | Entity field name | ViewModel property | `asp-for` value | Notes |
   |---|---|---|---|---|
   | (from HTML prototype) | (from `[Entity].cs`) | (must match entity field) | (must match ViewModel property) | Unmapped fields: `// TODO` |

   Also include:
   - **Input count check:** total `<input>` + `<select>` + `<textarea>` elements in the HTML prototype vs. `asp-for` bindings generated (must be equal, or each gap is a `TODO`)
   - **Dropdown sources:** for each `<select>`, the lookup entity and DTO property names used in `PopulateDropdownsAsync`

2. **If a UI field cannot be mapped to an entity/DTO property** (field exists in the HTML prototype but has no matching property in the entity or DTO), do **not** invent a property name or silently omit it. Instead, add a `// TODO` comment in `PopulateDropdownsAsync` or in the `Index` action where the model is populated:
   ```csharp
   // TODO: [HtmlFieldId] (“[Label text]”) — no matching entity field found.
   // Confirm the correct entity property with the team before implementing.
   ```
   Also add a matching `// TODO` comment on a placeholder property in the ViewModel:
   ```csharp
   // TODO: [HtmlFieldId] — no entity field mapped. Confirm with team.
   public string? [HtmlFieldId]Unmapped { get; set; }
   ```
   A clearly marked TODO is always preferable to a silent omission or a wrong mapping.

3. **Dropdown `Selected` comparisons must use the same property as `asp-for`.** When building `SelectListItem` lists in `PopulateDropdownsAsync`, the `Selected = string.Equals(model.[PropertyName], ...)` line must reference the exact ViewModel property that the dropdown’s `asp-for` binds to.
4. **`PopulateDropdownsAsync` must use the correct DTO type for the lookup endpoint — not the CRUD DTO.** Before writing `PopulateDropdownsAsync`, confirm the return type of `GetTestItemsAsync()` (or equivalent) at the service interface. If the service returns `ApiResponseDto<List<TestOrProductDto>>`, use `t.ItemCode` and `t.ItemDescription`, **not** `t.TestCode`. Using a property from the wrong DTO type will compile (if the CRUD DTO happens to have a similarly named property) but will either silently produce wrong values or throw a mapper exception at runtime. The rule: **property names in `PopulateDropdownsAsync` must come from the `[LookupEntity]Dto`, not from the CRUD `[Entity]Dto`.**
**LAYER BOUNDARY — STRICTLY ENFORCED:**

> MVC controllers in `Apha.FPSApps.Web` must **only** inject `IXxxService` interfaces from `Apha.FPSApps.Application`. They must **never** inject:
>
> - `I[App]ApiClient` (infrastructure aggregate client — e.g. `IPactApiClient`, `IFpsApiClient`)
> - Any `I[App][Entity]ApiClient` (individual infrastructure HTTP client — e.g. `IPactWorkGroupApiClient`)
> - Any repository interface from the backend (`I[Entity]Repository`)
> - Any `DbContext` type
>
> The dependency graph must be: `MVC Controller → IXxxService → I[App]ApiClient → I[App][Entity]ApiClient → HTTP`
>
> If dropdown population requires data from a second domain — for example, populating a WorkGroup dropdown alongside a primary MOLog service — **add `GetAllWorkGroupsAsync()` to an already-injected service** (e.g. the primary `I[FormName]Service`, or an existing `I[RelatedEntity]Service` that already queries work-groups), **or** inject that second service interface. Never bypass the service layer by injecting the API client directly.
>
> **Common violation pattern (WRONG):**
> ```csharp
> // ❌ WRONG — _pactClient is infrastructure, must not appear in an MVC controller
> private readonly IPactApiClient _pactClient;
> public async Task PopulateDropdownsAsync(ViewModel vm)
> {
>     var wg = await _pactClient.PactWorkGroup.GetAllWorkGroupsAsync(); // layer violation
> }
> ```
>
> **Correct pattern:**
> ```csharp
> // ✅ CORRECT — call through the service interface only
> private readonly IProjectJobCodeService _jobCodeService; // already has GetAllWorkGroupsAsync()
> public async Task PopulateDropdownsAsync(ViewModel vm)
> {
>     var wg = await _jobCodeService.GetAllWorkGroupsAsync(); // service layer respected
> }
> ```

**Pattern:**
```csharp
using Apha.FPSApps.Application.Dtos.[App];
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.[App].Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.[App].Controllers
{
    [Area("[App]")]
    [Authorize(Roles = "[App]Admin,[App]User")]
    [AuthorizeForScopes(ScopeKeySection = "[App]ApiSettings:Scope")]
    public class [FormName]Controller : Controller
    {
        private readonly IMapper _mapper;
        private readonly I[FormName]Service _service;
        // Add further IXxxService fields for any additional lookups needed by dropdowns.
        // Never add I[App]ApiClient or any I[App][Entity]ApiClient here.

        public [FormName]Controller(IMapper mapper, I[FormName]Service service)
        {
            _mapper = mapper;
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new [FormName]ViewModel
            {
                // Populate dropdowns from lookup service calls
                // DataGrid forms: also build DataGridConfig — see 03b
            };
            return View(viewModel);
        }
    }
}
```

> **DataGrid forms:** `Load[FormName]Grid`, `Get[FormName]GridConfigAsync`, `Create`, `Edit`, and `Delete`
> controller endpoints are documented in
> 📄 [03b — DataGrid Rules](./../instructions/03b-datagrid.instructions.md) (Step 17b).
```

> **→ Frontend.md:** Append rows to `## File Changes` for the controller file and any supporting files created in this step.

---

## Step 18 — Apha.FPSApps.Web: Razor Views

This step generates `Index.cshtml` and up to three partial views — one per HTML prototype present for the form. All files are placed in the **same folder**:

**Location:** `Apha.FPSApps.Web/Areas/[App]/Views/[FormName]/`

### HTML Prototype → View file mapping

Two patterns are supported. Identify which is present before generating any views.

**Single-file pattern** — `[FormName].html` exists (no `-add`/`-edit` files):

| HTML source file | Generated file | Model type | Note |
|---|---|---|---|
| `[FormName].html` | `Index.cshtml` | `[FormName]ViewModel` | One view only — no CRUD partials generated |

**Multi-file pattern** — `[FormName]-add.html` and `[FormName]-edit.html` exist:

| HTML source file | Generated file | Model type | Returned by |
|---|---|---|---|
| *(grid pattern — no dedicated prototype)* | `Index.cshtml` | `[FormName]ViewModel` | `Index` (GET) |
| `[FormName]-add.html` | `_AddEdit[FormName].cshtml` | `[FormName]Item` | `Create` (GET) |
| `[FormName]-edit.html` | `_AddEdit[FormName].cshtml` | `[FormName]Item` | `Edit` (GET) |
| `[FormName]-delete.html` | `_Delete[FormName].cshtml` | `[FormName]Item` | `Delete` (GET) *(only if file exists; **not** applicable to DataGrid forms — see note below)* |

> **DataGrid forms — delete exception.** DataGrid delete does **not** use a modal partial, a `_Delete[FormName].cshtml`, or a `Delete (GET)` controller action. If a `[FormName]-delete.html` prototype exists, **ignore it** for DataGrid forms. Delete is handled by a browser `confirm()` dialog in JavaScript followed by a direct `[HttpDelete]` AJAX call. See 03b — DataGrid Rules (Step 18b) for the template.

> **One combined partial for Add and Edit.** `Create (GET)` and `Edit (GET)` both return the same `_AddEdit[FormName].cshtml` partial. Use a hidden `<input id="hdnIsEdit" value="...">` field inside the partial to distinguish modes at save time. The partial sets the title and button label based on whether the model's key field is populated. See `_AddEditStaff.cshtml` and `_AddEditStaffJob.cshtml` in `Views/StaffMaintenance/` and `Views/StaffJob/` as canonical references.

> **Never copy an inline modal from the HTML prototype.** The HTML prototype may contain a `<div class="modal">` — do NOT replicate it in the Razor view. The shared `#modalPopup` / `#modaPopupBody` in `_Layout.cshtml` is the only modal container used in this application. CRUD partial content is always loaded via `$.get(...)` into `#modaPopupBody`, then `$('#modalPopup').addClass('show')` is called to display it.

### Add vs Edit vs Delete — difference rules

> **DataGrid forms:** The complete `_AddEdit[FormName].cshtml` template,
> field editability rules (Editable / Read-only / Computed from the Grid Operations Profile), and
> the JavaScript save/delete/closeModal patterns are all in
> 📄 [03b — DataGrid Rules](./../instructions/03b-datagrid.instructions.md) (Step 18b).
>
> Summary:
> - Key field → always `<input type="hidden">` in edit modal
> - Read-only fields (both `Enabled=NotDefault` AND `Locked=NotDefault` in subform) → always `readonly` in modal
> - Computed fields (formula `ControlSource`) → always `readonly` + client-side JS recalc, never in save payload
> - **Delete → JS `confirm()` dialog only, no modal partial** — there is no `_Delete[FormName].cshtml` and no `Delete (GET)` action for DataGrid forms
> - Use `[FormName]Item` as the model for `_AddEdit[FormName].cshtml`

---

### View Binding Rules — STRICTLY ENFORCED

> These rules prevent the class of bug where a label says "Transfer Income" but the `asp-for` points to `BudgetCvl`.

1. **Every `asp-for` attribute must bind to the ViewModel property whose name matches the entity field being displayed.** Before writing any `<input asp-for="...">` or `<select asp-for="...">`, confirm the property name matches the entity/DTO. Never bind a field to a same-type neighbour just because the label text matches.

2. **Derive the label text from the HTML prototype label, not from the `Display(Name)` attribute.** The `Display` attribute is generated from the field name — the HTML prototype label is the authoritative UI label. If they differ, the HTML prototype wins.

3. **Each HTML form column maps to the corresponding ViewModel section — do not cross columns.** Fields in the HTML `form-left` div go in the left column in the View; fields in `form-center` go in the right column. Never move a field to a different column during conversion.

4. **Verify the full field list against the HTML prototype before writing any `<input>` or `<select>` tags.** Build a mapping table mentally or in a comment: `HTML id → entity field name → ViewModel property name → asp-for value`. All four must be consistent.

5. **For every text input that maps to a currency/decimal field, use `class="govuk-input currency-input"`** and confirm the property type in the DTO is `decimal?` (not `string`), matching the entity.

6. **No HTML field may be omitted from the View.** Every `<input>`, `<select>`, and `<textarea>` present in the HTML prototype must appear in the generated View. Work through the HTML prototype from top to bottom — left column first, then right column — and emit a View control for each one in order. After writing every control, count the HTML fields and the generated `asp-for` bindings: the counts must match.

7. **If a UI field has no confirmed entity mapping, render it with a `TODO` comment instead of omitting it or guessing:**
   ```razor
   @* TODO: [html-id] (“[Label text]”) — no entity field confirmed. Renders as disabled until mapping is resolved. *@
   <div class="govuk-form-group sup_margin_0">
       <label class="govuk-label" for="[html-id]">[Label text]:</label>
       <input type="text" id="[html-id]" class="govuk-input" disabled
              title="TODO: map to entity field" />
   </div>
   ```
   A visible placeholder with a TODO is always preferable to a silent omission.

**General View rules:**
- Replicate the HTML prototype **exactly** — same DOM structure, CSS classes, IDs, inline styles, section order
- Do **not** rename, remove, reorder, or redesign any element
- Replace form controls with ASP.NET Core Tag Helpers (`asp-for`, `asp-action`, `asp-controller`, `asp-items`)
- Add `@model` directive with the strongly typed ViewModel
- Include `@Html.AntiForgeryToken()` in every `<form>`
- Use `@Html.ValidationSummary()` and `asp-validation-for` for field-level validation messages
- **Only** use `@await Html.PartialAsync("_DataGrid", Model.[GridName]Grid)` wrapped in `<div id="gridContainer_[gridId]">` when the FRM `DefaultView` is `1` or `2` **AND** the HTML prototype contains a `<table>` with data rows. If `DefaultView = 0` (Single Form) or no `<table>` exists in the HTML — omit the DataGrid partial entirely
- Keep all JavaScript from the HTML prototype unchanged — add it under `@section Scripts { }`
- Set `ViewData["Title"]` to match the MS Access form caption

**Template:**
```razor
@using Apha.FPSApps.Web.Models.Components.DataGrid
@model Apha.FPSApps.Web.Areas.[App].Models.[FormName]ViewModel

@{
    ViewData["Title"] = "[FPS Year] : [Form Caption]";
}

<main id="main-content">
    <div class="container-fluid">
        <!-- Breadcrumb -->
        <div class="govuk-grid-row">
            <div class="govuk-grid-column-full">
                <div style="padding: 5px 5px; border-bottom: 1px solid #b4b4b4" class="row">
                    <div style="display: flex; font-size: 14px; align-items: center;">
                        <nav aria-label="Breadcrumb">
                            <a href="@Url.Action("Index", "Home", new { area = "[App]" })">Home</a> &gt; [Breadcrumb from HTML]
                        </nav>
                    </div>
                </div>
            </div>
        </div>

        <!-- Title -->
        <div class="govuk-grid-row">
            <div class="govuk-grid-column-full">
                <div style="border-bottom: 2px solid #1d70b8; height:48px;">
                    <h1 style="padding: 15px 0 5px 17px; font-size:16px; font-weight:700; float:left; margin:0;">
                        [Form Title from HTML]
                    </h1>
                </div>
            </div>
        </div>

        <!-- Grid Container (one per MS Access subform/continuous form) -->
        <div id="gridContainer_[gridId]">
            @await Html.PartialAsync("_DataGrid", Model.[GridName]Grid)
        </div>
    </div>
</main>

@section Scripts {
    <script>
        // DataGrid JavaScript (reloadGrid, add, edit, delete, save, closeModal functions):
        // see 03b — DataGrid Rules (Step 18b) for the complete template.
        // For non-DataGrid forms, paste the JavaScript from the HTML prototype here unchanged.
    </script>
}
```

> **DataGrid forms:** The full `Index.cshtml` DataGrid block (Add button, grid container, JS functions) and
> `_AddEdit[FormName].cshtml` template are in
> 📄 [03b — DataGrid Rules](./../instructions/03b-datagrid.instructions.md) (Step 18b).

---

### `_Add[FormName].cshtml` / `_Edit[FormName].cshtml` / `_Delete[FormName].cshtml`

> **DataGrid forms only.** These partial views are fully documented in
> 📄 [03b — DataGrid Rules](./../instructions/03b-datagrid.instructions.md) (Step 18b).
>
> For reference the model types are:
> - `_AddEdit[FormName].cshtml` — `@model [FormName]Item` (Add and Edit share one partial; used when AllowAdd or AllowEdit is true)
> - **No `_Delete[FormName].cshtml` for DataGrid forms** — delete uses a JS `confirm()` dialog, not a modal partial

> **→ Frontend.md:** Append rows to `## File Changes` for every view and partial file created in this step.

---

### Verify Phase 2 File Changes

> **Verify `[App]-[FormName]-Frontend.md` is complete** — open the file and confirm both sections are fully populated:
> 1. `## Field Mapping` — all HTML inputs mapped, input count check done, dropdown sources listed.
> 2. `## File Changes — Phase 2 Frontend` — a row exists for every file created or modified during Steps 10–18.
>
> If any row is missing, append it now. File changes are written progressively at each step — this is a completeness check only.
>
> **Rules for each row:**
> - **Action**: `CREATED` | `MODIFIED` | `REMOVED`
> - For `MODIFIED` files, the **Reason** must state what was added or changed specifically
> - Do not omit modified files (API client aggregates, mapper profiles, DI registrations)

---

> **Lock file — phase end:** Run `Get-Date -Format 'yyyy-MM-ddTHH:mm:ss'` and update `zPostRunValidationArtefacts/.codingagent-lock`:
> - Update the `Phase 2 — Frontend Layers` row: set `Status = COMPLETED` and fill in the `End` timestamp.
