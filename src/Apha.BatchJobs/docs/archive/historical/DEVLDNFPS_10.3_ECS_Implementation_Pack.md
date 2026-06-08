# DEVLDNFPS LLD 10.3 Implementation Pack

## Intent
This document is an implementation companion to your LLD section 10.3.
It does not replace your LLD. It translates it into deployable and auditable changes.

## Scope Kept From LLD
- Region: eu-west-2
- In-scope task families:
  - DEVLDNFPS_FPS (FPS API)
  - DEVLDNFPS_BATCHJOBS (Batch Jobs, confirm exact family name in LLD)
- Container port: 8080
- Environment files in S3 bucket devldnfps-env
- Secrets from AWS Secrets Manager:
  - ConnectionStrings__FPSConnectionString
  - AzureAd__ClientId
  - AzureAd__ClientSecret
- Log groups:
  - devldnfps-app-fps
  - devldnfps-app-batchjobs
- Task resources: 1 vCPU, 2 GB memory

## Normalization Rules Applied
These are implementation-safe normalizations, not design changes.

1. Use awslogs for log driver value.
2. Use Fargate units: cpu=1024 and memory=2048.
3. Use awsvpc network mode.
4. Use requiresCompatibilities=["FARGATE"].
5. Keep containerPort=8080 and hostPort=8080 for consistency.
6. Remove spacing errors in S3 ARNs.
7. Use Task Definition Name consistently for the two in-scope services.

## Priority Backlog

### P1: Must complete before first DEV deployment
1. Create and validate both in-scope ECS task definitions using the task catalog in this pack.
2. Create role policies for:
   - devldnfps-ecs-execution-role
   - devldnfps-ecs-task-role
3. Confirm Secrets Manager secret names and key mapping for each secret.
4. Create CloudWatch log groups and retention policy.
5. Create ECS task security group with approved inbound/outbound rules.
6. Validate runtime pull path:
   ECR -> ECS execution role -> task start -> logs -> secret retrieval -> app boot.

Acceptance criteria:
- Both in-scope task definitions register successfully.
- ECS task can pull image and start without AccessDenied.
- Container can read env file and secrets.
- Logs are written to the expected log group and stream prefix.

### P2: Must complete before SIT/UAT hardening
1. Add KMS permissions if S3 env files or secrets use CMK keys.
2. Restrict S3 resource scope to exact bucket and key prefix.
3. Restrict Secrets Manager resource scope to exact secret ARNs.
4. Restrict ECR actions to account and repository list used by FPS API and Batch Jobs.
5. Add ALB to ECS SG source-only ingress model if not already implemented.

Acceptance criteria:
- IAM policy simulator shows least privilege with expected allows only.
- Security review confirms no broad wildcards beyond unavoidable actions.

### P3: Recommended for production readiness
1. Add alarms for task launch failures, high error logs, and unhealthy target counts.
2. Add deployment rollback criteria and runbook links.
3. Enforce immutable image tagging and optional digest pinning.
4. Add default tagging policy enforcement in IaC.

Acceptance criteria:
- Alarm set is active and tested.
- Rollback runbook tested by game-day dry run.

## Deployment Sequence
1. Create log groups.
2. Create/attach IAM policies and roles.
3. Confirm S3 env objects and secret values exist.
4. Register task definitions.
5. Deploy FPS API service and Batch Jobs runtime.
6. Run smoke tests per service on /health endpoint.

## Verification Checklist
1. aws ecs describe-task-definition returns cpu=1024 and memory=2048.
2. aws ecs run-task succeeds for each family with no iam or image pull errors.
3. CloudWatch logs show app startup and no missing secret key errors.
4. Security group allows only expected paths:
   - Inbound 8080 from ALB SG
   - Outbound 5432 to RDS SG
   - Outbound 6379 to Redis SG
   - Outbound 443 via approved egress path

## Source Artifacts In This Repo
- Task catalog: docs/devldnfps/devldnfps-ecs-task-catalog.json
- FPS API task definition payload: docs/devldnfps/ecs-task-definition-fps-api.json
- Batch Jobs task definition payload: docs/devldnfps/ecs-task-definition-batchjobs.json
- Execution role policy: docs/devldnfps/devldnfps-ecs-execution-role-policy.json
- Task role policy: docs/devldnfps/devldnfps-ecs-task-role-policy.json
- Security group rules: docs/devldnfps/devldnfps-ecs-task-security-group-rules.md

## Out Of Scope For This Pack
- FPS Apps
- PACT API
- Costbook API
- PIMS API
