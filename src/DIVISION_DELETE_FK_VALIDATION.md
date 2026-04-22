# Division Delete Foreign Key Validation

## Overview

Added foreign key validation to the Division delete operation to prevent deletion of divisions that are referenced in other tables. This ensures data integrity and provides clear error messages to users.

## Changes Made

### 1. Service Layer - `DivisionService.cs`

**File:** `Apha.FPS\Apha.FPS.Application\Services\DivisionService.cs`

**Method:** `DeleteDivisionAsync`

Added FK validation check before attempting to delete:

```csharp
public async Task<bool> DeleteDivisionAsync(string divName)
{
    if (string.IsNullOrWhiteSpace(divName))
    {
        throw new ArgumentException("Division name cannot be null or empty.", nameof(divName));
    }

    Console.WriteLine($"[DivisionService] Attempting to delete division: {divName}");

    // Check if the division name is referenced in other tables (foreign key check)
    Console.WriteLine($"[DivisionService] Checking FK references for: {divName}");
    var referencedTables = await _divisionRepository.GetDivisionForeignKeyReferencesAsync(divName);
    Console.WriteLine($"[DivisionService] FK references found: {referencedTables.Count} - Tables: {string.Join(", ", referencedTables)}");

    if (referencedTables.Any())
    {
        var tableList = string.Join(", ", referencedTables);
        Console.WriteLine($"[DivisionService] THROWING FK VALIDATION ERROR: Cannot delete Division as it is used in {tableList}");
        throw new InvalidOperationException($"Cannot delete Division as it is used in {tableList}");
    }

    Console.WriteLine($"[DivisionService] No FK references found, proceeding with delete");
    return await _divisionRepository.DeleteDivisionAsync(divName);
}
```

**Key Changes:**
- Calls `GetDivisionForeignKeyReferencesAsync` to check for FK references in:
  - `tblkpprofitcentre` (ProfitCentre table)
  - `divisiongrade` (DivisionGrade table)
- Throws `InvalidOperationException` with descriptive message if FK references exist
- Proceeds with delete only if no FK references found

### 2. API Controller - `DivisionController.cs`

**File:** `Apha.FPS\Apha.FPS.Api\Controllers\DivisionController.cs`

**Method:** `DeleteDivisionAsync`

Added specific exception handling for FK validation errors:

```csharp
catch (InvalidOperationException ex)
{
    _logger.LogWarning(ex, "[DeleteDivision] Business logic error: {Message}", ex.Message);
    return BadRequest(new 
    { 
        success = false,
        message = ex.Message,
        errors = new[] { new { code = "BUSINESS_LOGIC_ERROR", message = ex.Message } }
    });
}
```

**Key Changes:**
- Added `InvalidOperationException` catch block before the generic `Exception` catch
- Returns `BadRequest` (HTTP 400) with structured error response
- Error response format matches the pattern used in Create/Update operations
- `ApiResponseActionFilter` will wrap this response in the standard `ApiResponse` structure

### 3. Frontend JavaScript - `Index.cshtml`

**File:** `Apha.FPSApps\Apha.FPSApps.Web\Areas\FPS\Views\DivisionMaintenance\Index.cshtml`

**Function:** `deleteDivision`

Enhanced error handling in the AJAX error callback:

```javascript
error: function (xhr, status, error) {
    console.error('Error deleting:', error, xhr);
    // Handle FK constraint or business logic errors
    if (xhr.status === 400 && xhr.responseJSON) {
        var errorMessage = xhr.responseJSON.message || 'Failed to delete division';
        alert(errorMessage);
    } else {
        alert('An error occurred while deleting the division');
    }
}
```

**Key Changes:**
- Checks for HTTP 400 (BadRequest) status
- Extracts error message from response JSON
- Displays specific FK validation error to user
- Falls back to generic error message for other errors

## Flow Diagram

### Successful Delete (No FK References)

```
User clicks Delete → Confirmation → AJAX DELETE request
    ↓
Frontend Controller: DeleteDivisionAsync
    ↓
Application Service: DeleteDivisionAsync
    ↓
Repository: GetDivisionForeignKeyReferencesAsync → []
    ↓
Repository: DeleteDivisionAsync → true
    ↓
API Controller: Ok(true)
    ↓
ApiResponseActionFilter: Wraps response
{
    "success": true,
    "data": true,
    "errors": null,
    "meta": { ... }
}
    ↓
Frontend: Success alert → Page reload
```

