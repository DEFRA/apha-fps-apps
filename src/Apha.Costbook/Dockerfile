# ================================
# -------- Development Stage ----------
# ================================
ARG PARENT_VERSION=dotnet10.0
FROM defradigital/dotnetcore-development:$PARENT_VERSION AS development
# Re-declare ARG inside stage to use in LABEL
ARG PARENT_VERSION=dotnet10.0

LABEL uk.gov.defra.parent-image=defra-dotnetcore-development:${PARENT_VERSION}

WORKDIR /home/dotnet/src

COPY --chown=root:dotnet --chmod=640 src/ .
USER root
RUN find . -type d -exec chmod g+wx {} \;
USER dotnet

# Restore, build and publish the Web project
RUN dotnet restore ./Apha.Costbook/Apha.Costbook.Api/Apha.Costbook.Api.csproj
RUN dotnet publish ./Apha.Costbook/Apha.Costbook.Api -c Release -o /home/dotnet/out /p:UseAppHost=false

# ================================
# -------- Production Stage --------
# ================================
ARG PARENT_VERSION=dotnet10.0
FROM defradigital/dotnetcore:$PARENT_VERSION AS production
# Re-declare ARG inside stage to use in LABEL
ARG PARENT_VERSION=dotnet10.0
LABEL uk.gov.defra.parent-image=defra-dotnetcore:${PARENT_VERSION}

USER 0
RUN apk update && apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ARG PORT=8080
EXPOSE ${PORT}

WORKDIR /app

# Copy published output as root — no write permissions assigned to app user
COPY --from=development /home/dotnet/out/ ./

# Files: read only, Dirs: read+execute only — no write anywhere
RUN find . -type f -exec chmod 444 {} \; && \
    find . -type d -exec chmod 555 {} \;

USER app
# USER app
# WORKDIR /app
# COPY --from=development /home/dotnet/out/ ./

ENTRYPOINT ["dotnet", "Apha.Costbook.Api.dll"]


