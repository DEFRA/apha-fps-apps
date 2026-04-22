# Division Edit Fields - DivisionId and DivName Now Editable

## Summary
Both **DivisionId** and **DivName** fields are now editable in the Edit Division modal.

## ⚠️ Important Considerations

### DivName is a Primary Key
- **DivName** is the PRIMARY KEY for the `fps.tlkpdivision` table
- Editing primary keys can have implications for data integrity
- A warning is displayed: "(Primary Key - Edit with caution)"
- The system now tracks the original DivName to ensure the correct record is updated

### DivisionId is a Regular Field
- **DivisionId** is a regular integer field (not a key, not auto-generated)
- It can be safely edited

## Changes Made

### 1. View Changes (_AddEditDivision.cshtml)

#### DivisionId Field
**Before:**
```razor
@if (isEditMode)
{
    <input type="number" asp-for="DivisionId" class="govuk-input" readonly>
}
else
{
    <input type="number" asp-for="DivisionId" class="govuk-input" required min="1">
}
```

**After:**
```razor
<input type="number" asp-for="DivisionId" class="govuk-input" required min="1">
```
- Now always editable in both Add and Edit modes

#### DivName Field
**Before:**
```razor
@if (isEditMode)
{
    <input type="text" asp-for="DivName" class="govuk-input" readonly>
}
else
{
    <input type="text" asp-for="DivName" required class="govuk-input" maxlength="255">
}
```

**After:**
```razor
<label class="govuk-label" asp-for="DivName">
    Division Name 
    @if (isEditMode) 
    { 
        <span class="govuk-caption-m">(Primary Key - Edit with caution)</span> 
    }
</label>
@if (isEditMode)
{
    <input type="text" asp-for="DivName" required class="govuk-input" maxlength="255" data-original-value="@Model.DivName">
}
else
{
    <input type="text" asp-for="DivName" required class="govuk-input" maxlength="255">
}
```
- Now editable with a warning message
- Stores original value in `data-original-value` attribute for tracking

#### Hidden Field for Original DivName
```razor
@if (isEditMode)
{
    <!-- Hidden field to store original DivName for update operations -->
    <input type="hidden" id="originalDivName" value="@Model.DivName" />
}
```
- Tracks the original DivName to identify which record to update

### 2. JavaScript Changes (Index.cshtml)

#### Updated updateDivision() Function
```javascript
// Get the original DivName from hidden field (for API identification)
var originalDivName = $('#originalDivName').val();
var newDivName = form.find('[name="DivName"]').val();

var data = {
    DivisionId: divisionId,
    DivName: newDivName,  // New name sent in data
    AgencyId: parseInt(form.find('[name="AgencyId"]').val()),
    CentOverhead: centOverhead
};

console.log('Updating division data:', data);
console.log('Original DivName for API call:', originalDivName);

// Use original DivName in the URL to identify the record to update
$.ajax({
    url: '@Url.Action("Edit", "DivisionMaintenance", new { area = "FPS" })?originalDivName=' + encodeURIComponent(originalDivName),
    type: 'POST',
    data: JSON.stringify(data),
    contentType: 'application/json; charset=utf-8',
    // ... rest of the AJAX call
});
```

**Key Changes:**
- Extracts `originalDivName` from the hidden field
- Passes `originalDivName` as a query parameter to the API
- Sends the new DivName in the request body

### 3. Controller Changes (DivisionMaintenanceController.cs)

#### Updated Edit Action Method
```csharp
/// <summary>
/// Updates an existing division.
/// </summary>
/// <param name="divisionViewModel">The updated division data.</param>
/// <param name="originalDivName">The original division name (used when division name is changed).</param>
[HttpPost]
public async Task<IActionResult> Edit(
    [FromBody] DivisionViewModel divisionViewModel, 
    [FromQuery] string? originalDivName = null)
{
    // ... validation code ...

    // Use originalDivName if provided (when division name is being changed), otherwise use current name
    var identifyingDivName = !string.IsNullOrWhiteSpace(originalDivName) 
        ? originalDivName 
        : divisionViewModel.DivName;

    var divisionDto = _mapper.Map<DivisionDto>(divisionViewModel);
    var result = await _divisionService.UpdateDivisionAsync(identifyingDivName, divisionDto);

    // ... rest of the method ...
}
```

