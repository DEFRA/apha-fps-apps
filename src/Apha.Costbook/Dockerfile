# ================================
# -------- Development Stage -----------
# ================================
ARG PARENT_VERSION=dotnet10.0
FROM defradigital/dotnetcore-development:$PARENT_VERSION AS development
# Re-declare ARG inside stage to use in LABEL
ARG PARENT_VERSION=dotnet10.0

LABEL uk.gov.defra.parent-image=defra-dotnetcore-development:${PARENT_VERSION}

WORKDIR /home/dotnet/src

# COPY --chown=dotnet:dotnet src/. .
# Copy with no write permissions at copy time (444 for all),
# then fix dir execute bit — SonarCloud sees no write bits at any layer
# sonar-disable-next-line dockerfile:S6504
# COPY --chown=dotnet:dotnet src/. .
COPY --chown=root:dotnet --chmod=640 src/ .
USER root
RUN chmod -R g+X .
# # Dirs: give group rwx (770 style) so dotnet can write obj/bin during build
# RUN chmod -R g+X . && \        
#     find . -type d -exec chmod g+w {} \;
USER dotnet

# Immediately restore directory traversal+write so dotnet restore can work
# RUN find . -type d -exec chmod 755 {} \;

# Restore, build and publish the Web project
RUN dotnet restore ./Apha.Costbook/Apha.Costbook.Api/Apha.Costbook.Api.csproj
RUN dotnet publish ./Apha.Costbook/Apha.Costbook.Api -c Release -o /home/dotnet/out /p:UseAppHost=false

# # Remove write permissions from copied source (files: 444, dirs: 555)
# RUN find . -type d -exec chmod 555 {} \; && \
#     find . -type f -exec chmod 444 {} \;

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
COPY --from=development /home/dotnet/out/ ./

ENTRYPOINT ["dotnet", "Apha.Costbook.Api.dll"]


