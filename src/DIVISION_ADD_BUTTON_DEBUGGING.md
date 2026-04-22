# Division Add Button - Debugging Guide

## ✅ Confirmed Working Components

### 1. **Partial View (_AddEditDivision.cshtml)**
- **Location**: `Apha.FPSApps.Web\Areas\FPS\Views\DivisionMaintenance\_AddEditDivision.cshtml`
- **Status**: ✅ File exists and contains all 4 fields in correct order:
  - Division ID (hidden in Add mode, visible in Edit mode)
  - Agency ID (dropdown)
  - Division Name (text input)
  - Central Overhead (number input)

### 2. **Controller Create Action**
- **Location**: `DivisionMaintenanceController.cs` - Line 116
- **Method**: `Create()` GET action
- **Returns**: `PartialView("_AddEditDivision", model)`
- **Status**: ✅ Correctly configured

### 3. **Add Button JavaScript**
- **Location**: `Index.cshtml` - Lines 74-93
- **Function**: `addDivision(btn)`
- **AJAX Call**: 
  ```javascript
  url: '@Url.Action("Create", "DivisionMaintenance", new { area = "FPS" })'
  ```
- **Modal Target**: `$('#modaPopupBody').html(html)`
- **Status**: ✅ Correctly wired

### 4. **Modal Popup Structure**
- **Location**: `_Layout.cshtml` - Lines 335-340
- **Modal ID**: `modalPopup`
- **Content Container**: `modaPopupBody`
- **Status**: ✅ Exists in layout

## 🔍 Troubleshooting Steps

### Step 1: Check Browser Developer Console
1. Open your browser's Developer Tools (F12)
2. Go to the **Console** tab
3. Click the **Add** button
4. Look for any of these messages:
   - ✅ `"addDivision called"` - Add button was clicked
   - ✅ `"Create response received"` - Server responded successfully
   - ❌ Any **red error messages** - JavaScript errors
   - ❌ `"Error in addDivision"` - AJAX call failed

### Step 2: Check Network Tab
1. In Developer Tools, go to the **Network** tab
2. Click the **Add** button
3. Look for a request to: `/FPS/DivisionMaintenance/Create`
4. Check:
   - **Status Code**: Should be `200 OK`
   - **Response**: Should contain HTML with form fields
   - **Preview**: Should show the form HTML

### Step 3: Check Modal Visibility
After clicking Add button, inspect the modal in Developer Tools:

```html
<div class="modal fade show" id="modalPopup" style="display: flex;">
    <div class="modal-dialog modal-md">
        <div id="modaPopupBody" class="modal-content">
            <!-- Should contain the form HTML here -->
        </div>
    </div>
</div>
```

**What to check:**
- Modal has class `show` added
- Style is `display: flex`
- `modaPopupBody` contains the form fields

### Step 4: Check if Fields Are Rendered But Hidden
1. Right-click on the modal area > Inspect
2. Check if form fields exist but are hidden by CSS
3. Look for:
   - `display: none`
   - `visibility: hidden`
   - `opacity: 0`
   - Height/width set to 0

## 🐛 Common Issues & Solutions

### Issue 1: Modal Opens But No Fields Visible
**Possible Cause**: CSS hiding fields or modal-body has no height
**Solution**: Check CSS for `.modal-content`, `.modal-body`, `.govuk-form-group`

### Issue 2: "Create response received" But No HTML
**Possible Cause**: Response is empty or contains errors
**Solution**: 
1. Check Network tab Response
2. Add breakpoint in Controller Create() method
3. Verify Model is being created correctly

### Issue 3: JavaScript Function Not Found
**Possible Cause**: Scripts not loaded or naming mismatch
**Solution**: Check if `ajax-form-validation.js` is loaded

### Issue 4: Modal Doesn't Open At All
**Possible Cause**: `addDivision()` not called or modalPopup not found
**Solution**: Check DataGrid button configuration

## 📋 Expected Behavior

When working correctly, here's what should happen:

1. **Click Add Button**
   - Console logs: `"addDivision called"`
   
2. **AJAX Request Sent**
   - Network shows: `GET /FPS/DivisionMaintenance/Create`
   - Server returns HTML partial view
   
3. **HTML Injected**
   - Console logs: `"Create response received"`
   - HTML inserted into `#modaPopupBody`
   
4. **Modal Displayed**
   - Modal gets class `show`
   - Modal style set to `display: flex`
   - Form fields become visible with:
     - **Agency ID** dropdown (empty except "-- Select Agency --")
     - **Division Name** text input
     - **Central Overhead** number input
   
5. **Agencies Loaded**
   - jQuery `$(document).ready()` fires
   - `loadAgencies()` populates dropdown with 3 sample agencies:
     - APHA (id: 1)
     - DEFRA (id: 2)
     - VMD (id: 3)

## 🔧 Quick Test

Run this in browser console after clicking Add button:

```javascript
// Check if modal exists
console.log('Modal exists:', $('#modalPopup').length > 0);

// Check modal content
console.log('Modal content:', $('#modaPopupBody').html());

// Check if form exists
console.log('Form exists:', $('#addDivisionForm').length > 0);

// Check form fields
console.log('AgencyId field:', $('#AgencyId').length > 0);
console.log('DivName field:', $('input[name="DivName"]').length > 0);
console.log('CentOverhead field:', $('input[name="CentOverhead"]').length > 0);

// Check dropdown options
console.log('Agency options:', $('#AgencyId option').length);
```

Expected output if working:
```
Modal exists: true
Modal content: [full HTML of the form]
Form exists: true
AgencyId field: true
DivName field: true
CentOverhead field: true
Agency options: 4 (1 empty + 3 agencies)
```

## 🎯 Next Steps

1. **Open browser Developer Tools (F12)**
2. **Go to Console tab**
3. **Click the Add button**
4. **Share any error messages or unexpected console output**
5. **Check Network tab for the Create request**
6. **Copy the Response from Network tab**

This will help identify exactly where the issue is occurring.
