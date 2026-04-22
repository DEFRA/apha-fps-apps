# ✅ UPDATE FUNCTION FIXED - Variables Were Missing!

## The Problem

The `updateDivision()` function was using variables **without declaring them**:

### Before (Broken Code):
```javascript
function updateDivision() {
    console.log('updateDivision called');
    clearValidationErrors('#modaPopupBody');
    var form = $('#editDivisionForm');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    // ❌ NO VARIABLE DECLARATIONS!

    var rawCentOverhead = form.find('[name="CentOverhead"]').val();
    var centOverhead = rawCentOverhead !== '' ? parseFloat(rawCentOverhead) : null;

    var rawDivisionId = form.find('[name="DivisionId"]').val();
    var divisionId = rawDivisionId !== '' ? parseInt(rawDivisionId) : null;

    var data = {
        DivisionId: divisionId,
        DivName: newDivName,  // ❌ newDivName is NOT DEFINED!
        AgencyId: parseInt(form.find('[name="AgencyId"]').val()),
        CentOverhead: centOverhead
    };

    console.log('Updating division data:', data);
    console.log('Original DivName for API call:', originalDivName);  // ❌ originalDivName is NOT DEFINED!

    // Use original DivName in the URL to identify the record to update
    $.ajax({
        url: '@Url.Action("Edit", "DivisionMaintenance", new { area = "FPS" })?originalDivName=' + encodeURIComponent(originalDivName),  // ❌ UNDEFINED!
        // ...
    });
}
```

### What Was Missing:
1. **`originalDivName`** - needed to identify which division to update
2. **`newDivName`** - the (possibly changed) division name from the form
3. **Validation logic** - to prevent primary key changes

### Why It Broke:
When I fixed the "duplicate variables" bug earlier, I accidentally removed **ALL** variable declarations instead of just removing the duplicates. This left the function referencing undefined variables, causing:
- JavaScript error: `Uncaught ReferenceError: originalDivName is not defined`
- AJAX call fails because URL has `undefined` in it
- Update doesn't work at all

---

## The Fix

### After (Working Code):
```javascript
function updateDivision() {
    console.log('updateDivision called');
    clearValidationErrors('#modaPopupBody');
    var form = $('#editDivisionForm');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    // ✅ DECLARE VARIABLES FIRST
    var originalDivName = $('#originalDivName').val();
    var newDivName = form.find('[name="DivName"]').val();

    // ✅ VALIDATE PRIMARY KEY NOT CHANGED
    if (originalDivName && originalDivName !== newDivName) {
        var errorSummary = $('#modaPopupBody').find('.govuk-error-summary');
        var errorList = errorSummary.find('.govuk-error-summary__list');
        
        errorList.empty();
        errorList.append('<li><a href="#DivName">Division name cannot be updated as it is primary key</a></li>');
        
        var divNameGroup = form.find('[name="DivName"]').closest('.govuk-form-group');
        divNameGroup.addClass('govuk-form-group--error');
        divNameGroup.find('.govuk-error-message').text('Division name cannot be updated as it is primary key').show();
        
        errorSummary.show();
        console.error('DivName change attempted:', originalDivName, '->', newDivName);
        return;
    }

    var rawCentOverhead = form.find('[name="CentOverhead"]').val();
    var centOverhead = rawCentOverhead !== '' ? parseFloat(rawCentOverhead) : null;

    var rawDivisionId = form.find('[name="DivisionId"]').val();
    var divisionId = rawDivisionId !== '' ? parseInt(rawDivisionId) : null;

    var data = {
        DivisionId: divisionId,
        DivName: newDivName,  // ✅ NOW DEFINED!
        AgencyId: parseInt(form.find('[name="AgencyId"]').val()),
        CentOverhead: centOverhead
    };

    console.log('Updating division data:', data);
    console.log('Original DivName for API call:', originalDivName);  // ✅ NOW DEFINED!

    // Use original DivName in the URL to identify the record to update
    $.ajax({
        url: '@Url.Action("Edit", "DivisionMaintenance", new { area = "FPS" })?originalDivName=' + encodeURIComponent(originalDivName),  // ✅ WORKS!
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            console.log('Update result:', result);
            if (result.success) {
                alert(result.message || 'Division updated successfully');
                closeModal();
                window.location.reload();
            } else {
                displayServerValidationErrors(result.errors, result.message, '#modaPopupBody')
            }
        },
        error: function (xhr) {
            console.error('Error updating:', xhr);
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                alert('An error occurred while updating.');
            }
        }
    });
}
```

---

## What's Fixed Now ✅

