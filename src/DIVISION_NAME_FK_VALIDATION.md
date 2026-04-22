# ✅ Division Name Foreign Key Validation - Complete Implementation

## Overview
Implemented smart Division Name editing:
- ✅ Division Name is now **editable** in edit mode
- ✅ **No warning message** about primary key
- ✅ **No client-side validation** blocking changes  
- ✅ **Server-side check** for foreign key references
- ✅ Shows error **"Cannot edit Division Name as it is used in [table names]"** if referenced
- ✅ **Allows update** if not referenced in other tables

---

## What Changed

### 1. View Layer (_AddEditDivision.cshtml)
**Removed:**
- ❌ `readonly="@isEditMode"` attribute
- ❌ Warning message "(Primary Key - Cannot be updated)"

**Now:**
```razor
<div class="govuk-form-group sup_margin_0">
    <label class="govuk-label" asp-for="DivName">Division Name</label>
    <input type="text" asp-for="DivName" required class="govuk-input" maxlength="255">
    <span asp-validation-for="DivName" class="govuk-error-message" style="display:none;"></span>
</div>
```
✅ **Fully editable** in both add and edit modes

---

### 2. JavaScript (Index.cshtml)
**Removed:**
- ❌ Client-side validation checking if DivName changed
- ❌ Error display preventing form submission

**Now:**
```javascript
function updateDivision() {
    // ... validation ...
    
    var originalDivName = $('#originalDivName').val();
    var newDivName = form.find('[name="DivName"]').val();

    // NO CLIENT-SIDE CHECK - let server decide
    
    var data = {
        DivisionId: divisionId,
        DivName: newDivName,  // Can be different from original
        AgencyId: parseInt(form.find('[name="AgencyId"]').val()),
        CentOverhead: centOverhead
    };
    
    // Send to server for validation
}
```
✅ **No blocking** - user can change DivName freely

---

### 3. Repository Interface (IDivisionRepository.cs)
**Added new method:**
```csharp
/// <summary>
/// Checks if a division name is referenced in other tables as a foreign key.
/// </summary>
/// <param name="divName">Division name to check for references.</param>
/// <returns>List of table names where the division name is referenced. Empty list if no references.</returns>
Task<List<string>> GetDivisionForeignKeyReferencesAsync(string divName);
```

---

### 4. Repository Implementation (DivisionRepository.cs)
**Added method:**
```csharp
public async Task<List<string>> GetDivisionForeignKeyReferencesAsync(string divName)
{
    var referencedTables = new List<string>();

    if (string.IsNullOrWhiteSpace(divName))
    {
        return referencedTables;
    }

    // Check ProfitCentre table (division field references divname)
    var profitCentreExists = await _context.Set<ProfitCentre>()
        .AsNoTracking()
        .AnyAsync(pc => pc.Division == divName);

    if (profitCentreExists)
    {
        referencedTables.Add("tlkpProfitCentre");
    }

    // Easy to add more tables:
    // var otherTableExists = await _context.Set<OtherTable>()
    //     .AsNoTracking()
    //     .AnyAsync(t => t.DivName == divName);
    // if (otherTableExists) referencedTables.Add("OtherTable");

    return referencedTables;
}
```

**Tables Checked:**
1. ✅ **tlkpProfitCentre** - checks `Division` field

**To Add More Tables:**
Just copy the pattern and add more checks before the `return` statement.

---

### 5. Service Layer (DivisionService.cs)
**Updated validation logic:**
```csharp
// Check if DivName is being changed
if (!originalDivName.Equals(divisionDto.DivName, StringComparison.OrdinalIgnoreCase))
{
    // 1. Check for duplicate new name
    var nameConflict = await _divisionRepository.DivisionExistsAsync(divisionDto.DivName);
    if (nameConflict)
    {
        throw new InvalidOperationException($"Cannot rename to '{divisionDto.DivName}' - division already exists.");
    }

    // 2. Check if the division name is referenced in other tables (NEW!)
    var referencedTables = await _divisionRepository.GetDivisionForeignKeyReferencesAsync(originalDivName);
    if (referencedTables.Any())
    {
        var tableList = string.Join(", ", referencedTables);
        throw new InvalidOperationException($"Cannot edit Division Name as it is used in {tableList}");
    }
}

// If we get here, the update is allowed
var division = _mapper.Map<Division>(divisionDto);
var updatedDivision = await _divisionRepository.UpdateDivisionAsync(originalDivName, division);
return _mapper.Map<DivisionDto>(updatedDivision);
```

