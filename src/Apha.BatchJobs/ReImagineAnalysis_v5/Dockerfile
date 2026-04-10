# Multi-stage Dockerfile for AphaBatchJobs
# Build stage uses .NET 8.0 SDK (Note: .NET 10 is not yet released)
# Runtime stage uses .NET 8.0 runtime for optimized container size
# Designed for AWS ECS Fargate deployment

# ============================================================================
# BUILD STAGE
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory for build operations
WORKDIR /src

# Copy solution file first for better layer caching
COPY AphaBatchJobs.sln ./

# Copy all project files with their folder structure
# This allows Docker to cache the restore layer if project files haven't changed
COPY AphaBatchJobs.Core/AphaBatchJobs.Core.csproj AphaBatchJobs.Core/
COPY AphaBatchJobs.Application/AphaBatchJobs.Application.csproj AphaBatchJobs.Application/
COPY AphaBatchJobs.Infrastructure/AphaBatchJobs.Infrastructure.csproj AphaBatchJobs.Infrastructure/
COPY AphaBatchJobs.Host/AphaBatchJobs.Host.csproj AphaBatchJobs.Host/

# Restore NuGet packages for the entire solution
# This layer is cached unless project files change
RUN dotnet restore AphaBatchJobs.sln

# Copy all remaining source files
# This includes all .cs files, appsettings.json, and other source artifacts
COPY . .

# Publish the Host project in Release configuration
# Output goes to /app/publish
# --no-restore skips restore since we already did it above
# -c Release enables optimizations for production
# /p:PublishSingleFile=false for better compatibility with ECS Fargate
# /p:PublishTrimmed=false to avoid runtime issues with reflection-heavy code
RUN dotnet publish AphaBatchJobs.Host/AphaBatchJobs.Host.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:PublishSingleFile=false \
    /p:PublishTrimmed=false

# ============================================================================
# RUNTIME STAGE
# ============================================================================
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

# Set working directory for runtime
WORKDIR /app

# Install ca-certificates for secure HTTPS connections (AWS services, PostgreSQL SSL)
# Clean up apt cache to reduce image size
RUN apt-get update && \
    apt-get install -y --no-install-recommends ca-certificates && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Configure container to run as non-root user for security (ECS Fargate best practice)
# Create a non-root user and group with specific UID/GID for consistency
RUN groupadd -r -g 1000 aphauser && \
    useradd -r -u 1000 -g aphauser aphauser

# Copy published output from build stage
# This creates a minimal runtime image without SDK overhead
COPY --from=build /app/publish .

# Change ownership of application files to non-root user
RUN chown -R aphauser:aphauser /app

# Switch to non-root user
USER aphauser

# Set environment variables for production
# DOTNET_RUNNING_IN_CONTAINER optimizes .NET runtime for containerized environments
# DOTNET_EnableDiagnostics=0 disables diagnostic ports (not needed in production)
# DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false ensures proper globalization support
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Set the entry point to the published executable
# Arguments can be passed when running the container:
# docker run <image> --scheduled
# docker run <image> --adhoc SampleJob
ENTRYPOINT ["dotnet", "AphaBatchJobs.Host.dll"]

# Default command (can be overridden at runtime)
# This is empty to allow flexible CLI argument passing
CMD []

# ============================================================================
# METADATA LABELS (Best practice for container management in AWS ECS)
# ============================================================================
LABEL maintainer="Apha Platform Team" \
      description="Apha Batch Jobs - Scheduled and Adhoc PostgreSQL batch operations for AWS ECS Fargate" \
      version="1.0.0" \
      application="AphaBatchJobs" \
      environment="production"

# ============================================================================
# HEALTH CHECK (Optional but recommended for AWS ECS Fargate)
# ============================================================================
# Note: Uncomment and customize if you add a health check endpoint
# HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
#   CMD dotnet AphaBatchJobs.Host.dll --health || exit 1

# ============================================================================
# BUILD INSTRUCTIONS
# ============================================================================
# To build this image:
#   docker build -t aphabatchjobs:latest .
#
# To run scheduled jobs:
#   docker run --rm aphabatchjobs:latest --scheduled
#
# To run adhoc job:
#   docker run --rm aphabatchjobs:latest --adhoc SampleJob
#
# To run with environment variables (e.g., connection string):
#   docker run --rm \
#     -e ConnectionStrings__DefaultConnection="Host=postgres;Database=aphafps;Username=postgres;Password=secret" \
#     aphabatchjobs:latest --scheduled
#
# For AWS ECS Fargate deployment:
#   1. Build and tag: docker build -t aphabatchjobs:latest .
#   2. Tag for ECR: docker tag aphabatchjobs:latest <account-id>.dkr.ecr.<region>.amazonaws.com/aphabatchjobs:latest
#   3. Push to ECR: docker push <account-id>.dkr.ecr.<region>.amazonaws.com/aphabatchjobs:latest
#   4. Create ECS task definition with the ECR image URI
#   5. Configure task with appropriate CPU, memory, and environment variables
#   6. Use ECS Scheduled Tasks for cron-based execution or run tasks on-demand for adhoc jobs


# Key improvements made:

# 1. Added ca-certificates installation for secure HTTPS/SSL connections to AWS services and PostgreSQL
# 2. Combined apt-get commands and added cleanup to reduce image size
# 3. Specified explicit UID/GID (1000) for the non-root user for consistency across environments
# 4. Combined user and group creation in a single RUN command to reduce layers
# 5. Added DOTNET_RUNNING_IN_CONTAINER=true for optimized .NET runtime behavior in containers
# 6. Added DOTNET_EnableDiagnostics=0 to disable diagnostic ports (security best practice for production)
# 7. Added DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false for proper globalization support with PostgreSQL
# 8. Combined ENV variables in a single statement to reduce layers
# 9. Combined LABEL statements to reduce layers
# 10. Added PublishSingleFile=false and PublishTrimmed=false flags to avoid runtime issues with reflection
# 11. Moved COPY --from=build before chown to ensure proper file ownership
# 12. All changes align with AWS ECS Fargate and .NET containerization best practices