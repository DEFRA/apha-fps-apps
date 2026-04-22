# Division Schema Update Summary

## Database Schema Clarification

Based on the correct database schema for `fps.tlkpdivision`:

- **DivisionId** (integer) - Regular integer field, NOT auto-generated
- **DivisionName** (citext) - **PRIMARY KEY** (case-insensitive text)
- **AgencyId** (integer) - **FOREIGN KEY** to `fps.tlkpagency(agencyid)`
- **CentOverhead** (money) - Nullable decimal field

## Changes Made

### 1. Entity Layer (`Apha.FPS.Core\Entities\Division.cs`)

**Updated:**
```csharp
/// <summary>
/// Division identifier (regular integer field, not auto-generated).
/// </summary>
[Column("divisionid")]
public int? DivisionId { get; set; }
```

**Key Points:**
- Removed "Auto-generated sequence value" comment
- Clarified it's a regular integer field

### 2. DbContext Layer (`Apha.FPS.DataAccess\Data\FpsDbContext.cs`)

**Updated Division entity configuration:**
```csharp
modelBuilder.Entity<Division>(entity =>
{
    entity.HasKey(e => e.DivName).HasName("pk__tlkpdivision__10566f31");

    entity.Property(e => e.DivisionId)
        .HasComment("Division identifier (regular integer field, not auto-generated).")
        .HasColumnName("divisionid");

    entity.Property(e => e.AgencyId)
        .HasComment("Parent agency identifier (foreign key to fps.tlkpagency).")
        .HasColumnName("agencyid");
});
```

**Key Points:**
- Updated DivisionId comment from "Auto-generated" to "not auto-generated"
- Clarified AgencyId is a foreign key
- Primary key is correctly set to DivName

### 3. Application DTOs

**Updated files:**
- `Apha.FPS.Application\Dtos\DivisionDto.cs` (API layer)
- `Apha.FPSApps.Application\Dtos\FPS\DivisionDto.cs` (Web layer)

**Changes:**
```csharp
/// <summary>
/// Division identifier (regular integer field, not auto-generated).
/// </summary>
public int? DivisionId { get; set; }

/// <summary>
/// Parent agency identifier (foreign key to fps.tlkpagency).
/// </summary>
public int AgencyId { get; set; }

/// <summary>
/// Division name (primary key - case-insensitive text).
/// </summary>
public string DivName { get; set; } = null!;
```

### 4. API Contracts

**Updated files:**
- `Apha.Common\Contracts\FPS\DivisionReq.cs`
- `Apha.Common\Contracts\FPS\DivisionRes.cs`

**Changes:**
- Updated all comments to clarify DivisionId is not auto-generated
- Marked AgencyId as foreign key to fps.tlkpagency
- Clarified DivName is the primary key

### 5. ViewModel Layer (`Apha.FPSApps.Web\Areas\FPS\Models\DivisionMaintenanceViewModel.cs`)

**Updated DivisionId:**
```csharp
/// <summary>
/// Division identifier (regular integer field, not auto-generated).
/// </summary>
[Display(Name = "Division ID")]
[GridColumn(Width = 100, Type = GridColumnType.Number, IsFilterable = true)]
public int? DivisionId { get; set; }
```

**Updated AgencyId:**
```csharp
/// <summary>
/// Parent agency identifier (foreign key to fps.tlkpagency).
/// </summary>
[Display(Name = "Agency ID")]
[GridColumn(Width = 100, Type = GridColumnType.Number, IsFilterable = true)]
[Required(ErrorMessage = "Agency is required")]
public int AgencyId { get; set; }
```

**Key Points:**
- Added clarification that AgencyId is a foreign key
- DivisionId is displayed and filterable in grid
- Both fields are visible in the grid

### 3. View Layer (`_AddEditDivision.cshtml`)

**Add Mode - DivisionId Field:**
```razor
<div class="govuk-form-group sup_margin_0">
    <label class="govuk-label" asp-for="DivisionId">Division ID</label>
    @if (isEditMode)
    {
        <input type="number" asp-for="DivisionId" class="govuk-input" readonly>
    }
    else
    {
        <input type="number" asp-for="DivisionId" class="govuk-input" required min="1">
        <span asp-validation-for="DivisionId" class="govuk-error-message" style="display:none;"></span>
    }
</div>
```

**Key Changes:**
- **Add Mode**: DivisionId is now an **editable number input field**
  - Required field
  - Minimum value: 1
  - User must manually enter the Division ID
  - Validation enabled
  
- **Edit Mode**: DivisionId is **read-only**
  - Displays the existing value
  - Cannot be changed

### 4. JavaScript Layer (`Index.cshtml`)

**Updated saveDivision() function:**
```javascript
var rawDivisionId = form.find('[name="DivisionId"]').val();
var divisionId = rawDivisionId !== '' ? parseInt(rawDivisionId) : null;

var data = {
    DivisionId: divisionId,  // Now properly parsed as integer
    DivName: form.find('[name="DivName"]').val(),
    AgencyId: parseInt(form.find('[name="AgencyId"]').val()),
    CentOverhead: centOverhead
};
```

