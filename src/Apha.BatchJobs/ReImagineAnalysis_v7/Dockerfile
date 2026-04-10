# Multi-stage Dockerfile for AphaBatchJobs
# Build stage uses .NET 10 SDK to compile and publish the application
# Runtime stage uses .NET 10 Runtime for optimized container size

# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file
COPY AphaBatchJobs.sln ./

# Copy all project files
COPY AphaBatchJobs.Host/AphaBatchJobs.Host.csproj AphaBatchJobs.Host/
COPY AphaBatchJobs.Application/AphaBatchJobs.Application.csproj AphaBatchJobs.Application/
COPY AphaBatchJobs.Core/AphaBatchJobs.Core.csproj AphaBatchJobs.Core/
COPY AphaBatchJobs.Infrastructure/AphaBatchJobs.Infrastructure.csproj AphaBatchJobs.Infrastructure/

# Restore NuGet packages for all projects
RUN dotnet restore AphaBatchJobs.sln

# Copy all source code
COPY AphaBatchJobs.Host/ AphaBatchJobs.Host/
COPY AphaBatchJobs.Application/ AphaBatchJobs.Application/
COPY AphaBatchJobs.Core/ AphaBatchJobs.Core/
COPY AphaBatchJobs.Infrastructure/ AphaBatchJobs.Infrastructure/

# Build and publish the Host project with optimizations
WORKDIR /src/AphaBatchJobs.Host
RUN dotnet publish -c Release -o /app/publish --no-restore \
    /p:PublishTrimmed=false \
    /p:PublishSingleFile=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime

# Create non-root user for security best practices
RUN groupadd -r appuser && useradd -r -g appuser appuser

WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose health check endpoint if applicable (commented out - uncomment if needed)
# EXPOSE 8080

# Set environment variables for production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

# Set the entry point for the container
ENTRYPOINT ["dotnet", "AphaBatchJobs.Host.dll"]


# Key improvements made:

# 1. **Security Enhancement**: Added non-root user (appuser) to run the application
#    - Follows AWS and container security best practices
#    - Reduces attack surface by not running as root

# 2. **Build Optimization**: Added explicit publish parameters
#    - PublishTrimmed and PublishSingleFile set to false for clarity
#    - Can be adjusted based on specific requirements

# 3. **Runtime Configuration**: Added essential environment variables
#    - DOTNET_SYSTEM_GLOBALIZATION_INVARIANT for proper localization support
#    - DOTNET_RUNNING_IN_CONTAINER for optimized runtime behavior
#    - DOTNET_EnableDiagnostics=0 to reduce overhead in production

# 4. **File Permissions**: Set proper ownership of application files to non-root user

# 5. **Documentation**: Added placeholder for EXPOSE directive if health checks are needed

# 6. **AWS Compatibility**: Non-root user execution aligns with AWS ECS/EKS security best practices