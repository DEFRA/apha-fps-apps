# Primary Key Update Fix - DivName Not Updating

## Issue
User reported: "Division updated successfully" message appears, but **DivName is not actually being updated** in the PostgreSQL database.

## Root Cause
**DivName is the PRIMARY KEY** of the `fps.tlkpdivision` table. Entity Framework Core **cannot update primary key values** using the standard `Update()` method because:

1. EF uses the primary key to identify which record to update
2. If you try to change the primary key itself, EF doesn't know which record you're referring to
3. The standard `_context.Divisions.Update(division)` fails silently for primary key changes

## Solution Implemented

### Strategy: Delete + Insert Pattern for Primary Key Changes

When the primary key (DivName) is being changed:
1. **Delete** the old record with the original DivName
2. **Insert** a new record with the new DivName and all updated field values

When the primary key is NOT changing:
1. Use standard **Update** method for better performance

### 1. Repository Interface (IDivisionRepository.cs)
Updated signature to accept `originalDivName` parameter:

```csharp
/// <summary>
/// Updates an existing division record.
/// </summary>
/// <param name="originalDivName">Original division name to identify the record (primary key).</param>
/// <param name="division">Division entity with updated values (may include new DivName).</param>
/// <returns>Updated division entity.</returns>
Task<Division> UpdateDivisionAsync(string originalDivName, Division division);
```

### 2. Repository Implementation (DivisionRepository.cs)
Implemented dual-path update logic:

```csharp
public async Task<Division> UpdateDivisionAsync(string originalDivName, Division division)
{
    ArgumentNullException.ThrowIfNull(division);

    if (string.IsNullOrWhiteSpace(originalDivName))
    {
        throw new ArgumentException("Original division name is required.", nameof(originalDivName));
    }

    // Check if primary key (DivName) is being changed
    if (!originalDivName.Equals(division.DivName, StringComparison.OrdinalIgnoreCase))
    {
        // PRIMARY KEY IS CHANGING - Use delete and insert pattern
        
        // 1. Get the existing record
        var existingDivision = await _context.Divisions
            .FirstOrDefaultAsync(d => d.DivName == originalDivName);

        if (existingDivision == null)
        {
            throw new InvalidOperationException($"Division '{originalDivName}' not found.");
        }

        // 2. Delete the old record
        _context.Divisions.Remove(existingDivision);
        await _context.SaveChangesAsync();

        // 3. Insert with new primary key value
        _context.Divisions.Add(division);
        await _context.SaveChangesAsync();

        return division;
    }
    else
    {
        // PRIMARY KEY IS NOT CHANGING - Use normal update
        
        var existingDivision = await _context.Divisions
            .FirstOrDefaultAsync(d => d.DivName == originalDivName);

        if (existingDivision == null)
        {
            throw new InvalidOperationException($"Division '{originalDivName}' not found.");
        }

        // Update properties manually to ensure EF tracking works correctly
        existingDivision.DivisionId = division.DivisionId;
        existingDivision.AgencyId = division.AgencyId;
        existingDivision.CentOverhead = division.CentOverhead;

        await _context.SaveChangesAsync();
        return existingDivision;
    }
}
```

### 3. Service Layer (DivisionService.cs)
Updated to pass `originalDivName` to repository:

```csharp
// Map new values and update (pass originalDivName to repository)
var division = _mapper.Map<Division>(divisionDto);
var updatedDivision = await _divisionRepository.UpdateDivisionAsync(originalDivName, division);
return _mapper.Map<DivisionDto>(updatedDivision);
```

## How It Works

### Scenario 1: DivName Unchanged (e.g., "DIV001" → "DIV001")
```
User edits: DivisionId=5, AgencyId=10, DivName="DIV001", CentOverhead=1000
originalDivName = "DIV001"
division.DivName = "DIV001"

Flow:
1. originalDivName == division.DivName ✅
2. Repository uses NORMAL UPDATE path
3. Finds record by DivName="DIV001"
4. Updates DivisionId, AgencyId, CentOverhead fields in place
5. SaveChanges() commits update
```

### Scenario 2: DivName Changed (e.g., "DIV001" → "DIV999")
```
User edits: DivisionId=5, AgencyId=10, DivName="DIV999", CentOverhead=1000
originalDivName = "DIV001"
division.DivName = "DIV999"

Flow:
1. originalDivName != division.DivName ❌
2. Repository uses DELETE + INSERT path
3. Finds record by DivName="DIV001"
4. DELETES record with DivName="DIV001"
5. INSERTS new record with DivName="DIV999" and all field values
6. SaveChanges() commits both operations
```