**Key Changes:**
- Added `originalDivName` query parameter (optional)
- Uses `originalDivName` to identify the record when DivName is changed
- Falls back to `divisionViewModel.DivName` if `originalDivName` is not provided (backward compatible)

## How It Works

### Scenario 1: Editing DivisionId Only
1. User opens Edit Division modal
2. Changes DivisionId from 10 to 20
3. Clicks Update
4. System uses DivName (unchanged) to identify and update the record
5. DivisionId is updated in the database

### Scenario 2: Editing DivName (Primary Key)
1. User opens Edit Division modal for "Division A"
2. Changes DivName to "Division B"
3. Hidden field stores original value: "Division A"
4. Clicks Update
5. JavaScript sends:
   - Query parameter: `originalDivName=Division A` (to identify the record)
   - Body: `DivName=Division B` (new value to save)
6. Controller uses "Division A" to find the record via API
7. API updates the record with new values including "Division B"
8. Database primary key is updated

### Scenario 3: Editing Both Fields
1. User changes both DivisionId and DivName
2. Both changes are sent to the API
3. Original DivName is used for record identification
4. Both fields are updated in the database

## Data Flow

```
User Edit → Modal Form
           ↓
    JavaScript (updateDivision)
           ↓
    Tracks originalDivName (hidden field)
           ↓
    AJAX POST with:
    - URL Query: ?originalDivName=<original>
    - Body: { DivName: <new>, DivisionId: <new>, ... }
           ↓
    DivisionMaintenanceController.Edit()
           ↓
    Uses originalDivName for UpdateDivisionAsync()
           ↓
    DivisionService
           ↓
    FpsDivisionApiClient
           ↓
    FPS API (DivisionController)
           ↓
    DivisionService (API layer)
           ↓
    DivisionRepository
           ↓
    Database Update
```

## Testing

### Test Case 1: Edit DivisionId
1. Open Edit Division for "Test Division"
2. Change DivisionId from 1 to 99
3. Click Update
4. Verify: DivisionId updated to 99, DivName unchanged

### Test Case 2: Edit DivName
1. Open Edit Division for "Old Name"
2. Change DivName to "New Name"
3. Click Update
4. Verify: Record with "Old Name" is now "New Name"
5. Verify: Record can be found in grid with "New Name"

### Test Case 3: Edit Both Fields
1. Open Edit Division for "Test Division" (DivisionId: 1)
2. Change DivisionId to 99
3. Change DivName to "Updated Division"
4. Click Update
5. Verify: Both fields updated correctly

### Test Case 4: Validation
1. Try to edit DivName to empty string
2. Verify: Validation error displayed
3. Try to edit DivisionId to negative number
4. Verify: Validation error displayed

## Database Considerations

### Primary Key Update Implications
When DivName is changed:
- PostgreSQL will update the primary key value
- If there are any foreign key relationships, they must have `ON UPDATE CASCADE` set
- The update operation may be slower than updating a regular field
- Unique constraint is checked during the update

### Recommendations
1. **Test thoroughly** in development environment before using in production
2. **Consider impact** on any foreign key relationships
3. **Document** when and why primary keys are changed
4. **Create audit logs** for primary key changes if needed
5. **Verify foreign key cascades** are configured correctly

## Files Modified

1. **Apha.FPSApps\Apha.FPSApps.Web\Areas\FPS\Views\DivisionMaintenance\_AddEditDivision.cshtml**
   - Made DivisionId always editable
   - Made DivName editable with warning
   - Added hidden field for original DivName tracking

2. **Apha.FPSApps\Apha.FPSApps.Web\Areas\FPS\Views\DivisionMaintenance\Index.cshtml**
   - Updated `updateDivision()` JavaScript function
   - Added originalDivName tracking and query parameter

3. **Apha.FPSApps\Apha.FPSApps.Web\Areas\FPS\Controllers\DivisionMaintenanceController.cs**
   - Updated `Edit` action to accept `originalDivName` parameter
   - Added logic to use originalDivName for record identification

## Build Status
✅ **Build Successful** - No compilation errors

## Backward Compatibility
The changes are backward compatible:
- If `originalDivName` is not provided, the controller uses `divisionViewModel.DivName`
- Existing API calls continue to work
- No breaking changes to data models or database schema
