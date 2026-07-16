# syntax=docker/dockerfile:1.7

# --- Build stage ------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first so NuGet restore is cached per-layer.
COPY ProjectRelay.sln Directory.Build.props global.json ./
COPY src/ src/
COPY tests/ tests/

RUN dotnet restore ProjectRelay.sln

RUN dotnet publish src/Host/Relay.Api/Relay.Api.csproj \
        --configuration Release \
        --no-restore \
        --output /app/publish

# --- Runtime stage ----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user for the app to run as.
RUN groupadd --system relay && useradd --system --gid relay --create-home relay
USER relay

COPY --from=build --chown=relay:relay /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Relay.Api.dll"]
