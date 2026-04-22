# Edit Button Not Opening Modal - Debugging Guide

## Issue
User reported: "When I click edit button on grid, the screen doesn't pop up"

## Root Cause Fixed
The PowerShell command I used to inject the validation code escaped the apostrophes incorrectly:
- **Before**: `can\\'t` (double escaped)
- **After**: `can't` (correctly escaped)

This caused a JavaScript syntax error that broke the entire script execution, preventing the `editDivision()` function from working.

## What I Fixed
✅ **File**: `Index.cshtml`
✅ **Issue**: Changed `can\\'t` to `can't` in two locations (lines 192 and 196)
✅ **Build**: Successful

## Verification Steps

### 1. Check Browser Console for JavaScript Errors
1. Open the Division Maintenance page
2. Press **F12** to open Developer Tools
3. Click the **Console** tab
4. Look for any **red error messages**
5. If you see syntax errors about apostrophes, the fix should resolve them

### 2. Check if Authentication is Working
Both controllers still have `[AllowAnonymous]`:
- ✅ Web Controller: `DivisionMaintenanceController.cs` - Line 20
- ✅ API Controller: `DivisionController.cs` - Line 16

### 3. Test Edit Button Functionality
1. **Open Division Maintenance page**
2. **Open browser Developer Tools (F12)**
3. **Click Console tab**
4. **Click the Edit button** on any division row
5. **Expected console output**:
   ```
   editDivision called with button: [object HTMLButtonElement]
   Division name: DIV001
   Edit response received
   ```
6. **Expected result**: Modal should open with edit form

### 4. If Modal Still Doesn't Open

#### Check Console for Errors
Look for these specific errors:

**Error 1: JavaScript Syntax Error**
```
Uncaught SyntaxError: missing ) after argument list
```
**Solution**: The apostrophe fix should resolve this. Refresh the page (Ctrl+F5).

**Error 2: jQuery Not Loaded**
```
Uncaught ReferenceError: $ is not defined
```
**Solution**: Check if jQuery is loaded in `_Layout.cshtml`

**Error 3: Modal Element Not Found**
```
Console shows: undefined
```
**Solution**: Check if `<div id="modalPopup">` exists in the layout

**Error 4: AJAX Error**
```
Error in editDivision: [error details]
```
**Solution**: Check Network tab for HTTP status code (401, 403, 500, etc.)

### 5. Check Network Tab
1. Open **Developer Tools (F12)**
2. Click **Network** tab
3. Click Edit button
4. Look for the request to `/FPS/DivisionMaintenance/Edit?divName=XXX`
5. **Expected**: HTTP 200 OK with HTML response
6. **If 401/403**: Authentication issue (but AllowAnonymous is set)
7. **If 500**: Server error - check server logs

### 6. Verify Modal HTML Structure
Check that the FPS Layout has the modal container:

**File**: `Apha.FPSApps\Apha.FPSApps.Web\Areas\FPS\Views\Shared\_Layout.cshtml`

Should contain:
```html
<div class="modal fade" id="modalPopup" tabindex="-1" role="dialog">
    <div class="modal-dialog modal-lg" role="document">
        <div class="modal-content" id="modaPopupBody">
            <!-- Content loaded via AJAX -->
        </div>
    </div>
</div>
```

### 7. Quick JavaScript Test
Open browser console and run:
```javascript
// Test if jQuery is loaded
console.log(typeof $);  // Should output: "function"

// Test if editDivision function exists
console.log(typeof window.editDivision);  // Should output: "function"

// Test if modal element exists
console.log($('#modalPopup').length);  // Should output: 1

// Test if modal body exists
console.log($('#modaPopupBody').length);  // Should output: 1

// Manually call editDivision with a test button
var testBtn = $('<button data-id="TEST"></button>');
editDivision(testBtn[0]);
```

## Most Likely Solutions

### Solution 1: Hard Refresh (Most Common)
**The JavaScript file is cached by the browser**
1. Press **Ctrl + Shift + R** (Chrome/Edge)
2. Or **Ctrl + F5** (Firefox)
3. Or clear browser cache in Settings

### Solution 2: Restart Application
Since we modified method signatures earlier:
1. **Stop debugging** (Shift + F5)
2. **Clean solution**: Build → Clean Solution
3. **Rebuild**: Build → Rebuild Solution
4. **Start debugging** (F5)

### Solution 3: Check if Script Section is Loaded
The `Index.cshtml` uses `@section Scripts { }` block. Verify that `_Layout.cshtml` has:
```razor
@RenderSection("Scripts", required: false)
```

## Testing After Fix

### Test 1: Edit Button Opens Modal
1. Click Edit button
2. ✅ Modal opens
3. ✅ Form populated with division data
4. ✅ DivName field is readonly
5. ✅ Warning text shows: "(Primary Key - Cannot be updated)"

### Test 2: Edit Without Changing DivName
1. Click Edit
2. Change DivisionId or AgencyId
3. Keep DivName unchanged
4. Click Update
5. ✅ "Division updated successfully"

### Test 3: Try to Change DivName (Should Fail)
1. Click Edit
2. Use DevTools to remove readonly attribute from DivName input
3. Change DivName value
4. Click Update
5. ✅ Error message appears: "Division name can't be updated as it is primary key"

## Additional Debugging Commands

### Check File Encoding
Run in PowerShell:
```powershell
$file = 'D:\Users\atos.user14\source\repos\DEFRA\apha-fps-apps\src\Apha.FPSApps\Apha.FPSApps.Web\Areas\FPS\Views\DivisionMaintenance\Index.cshtml'
Get-Content $file | Select-String "can't"
```
**Expected output**: Should show lines with `can't` (not `can\\'t`)

### Verify JavaScript Syntax
Run in PowerShell:
```powershell
$file = 'D:\Users\atos.user14\source\repos\DEFRA\apha-fps-apps\src\Apha.FPSApps\Apha.FPSApps.Web\Areas\FPS\Views\DivisionMaintenance\Index.cshtml'
$content = Get-Content $file -Raw
if ($content -match "can\\\\'t") {
    Write-Host "ERROR: Found double-escaped apostrophes!" -ForegroundColor Red
} else {
    Write-Host "OK: No double-escaped apostrophes found" -ForegroundColor Green
}
```

## Summary
✅ **Fixed**: JavaScript apostrophe escaping issue  
✅ **Verified**: AllowAnonymous is still present on both controllers  
✅ **Build**: Successful  
✅ **Action Required**: 
1. **Hard refresh** your browser (Ctrl + Shift + R)
2. Test edit button again
3. Check browser console for any remaining errors

If the issue persists after hard refresh, follow the debugging steps above and report:
1. Any console error messages (exact text)
2. Network tab response (HTTP status code)
3. Screenshot of the console when clicking Edit button
