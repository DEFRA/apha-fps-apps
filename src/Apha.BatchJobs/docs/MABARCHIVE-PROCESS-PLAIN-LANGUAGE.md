# MAB Archive Process (Plain Language)

## What this job does
The MAB Archive job keeps reporting data up to date.
It copies data from the FPS source tables into MAB Archive tables.

## When it runs
- It runs automatically every weekday at 8:00 PM (UTC).

## How it decides the year to process
- If the month is May to December: it processes the current year.
- If the month is January to April: it processes the previous year as the main load.

## Main process steps (for the main year)
1. Rebuild totals data from FPS source tables.
2. Delete old archive data for that year.
3. Load fresh archive data for that year.

## Extra step in January to April
- It also refreshes one current-year table: `my_tlkpproject_all`.
- This is a partial refresh only. Other current-year archive tables are not fully reloaded in this period.

## Safety and reliability
- The job takes a lock first, so two runs cannot overlap.
- It runs inside one transaction.
- If any step fails, all changes are rolled back.

## If something goes wrong
- The run fails safely (rollback).
- A failure email is sent to the configured admin contact.
- The email includes key details (job name, run id, and error information).

## In one sentence
This is a scheduled clean-and-reload process that keeps MAB Archive financial reporting data accurate and consistent.
