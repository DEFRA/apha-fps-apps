# Division Grid Filtering and Sorting Implementation

## Overview

Implemented filtering and sorting capabilities for the Division Maintenance grid to allow users to filter by Division Name and sort by Division ID, Division Name, and Agency ID.

## Changes Made

### 1. Backend - Repository Layer

**File:** `Apha.FPS\Apha.FPS.DataAccess\Repositories\DivisionRepository.cs`

#### Updated `GetAllDivisionsPagedAsync` Method

**Before:**
```csharp
public async Task<PagedData<Division>> GetAllDivisionsPagedAsync(PaginationParameters<string> query)
{
    ArgumentNullException.ThrowIfNull(query);

    var divisionsQuery = _context.Divisions
        .AsNoTracking()
        .OrderBy(d => d.DivName);

    var totalCount = await divisionsQuery.CountAsync();

    var divisions = await divisionsQuery
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();
    // ...
}
```

**After:**
```csharp
public async Task<PagedData<Division>> GetAllDivisionsPagedAsync(PaginationParameters<string> query)
{
    ArgumentNullException.ThrowIfNull(query);

    var divisionsQuery = _context.Divisions
        .AsNoTracking()
        .AsQueryable();

    // Apply filtering
    divisionsQuery = ApplyDivisionFilter(divisionsQuery, query.Filter);

    // Get total count before paging
    var totalCount = await divisionsQuery.CountAsync();

    // Apply sorting
    divisionsQuery = ApplySorting(divisionsQuery, query.SortBy, query.Descending);

    // Apply paging
    var divisions = await divisionsQuery
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();
    // ...
}
```

#### Added Helper Methods

**1. Filter Method:**
```csharp
private static IQueryable<Division> ApplyDivisionFilter(IQueryable<Division> query, string? filter)
{
    if (string.IsNullOrWhiteSpace(filter))
    {
        return query;
    }

    var filterDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(filter);
    if (filterDict == null || filterDict.Count == 0)
    {
        return query;
    }

    if (filterDict.TryGetValue("DivisionId", out var divisionId) && !string.IsNullOrWhiteSpace(divisionId))
    {
        if (int.TryParse(divisionId, out var divisionIdValue))
        {
            query = query.Where(d => d.DivisionId == divisionIdValue);
        }
    }

    if (filterDict.TryGetValue("AgencyId", out var agencyId) && !string.IsNullOrWhiteSpace(agencyId))
    {
        if (int.TryParse(agencyId, out var agencyIdValue))
        {
            query = query.Where(d => d.AgencyId == agencyIdValue);
        }
    }

    if (filterDict.TryGetValue("DivName", out var divName) && !string.IsNullOrWhiteSpace(divName))
    {
        query = query.Where(d => d.DivName.Contains(divName));
    }

    return query;
}
```

**2. Sorting Methods:**
```csharp
private static IQueryable<Division> ApplySorting(IQueryable<Division> query, string? sortBy, bool descending)
{
    if (string.IsNullOrEmpty(sortBy))
    {
        return query.OrderBy(d => d.DivName);
    }

    return ApplySortingByProperty(query, sortBy.ToLower(), descending);
}

private static IQueryable<Division> ApplySortingByProperty(IQueryable<Division> query, string property, bool descending)
{
    return property switch
    {
        "divisionid" => descending ? query.OrderByDescending(d => d.DivisionId) : query.OrderBy(d => d.DivisionId),
        "agencyid" => descending ? query.OrderByDescending(d => d.AgencyId) : query.OrderBy(d => d.AgencyId),
        "divname" => descending ? query.OrderByDescending(d => d.DivName) : query.OrderBy(d => d.DivName),
        "centoverhead" => descending ? query.OrderByDescending(d => d.CentOverhead) : query.OrderBy(d => d.CentOverhead),
        _ => query.OrderBy(d => d.DivName)
    };
}
```