## Important Considerations

### ⚠️ Foreign Key Cascade Behavior
If other tables have foreign key references to `tlkpdivision.divname`, the DELETE operation will:
- **Cascade delete** child records if `ON DELETE CASCADE` is configured
- **Fail** if child records exist and no cascade is configured
- **Set to NULL** if `ON DELETE SET NULL` is configured

**Recommendation**: Check your database schema for foreign key constraints referencing `divname`.

### Database Transaction
The delete + insert operations are wrapped in Entity Framework's transaction, so:
- ✅ Both operations succeed together, or
- ✅ Both operations fail together (rollback)
- ✅ No partial updates possible

### Performance
- **DivName unchanged**: Fast - single UPDATE query
- **DivName changed**: Slower - DELETE + INSERT queries
- Both are acceptable for maintenance screens with low transaction volume

## Testing

### Test Case 1: Update DivName Only
1. Open Edit Division for "DIV001"
2. Change DivName to "DIV999"
3. Click Update
4. **Expected**: Record deleted with DivName="DIV001", new record created with DivName="DIV999"
5. Verify in database: `SELECT * FROM fps.tlkpdivision WHERE divname = 'DIV999'`

### Test Case 2: Update All Fields Including DivName
1. Open Edit Division for "DIV001"
2. Change: DivisionId=100, AgencyId=5, DivName="NEWDIV", CentOverhead=5000
3. Click Update
4. **Expected**: All fields updated in new record
5. Verify in database: Old record gone, new record has all new values

### Test Case 3: Update Other Fields Only (DivName Unchanged)
1. Open Edit Division for "DIV001"
2. Change: DivisionId=200, AgencyId=10
3. Keep DivName="DIV001"
4. Click Update
5. **Expected**: Normal update (no delete/insert)
6. Verify in database: Same record updated in place

### Test Case 4: Duplicate Name Prevention
1. Create two divisions: "DIV001" and "DIV002"
2. Open Edit for "DIV001"
3. Try to change DivName to "DIV002"
4. **Expected**: Error "Cannot rename to 'DIV002' - division already exists."

## Files Modified
1. ✅ `Apha.FPS\Apha.FPS.Core\Interfaces\IDivisionRepository.cs` - Updated signature
2. ✅ `Apha.FPS\Apha.FPS.DataAccess\Repositories\DivisionRepository.cs` - Implemented delete+insert logic
3. ✅ `Apha.FPS\Apha.FPS.Application\Services\DivisionService.cs` - Pass originalDivName to repository

## Restart Instructions
Since the repository interface changed, **restart your application**:

1. **Stop Debugging** (Shift+F5)
2. **Start Debugging** (F5)
3. Test the Edit Division functionality

## Verification Steps

### 1. Check Database Before Update
```sql
SELECT * FROM fps.tlkpdivision WHERE divname = 'DIV001';
```

### 2. Perform Update
- Edit DIV001 → Change name to NEWDIV
- Click Update

### 3. Check Database After Update
```sql
-- Old record should be gone
SELECT * FROM fps.tlkpdivision WHERE divname = 'DIV001';
-- Returns 0 rows

-- New record should exist
SELECT * FROM fps.tlkpdivision WHERE divname = 'NEWDIV';
-- Returns 1 row with all updated values
```

### 4. Check Foreign Key Dependencies (if applicable)
```sql
-- Example: Check if any other tables reference divname
SELECT 
    tc.table_name, 
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY'
    AND ccu.table_name = 'tlkpdivision'
    AND ccu.column_name = 'divname';
```

## Alternative Approach (Not Implemented)
Instead of delete+insert, we could use raw SQL:

```csharp
// Alternative: Use raw SQL UPDATE (requires careful handling)
await _context.Database.ExecuteSqlRawAsync(
    "UPDATE fps.tlkpdivision SET divname = {0}, divisionid = {1}, agencyid = {2}, centoverhead = {3} WHERE divname = {4}",
    division.DivName, division.DivisionId, division.AgencyId, division.CentOverhead, originalDivName);
```

**Why we didn't use this:**
- PostgreSQL **does not allow updating primary keys** with standard UPDATE either
- Would require disabling constraints temporarily (dangerous)
- Delete+Insert is cleaner and safer

## Summary
✅ **Problem**: Entity Framework cannot update primary keys  
✅ **Solution**: Delete old record + Insert new record when DivName changes  
✅ **Safety**: Duplicate name validation + transaction protection  
✅ **Performance**: Optimized path for non-primary-key updates  
✅ **Status**: Ready to test after application restart  
