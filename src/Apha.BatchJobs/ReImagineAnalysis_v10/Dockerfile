# Multi-stage Dockerfile for AphaBatchJobs
# Stage 1: Build stage using .NET 8 SDK
# Stage 2: Runtime stage using .NET 8 Runtime
# Optimized for AWS ECS Fargate deployment with minimal image size

# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files separately for better layer caching
COPY ["AphaBatchJobs.sln", "./"]
COPY ["AphaBatchJobs.Host/AphaBatchJobs.Host.csproj", "AphaBatchJobs.Host/"]
COPY ["AphaBatchJobs.Application/AphaBatchJobs.Application.csproj", "AphaBatchJobs.Application/"]
COPY ["AphaBatchJobs.Core/AphaBatchJobs.Core.csproj", "AphaBatchJobs.Core/"]
COPY ["AphaBatchJobs.Infrastructure/AphaBatchJobs.Infrastructure.csproj", "AphaBatchJobs.Infrastructure/"]

# Restore NuGet packages - this layer will be cached if project files don't change
RUN dotnet restore "AphaBatchJobs.sln"

# Copy all source code after restore for better layer caching
COPY ["AphaBatchJobs.Host/", "AphaBatchJobs.Host/"]
COPY ["AphaBatchJobs.Application/", "AphaBatchJobs.Application/"]
COPY ["AphaBatchJobs.Core/", "AphaBatchJobs.Core/"]
COPY ["AphaBatchJobs.Infrastructure/", "AphaBatchJobs.Infrastructure/"]

# Build and publish the Host project with optimizations
WORKDIR "/src/AphaBatchJobs.Host"
RUN dotnet publish "AphaBatchJobs.Host.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    --runtime linux-x64 \
    --self-contained false \
    /p:UseAppHost=false \
    /p:PublishTrimmed=false \
    /p:PublishSingleFile=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/runtime:8.0-jammy AS runtime
WORKDIR /app

# Install necessary dependencies for PostgreSQL and AWS integrations
# Includes ca-certificates for SSL/TLS connections to AWS services and PostgreSQL
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        tzdata \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security best practices in AWS ECS
RUN groupadd -r -g 1000 aphauser && useradd -r -u 1000 -g aphauser aphauser

# Copy published application from build stage
COPY --from=build --chown=aphauser:aphauser /app/publish .

# Switch to non-root user before running the application
USER aphauser

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_USE_POLLING_FILE_WATCHER=true

# Expose metadata labels for AWS ECS task definitions
LABEL maintainer="AphaBatchJobs Team" \
      description="AphaBatchJobs .NET 8 Runtime Container" \
      version="1.0"

# Health check for AWS ECS Fargate container health monitoring
# Note: For batch jobs, consider removing or adjusting based on job nature
# This is a basic runtime check - replace with application-specific health endpoint if available
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD dotnet --info > /dev/null 2>&1 || exit 1

# Set entrypoint to run the application
ENTRYPOINT ["dotnet", "AphaBatchJobs.Host.dll"]


# Key improvements made:

# 1. **Runtime Base Image**: Changed to `mcr.microsoft.com/dotnet/runtime:8.0-jammy` for better compatibility and security updates
# 2. **Build Optimization**: Added `--runtime linux-x64` for explicit runtime targeting
# 3. **Security**: Added explicit UID/GID (1000) for the non-root user for better AWS ECS compatibility
# 4. **Dependencies**: Added ca-certificates and tzdata for SSL/TLS connections to PostgreSQL and AWS services
# 5. **Environment Variables**: Added DOTNET_RUNNING_IN_CONTAINER, DOTNET_EnableDiagnostics, and DOTNET_USE_POLLING_FILE_WATCHER for container optimization
# 6. **Ownership**: Used --chown flag in COPY command for better efficiency
# 7. **Health Check**: Improved health check with better timeout and output redirection
# 8. **Labels**: Added Docker labels for better container metadata in AWS ECS
# 9. **Cleanup**: Added apt-get clean and removal of package lists to reduce image size
# 10. **Layer Optimization**: Maintained proper layer caching strategy for faster builds