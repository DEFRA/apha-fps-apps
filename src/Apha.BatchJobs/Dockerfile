FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BatchJobs.csproj ./
COPY Apha.BatchJobs.Worker/ ./Apha.BatchJobs.Worker/
COPY Apha.BatchJobs.Application/ ./Apha.BatchJobs.Application/
COPY Apha.BatchJobs.Domain/ ./Apha.BatchJobs.Domain/
COPY Apha.BatchJobs.Infrastructure/ ./Apha.BatchJobs.Infrastructure/

RUN dotnet publish BatchJobs.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "BatchJobs.dll"]