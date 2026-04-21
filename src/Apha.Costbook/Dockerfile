# ================================
# -------- Development Stage ---------
# ================================
ARG PARENT_VERSION=dotnet10.0
FROM defradigital/dotnetcore-development:$PARENT_VERSION AS development
# Re-declare ARG inside stage to use in LABEL
ARG PARENT_VERSION=dotnet10.0

LABEL uk.gov.defra.parent-image=defra-dotnetcore-development:${PARENT_VERSION}

WORKDIR /home/dotnet/src

COPY --chown=dotnet:dotnet --chmod=400 src/ .

# Restore execute bit on dirs only (555 = r-xr-xr-x, still no write)
RUN find . -type d -exec chmod 555 {} \; && \
    find . -type f -exec chmod 444 {} \;

# # Copy all source code into the image under /home/dotnet/src
# COPY --chmod=555 --chown=dotnet:dotnet src/. .
# RUN find . -type d -exec chmod 555 {} \; && \
#     find . -type f -exec chmod 444 {} \;
# # Remove write permissions from copied source (files: 444, dirs: 555)

# Restore, build and publish the API project
# Redirect intermediate output outside the read-only source tree
RUN dotnet restore ./Apha.Costbook/Apha.Costbook.Api/Apha.Costbook.Api.csproj \
    /p:BaseIntermediateOutputPath=/home/dotnet/obj/
RUN dotnet publish ./Apha.Costbook/Apha.Costbook.Api -c Release -o /home/dotnet/out \
    /p:UseAppHost=false \
    /p:BaseIntermediateOutputPath=/home/dotnet/obj/

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

USER app
WORKDIR /app
COPY --chmod=555 --from=development /home/dotnet/out/ ./

ENTRYPOINT ["dotnet", "Apha.Costbook.Api.dll"]


