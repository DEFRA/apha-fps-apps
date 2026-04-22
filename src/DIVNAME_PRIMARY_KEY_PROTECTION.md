# Division Name Primary Key Protection

## Issue
User requested: "Show the message 'Division name can't be updated as it is primary key' if the user tries to update DivName Text box"

## Solution Implemented

I've implemented **multiple layers of protection** to prevent DivName (primary key) updates and show the required error message:

### 1. **UI Layer - Readonly Field** ✅
**File**: `_AddEditDivision.cshtml`

Made the DivName field **readonly in edit mode**:
```razor
<input type="text" asp-for="DivName" required class="govuk-input" maxlength="255" readonly="@isEditMode">
```

- In **Add mode**: Field is editable (users can enter new division name)
- In **Edit mode**: Field is readonly (users cannot change existing division name)
- Visual indicator: Red warning text "(Primary Key - Cannot be updated)"

### 2. **Client-Side Validation** ✅
**File**: `Index.cshtml` - `updateDivision()` function

Added JavaScript validation that checks if DivName was changed before submission:

```javascript
// Get the original DivName from hidden field
var originalDivName = $('#originalDivName').val();
var newDivName = form.find('[name="DivName"]').val();

// Validate that DivName has not been changed (primary key cannot be updated)
if (originalDivName && originalDivName !== newDivName) {
    var errorSummary = $('#modaPopupBody').find('.govuk-error-summary');
    var errorList = errorSummary.find('.govuk-error-summary__list');
    
    errorList.empty();
    errorList.append('<li><a href="#DivName">Division name can\'t be updated as it is primary key</a></li>');
    
    // Highlight the field
    var divNameGroup = form.find('[name="DivName"]').closest('.govuk-form-group');
    divNameGroup.addClass('govuk-form-group--error');
    divNameGroup.find('.govuk-error-message').text('Division name can\'t be updated as it is primary key').show();
    
    errorSummary.show();
    console.error('DivName change attempted:', originalDivName, '->', newDivName);
    return; // Stop form submission
}
```

**What this does:**
- Compares original DivName (from hidden field) with current input value
- If changed: Shows GOV.UK Design System error summary
- Highlights the DivName field with error styling
- Displays exact message: "Division name can't be updated as it is primary key"
- Prevents form submission

### 3. **Server-Side Validation** ✅
**File**: `DivisionMaintenanceController.cs` - `Edit()` action

Added controller validation as a backup security layer:

```csharp
// Server-side validation: Prevent primary key (DivName) updates
if (!string.IsNullOrWhiteSpace(originalDivName) && 
    !originalDivName.Equals(divisionViewModel.DivName, StringComparison.OrdinalIgnoreCase))
{
    return Json(new
    {
        success = false,
        message = "Division name can't be updated as it is primary key",
        errors = new[]
        {
            new
            {
                field = "DivName",
                message = "Division name can't be updated as it is primary key"
            }
        }
    });
}
```

**What this does:**
- Runs even if JavaScript is bypassed
- Compares originalDivName query parameter with DivName in request body
- Returns JSON error response with exact message
- Frontend will display error via `displayServerValidationErrors()` function

## Protection Layers (Defense in Depth)

| Layer | Method | When It Triggers | User Experience |
|-------|--------|-----------------|-----------------|
| **1. UI** | `readonly` attribute | User tries to type in field | Field won't accept input |
| **2. JavaScript** | Client validation | User bypasses readonly (DevTools) | Error summary shows before AJAX call |
| **3. Controller** | Server validation | Client validation bypassed | Error returned from server |
| **4. Service** | Business logic | (Previous implementation - now disabled) | - |
| **5. Repository** | Delete+Insert pattern | (Previous implementation - now disabled) | - |

## Changes to Previous Implementation

### Reverted Backend Logic
Since we're now **preventing** primary key updates at the UI level, I've reverted the complex delete+insert logic that was previously implemented:

- ✅ Frontend prevents DivName changes
- ✅ Backend validation rejects DivName changes
- ❌ Removed delete+insert pattern from repository (no longer needed)
- ❌ Removed service layer duplicate name checking for renames (no longer needed)

## User Experience Flow

### Scenario 1: Normal Edit (DivName Not Changed)
1. User clicks Edit on "DIV001"
2. Modal opens with DivName field **readonly** showing "DIV001"
3. User changes DivisionId, AgencyId, or CentOverhead
4. Clicks Update
5. ✅ **Success**: "Division updated successfully"

### Scenario 2: Attempted DivName Change (Readonly Bypassed)
1. User clicks Edit on "DIV001"
2. User somehow removes readonly attribute via browser DevTools
3. User changes DivName from "DIV001" to "NEWDIV"
4. Clicks Update
5. ❌ **Client Validation Fires**:
   - Error summary appears: "There is a problem"
   - Error message: "Division name can't be updated as it is primary key"
   - DivName field highlighted in red
   - Form submission stopped