### 2. Frontend - ViewModel Configuration

**File:** `Apha.FPSApps\Apha.FPSApps.Web\Areas\FPS\Models\DivisionMaintenanceViewModel.cs`

The `DivisionViewModel` already has the correct `GridColumn` attributes configured:

```csharp
public class DivisionViewModel
{
    [Display(Name = "Division ID")]
    [GridColumn(Width = 100, Type = GridColumnType.Number, IsFilterable = true)]
    public int? DivisionId { get; set; }

    [Display(Name = "Agency ID")]
    [GridColumn(Width = 100, Type = GridColumnType.Number, IsFilterable = true)]
    public int AgencyId { get; set; }

    [Display(Name = "Division Name")]
    [GridColumn(Width = 250, Type = GridColumnType.Text, IsFilterable = true)]
    public string DivName { get; set; } = null!;

    [Display(Name = "Central Overhead")]
    [GridColumn(Width = 150, Type = GridColumnType.Number)]
    public decimal? CentOverhead { get; set; }
}
```

**Key Settings:**
- ✅ `IsFilterable = true` on DivisionId, AgencyId, and DivName
- ✅ Proper `GridColumnType` set for each column (Number/Text)
- ✅ Column widths configured

### 3. Frontend - DataGrid UI

**File:** `Apha.FPSApps\Apha.FPSApps.Web\Views\Shared\_DataGrid.cshtml`

The DataGrid partial view already has built-in support for:

#### Sorting Headers:
```razor
<th scope="col" style="width: @column.Width px; cursor:pointer;"
    class="sortable-header"
    data-column="@column.PropertyName"
    data-sortdir="@(!Model.Pagination.SortDirection)">
    @column.DisplayName <span class="sort-icon">@sortIcon</span>
    <div class="column-resizer"></div>
</th>
```

#### Filter Inputs:
```razor
@if (column.IsFilterable)
{
    <input type="text" class="grid-filter govuk-input govuk-!-font-size-16" 
           data-filter="@column.PropertyName" 
           value="@filterValue" 
           placeholder="Filter..." />
}
```

## Features Implemented

### Filtering

Users can filter divisions by:

1. **Division ID** - Exact match filter (numeric)
   - Type a number to filter by Division ID
   - Example: `1` shows only divisions with ID = 1

2. **Agency ID** - Exact match filter (numeric)
   - Type a number to filter by Agency ID
   - Example: `5` shows only divisions with Agency ID = 5

3. **Division Name** - Contains filter (text)
   - Type text to filter by Division Name
   - Example: `VSD` shows divisions containing "VSD"
   - Case-sensitive partial match

### Sorting

Users can sort divisions by clicking column headers:

1. **Division ID** - Ascending/Descending
2. **Agency ID** - Ascending/Descending
3. **Division Name** - Ascending/Descending
4. **Central Overhead** - Ascending/Descending (bonus)

**Default Sort:** Division Name (Ascending)

**Sort Indicators:**
- ▲ - Ascending order
- ▼ - Descending order

## User Experience

### Filter Usage

1. User types in filter input box below column header
2. Filter is applied automatically (on change/blur)
3. Grid refreshes with filtered results via AJAX
4. Pagination is recalculated based on filtered results

### Sort Usage

1. User clicks on column header
2. Sort icon appears (▲ or ▼)
3. Grid refreshes with sorted results via AJAX
4. Second click reverses sort direction

### Combined Filtering and Sorting

Users can:
- Apply multiple filters simultaneously
- Sort filtered results
- Filters persist when changing sort order
- Filters and sort persist across pagination

## Technical Flow

### Filter Flow
```
User types in filter → JavaScript captures change
    ↓
AJAX POST to LoadDivisionGrid with filter JSON
    ↓
Controller deserializes filter
    ↓
Mapper converts to QueryParameters
    ↓
Service calls API with QueryParameters
    ↓
Repository applies filter via ApplyDivisionFilter
    ↓
Filtered results returned
    ↓
Grid refreshes with filtered data
```

