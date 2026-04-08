# SQL Procedures Business Context

## Overview
These 29 SQL stored procedures handle year-end financial operations for the APHA FPS (Field Pathology Services) system. They aggregate project costs, transfer data between year databases, and generate financial summaries.

## Procedure Categories

### 1. FPS Totals Management (2 procedures)
- **sp_createFPSTotals**: Aggregates project costs (additional, animal, staff, test) and income to create yearly totals
- **sp_deleteFPSTotals**: Clears existing totals before recalculation

### 2. Multi-Year Transfer Operations (17 procedures with sp_AddMY_ prefix)
Transfer current year data to multi-year historical database:
- FPSYearTotals, MonthlyOutput, MonthlyTime
- ProfitCentreGrade, Proj_Invoice, Proj_SubContract
- ProjectMonthFinal, Staff, TestOrProduct
- TimeCostCalcs, WorkGroup, WorkGroupGrade
- tblAdditionalCosts, tblAnimalReq, tblAnimals
- tblContract, tblProfitCentre, tblStaffJob
- tlkpProgram, tlkpProject, tlkpProject_All, tlkpTestReqmt

### 3. Full Year Data Operations (2 procedures)
- **sp_AddYearsFPSData**: Copy complete year data with all related tables
- **sp_DeleteYearsFPSData**: Remove all data for a specific year

### 4. External Data Import (1 procedure)
- **sp_LoadFromFPS**: Load project data from external FPS system

### 5. Global Lookups (2 procedures)
- **sp_AddG_tlkpProject**: Add projects to global lookup table
- **sp_addMY_YearDetails**: Add year metadata for tracking transfers

## Key Business Rules
- Year-end transfers must be transactional (all succeed or all rollback)
- NULL values in cost columns should default to 0
- TotalCosts = sum of all cost categories
- TotalIncome = CustIncome + TransferIncome
- Multi-year transfers preserve original data with transfer timestamp
- Data validation required before transfer (no orphan records)

## Current Implementation
- Microsoft Access Database with VBA macros
- Manual execution by administrators
- No logging or error tracking
- No retry on failure

## Target Requirements
- Automated scheduled execution (cron-based)
- Structured logging with CloudWatch
- Transaction management with rollback
- Cross-domain orchestration (FPS, PACT, PIMS, Costbook)
- PostgreSQL database backend
- .NET 10 Console Application

## Data Volume
- Projects: ~500-1000 per year
- Multi-year history: 10+ years
- Year-end transfer: runs once per fiscal year
- Summary generation: on-demand, 10-50 times per year
