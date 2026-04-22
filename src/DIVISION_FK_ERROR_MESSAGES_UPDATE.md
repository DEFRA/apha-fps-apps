# Division FK Validation Error Messages Update

## Overview

Updated the foreign key validation error messages in the Division maintenance feature to be more user-friendly by removing technical table names and making them consistent across add, edit, and delete operations.

## Changes Made

### File: `Apha.FPS\Apha.FPS.Application\Services\DivisionService.cs`

Updated three error messages to hide internal database table names from users:

#### 1. Create Operation (Add Division)

**Before:**
```csharp
throw new InvalidOperationException($"Cannot add Division Name as it is already used in {tableList}");
```

**After:**
```csharp
throw new InvalidOperationException("Unable to add the division name as it is already in use.");
```

**Lines Changed:** 69-74

---

#### 2. Update Operation (Edit Division)

**Before:**
```csharp
throw new InvalidOperationException($"Cannot edit Division Name as it is used in {tableList}");
```

**After:**
```csharp
throw new InvalidOperationException("Unable to edit the division name as it is already in use.");
```

**Lines Changed:** 120-126

---

#### 3. Delete Operation (Delete Division)

**Before:**
```csharp
throw new InvalidOperationException($"Cannot delete Division as it is used in {tableList}");
```

**After:**
```csharp
throw new InvalidOperationException("Unable to delete the division as it is currently in use.");
```

**Lines Changed:** 149-154

---

## Rationale

### Why These Changes?

1. **User-Friendly**: End users don't need to know about internal database table names like `tblkpprofitcentre` or `divisiongrade`
2. **Professional**: Messages are more polished and business-focused
3. **Consistent**: All three operations now use similar phrasing
4. **Security**: Reduces exposure of internal database schema details
5. **Clarity**: Users understand the issue without technical jargon

### Message Comparison

| Operation | Old Message | New Message |
|-----------|-------------|-------------|
| **Add** | Cannot add Division Name as it is already used in tblkpprofitcentre, divisiongrade | Unable to add the division name as it is already in use. |
| **Edit** | Cannot edit Division Name as it is used in tblkpprofitcentre | Unable to edit the division name as it is already in use. |
| **Delete** | Cannot delete Division as it is used in tblkpprofitcentre, divisiongrade | Unable to delete the division as it is currently in use. |

## Validation Style Verification

### GOV.UK Design System Compliance

Verified that `_AddEditDivision.cshtml` follows the same validation pattern as `_AddEditProgram.cshtml`:

✅ **Error Summary Block:**
```html
<div class="govuk-error-summary" role="alert"
     aria-labelledby="error-summary-title" tabindex="-1" style="display:none;">
    <h2 class="govuk-error-summary__title" id="error-summary-title">
        There is a problem
    </h2>
    <div class="govuk-error-summary__body">
        <ul class="govuk-list govuk-error-summary__list">
        </ul>
    </div>
</div>
```

✅ **Form Groups:**
```html
<div class="govuk-form-group sup_margin_0">
    <label class="govuk-label" asp-for="DivName">Division Name</label>
    <input type="text" asp-for="DivName" required class="govuk-input" maxlength="255">
    <span asp-validation-for="DivName" class="govuk-error-message" style="display:none;"></span>
</div>
```

✅ **Inline Validation Messages:**
- Uses `asp-validation-for` helper
- Class: `govuk-error-message`
- Initially hidden with `style="display:none;"`

### JavaScript Validation Handler

The `ajax-form-validation.js` file handles:

1. **Client-side validation** - `displayClientValidationErrors()`
2. **Server-side validation** - `displayServerValidationErrors()`
3. **Clear validation** - `clearValidationErrors()`

These functions work with the GOV.UK Design System classes:
- `govuk-error-summary` - Summary of errors at top of form
- `govuk-form-group--error` - Highlights form groups with errors
- `govuk-input--error` - Highlights individual input fields
- `field-validation-error` - Shows inline error messages

## User Experience Flow

### Add Division with FK Reference

**Scenario:** User tries to add a division name "ACDP" that already exists in profit centre table.

**Before:**
```
Error: Cannot add Division Name as it is already used in tblkpprofitcentre
```

**After:**
```
Error: Unable to add the division name as it is already in use.
```

---

### Edit Division Name with FK Reference

**Scenario:** User tries to rename division "VSD" to "ACDP" but "VSD" is referenced in other tables.

**Before:**
```
Error: Cannot edit Division Name as it is used in tblkpprofitcentre, divisiongrade
```

**After:**
```
Error: Unable to edit the division name as it is already in use.
```

---

### Delete Division with FK Reference

**Scenario:** User tries to delete division "VSD" that is used in profit centre records.

**Before:**
```
Error: Cannot delete Division as it is used in tblkpprofitcentre
```

**After:**
```
Error: Unable to delete the division as it is currently in use.
```

## Error Display

All three operations display the error in the same way:

1. **Modal Form** (Add/Edit):
   - Error appears in `govuk-error-summary` at top of modal
   - Form remains open for user to correct
   - Focus is set to error summary for accessibility

2. **AJAX Alert** (Delete):
   - Error appears in browser alert dialog
   - User clicks OK to dismiss
   - Grid remains unchanged

## Technical Details

### Console Logging

Console logs still include table names for debugging purposes:

```csharp
Console.WriteLine($"[DivisionService] THROWING FK VALIDATION ERROR: Unable to add the division name as it is already in use.");
```

The `tableList` variable is still constructed and logged but not shown to users:

```csharp
var tableList = string.Join(", ", referencedTables);
Console.WriteLine($"[DivisionService] FK references found in: {tableList}");
```

### FK Reference Tables Checked

The validation still checks the same tables:
- `tblkpprofitcentre` (ProfitCentre entity)
- `divisiongrade` (DivisionGrade entity)

Only the error message shown to users has changed.

## Testing

### Test Cases

#### Test 1: Add Division with Existing FK Reference
**Steps:**
1. Try to add division name "ACDP" that exists in profit centre
2. **Expected:** "Unable to add the division name as it is already in use."

#### Test 2: Edit Division Name with FK Reference
**Steps:**
1. Try to rename division "VSD" to "ACDP" where "VSD" is used in other tables
2. **Expected:** "Unable to edit the division name as it is already in use."

#### Test 3: Delete Division with FK Reference
**Steps:**
1. Try to delete division "VSD" that is referenced in profit centre
2. **Expected:** "Unable to delete the division as it is currently in use."

#### Test 4: All Operations with No FK References
**Steps:**
1. Add/Edit/Delete divisions that have no FK references
2. **Expected:** Operations succeed without FK validation errors

## Build Status

✅ **Build Successful** - All changes compile without errors

## Benefits

1. ✅ **Improved UX** - Clear, professional error messages
2. ✅ **Consistency** - Same message style across all operations
3. ✅ **Security** - Internal schema not exposed to users
4. ✅ **Maintainability** - Debug logs still contain technical details
5. ✅ **Accessibility** - Works with screen readers and GOV.UK patterns

## Notes

- Error messages are now simplified and business-focused
- Technical details remain in console logs for debugging
- Validation structure matches GOV.UK Design System standards
- No changes needed to `_AddEditDivision.cshtml` - already using correct pattern
- JavaScript validation handlers work with new messages without modification
