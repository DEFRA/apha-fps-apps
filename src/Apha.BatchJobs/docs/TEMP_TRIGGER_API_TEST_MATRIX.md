# Temporary Trigger API Test Matrix

Scope: temporary trigger endpoint in Apha.BatchJobs.Api used for end-to-end FPS flow rehearsal.

| Scenario ID | Scenario | Preconditions | API Call | Expected Result | Pass Criteria | Automated Test |
|---|---|---|---|---|---|---|
| TAPI-001 | Accept trigger when job is not running | Job exists, no active lock | `POST /api/batch-jobs/HealthCheck/trigger` | `202 Accepted` with `accepted=true`, `operationId`, `jobName` | Trigger request returns immediately and job is queued for async execution | `JobStatusControllerTriggerTests.Trigger_WhenJobCanRun_ReturnsAcceptedAndOperationId` |
| TAPI-002 | Reject trigger when job is already running | Job exists, active lock present | `POST /api/batch-jobs/HealthCheck/trigger` | `409 Conflict` with `accepted=false`, lock metadata | No new execution is started while lock is active | `JobStatusControllerTriggerTests.Trigger_WhenJobAlreadyRunning_ReturnsConflictAndDoesNotStartNewRun` |
| TAPI-003 | Reject trigger for unknown job | Job name is not registered | `POST /api/batch-jobs/UnknownJob/trigger` | `404 Not Found` with error payload | Unknown job does not start execution and response is explicit | `JobStatusControllerTriggerTests.Trigger_WhenJobNotRegistered_ReturnsNotFound` |

## Notes

- These tests validate API contract behavior for temporary orchestration-in-process.
- When moved to FPS backend, keep these contract expectations unchanged where possible.