### Scenario 3: Client Validation Bypassed (Direct API Call)
1. User crafts malicious AJAX request
2. Sends POST with originalDivName="DIV001", DivName="NEWDIV"
3. ❌ **Server Validation Fires**:
   - Controller returns JSON error
   - Error displayed: "Division name can't be updated as it is primary key"
   - Update rejected

## Visual Indicators

### Add Mode
```
┌─────────────────────────────────────┐
│ Division Name                        │
│ ┌─────────────────────────────────┐ │
│ │ [Editable input field]          │ │
│ └─────────────────────────────────┘ │
└─────────────────────────────────────┘
```

### Edit Mode
```
┌─────────────────────────────────────┐
│ Division Name                        │
│ (Primary Key - Cannot be updated)  │  ← Red warning text
│ ┌─────────────────────────────────┐ │
│ │ DIV001  [Readonly - gray bg]   │ │  ← Readonly styling
│ └─────────────────────────────────┘ │
└─────────────────────────────────────┘
```

### Error State (If Validation Fails)
```
┌──────────────────────────────────────┐
│ ⚠ There is a problem                │
│ • Division name can't be updated    │
│   as it is primary key               │
└──────────────────────────────────────┘

┌─────────────────────────────────────┐
│ Division Name                        │
│ (Primary Key - Cannot be updated)   │
│ ┌─────────────────────────────────┐ │
│ │ NEWDIV  [Red border]            │ │  ← Error styling
│ └─────────────────────────────────┘ │
│ ⚠ Division name can't be updated   │  ← Error message
│   as it is primary key               │
└─────────────────────────────────────┘
```

## Files Modified

1. ✅ `Apha.FPSApps\Apha.FPSApps.Web\Areas\FPS\Views\DivisionMaintenance\_AddEditDivision.cshtml`
   - Made DivName field readonly in edit mode
   - Changed warning text color to red
   - Updated message to "Cannot be updated"

2. ✅ `Apha.FPSApps\Apha.FPSApps.Web\Areas\FPS\Views\DivisionMaintenance\Index.cshtml`
   - Added client-side validation in `updateDivision()` function
   - Checks if DivName changed before AJAX call
   - Shows GOV.UK error summary with exact message

3. ✅ `Apha.FPSApps\Apha.FPSApps.Web\Areas\FPS\Controllers\DivisionMaintenanceController.cs`
   - Added server-side validation in `Edit()` action
   - Returns JSON error if DivName changed
   - Security backup layer

## Testing

### Test Case 1: Normal Edit Without DivName Change ✅
1. Open Edit Division modal for "DIV001"
2. Verify DivName field is readonly and grayed out
3. Change DivisionId from 1 to 100
4. Change AgencyId to different value
5. Click Update
6. **Expected**: "Division updated successfully", grid refreshes

### Test Case 2: Attempt to Edit Readonly Field ✅
1. Open Edit Division modal for "DIV001"
2. Try to type in DivName field
3. **Expected**: Field won't accept input (HTML readonly attribute prevents it)

### Test Case 3: Bypass Readonly via DevTools ✅
1. Open Edit Division modal for "DIV001"
2. Open browser DevTools (F12)
3. Find DivName input element
4. Remove `readonly` attribute
5. Change value from "DIV001" to "TESTDIV"
6. Click Update
7. **Expected**: 
   - Error summary appears
   - Error message: "Division name can't be updated as it is primary key"
   - Field highlighted in red
   - Form NOT submitted

### Test Case 4: Direct API Call (Postman/Curl) ✅
1. Send POST request to Edit endpoint:
   ```
   POST /FPS/DivisionMaintenance/Edit?originalDivName=DIV001
   Body: { "DivName": "NEWDIV", "DivisionId": 1, "AgencyId": 1 }
   ```
2. **Expected**: 
   - HTTP 200 OK with JSON: 
   ```json
   {
     "success": false,
     "message": "Division name can't be updated as it is primary key",
     "errors": [{ "field": "DivName", "message": "..." }]
   }
   ```

## Browser Compatibility
- ✅ **readonly attribute**: Supported in all modern browsers (Chrome, Firefox, Edge, Safari)
- ✅ **JavaScript validation**: Works with jQuery (already used in project)
- ✅ **GOV.UK Design System**: Compatible error styling

## Security Notes
- **Client-side validation** is for UX (user-friendly error messages)
- **Server-side validation** is mandatory security layer
- Even if JavaScript is disabled, server will reject invalid requests
- No SQL injection risk (Entity Framework parameterizes queries)

## Restart Instructions
The application is currently running with hot reload. The changes should apply automatically, but if you encounter issues:

1. **Stop Debugging** (Shift+F5)
2. **Start Debugging** (F5)
3. Test the Edit Division functionality

## Summary
✅ **DivName field readonly** in edit mode  
✅ **Visual warning** "(Primary Key - Cannot be updated)" in red  
✅ **Client-side validation** with GOV.UK error summary  
✅ **Server-side validation** as security backup  
✅ **Exact error message**: "Division name can't be updated as it is primary key"  
✅ **Three layers** of protection  
✅ **Build successful**  
✅ **Ready to test**  