### Sort Flow
```
User clicks column header → JavaScript captures click
    ↓
AJAX POST to LoadDivisionGrid with sortBy & descending
    ↓
Controller passes sort parameters
    ↓
Mapper converts to QueryParameters
    ↓
Service calls API with QueryParameters
    ↓
Repository applies sort via ApplySorting
    ↓
Sorted results returned
    ↓
Grid refreshes with sorted data
```

## Filter Format

Filters are sent as JSON string:
```json
{
  "DivisionId": "1",
  "AgencyId": "5",
  "DivName": "VSD"
}
```

## Sort Format

Sort parameters:
- `SortBy`: Column property name (e.g., "DivisionId", "AgencyId", "DivName")
- `Descending`: Boolean (true/false)

## Example Queries

### Filter by Division Name containing "VSD"
```
Request:
{
  "Filter": "{\"DivName\":\"VSD\"}",
  "Page": 1,
  "PageSize": 10
}

SQL Generated:
SELECT * FROM fps.division 
WHERE DivName LIKE '%VSD%'
ORDER BY DivName
LIMIT 10 OFFSET 0
```

### Sort by Agency ID Descending
```
Request:
{
  "SortBy": "AgencyId",
  "Descending": true,
  "Page": 1,
  "PageSize": 10
}

SQL Generated:
SELECT * FROM fps.division 
ORDER BY AgencyId DESC
LIMIT 10 OFFSET 0
```

### Combined: Filter by Agency ID and Sort by Division Name
```
Request:
{
  "Filter": "{\"AgencyId\":\"5\"}",
  "SortBy": "DivName",
  "Descending": false,
  "Page": 1,
  "PageSize": 10
}

SQL Generated:
SELECT * FROM fps.division 
WHERE AgencyId = 5
ORDER BY DivName ASC
LIMIT 10 OFFSET 0
```

## Testing

### Test Scenarios

#### Scenario 1: Filter by Division Name
**Steps:**
1. Navigate to Division Maintenance
2. Type "VSD" in Division Name filter
3. **Expected:** Grid shows only divisions with "VSD" in name

#### Scenario 2: Filter by Division ID
**Steps:**
1. Navigate to Division Maintenance
2. Type "1" in Division ID filter
3. **Expected:** Grid shows only divisions with ID = 1

#### Scenario 3: Sort by Agency ID
**Steps:**
1. Navigate to Division Maintenance
2. Click "Agency ID" column header
3. **Expected:** Grid sorts by Agency ID ascending, shows ▲ icon
4. Click again
5. **Expected:** Grid sorts by Agency ID descending, shows ▼ icon

#### Scenario 4: Combined Filter and Sort
**Steps:**
1. Navigate to Division Maintenance
2. Type "5" in Agency ID filter
3. Click "Division Name" column header
4. **Expected:** Grid shows only Agency ID = 5, sorted by Division Name

#### Scenario 5: Clear Filters
**Steps:**
1. Apply filters
2. Clear filter inputs
3. **Expected:** Grid shows all divisions

## Benefits

1. ✅ **Improved Usability** - Users can quickly find specific divisions
2. ✅ **Better Performance** - Filtering done at database level
3. ✅ **Consistent UX** - Matches other grids in the application
4. ✅ **Flexible** - Multiple filters can be combined
5. ✅ **Persistent** - Filters and sort persist across pagination

## Build Status

✅ **Build Successful** - All changes compile without errors

## Notes

- The DataGrid component (`_DataGrid.cshtml`) handles all client-side filter and sort interactions
- Filter values are sent as JSON string to maintain flexibility
- Sort is case-insensitive (property names converted to lowercase)
- Default sort is by Division Name ascending when no sort specified
- Total record count updates based on filtered results
- Pagination works correctly with filtered and sorted data
