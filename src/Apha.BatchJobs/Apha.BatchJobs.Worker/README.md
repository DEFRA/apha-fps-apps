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
- BatchJobs:StrictExecutionContractMode (appsettings only; no environment-variable override)

## Trigger Event Contract

Worker-trigger requests are published to EventBridge using PutEvents with the following payload shape:

```json
{
	"source": "pact.api",
	"detail-type": "BatchJobTriggerRequested",
	"detail": {
		"jobExecutionId": "7f9d2f2e-8d1b-4f7a-9d25-6d6e8a9f3c12",
		"jobName": "RecreateSummaries",
		"runMode": "Manual",
		"requestedBy": "user.name@defra.gov.uk",
		"requestedAtUtc": "2026-06-09T13:41:27Z",
		"parametersJson": "{\"month\":\"2026-06\"}"
	}
}
```

## Exit Codes

- 0 success
- 1 unhandled failure
- 2 unknown job or factory error
- 3 cancelled

## Fast Troubleshooting

- Unknown job: verify BATCH_JOB_NAME matches a registered job.
- Connection failure: validate host, port, database, and credentials.
- Missing config: run from src/Apha.BatchJobs.