### Failed Delete (FK References Exist)

```
User clicks Delete → Confirmation → AJAX DELETE request
    ↓
Frontend Controller: DeleteDivisionAsync
    ↓
Application Service: DeleteDivisionAsync
    ↓
Repository: GetDivisionForeignKeyReferencesAsync → ["tblkpprofitcentre"]
    ↓
Service throws InvalidOperationException:
"Cannot delete Division as it is used in tblkpprofitcentre"
    ↓
API Controller catches InvalidOperationException
    ↓
Returns BadRequest:
{
    "success": false,
    "message": "Cannot delete Division as it is used in tblkpprofitcentre",
    "errors": [{ "code": "BUSINESS_LOGIC_ERROR", "message": "..." }]
}
    ↓
ApiResponseActionFilter: Wraps error response
{
    "success": false,
    "data": null,
    "errors": [{ "code": "BUSINESS_LOGIC_ERROR", "message": "..." }],
    "meta": { "message": "Cannot delete Division as it is used in tblkpprofitcentre", ... }
}
    ↓
Frontend: HTTP 400 error → Displays FK validation message to user
```

## Foreign Key Tables Checked

The validation checks if the division name exists in the `division` column of:

1. **tblkpprofitcentre** (ProfitCentre entity)
   - Entity: `ProfitCentre`
   - Column: `Division`

2. **divisiongrade** (DivisionGrade entity)
   - Entity: `DivisionGrade`
   - Column: `Division`

## Error Messages

### FK Constraint Violation (Single Table)
```
Cannot delete Division as it is used in tblkpprofitcentre
```

### FK Constraint Violation (Multiple Tables)
```
Cannot delete Division as it is used in tblkpprofitcentre, divisiongrade
```

### Division Not Found
```
Division with name 'XXX' not found
```

## Testing Scenarios

### Scenario 1: Delete Division with No FK References
**Steps:**
1. User clicks delete on a division not used in any other tables
2. Confirms deletion
3. **Expected Result:** Division deleted successfully, success message displayed

### Scenario 2: Delete Division with FK References in ProfitCentre
**Steps:**
1. User clicks delete on a division used in tblkpprofitcentre
2. Confirms deletion
3. **Expected Result:** Error message: "Cannot delete Division as it is used in tblkpprofitcentre"

### Scenario 3: Delete Division with FK References in DivisionGrade
**Steps:**
1. User clicks delete on a division used in divisiongrade table
2. Confirms deletion
3. **Expected Result:** Error message: "Cannot delete Division as it is used in divisiongrade"

### Scenario 4: Delete Division with FK References in Both Tables
**Steps:**
1. User clicks delete on a division used in both tables
2. Confirms deletion
3. **Expected Result:** Error message: "Cannot delete Division as it is used in tblkpprofitcentre, divisiongrade"

## Consistency with Add/Edit Operations

The delete FK validation now uses the **same pattern** as the add/edit operations:

| Operation | FK Validation | Error Format |
|-----------|---------------|--------------|
| **Create** | Checks if division name already exists in FK tables | `InvalidOperationException` → `BadRequest` with message |
| **Update** | Checks if renaming would violate FK constraints | `InvalidOperationException` → `BadRequest` with message |
| **Delete** | Checks if division is referenced in FK tables | `InvalidOperationException` → `BadRequest` with message |

## Benefits

1. ✅ **Data Integrity:** Prevents orphaned references in related tables
2. ✅ **User-Friendly:** Clear error messages explain why deletion is blocked
3. ✅ **Consistent:** Same validation pattern as create/update operations
4. ✅ **Debuggable:** Console logging for troubleshooting
5. ✅ **Maintainable:** Centralized FK checking in repository layer

## Notes

- The `GetDivisionForeignKeyReferencesAsync` method in the repository already existed and is reused for delete validation
- The validation is performed **before** attempting database deletion, preventing database errors
- The error response is wrapped by `ApiResponseActionFilter` to maintain consistent API response structure
- The frontend properly handles HTTP 400 errors and displays the specific validation message
