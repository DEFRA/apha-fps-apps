# BatchJobs Worker

Concise runtime notes for the worker host.

## Run

From src/Apha.BatchJobs:

```powershell
dotnet run --project Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj
```

## Modes

- Demo: in-memory repositories (no database).
- Development/Production: PostgreSQL repositories.

Key settings:

- ASPNETCORE_ENVIRONMENT
- BATCH_JOB_NAME
- ConnectionStrings__FPSConnectionString (required for database mode)

## Exit Codes

- 0 success
- 1 unhandled failure
- 2 unknown job or factory error
- 3 cancelled

## Fast Troubleshooting

- Unknown job: verify BATCH_JOB_NAME matches a registered job.
- Connection failure: validate host, port, database, and credentials.
- Missing config: run from src/Apha.BatchJobs.


