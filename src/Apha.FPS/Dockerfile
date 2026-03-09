# -------- Set base image version --------
ARG PARENT_VERSION=dotnet8.0

# ================================
# -------- Development Stage --------
# ================================
FROM defradigital/dotnetcore-development:$PARENT_VERSION AS development
ARG PARENT_VERSION=dotnet8.0

LABEL uk.gov.defra.parent-image=defra-dotnetcore-development:${PARENT_VERSION}
WORKDIR /home/dotnet/src


