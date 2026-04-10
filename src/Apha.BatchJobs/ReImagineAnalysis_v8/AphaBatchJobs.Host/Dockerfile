# Multi-stage Dockerfile for AphaBatchJobs.Host
# Build stage uses .NET 8 SDK, runtime stage uses .NET 8 runtime
# Optimized for AWS ECS Fargate deployment with PostgreSQL batch operations

# ============================================================================
# BUILD STAGE
# ============================================================================
# Use official .NET 8 SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory for build operations
WORKDIR /src

# Copy solution file first for better layer caching
COPY AphaBatchJobs.sln ./

# Copy all project files maintaining folder structure for proper restore
# This approach ensures NuGet restore can resolve all project references correctly
COPY AphaBatchJobs.Host/AphaBatchJobs.Host.csproj AphaBatchJobs.Host/
COPY AphaBatchJobs.Application/AphaBatchJobs.Application.csproj AphaBatchJobs.Application/
COPY AphaBatchJobs.Core/AphaBatchJobs.Core.csproj AphaBatchJobs.Core/
COPY AphaBatchJobs.Infrastructure/AphaBatchJobs.Infrastructure.csproj AphaBatchJobs.Infrastructure/

# Restore NuGet packages for the entire solution
# This step is cached unless project files change, improving build performance
RUN dotnet restore AphaBatchJobs.sln

# Copy all source code files for all projects
# Copying after restore ensures better Docker layer caching
COPY AphaBatchJobs.Host/ AphaBatchJobs.Host/
COPY AphaBatchJobs.Application/ AphaBatchJobs.Application/
COPY AphaBatchJobs.Core/ AphaBatchJobs.Core/
COPY AphaBatchJobs.Infrastructure/ AphaBatchJobs.Infrastructure/

# Build and publish the Host project in Release configuration
# --no-restore skips restore since we already restored in previous step
# -p:PublishSingleFile=false ensures compatibility with AWS ECS Fargate
# -p:PublishTrimmed=false prevents trimming issues with reflection-heavy code
# Output goes to /app/publish for copying to runtime stage
RUN dotnet publish AphaBatchJobs.Host/AphaBatchJobs.Host.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:PublishSingleFile=false \
    -p:PublishTrimmed=false

# ============================================================================
# RUNTIME STAGE
# ============================================================================
# Use official .NET 8 runtime image for smaller final image size
# Runtime image does not include SDK tools, reducing attack surface and image size
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

# Create non-root user for security best practices in AWS ECS
# Running as non-root reduces security risks and follows AWS security guidelines
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Set working directory for runtime operations
WORKDIR /app

# Copy published application from build stage
# This includes all compiled assemblies, dependencies, and configuration files
COPY --from=build /app/publish .

# Change ownership of application files to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user for running the application
USER appuser

# Set entrypoint to run the application
# Uses exec form for proper signal handling in container environments
# This allows graceful shutdown when AWS ECS Fargate sends SIGTERM
ENTRYPOINT ["dotnet", "AphaBatchJobs.Host.dll"]

# ============================================================================
# DOCKER BUILD AND RUN INSTRUCTIONS
# ============================================================================
# Build the Docker image:
#   docker build -t aphabatchjobs:latest .
#
# Run scheduled jobs:
#   docker run --rm aphabatchjobs:latest --scheduled
#
# Run adhoc job:
#   docker run --rm aphabatchjobs:latest --adhoc SampleJob
#
# Run with environment-specific configuration:
#   docker run --rm -e ASPNETCORE_ENVIRONMENT=Production aphabatchjobs:latest --scheduled
#
# Run with custom connection string (AWS ECS Fargate):
#   docker run --rm \
#     -e ConnectionStrings__DefaultConnection="Host=your-rds-endpoint;Database=aphafps;Username=user;Password=pass" \
#     aphabatchjobs:latest --scheduled
#
# AWS ECS Task Definition considerations:
#   - Ensure proper IAM roles for RDS access
#   - Use AWS Secrets Manager for connection strings
#   - Configure appropriate CPU and memory limits
#   - Set stopTimeout to allow graceful shutdown (recommended: 30-120 seconds)
# ============================================================================


**Key improvements made:**

1. **Security Enhancement**: Added non-root user (`appuser`) to run the application, following AWS security best practices and reducing attack surface in ECS Fargate environments.

2. **Build Optimization**: Added explicit publish flags (`-p:PublishSingleFile=false` and `-p:PublishTrimmed=false`) to prevent potential issues with reflection-heavy code and ensure compatibility with AWS ECS Fargate.

3. **File Permissions**: Added proper ownership change (`chown`) for application files to ensure the non-root user has appropriate access.

4. **Documentation Enhancement**: Added AWS ECS-specific considerations in the comments to guide proper deployment configuration.

These changes align with .NET 8, PostgreSQL, and AWS best practices without adding new functionality, focusing solely on security, reliability, and deployment optimization.