**Validation Flow:**
1. ✅ Check if name changed
2. ✅ If yes, check for duplicate
3. ✅ If yes, check for foreign key references  
4. ✅ If no references, allow update (delete + insert)
5. ✅ If not changing, allow update (normal update)

---

### 6. Controller (DivisionMaintenanceController.cs)
**Removed:**
- ❌ Server-side check preventing DivName changes

**Now relies on service layer** for all business logic validation.

---

## How It Works

### Scenario 1: DivName NOT Used in Other Tables ✅
```
User Action:
- Opens Edit for "DIV001"
- Changes DivName to "DIV999"
- Clicks Update

Server Processing:
1. Service receives: originalDivName="DIV001", newDivName="DIV999"
2. Checks if "DIV999" already exists → NO ✅
3. Checks if "DIV001" referenced in:
   - tlkpProfitCentre → NO ✅
4. Executes update:
   - Deletes record with DivName="DIV001"
   - Inserts record with DivName="DIV999"
5. Returns success

Result: ✅ "Division updated successfully"
Grid shows DIV999 instead of DIV001
```

### Scenario 2: DivName IS Used in ProfitCentre Table ❌
```
User Action:
- Opens Edit for "DIV001"
- Changes DivName to "DIV999"
- Clicks Update

Server Processing:
1. Service receives: originalDivName="DIV001", newDivName="DIV999"
2. Checks if "DIV999" already exists → NO ✅
3. Checks if "DIV001" referenced in:
   - tlkpProfitCentre → YES (found records with Division="DIV001") ❌
4. Throws exception: "Cannot edit Division Name as it is used in tlkpProfitCentre"

Result: ❌ Error message shown to user
Update is blocked
```

### Scenario 3: DivName Unchanged (Edit Other Fields) ✅
```
User Action:
- Opens Edit for "DIV001"
- Changes DivisionId from 1 to 100
- Changes AgencyId to 5
- Keeps DivName as "DIV001"
- Clicks Update

Server Processing:
1. Service receives: originalDivName="DIV001", newDivName="DIV001"
2. Names match → Skip duplicate and foreign key checks
3. Executes normal update (no delete/insert)

Result: ✅ "Division updated successfully"
DivisionId and AgencyId updated, DivName unchanged
```

### Scenario 4: Duplicate Name ❌
```
User Action:
- Opens Edit for "DIV001"
- Changes DivName to "DIV002" (already exists)
- Clicks Update

Server Processing:
1. Service receives: originalDivName="DIV001", newDivName="DIV002"
2. Checks if "DIV002" already exists → YES ❌
3. Throws exception: "Cannot rename to 'DIV002' - division already exists."

Result: ❌ Error message shown to user
```

---

## Database Schema Reference

### Division Table (tlkpDivision)
```sql
CREATE TABLE fps.tlkpdivision (
    divname VARCHAR(255) PRIMARY KEY,  -- PRIMARY KEY
    divisionid INTEGER NOT NULL,
    agencyid INTEGER NOT NULL,
    centoverhead DECIMAL(19,2)
);
```

### ProfitCentre Table (tlkpProfitCentre)
```sql
CREATE TABLE fps.tlkpprofitcentre (
    profitcentreid VARCHAR PRIMARY KEY,
    profitcentrename VARCHAR NOT NULL,
    division VARCHAR,  -- FOREIGN KEY to tlkpdivision.divname
    conttarget DECIMAL,
    -- ... other fields
    FOREIGN KEY (division) REFERENCES fps.tlkpdivision(divname)
);
```

**Foreign Key Relationship:**
- `tlkpProfitCentre.division` → `tlkpDivision.divname`
- If ProfitCentre records exist with a specific division name, that name cannot be changed

---

## Testing Guide

### Test 1: Update DivName (No FK References) ✅
**Setup:**
1. Ensure DIV001 exists in tlkpDivision
2. Ensure NO records in tlkpProfitCentre with division="DIV001"

**Steps:**
1. Click Edit on DIV001
2. Change DivName to "TESTDIV"
3. Click Update

**Expected:**
- ✅ Success message: "Division updated successfully"
- ✅ Grid refreshes showing "TESTDIV" instead of "DIV001"
- ✅ Database: old record deleted, new record inserted

**Verify in Database:**
```sql
SELECT * FROM fps.tlkpdivision WHERE divname = 'DIV001';
-- Returns 0 rows

SELECT * FROM fps.tlkpdivision WHERE divname = 'TESTDIV';
-- Returns 1 row with all field values
```

---

