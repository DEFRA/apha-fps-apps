FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BatchJobs.csproj ./
COPY Apha.BatchJobs.Worker/ ./Apha.BatchJobs.Worker/
COPY Apha.BatchJobs.Application/ ./Apha.BatchJobs.Application/
COPY Apha.BatchJobs.Domain/ ./Apha.BatchJobs.Domain/
COPY Apha.BatchJobs.Infrastructure/ ./Apha.BatchJobs.Infrastructure/
COPY appsettings.json ./
# appsettings.Development.json is intentionally NOT copied — production secrets
# are supplied via ECS task environment variables / Secrets Manager at runtime.

RUN dotnet publish Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published application (does not contain appsettings.Development.json)
COPY --from=build /app/publish ./

# Base configuration only — no Development file in production image
COPY appsettings.json ./

# Runtime environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_GCConserveMemory=1

# Run as non-root user
RUN useradd -m -u 10001 batchjobs && chown -R batchjobs:batchjobs /app
USER batchjobs

LABEL maintainer="Apha Engineering"
LABEL managed="true"
LABEL application="batch-jobs"
LABEL description="Containerized .NET 10 batch jobs worker for FPS application"

ENTRYPOINT ["dotnet", "Apha.BatchJobs.Worker.dll"]
CMD []