**Updated updateDivision() function:**
```javascript
var rawDivisionId = form.find('[name="DivisionId"]').val();
var divisionId = rawDivisionId !== '' ? parseInt(rawDivisionId) : null;

var data = {
    DivisionId: divisionId,  // Now properly parsed as integer
    DivName: form.find('[name="DivName"]').val(),
    AgencyId: parseInt(form.find('[name="AgencyId"]').val()),
    CentOverhead: centOverhead
};
```

**Key Changes:**
- DivisionId is now properly parsed as an integer before sending to the server
- Both Create and Update operations handle DivisionId consistently

## Form Field Order (Grid and Form)

### Grid Columns (Left to Right):
1. **Division ID** - 100px width, filterable
2. **Agency ID** - 100px width, filterable
3. **Division Name** - 250px width, filterable (PRIMARY KEY)
4. **Central Overhead** - 150px width

### Add Form Fields (Top to Bottom):
1. **Division ID** - Number input, required, editable
2. **Agency ID** - Dropdown select, required
3. **Division Name** - Text input, required (PRIMARY KEY)
4. **Central Overhead** - Number input, optional

### Edit Form Fields (Top to Bottom):
1. **Division ID** - Number input, read-only
2. **Agency ID** - Dropdown select, editable
3. **Division Name** - Text input, read-only (PRIMARY KEY - cannot change)
4. **Central Overhead** - Number input, editable

## User Experience

### When Adding a Division:
1. User clicks **Add** button
2. Modal opens with form showing:
   - **Division ID**: Empty number input (user must enter)
   - **Agency ID**: Dropdown to select from available agencies
   - **Division Name**: Empty text input (PRIMARY KEY)
   - **Central Overhead**: Optional decimal input
3. User must provide:
   - ✅ Valid Division ID (integer ≥ 1)
   - ✅ Agency ID from dropdown
   - ✅ Unique Division Name
   - ⚪ Central Overhead (optional)
4. Click **Save** to create

### When Editing a Division:
1. User clicks **Edit** on a grid row
2. Modal opens with form showing:
   - **Division ID**: Displayed, cannot change
   - **Agency ID**: Can change via dropdown
   - **Division Name**: Displayed, cannot change (PRIMARY KEY)
   - **Central Overhead**: Can change
3. User can modify:
   - ✅ Agency ID
   - ✅ Central Overhead
4. Click **Update** to save changes

## Validation Rules

### DivisionId
- **Type**: Integer (nullable in entity, but required in Add mode)
- **Add Mode**: Required, must be ≥ 1
- **Edit Mode**: Read-only, displayed
- **Database**: Regular integer field (not auto-generated)

### AgencyId
- **Type**: Integer (required)
- **Validation**: Required (foreign key constraint)
- **Database**: Foreign key to `fps.tlkpagency(agencyid)`

### DivisionName
- **Type**: String (citext - case-insensitive)
- **Validation**: Required, max 255 characters
- **Database**: PRIMARY KEY
- **Edit Mode**: Read-only (primary keys cannot be changed)

### CentOverhead
- **Type**: Decimal (nullable)
- **Validation**: Optional, must be ≥ 0
- **Database**: Money type

## Build Status

✅ **Build Successful** - All changes compile without errors

## Next Steps

1. **Restart the application** (Hot Reload may not apply all JavaScript changes)
2. **Test Add Division**:
   - Enter Division ID manually (e.g., 10)
   - Select an Agency ID from dropdown
   - Enter a unique Division Name
   - Optionally enter Central Overhead
   - Click Save
3. **Test Edit Division**:
   - Click Edit on an existing division
   - Verify Division ID and Division Name are read-only
   - Change Agency ID or Central Overhead
   - Click Update

## Important Notes

⚠️ **Division Name is the Primary Key**
- Division names must be unique across the entire table
- Once created, division names cannot be changed (primary key constraint)
- Division names are case-insensitive (citext type in PostgreSQL)

⚠️ **DivisionId is NOT Auto-Generated**
- Users must manually enter a Division ID when creating a new division
- Application should validate that the DivisionId doesn't already exist
- Consider adding server-side validation for uniqueness

⚠️ **AgencyId Foreign Key**
- The dropdown should load actual agencies from `fps.tlkpagency`
- Currently using sample data (APHA, DEFRA, VMD)
- Should be replaced with actual API call to load agencies

## Recommended Enhancements

1. **Load Real Agencies**: Replace sample agency data with actual API call
2. **Validate DivisionId Uniqueness**: Add server-side check to prevent duplicate Division IDs
3. **Display Agency Name**: In grid and form, show both Agency ID and Agency Name
4. **Auto-suggest Division ID**: Optionally suggest the next available Division ID
5. **Bulk Import**: Consider adding ability to import divisions from file
