# ================================
# -------- Development Stage --------
# ================================
ARG PARENT_VERSION=dotnet10.0
FROM defradigital/dotnetcore-development:$PARENT_VERSION AS development
# Re-declare ARG inside stage to use in LABEL
ARG PARENT_VERSION=dotnet10.0

LABEL uk.gov.defra.parent-image=defra-dotnetcore-development:${PARENT_VERSION}

WORKDIR /home/dotnet/src

# Copy all source code into the image under /home/dotnet/src
COPY --chown=dotnet:dotnet src/. .

# Remove write permissions from copied source (files: 444, dirs: 555)
RUN find . -type d -exec chmod 555 {} \; && \
    find . -type f -exec chmod 444 {} \;

# Restore, build and publish the Web project
# Redirect intermediate output outside the read-only source tree
RUN dotnet restore ./Apha.FPSApps/Apha.FPSApps.Web/Apha.FPSApps.Web.csproj \
    /p:BaseIntermediateOutputPath=/home/dotnet/obj/
RUN dotnet publish ./Apha.FPSApps/Apha.FPSApps.Web -c Release -o /home/dotnet/out \
    /p:UseAppHost=false \
    /p:BaseIntermediateOutputPath=/home/dotnet/obj/

# ================================
# -------- Production Stage ---------
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

USER app
WORKDIR /app
COPY --from=development /home/dotnet/out/ ./

ENTRYPOINT ["dotnet", "Apha.FPSApps.Web.dll"]


