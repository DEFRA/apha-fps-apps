# Docker Image Build and ECR Push Guide

## Overview
This document describes the process for building the BatchJobs container image and pushing it to AWS ECR (Elastic Container Registry).

## Prerequisites
- Docker Desktop or Docker Engine installed
- AWS CLI v2 configured with credentials
- ECR repository created in AWS: `[account-id].dkr.ecr.[region].amazonaws.com/apha-batch-jobs`
- Appropriate IAM permissions for ECR operations

## Build Steps

### 1. Local Docker Build

Build the image locally for testing:

```bash
cd src/Apha.BatchJobs
docker build -t apha-batch-jobs:latest -t apha-batch-jobs:v1.0.0 .
```

### 2. Test Locally with Docker Compose

Run the complete stack locally with database:

```bash
docker-compose up --build
```

This will:
- Start a PostgreSQL database container
- Build and run the batch-jobs application
- Wait for the database to be healthy before starting the app

To view logs:
```bash
docker-compose logs -f batch-jobs
docker-compose logs -f postgres
```

To stop:
```bash
docker-compose down
```

### 3. Push to AWS ECR

#### Step 3a: Authenticate Docker with ECR

```bash
aws ecr get-login-password --region us-east-1 \
  | docker login --username AWS --password-stdin [account-id].dkr.ecr.us-east-1.amazonaws.com
```

Replace:
- `[account-id]` - Your AWS account ID
- `us-east-1` - Your AWS region

#### Step 3b: Tag the Image

```bash
docker tag apha-batch-jobs:latest [account-id].dkr.ecr.us-east-1.amazonaws.com/apha-batch-jobs:latest
docker tag apha-batch-jobs:v1.0.0 [account-id].dkr.ecr.us-east-1.amazonaws.com/apha-batch-jobs:v1.0.0
```

#### Step 3c: Push to ECR

```bash
docker push [account-id].dkr.ecr.us-east-1.amazonaws.com/apha-batch-jobs:latest
docker push [account-id].dkr.ecr.us-east-1.amazonaws.com/apha-batch-jobs:v1.0.0
```

### 4. Verify Image in ECR

```bash
aws ecr describe-images \
  --repository-name apha-batch-jobs \
  --region us-east-1
```

## Automated CI/CD Pipeline (Recommended)

For production deployments, use GitHub Actions or AWS CodePipeline:

### GitHub Actions Example

Create `.github/workflows/build-and-push.yml`:

```yaml
name: Build and Push to ECR

on:
  push:
    branches: [main, feature-batchjobs]
    paths:
      - 'src/Apha.BatchJobs/**'

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v2
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1
      
      - name: Login to ECR
        run: |
          aws ecr get-login-password --region us-east-1 \
            | docker login --username AWS --password-stdin \
            ${{ secrets.AWS_ACCOUNT_ID }}.dkr.ecr.us-east-1.amazonaws.com
      
      - name: Build image
        run: |
          cd src/Apha.BatchJobs
          docker build \
            -t ${{ secrets.AWS_ACCOUNT_ID }}.dkr.ecr.us-east-1.amazonaws.com/apha-batch-jobs:${{ github.sha }} \
            -t ${{ secrets.AWS_ACCOUNT_ID }}.dkr.ecr.us-east-1.amazonaws.com/apha-batch-jobs:latest \
            .
      
      - name: Push to ECR
        run: |
          docker push ${{ secrets.AWS_ACCOUNT_ID }}.dkr.ecr.us-east-1.amazonaws.com/apha-batch-jobs:${{ github.sha }}
          docker push ${{ secrets.AWS_ACCOUNT_ID }}.dkr.ecr.us-east-1.amazonaws.com/apha-batch-jobs:latest
```

## Image Specifications

- **Base Image**: `mcr.microsoft.com/dotnet/runtime:10.0` (production runtime)
- **Build Image**: `mcr.microsoft.com/dotnet/sdk:10.0` (multi-stage build)
- **Architecture**: linux/amd64 (default)
- **User**: `batchjobs` (non-root, UID: 10001)
- **Working Directory**: `/app`
- **Configuration**: Via environment variables or SSM Parameter Store during ECS task launch

## Environment Variables

The application supports configuration through environment variables:

```
ASPNETCORE_ENVIRONMENT                          = Production (default)
ConnectionStrings__BatchJobsConnectionString    = Host=<host>;Port=5432;Database=batch_jobs_foundation_db;Username=<user>;Password=<pass>;Timeout=30
                                                  (supplied as a single env var or via ECS Secrets / SSM Parameter Store)
ApplicationInsights__Enabled                    = true/false
ApplicationInsights__InstrumentationKey         = (injected via ECS Secrets)
```

> **Note:** The application reads a single `ConnectionStrings__BatchJobsConnectionString` key that follows
> the standard .NET `ConnectionStrings` config section convention. The legacy `DatabaseConnection__*`
> keys listed in earlier documentation were incorrect and have been removed.

## ECS Fargate Deployment

For ECS Fargate task definitions, reference the ECR image:

```json
{
  "name": "batch-jobs",
  "image": "[account-id].dkr.ecr.us-east-1.amazonaws.com/apha-batch-jobs:latest",
  "memory": 512,
  "cpu": 256,
  "environment": [
    {
      "name": "ASPNETCORE_ENVIRONMENT",
      "value": "Production"
    }
  ],
  "secrets": [
    {
      "name": "ConnectionStrings__BatchJobsConnectionString",
      "valueFrom": "arn:aws:ssm:us-east-1:[account-id]:parameter/batch-jobs/connection-string"
    }
  ],
  "logConfiguration": {
    "logDriver": "awslogs",
    "options": {
      "awslogs-group": "/ecs/batch-jobs",
      "awslogs-region": "us-east-1",
      "awslogs-stream-prefix": "ecs"
    }
  }
}
```

## Troubleshooting

### Image Build Fails

1. Verify .NET SDK 10.0 compatibility
2. Check all project files exist in the COPY layer
3. Ensure appsettings.json files are present

```bash
docker build --no-cache -t apha-batch-jobs:latest .
```

### Push to ECR Fails

1. Check AWS credentials: `aws sts get-caller-identity`
2. Verify ECR repo exists: `aws ecr describe-repositories`
3. Verify IAM permissions for `ecr:GetDownloadUrlForLayer`, `ecr:BatchGetImage`, `ecr:PutImage`

### Container Exits Immediately

1. Check logs: `docker logs container-id`
2. Verify appsettings.json was copied into final image
3. Check database connectivity from container

## Security Best Practices

1. **Non-root user**: Container runs as `batchjobs` (UID 10001), not root
2. **Image scanning**: Enable ECR image scanning for vulnerabilities
3. **Secrets management**: Use AWS Secrets Manager + ECS task secrets, not environment variables
4. **Network isolation**: Deploy ECS tasks in private VPC subnets
5. **IAM roles**: Use least-privilege task execution and task roles
6. **Logging**: All output streams to CloudWatch Logs via awslogs driver

## References

- [Docker Multi-Stage Builds](https://docs.docker.com/build/building/multi-stage/)
- [AWS ECR Best Practices](https://docs.aws.amazon.com/AmazonECR/latest/userguide/best-practices.html)
- [ECS Task Definitions](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task_definitions.html)
