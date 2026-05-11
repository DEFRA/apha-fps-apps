# Database Scripts - FPS & MABArchive Seeding

## Overview

This folder contains SQL and PowerShell scripts for seeding local PostgreSQL database with test data for **RecreateSummaries** (fps schema) and **MABArchive** (mabarchive schema) batch jobs.

## Scripts

### Execution Order

Scripts should be executed in this order:

#### 1. **00-flush-test-data.sql** (Optional)
Clean slate: deletes all test data from both schemas.

**When to use:**
- Fresh database reset
- Removing old test run artifacts
- Ensuring clean state before reseed

**What it deletes:**
- fps schema: invoices, subcontracts, outputs, milestones, monthly baselines, projects
- mabarchive schema: project snapshots, year totals

**Scope:** Only deletes rows for projects `AH0001`, `TH0002`, `BS0003`, `RS0004` (test projects)

```bash
psql -h localhost -U postgres -d batch_jobs_foundation_db -f 00-flush-test-data.sql
```

#### 2. **seed-combined-fps-mabarchive.sql**
Single combined script seeding both schemas with multi-year test data.

**Data Seeded:**

**FPS Schema (RecreateSummaries):**
- 12 parent projects: 4 programmes (AH, TH, BS, RS) × 3 years (2024-2026)
- 48 monthly baselines (projectmonth)
- 25+ milestones with mixed status (on-time/late/pending)
- 9 test capabilities mapping codes to workgroups
- 9 test requirements with unit prices
- 15 monthly outputs (test volumes)
- 9 product codes
- 11 subcontracts by project/month
- 13 invoices by project/month

**MABArchive Schema (Scheduled Load):**
- 12 year-scoped financial totals (my_fpsyeartotals)
- 12 project snapshots (my_tlkpproject_all)
- Complete cost & income data across 3 years

**Idempotent:** Uses `ON CONFLICT` clauses—safe to re-run without data duplication.

```bash
psql -h localhost -U postgres -d batch_jobs_foundation_db -f seed-combined-fps-mabarchive.sql
```

### Quick Reseed with PowerShell

#### **reseed-local-db.ps1** (Recommended)
Orchestration script: flushes old data, then reseeds in single command.

**Usage:**
```powershell
# From repository root:
.\src\Apha.BatchJobs\docs\database\sql\reseed-local-db.ps1

# Or with custom connection params:
.\src\Apha.BatchJobs\docs\database\sql\reseed-local-db.ps1 `
  -DbHost localhost `
  -DbPort 5432 `
  -DbUser postgres `
  -DbName batch_jobs_foundation_db
```

**What it does:**
1. Validates PostgreSQL is installed
2. Runs flush script (00-flush-test-data.sql)
3. Runs seed script (seed-combined-fps-mabarchive.sql)
4. Shows summary of data loaded

**Output:**
```
✓ Flush complete
✓ Reseed complete

Data loaded:
  • 12 parent projects (4 programmes × 3 years)
  • 48+ milestones (on-time, late, pending)
  • Monthly outputs, invoices, subcontracts
  • MABArchive baseline totals & snapshots
```

## Seed Data Design

### Test Projects

| Code  | Programme | Customer | Manager      | Years      |
|-------|-----------|----------|--------------|------------|
| AH0001| Aquatic   | DEFRA    | John Smith   | 2024-2026  |
| TH0002| Terrestrial| DEFRA   | Jane Brown   | 2024-2026  |
| BS0003| Biosecurity| APHA    | Mike Johnson | 2024-2026  |
| RS0004| Research  | Academia | Sarah Davis  | 2024-2026  |

### Milestone Status Distribution

- **On-Time**: Actual date ≤ plan date
- **Late**: Actual date > plan date
- **Pending**: Actual date is NULL

Example coverage:
- AH0001 2024: On-time (Q1, Q4), Late (Q2), Pending (Q3)
- Each project has quarterly milestones

### Monthly Variation

Test volumes and financials vary by month to simulate realistic patterns:
- Monthly outputs: 30-80 units depending on project/test
- Invoices: $6.5K–$14K per month
- Subcontracts: $1K–$3.5K per month

## Verification Queries

All scripts include commented verification SQL at the end. Uncomment to validate:

```sql
-- Count records by table
SELECT 'fps.tlkpproject', COUNT(*) FROM fps.tlkpproject WHERE fpsyear IN (2024,2025,2026)
UNION ALL
SELECT 'fps.milestone', COUNT(*) FROM fps.milestone WHERE fpsyear IN (2024,2025,2026);

-- Check RecreateSummaries view
SELECT project, duemonth, COUNT(*) as milestone_count 
FROM fps.qrymilestone1 
WHERE fpsyear IN (2024,2025,2026)
GROUP BY project, duemonth;

-- Verify MABArchive baseline totals
SELECT year, parentproject, totalcosts, totalincome, requiredprofit 
FROM mabarchive.my_fpsyeartotals 
WHERE year IN (2024,2025,2026);
```

## Making Scripts Reusable

### Key Design Principles

1. **Idempotent Seed Data**
   - All INSERT statements use `ON CONFLICT ... DO NOTHING` or `ON CONFLICT ... DO UPDATE`
   - Safe to run multiple times—no duplicate key errors
   - Ideal for CI/CD pipelines

2. **Scoped Deletions**
   - Flush script only deletes test project data (AH0001, TH0002, BS0003, RS0004)
   - Won't affect production/other data in same database
   - Can safely run on shared databases for development

3. **Parameterized Connection**
   - PowerShell script accepts `-DbHost`, `-DbPort`, `-DbUser`, `-DbName` parameters
   - Defaults to `localhost` but works with remote servers
   - Easy to integrate with CI/CD (GitHub Actions, Jenkins, etc.)

4. **Clear Ordering**
   - Flush script deletes in correct FK dependency order (children→parents)
   - Seed script inserts in correct order (parents→children)
   - Both scripts are self-contained with BEGIN/COMMIT

### Integration Examples

**GitHub Actions:**
```yaml
- name: Reseed local database
  run: |
    ${{ github.workspace }}\src\Apha.BatchJobs\docs\database\sql\reseed-local-db.ps1 \
      -DbHost localhost \
      -DbPort 5432 \
      -DbUser postgres
```

**Docker Compose (Entrypoint):**
```dockerfile
ENTRYPOINT ["pwsh", "-Command", "./reseed-local-db.ps1"]
```

**Azure DevOps Pipeline:**
```yaml
- task: PowerShell@2
  inputs:
    targetType: 'filePath'
    filePath: '$(Build.SourcesDirectory)/src/Apha.BatchJobs/docs/database/sql/reseed-local-db.ps1'
    arguments: '-DbHost $(DbHost) -DbPort $(DbPort) -DbUser $(DbUser) -DbName $(DbName)'
```

## Troubleshooting

### "psql not found"
- Ensure PostgreSQL 16 is installed at `C:\Program Files\PostgreSQL\16\bin\psql.exe`
- Adjust path in script if installed elsewhere

### "password authentication failed"
- Script uses `.pgpass` or OS authentication
- To prompt for password, edit script to remove `-U postgres` and add interactive prompt

### "table xyz does not exist"
- Schema may not be created yet
- Run schema initialization scripts from `dbscript/schemas/` first

### "FK violation during flush"
- Flush script deletes in wrong order
- Verify foreign key definitions match deletion order in script

## Related Documentation

- [ASK-FROM-DBA.md](../ASK-FROM-DBA.md) - Cloud readiness validation checklist
- [database/README.md](../database/README.md) - Database structure overview
- [dbscript/](../dbscript/) - Schema definitions and table structures