1. **Variables declared properly** at the top of the function
2. **Validation added** to prevent DivName changes (primary key protection)
3. **AJAX URL** now has correct `originalDivName` parameter
4. **Update will work** for changing DivisionId, AgencyId, CentOverhead
5. **Build successful** ✅

---

## Testing Steps

### 1. Restart Application
Since the app is running:
1. **Stop debugging** (Shift + F5)
2. **Start debugging** (F5)

### 2. Hard Refresh Browser
**CRITICAL** - Browser has cached the broken JavaScript:
- Press **Ctrl + Shift + R** (or clear cache completely)

### 3. Test Update Function

#### Test 1: Update Other Fields (Should Work)
1. Click **Edit** on a division
2. Change **DivisionId** from 1 to 100
3. Change **AgencyId** to different value
4. Change **CentOverhead** to 5000.00
5. **Keep DivName unchanged** (it's readonly anyway)
6. Click **Update**
7. **Expected**:
   - Console shows: `Updating division data: {DivisionId: 100, DivName: "DIV001", ...}`
   - Console shows: `Original DivName for API call: DIV001`
   - Alert: "Division updated successfully"
   - Page reloads, changes visible in grid

#### Test 2: Try to Change DivName (Should Block)
1. Click **Edit** on a division
2. Use browser DevTools (F12) → Inspector
3. Find the DivName input field
4. Remove the `readonly` attribute
5. Change DivName from "DIV001" to "NEWDIV"
6. Click **Update**
7. **Expected**:
   - Error summary appears
   - Message: "Division name cannot be updated as it is primary key"
   - Field highlighted in red
   - Update blocked (no AJAX call)

#### Test 3: Check Console for Errors
1. Open Console (F12)
2. Click Edit button
3. Click Update button
4. **Expected output**:
   ```
   updateDivision called
   Updating division data: {DivisionId: 1, DivName: "DIV001", AgencyId: 1, CentOverhead: 1000}
   Original DivName for API call: DIV001
   Update result: {success: true, message: "Division updated successfully"}
   ```
5. **Should NOT see**:
   - ❌ `Uncaught ReferenceError: originalDivName is not defined`
   - ❌ `Uncaught ReferenceError: newDivName is not defined`

---

## Console Commands for Debugging

If update still doesn't work, run these in Console (F12):

### Check Variables Are Accessible
```javascript
// Click Edit button first, then in console:
var form = $('#editDivisionForm');
var originalDivName = $('#originalDivName').val();
var newDivName = form.find('[name="DivName"]').val();
console.log('Original:', originalDivName);
console.log('New:', newDivName);
// Should show the division name
```

### Check AJAX URL
```javascript
var originalDivName = $('#originalDivName').val();
var url = '/FPS/DivisionMaintenance/Edit?originalDivName=' + encodeURIComponent(originalDivName);
console.log('URL:', url);
// Should be: /FPS/DivisionMaintenance/Edit?originalDivName=DIV001
```

### Manual Test Update
```javascript
// Open Edit modal first
updateDivision();
// Watch console for output
```

---

## What If It Still Doesn't Work?

### Issue: "originalDivName is not defined" Error
**Cause**: Browser cache not cleared

**Fix**:
1. Close ALL browser windows
2. Reopen browser
3. Navigate to page
4. Hard refresh (Ctrl + Shift + R)

### Issue: Update Succeeds But No Changes
**Cause**: Server-side issue or validation failing

**Check**:
1. Open Network tab (F12)
2. Click Update button
3. Find the POST request to `/FPS/DivisionMaintenance/Edit?originalDivName=...`
4. Check Response:
   - If `{success: false, message: "..."}` → Read the error message
   - If HTTP 400 → Check validation errors
   - If HTTP 500 → Server error (check server logs)

### Issue: Hidden Field Not Found
**Cause**: Modal HTML doesn't have the hidden field

**Check**:
1. Click Edit button
2. In Console, run:
   ```javascript
   $('#originalDivName').length
   ```
3. Should return **1**
4. If returns **0**:
   - Check `_AddEditDivision.cshtml`
   - Verify it has: `<input type="hidden" id="originalDivName" value="@Model.DivName" />`

---

## Summary

✅ **Added missing variable declarations** (`originalDivName`, `newDivName`)  
✅ **Added validation** to prevent primary key changes  
✅ **Fixed AJAX URL** construction  
✅ **Build successful**  
✅ **Ready to test** after app restart and browser cache clear  

**Next Steps**:
1. **Restart app** (Shift+F5, then F5)
2. **Hard refresh browser** (Ctrl+Shift+R)
3. **Test update** with the steps above

The update function will now work correctly! 🚀
