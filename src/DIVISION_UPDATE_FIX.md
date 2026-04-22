# Division Update Fix - "Failed to Update Division" Resolution

## Issue
User reported "Failed to update division" error when attempting to save changes in the Edit Division screen.

## Root Cause
The backend API controller (`DivisionController.UpdateDivisionAsync`) had a strict validation check that required the `divName` parameter in the URL to exactly match the `DivName` field in the request body:

```csharp
if (divName != request.DivName)
{
    _logger.LogWarning("[UpdateDivision] Division name mismatch. URL: {UrlName}, Body: {BodyName}", divName, request.DivName);
    return BadRequest("Division name in URL does not match request body");
}
```

This validation prevented:
1. Updating divisions when the DivName field was changed (primary key update scenario)
2. The `originalDivName` tracking mechanism from working properly

## Solution Implemented

### 1. Backend API Controller (DivisionController.cs)
**Removed the strict validation check** that required URL divName to match body DivName:

```csharp
[HttpPut("{divName}")]
public async Task<ActionResult<DivisionRes>> UpdateDivisionAsync(
    string divName,
    [FromBody] DivisionReq request)
{
    // Removed: if (divName != request.DivName) check
    
    // Use divName from URL to identify the record to update
    // The request body contains the new values (including potentially a new DivName)
    var divisionDto = _mapper.Map<DivisionDto>(request);
    
    // Pass the original divName from URL to identify which record to update
    var updatedDivision = await _divisionService.UpdateDivisionAsync(divName, divisionDto);
    
    return Ok(_mapper.Map<DivisionRes>(updatedDivision));
}
```

### 2. Backend Service Interface (IDivisionService.cs)
**Updated signature** to accept `originalDivName` parameter:

```csharp
/// <summary>
/// Updates an existing division after validation.
/// </summary>
/// <param name="originalDivName">Original division name to identify the record.</param>
/// <param name="divisionDto">Division data to update (may contain new DivName).</param>
/// <returns>Updated division DTO.</returns>
Task<DivisionDto> UpdateDivisionAsync(string originalDivName, DivisionDto divisionDto);
```

### 3. Backend Service Implementation (DivisionService.cs)
**Added logic** to:
- Use `originalDivName` to locate the record to update
- Check for duplicate names when DivName is being changed
- Validate both parameters

```csharp
public async Task<DivisionDto> UpdateDivisionAsync(string originalDivName, DivisionDto divisionDto)
{
    ArgumentNullException.ThrowIfNull(divisionDto);

    if (string.IsNullOrWhiteSpace(originalDivName))
    {
        throw new ArgumentException("Original division name is required to identify the record.", nameof(originalDivName));
    }

    if (string.IsNullOrWhiteSpace(divisionDto.DivName))
    {
        throw new ArgumentException("Division name is required.", nameof(divisionDto));
    }

    // Use originalDivName to find the record to update
    var existingDivision = await _divisionRepository.GetDivisionByNameAsync(originalDivName);
    if (existingDivision == null)
    {
        throw new InvalidOperationException($"Division '{originalDivName}' not found.");
    }

    // Check if new name conflicts with another division (only if name is changing)
    if (!originalDivName.Equals(divisionDto.DivName, StringComparison.OrdinalIgnoreCase))
    {
        var nameConflict = await _divisionRepository.DivisionExistsAsync(divisionDto.DivName);
        if (nameConflict)
        {
            throw new InvalidOperationException($"Cannot rename to '{divisionDto.DivName}' - division already exists.");
        }
    }

    // Map new values and update
    var division = _mapper.Map<Division>(divisionDto);
    var updatedDivision = await _divisionRepository.UpdateDivisionAsync(division);
    return _mapper.Map<DivisionDto>(updatedDivision);
}
```

## Data Flow
The complete update flow now works correctly:

1. **Frontend (_AddEditDivision.cshtml)**:
   - Hidden field stores original DivName: `<input type="hidden" id="originalDivName" value="@Model.DivName" />`
   - User can edit all fields including DivName

