# Division Maintenance AddEdit Pattern Update

## Summary
Updated the `DivisionMaintenanceController` and `_AddEditDivision.cshtml` view to follow the same pattern used in `ProgramMaintenanceController` and `_AddEditProgram.cshtml`.

## Changes Made

### 1. DivisionMaintenanceController.cs
**File:** `Apha.FPSApps\Apha.FPSApps.Web\Areas\FPS\Controllers\DivisionMaintenanceController.cs`

#### Updated `Create()` GET Method
- **Before:** Returned partial view without a model
- **After:** Initializes an empty `DivisionViewModel` with default values and passes it to the view

```csharp
public IActionResult Create()
{
    var model = new DivisionViewModel
    {
        DivName = string.Empty,
        AgencyId = 0,
        CentOverhead = null
    };
    return PartialView("_AddEditDivision", model);
}
```

**Rationale:** The view expects a `Model` object to determine if it's in edit mode (`Model?.DivName`). Without passing a model, this would cause a null reference issue.

### 2. _AddEditDivision.cshtml View
**File:** `Apha.FPSApps\Apha.FPSApps.Web\Areas\FPS\Views\DivisionMaintenance\_AddEditDivision.cshtml`

#### Changes Made:
1. **Updated form IDs and modal title:**
   - Changed from `formEditDivision`/`formAddDivision` to `editDivisionForm`/`addDivisionForm` for consistency
   - Changed modal title text from "Update Division" to "Edit Division" to match Program pattern

2. **Standardized modal header:**
   - Changed from custom styling to standard GOV.UK pattern
   - Added `data-bs-dismiss="modal"` attribute to close button
   - Changed modal title ID to `divisionModalTitle` for consistency

3. **Improved modal body structure:**
   - Wrapped form in `<div class="row g-3">` for better layout consistency
   - Removed inline styles and unnecessary CSS classes (`col-12`, `p-0`)
   - Standardized error summary structure

4. **Simplified form field labels:**
   - Removed redundant ID attributes (handled by `asp-for`)
   - Removed trailing colons from labels for consistency
   - Used `asp-for` tag helpers consistently

## Pattern Comparison

### Program Maintenance Pattern (Reference)
```csharp
// Controller - Create GET
public async Task<IActionResult> Create()
{
    var model = new ProgramViewModel
    {
        ProgramNo = string.Empty,
        ProgramName = string.Empty,
        Directorate = string.Empty
    };
    await PopulateDropdownsAsync(model);
    return PartialView("_AddEditProgram", model);
}
```

### Division Maintenance Pattern (Updated)
```csharp
// Controller - Create GET
public IActionResult Create()
{
    var model = new DivisionViewModel
    {
        DivName = string.Empty,
        AgencyId = 0,
        CentOverhead = null
    };
    return PartialView("_AddEditDivision", model);
}
```

## Benefits

1. **Consistency:** Both controllers now follow the same pattern for handling create/edit operations
2. **Null Safety:** View always receives a valid model object, preventing null reference exceptions
3. **Maintainability:** Developers can easily understand the pattern by looking at either controller
4. **UI Consistency:** Modal styling and structure now matches across different maintenance screens

## Testing Recommendations

1. Test the "Add Division" functionality to ensure the modal opens correctly with empty fields
2. Test the "Edit Division" functionality to ensure fields are populated correctly
3. Verify validation works correctly for both create and edit scenarios
4. Check that the modal closes properly after successful save/update
5. Verify error messages display correctly in the error summary section

## Future Enhancements

Consider adding a `PopulateDropdownsAsync()` method similar to ProgramMaintenance if the Agency dropdown needs to be dynamically populated from an API service.
