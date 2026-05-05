# Multi-stage Dockerfile for AphaBatchJobs Host application
# Stage 1: Build stage using .NET 8.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set build argument for configuration (allows flexibility)
ARG BUILD_CONFIGURATION=Release

# Set working directory for build
WORKDIR /src

# Copy all project files for dependency restoration
# This is done first to leverage Docker layer caching
COPY ["AphaBatchJobsFoundationV3.Host/AphaBatchJobsFoundationV3.Host.csproj", "AphaBatchJobsFoundationV3.Host/"]
COPY ["AphaBatchJobsFoundationV3.Core/AphaBatchJobsFoundationV3.Core.csproj", "AphaBatchJobsFoundationV3.Core/"]
COPY ["AphaBatchJobsFoundationV3.Infrastructure/AphaBatchJobsFoundationV3.Infrastructure.csproj", "AphaBatchJobsFoundationV3.Infrastructure/"]

# Restore NuGet packages for all projects
# This step is cached unless csproj files change
RUN dotnet restore "AphaBatchJobsFoundationV3.Host/AphaBatchJobsFoundationV3.Host.csproj"

# Copy all source files
COPY . .

# Set working directory to Host project
WORKDIR "/src/AphaBatchJobsFoundationV3.Host"

# Publish the Host project directly (build step is redundant)
# --no-restore skips restore since we already restored above
# This creates an optimized deployment package
RUN dotnet publish "AphaBatchJobsFoundationV3.Host.csproj" \
    -c ${BUILD_CONFIGURATION} \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Stage 2: Runtime stage using .NET 8.0 ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Create non-root user for security best practices
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Set working directory for runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Set environment variable for production
ENV ASPNETCORE_ENVIRONMENT=Production

# Set environment variable for .NET runtime
ENV DOTNET_ENVIRONMENT=Production

# Expose port 8080 for the application (AWS standard)
EXPOSE 8080

# Set the entry point to run the Host application
ENTRYPOINT ["dotnet", "AphaBatchJobsFoundationV3.Host.dll"]


# Key improvements made:
# 1. Removed redundant 'dotnet build' step - 'dotnet publish' already builds the project
# 2. Added --no-restore flag to publish command to avoid duplicate restore
# 3. Added BUILD_CONFIGURATION argument for flexibility
# 4. Added non-root user (appuser) for security best practices
# 5. Changed ownership of /app directory to non-root user
# 6. Switched to non-root user before running the application
# 7. Used multi-line format for publish command for better readability
