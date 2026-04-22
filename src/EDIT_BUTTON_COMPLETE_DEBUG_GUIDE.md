# Edit Button Not Opening Modal - Step-by-Step Debug Guide

## IMMEDIATE ACTIONS (Do these first!)

### Step 1: Open Browser Console (F12)
1. Navigate to Division Maintenance page
2. Press **F12** to open Developer Tools
3. Click **Console** tab
4. Look for **RED ERROR MESSAGES**
5. **Copy and paste ANY errors you see**

### Step 2: Check if Scripts Loaded
In the console, type each command and press Enter:

```javascript
// Check 1: Is jQuery loaded?
typeof $
// Expected: "function" | If "undefined" = jQuery not loaded

// Check 2: Does editDivision function exist?
typeof window.editDivision
// Expected: "function" | If "undefined" = JavaScript not loaded or has syntax error

// Check 3: Does modal exist?
$('#modalPopup').length
// Expected: 1 | If 0 = modal HTML missing from layout

// Check 4: Does modal body exist?
$('#modaPopupBody').length
// Expected: 1 | If 0 = modal body missing

// Check 5: Do edit buttons exist?
$('.edit-row-btn').length
// Expected: number > 0 | If 0 = buttons not rendered

// Check 6: What's on the first edit button?
var btn = $('.edit-row-btn').first();
console.log('data-edit-function:', btn.data('edit-function'));
console.log('data-id:', btn.data('id'));
// Expected: data-edit-function: "editDivision", data-id: "DIV001" (or similar)
```

### Step 3: Test Manual Function Call
In console, run:

```javascript
// Create a test button
var testBtn = $('<button></button>').attr('data-id', 'TEST');
// Call editDivision
editDivision(testBtn[0]);
// Watch console for output
```

**What should happen:**
- Console shows: "editDivision called with button: ..."
- Console shows: "Division name: TEST"
- AJAX request sent to server
- Either error or success message

## COMMON ISSUES & SOLUTIONS

### Issue 1: JavaScript Syntax Error
**Symptoms:**
- Console shows: `Uncaught SyntaxError`
- Functions not defined (`typeof window.editDivision` returns "undefined")

**Solution:**
1. Hard refresh: **Ctrl + Shift + R**
2. Clear cache: Settings → Privacy → Clear browsing data
3. Restart Visual Studio and rebuild

### Issue 2: jQuery Not Loaded
**Symptoms:**
- Console shows: `$ is not defined`
- `typeof $` returns "undefined"

**Solution:**
Check `_Layout.cshtml` has jQuery script:
```html
<script src="~/lib/jquery/dist/jquery.min.js"></script>
```

### Issue 3: Modal HTML Missing
**Symptoms:**
- `$('#modalPopup').length` returns 0
- No modal container in page

**Solution:**
Check `Areas/FPS/Views/Shared/_Layout.cshtml` contains:
```html
<div class="modal fade" id="modalPopup" tabindex="-1" role="dialog">
    <div class="modal-dialog modal-lg" role="document">
        <div class="modal-content" id="modaPopupBody">
            <!-- Content loaded here -->
        </div>
    </div>
</div>
```

### Issue 4: Edit Buttons Not Wired Up
**Symptoms:**
- Clicking button does nothing
- No console output when clicking
- `btn.data('edit-function')` is undefined or empty

**Check DataGrid Component:**
The buttons should be generated with:
```html
<button type="button" 
        class="edit-row-btn" 
        data-edit-function="editDivision" 
        data-id="@item.DivName"
        onclick="window['editDivision'](this)">
    Edit
</button>
```

### Issue 5: AJAX Request Failing
**Symptoms:**
- Function called (console shows "editDivision called...")
- No modal opens
- Console shows AJAX error

**Check Network Tab:**
1. Open F12 → **Network** tab
2. Click Edit button
3. Look for request to `/FPS/DivisionMaintenance/Edit`
4. Click the request
5. Check:
   - **Status Code**: Should be 200
   - If 404: URL is wrong
   - If 401/403: Authentication issue
   - If 500: Server error (check server logs)
   - **Response**: Should be HTML (the modal content)

### Issue 6: Modal CSS Issue
**Symptoms:**
- AJAX succeeds
- Console shows "Edit response received"
- Modal doesn't appear (might be hidden or behind other elements)

