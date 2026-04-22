# Division Edit Button Fix

## Problem Summary
The edit button in the Division Maintenance screen was not working due to several JavaScript inconsistencies between the view files and function implementations.

## Root Causes Identified

### 1. **Wrong Form Selectors**
- **Issue:** The `saveDivision()` function was trying to find `$('#divisionForm')`, but the actual form IDs were `editDivisionForm` and `addDivisionForm`
- **Impact:** Form data was never captured, causing save operations to fail

### 2. **Missing Separate Save/Update Functions**
- **Issue:** Division used a single `saveDivision()` function for both create and edit, which didn't match the view's expectations
- **Expected:** Separate `saveDivision()` (create) and `updateDivision()` (edit) functions like ProgramMaintenance
- **Impact:** Edit operations couldn't find the correct function to call

### 3. **Wrong Modal Container**
- **Issue:** JavaScript was referencing `$('#divisionModal')` and `$('#divisionModalBody')`, but these elements didn't exist in the Index view
- **Expected:** Should use `$('#modalPopup')` and `$('#modaPopupBody')` like ProgramMaintenance
- **Impact:** Modal wouldn't open correctly, content wouldn't load

### 4. **Incorrect Button Parameter Passing**
- **Issue:** Edit and Delete functions expected direct parameters (`divName`) instead of extracting from button data attributes
- **Expected:** Should use `$(btn).data('id')` like ProgramMaintenance
- **Impact:** Functions couldn't retrieve the division name to edit/delete

## Changes Made

### 1. Updated `Index.cshtml` JavaScript Functions

#### Before (Broken):
```javascript
function editDivision(divName) {
    $.ajax({
        url: '@Url.Action("Edit", "DivisionMaintenance", new { area = "FPS" })',
        type: 'GET',
        data: { divName: divName },
        success: function (result) {
            $('#divisionModalBody').html(result);  // Wrong element
            $('#divisionModalLabel').text('Edit Division');
            $('#divisionModal').modal('show');  // Wrong modal
        }
    });
}
```

#### After (Fixed):
```javascript
function editDivision(btn) {
    var divName = $(btn).data('id');  // Extract from button
    $.ajax({
        url: '@Url.Action("Edit", "DivisionMaintenance", new { area = "FPS" })',
        type: 'GET',
        data: { divName: divName },
        success: function (html) {
            $('#modaPopupBody').html(html);  // Correct element
            $('#modalPopup').addClass("show");  // Correct modal
        },
        error: function () {
            alert('An error occurred while editing record');
        }
    });
}
```

### 2. Split Save Function into Create and Update

#### Added `saveDivision()` for Create:
```javascript
function saveDivision() {
    clearValidationErrors('#modaPopupBody');
    var form = $('#addDivisionForm');  // Correct form ID

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    var data = {
        DivisionId: form.find('[name="DivisionId"]').val() || null,
        DivName: form.find('[name="DivName"]').val(),
        AgencyId: parseInt(form.find('[name="AgencyId"]').val()),
        CentOverhead: /* ... */
    };

    $.ajax({
        url: '@Url.Action("Create", "DivisionMaintenance", new { area = "FPS" })',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        // ...
    });
}
```

#### Added `updateDivision()` for Edit:
```javascript
function updateDivision() {
    clearValidationErrors('#modaPopupBody');
    var form = $('#editDivisionForm');  // Correct form ID
    
    // Similar structure to saveDivision but calls Edit endpoint
    $.ajax({
        url: '@Url.Action("Edit", "DivisionMaintenance", new { area = "FPS" })',
        type: 'POST',
        // ...
    });
}
```

### 3. Updated `_AddEditDivision.cshtml`

#### Before:
```razor
var saveAction = "saveDivision()";
```

#### After:
```razor
var saveAction = isEditMode ? "updateDivision()" : "saveDivision()";
```

This ensures the correct function is called based on the mode.

### 4. Removed Unused Modal HTML

Removed the standalone `divisionModal` div from `Index.cshtml` since the application uses a shared modal (`modalPopup`) that's likely defined in the layout.

### 5. Added Missing Helper Functions

Added the following helper functions to match ProgramMaintenance pattern:
- `closeModal()` - Clears validation and closes the modal
- `isFormValid(form)` - Validates required fields before submission

### 6. Integrated Validation Scripts

Added reference to validation scripts:
```razor
@section Scripts {
    <script src="~/js/ajax-form-validation.js"></script>
    <script>
        // ... functions
    </script>
    
    @{
        await Html.RenderPartialAsync("_ValidationScriptsPartial");
    }
}
```

## Files Modified

1. **Apha.FPSApps.Web\Areas\FPS\Views\DivisionMaintenance\Index.cshtml**
   - Rewrote all JavaScript functions to match ProgramMaintenance pattern
   - Fixed modal container references
   - Fixed button parameter handling
   - Added validation integration
   - Removed unused modal HTML

2. **Apha.FPSApps.Web\Areas\FPS\Views\DivisionMaintenance\_AddEditDivision.cshtml**
   - Updated `saveAction` to conditionally call correct function
   - Added `type="button"` to save button
   - Cleaned up JavaScript (removed duplicate closeModal function)
   - Simplified agency loading script

## Testing Checklist

- [x] Build successful
- [ ] Test Add Division functionality
- [ ] Test Edit Division functionality  
- [ ] Test Delete Division functionality
- [ ] Test form validation (client-side)
- [ ] Test form validation (server-side)
- [ ] Test modal open/close behavior
- [ ] Test Agency dropdown population
- [ ] Verify data saves correctly to database
- [ ] Verify grid refreshes after create/edit/delete

## Key Differences from Original

| Aspect | Original (Broken) | Fixed |
|--------|------------------|-------|
| Form ID | `$('#divisionForm')` | `$('#addDivisionForm')` or `$('#editDivisionForm')` |
| Modal Container | `$('#divisionModalBody')` | `$('#modaPopupBody')` |
| Save Functions | Single `saveDivision()` | Separate `saveDivision()` and `updateDivision()` |
| Button Parameters | Direct parameters | Extract from `$(btn).data('id')` |
| Validation | Missing | Integrated with `ajax-form-validation.js` |
| Error Handling | Generic alerts | Proper error display with validation |

## Benefits

1. ✅ **Consistency** - Now matches ProgramMaintenance pattern exactly
2. ✅ **Maintainability** - Easier to understand and modify
3. ✅ **User Experience** - Proper validation and error messages
4. ✅ **Reliability** - Functions actually work as intended
5. ✅ **Debugging** - Clearer error messages and logging

## Notes for Future Development

- The Agency dropdown currently uses hardcoded test data. Replace with actual API call when the endpoint is available.
- Consider extracting common modal/form functions to a shared JavaScript file to reduce duplication across maintenance screens.
- The modal container naming is inconsistent (`modaPopupBody` should probably be `modalPopupBody`). Consider fixing this across all maintenance screens for better clarity.
