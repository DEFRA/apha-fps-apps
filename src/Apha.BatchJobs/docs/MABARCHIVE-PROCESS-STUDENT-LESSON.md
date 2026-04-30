# MAB Archive Process as a Story Lesson

## Imagine this story
Riya is a student learning how finance data moves in a company system.
Her teacher says:
"Think of MAB Archive as a school librarian. Every evening, the librarian checks, cleans, and updates books so tomorrow's reports are correct."

In this story:
- FPS tables = the "source books" (raw data)
- MAB Archive tables = the "library shelves" used for reporting
- MAB Archive job = the librarian doing nightly maintenance

## When the librarian works
The librarian works every weekday at 8:00 PM (UTC).
So each evening, one run happens automatically.

## The year rule (very important)
Teacher explains one smart rule:
- If month is May to December: focus on current year.
- If month is January to April: focus on previous year, plus a small current-year refresh.

Why?
Because early in the year, last year's financial data is still being finalized.

## Story Example 1: March run (month = 3)
Date: 15 March 2026
Current year = 2026
Previous year = 2025

Because March is in January-April:
- Main full load year = 2025
- Extra partial refresh year = 2026 (only one table)

### Step A: Rebuild totals for 2025
Suppose project P100 has these source cost pieces:
- Additional cost = 12000
- Animal cost = 8000
- Staff cost = 30000
- Test cost = 5000
- Plan casework debit = 2000

Total cost becomes:
12000 + 8000 + 30000 + 5000 + 2000 = 57000

This total is rebuilt in the FPS totals source for year 2025.

### Step B: Delete old 2025 archive rows
The job removes old 2025 rows from archive tables so stale data does not stay.

Example:
- Before delete, my_monthlyoutput for 2025 has 5000 rows.
- After delete, 2025 rows become 0.

### Step C: Load fresh 2025 archive rows
Now fresh rows are copied from FPS source into MAB Archive.

Example:
- my_monthlyoutput loads back to 5100 rows (new corrected data)
- my_tblstaffjob loads 2200 rows
- other archive tables are loaded in defined order

### Step D: Partial refresh for 2026 (January-April behavior)
Only one table is refreshed for current year:
- my_tlkpproject_all

Example:
- 2026 rows in my_tlkpproject_all change from 900 to 940
- other 2026 archive tables are not fully reloaded in this period

## Story Example 2: July run (month = 7)
Date: 20 July 2026
Because July is after April:
- Main full load year = 2026
- No partial refresh step

The job does only the full cycle for 2026:
1. Rebuild totals
2. Delete old 2026 archive data
3. Load fresh 2026 archive data

## Safety lesson (how the system avoids mistakes)
Teacher gives three safety rules:

1. Lock first
The job takes a lock so only one run can happen at a time.
No overlap means fewer data conflicts.

2. One transaction
All steps run in one transaction.
If one step fails, everything is rolled back.

3. Failure alert
If the run fails, an email alert is sent with run details for troubleshooting.

## Mini classroom summary
Student answer:
"MAB Archive is a scheduled clean-and-reload process. It chooses year by month, reloads full data for the main year, does a small current-year refresh in Jan-Apr, and protects data with lock + transaction + failure alerts."

That is the core idea.