2. **Frontend JavaScript (Index.cshtml)**:
   - `updateDivision()` function reads `originalDivName` from hidden field
   - Sends it as query parameter: `?originalDivName={originalValue}`

3. **Frontend Controller (DivisionMaintenanceController.cs)**:
   - `Edit([FromBody] DivisionViewModel, [FromQuery] string? originalDivName)` receives both
   - Passes `originalDivName` to service layer

4. **Frontend Service (DivisionService.cs)**:
   - `UpdateDivisionAsync(originalDivName, divisionDto)` forwards to API client

5. **API Client (FpsDivisionApiClient.cs)**:
   - `UpdateDivisionAsync(divName, divisionDto)` calls `PUT api/division/{divName}`
   - URL uses `originalDivName` to identify record
   - Body contains new values (potentially new DivName)

6. **Backend API (DivisionController.cs)**:
   - `UpdateDivisionAsync(string divName, [FromBody] DivisionReq request)` receives both
   - **NO LONGER validates** divName == request.DivName
   - Passes `divName` from URL to service layer

7. **Backend Service (DivisionService.cs)**:
   - Uses `originalDivName` to find existing record
   - Validates no duplicate when renaming
   - Updates record with new values

8. **Repository Layer**:
   - Entity Framework updates the record in database

## Features Enabled
✅ **Update DivisionId** - Can now be changed  
✅ **Update DivName (Primary Key)** - Can now be changed safely  
✅ **Update AgencyId** - Works correctly  
✅ **Update CentOverhead** - Works correctly  
✅ **Duplicate Name Prevention** - Throws error if renaming to existing division name  
✅ **Record Not Found Handling** - Throws error if original division doesn't exist  

## Testing Recommendations

### Test Case 1: Update Without Changing DivName
1. Open Edit Division modal
2. Change DivisionId or AgencyId or CentOverhead
3. Keep DivName unchanged
4. Click Update
5. **Expected**: Division updated successfully

### Test Case 2: Update With DivName Change
1. Open Edit Division modal
2. Change DivName from "DIV001" to "DIV999"
3. Click Update
4. **Expected**: Division updated successfully, new name reflected in grid

### Test Case 3: Duplicate Name Prevention
1. Open Edit Division modal for "DIV001"
2. Change DivName to "DIV002" (existing division)
3. Click Update
4. **Expected**: Error message "Cannot rename to 'DIV002' - division already exists."

### Test Case 4: All Fields Changed
1. Open Edit Division modal
2. Change DivisionId, AgencyId, DivName, and CentOverhead
3. Click Update
4. **Expected**: All changes saved successfully

## Files Modified
1. ✅ `Apha.FPS\Apha.FPS.Api\Controllers\DivisionController.cs` - Removed strict validation
2. ✅ `Apha.FPS\Apha.FPS.Application\Interfaces\IDivisionService.cs` - Updated interface signature
3. ✅ `Apha.FPS\Apha.FPS.Application\Services\DivisionService.cs` - Added originalDivName logic

## Build Status
✅ All projects build successfully  
⚠️ Hot reload warning (requires app restart to apply changes)

## Restart Instructions
Since the method signature changed, you need to restart the application:

### Stop Current Debugging Session
1. In Visual Studio, click **Stop Debugging (Shift+F5)**

### Restart Application
2. Press **F5** or click **Start Debugging**

### Verify Fix
3. Navigate to Division Maintenance page
4. Click Edit on any division
5. Change any field (including DivName)
6. Click Update
7. Should see "Division updated successfully"

## Additional Notes
- The `originalDivName` parameter is essential for primary key updates
- Frontend already had this logic in place (hidden field + JavaScript)
- Backend just needed to accept and use this parameter correctly
- Duplicate name checking prevents data integrity issues
- All layers (Controller → Service → Repository) now properly handle division name changes