### Test 2: Update DivName (WITH FK References) ❌
**Setup:**
1. Insert test data:
```sql
INSERT INTO fps.tlkpdivision (divname, divisionid, agencyid, centoverhead)
VALUES ('DIV_TEST', 1, 1, 1000.00);

INSERT INTO fps.tlkpprofitcentre (profitcentreid, profitcentrename, division)
VALUES ('PC001', 'Test Profit Centre', 'DIV_TEST');
```

**Steps:**
1. Click Edit on DIV_TEST
2. Change DivName to "DIV_NEW"
3. Click Update

**Expected:**
- ❌ Error message: "Cannot edit Division Name as it is used in tlkpProfitCentre"
- ❌ Update blocked
- ❌ Division name remains "DIV_TEST" in grid

**Verify in Console (F12):**
```
Update result: {
    success: false,
    message: "Failed to update division.",
    errors: [{
        field: "",
        message: "Cannot edit Division Name as it is used in tlkpProfitCentre"
    }]
}
```

---

### Test 3: Update Other Fields (DivName Unchanged) ✅
**Steps:**
1. Click Edit on any division
2. Change DivisionId to 999
3. Change AgencyId to 5
4. Keep DivName unchanged
5. Click Update

**Expected:**
- ✅ Success message: "Division updated successfully"
- ✅ Grid shows updated DivisionId and AgencyId
- ✅ DivName unchanged

---

### Test 4: Duplicate Name Check ❌
**Setup:**
- Ensure DIV001 and DIV002 both exist

**Steps:**
1. Click Edit on DIV001
2. Change DivName to "DIV002"
3. Click Update

**Expected:**
- ❌ Error message: "Cannot rename to 'DIV002' - division already exists."

---

## Adding More Tables to Check

To check additional tables for foreign key references, edit `DivisionRepository.cs`:

```csharp
public async Task<List<string>> GetDivisionForeignKeyReferencesAsync(string divName)
{
    var referencedTables = new List<string>();

    if (string.IsNullOrWhiteSpace(divName))
    {
        return referencedTables;
    }

    // Existing check
    var profitCentreExists = await _context.Set<ProfitCentre>()
        .AsNoTracking()
        .AnyAsync(pc => pc.Division == divName);
    if (profitCentreExists)
    {
        referencedTables.Add("tlkpProfitCentre");
    }

    // ADD NEW TABLE CHECKS HERE:
    
    // Example: Check Projects table
    var projectExists = await _context.Set<Project>()
        .AsNoTracking()
        .AnyAsync(p => p.DivName == divName);
    if (projectExists)
    {
        referencedTables.Add("tblProjects");
    }

    // Example: Check another table
    var anotherTableExists = await _context.Set<AnotherEntity>()
        .AsNoTracking()
        .AnyAsync(e => e.DivisionName == divName);
    if (anotherTableExists)
    {
        referencedTables.Add("tblAnotherTable");
    }

    return referencedTables;
}
```

**Steps to add new table:**
1. Find the entity class (e.g., `Project.cs`)
2. Find the field that references DivName
3. Copy the check pattern
4. Update the table name in `referencedTables.Add()`

---

## Files Modified

### Frontend:
1. ✅ `_AddEditDivision.cshtml` - Made DivName editable, removed warning
2. ✅ `Index.cshtml` - Removed client-side validation
3. ✅ `DivisionMaintenanceController.cs` - Removed controller validation

### Backend API:
4. ✅ `IDivisionRepository.cs` - Added `GetDivisionForeignKeyReferencesAsync` method
5. ✅ `DivisionRepository.cs` - Implemented FK reference check
6. ✅ `DivisionService.cs` - Added FK validation logic

---

## Restart Instructions

Since interface method was added, **restart required**:

1. **Stop debugging** (Shift + F5)
2. **Rebuild solution** (Ctrl + Shift + B)
3. **Start debugging** (F5)
4. **Hard refresh browser** (Ctrl + Shift + R)

---

## Summary

✅ **DivName now editable** in edit mode  
✅ **No warning message** displayed  
✅ **No client-side blocking** of changes  
✅ **Server validates** foreign key usage  
✅ **Shows specific table names** in error message  
✅ **Allows update** if no FK references  
✅ **Uses delete+insert** for primary key updates  
✅ **Currently checks**: tlkpProfitCentre  
✅ **Extensible**: Easy to add more tables  

**Error Message Format:**
- If referenced: `"Cannot edit Division Name as it is used in tlkpProfitCentre"`
- If multiple tables: `"Cannot edit Division Name as it is used in tlkpProfitCentre, tblProjects, tblOtherTable"`