**Solution in Console:**
```javascript
// Force show modal
var modal = $('#modalPopup');
modal.css({
    'display': 'flex',
    'z-index': '9999',
    'background-color': 'rgba(0,0,0,0.5)',
    'position': 'fixed',
    'top': '0',
    'left': '0',
    'width': '100%',
    'height': '100%'
});
modal.addClass('show');
```

## STEP-BY-STEP FIX PROCESS

### Option A: Use Clean JavaScript File
I've created `division-maintenance-scripts-clean.js` with corrected code.

**To use it:**
1. Copy content from `division-maintenance-scripts-clean.js`
2. Replace the entire `@section Scripts { }` block in `Index.cshtml`
3. Hard refresh browser (Ctrl + Shift + R)

### Option B: Fix Current File
1. **Stop debugging** (Shift + F5)
2. **Clean solution**: Build → Clean Solution
3. **Delete `bin` and `obj` folders** in Web project
4. **Rebuild**: Build → Rebuild Solution
5. **Start debugging** (F5)
6. **Hard refresh browser** (Ctrl + Shift + R)

### Option C: Manual JavaScript Test
1. Open Division Maintenance page
2. Open Console (F12)
3. Paste this entire code block:

```javascript
window.editDivision = function(btn) {
    console.log('MANUAL editDivision called');
    var divName = $(btn).data('id');
    console.log('Division name:', divName);
    
    if (!divName) {
        alert('No divName found!');
        return;
    }
    
    $.ajax({
        url: '/FPS/DivisionMaintenance/Edit',
        type: 'GET',
        data: { divName: divName },
        success: function (html) {
            console.log('SUCCESS - Response length:', html.length);
            $('#modaPopupBody').html(html);
            $('#modalPopup').css('display', 'flex').addClass('show');
        },
        error: function (xhr, status, error) {
            console.error('ERROR:', status, error);
            console.error('Status Code:', xhr.status);
            console.error('Response:', xhr.responseText);
            alert('AJAX Error: ' + error);
        }
    });
};
console.log('Manual editDivision loaded');
```

4. Try clicking Edit button again
5. Report what console shows

## WHAT TO REPORT BACK

Please copy and paste the output of these commands:

### Command Set 1: Check Environment
```javascript
console.log('=== ENVIRONMENT CHECK ===');
console.log('jQuery loaded:', typeof $ !== 'undefined');
console.log('jQuery version:', $.fn ? $.fn.jquery : 'N/A');
console.log('editDivision exists:', typeof window.editDivision);
console.log('Modal exists:', $('#modalPopup').length);
console.log('Modal body exists:', $('#modaPopupBody').length);
console.log('Edit buttons count:', $('.edit-row-btn').length);
```

### Command Set 2: Check First Button
```javascript
console.log('=== FIRST BUTTON CHECK ===');
var btn = $('.edit-row-btn').first();
if (btn.length) {
    console.log('data-edit-function:', btn.data('edit-function'));
    console.log('data-id:', btn.data('id'));
    console.log('onclick:', btn.attr('onclick'));
} else {
    console.log('NO EDIT BUTTONS FOUND!');
}
```

### Command Set 3: Check for JavaScript Errors
Look at the top of the console - are there any red error messages? Copy them exactly.

## ALTERNATIVE: Direct HTML Check

If nothing works, check the actual HTML:

1. Right-click Division Maintenance page → **View Page Source**
2. Search for (`Ctrl+F`): `modalPopup`
   - Should find: `<div class="modal" id="modalPopup"`
   - If not found: Modal not in layout
3. Search for: `editDivision`
   - Should find: `function editDivision(`
   - If not found: Scripts not loaded
4. Search for: `edit-row-btn`
   - Should find buttons with this class
   - Check if they have `data-id` attribute with division names

## LAST RESORT: Complete File Replacement

If all else fails, I can generate a completely new Index.cshtml file with verified working code.

**Before requesting this:**
- Run all diagnostic commands above
- Report exact console output
- Check if other pages work (to rule out global issues)

---

## Quick Win: Try This First!

**Most common fix:**
1. Press **Ctrl + Shift + Delete** in browser
2. Select "Cached images and files"
3. Click "Clear data"
4. Press **Ctrl + F5** to hard refresh page
5. Try Edit button again

**Still not working?**
Run the diagnostic commands above and report the output!
