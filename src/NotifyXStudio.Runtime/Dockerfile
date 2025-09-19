# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/NotifyXStudio.Runtime/NotifyXStudio.Runtime.csproj", "src/NotifyXStudio.Runtime/"]
COPY ["src/NotifyXStudio.Core/NotifyXStudio.Core.csproj", "src/NotifyXStudio.Core/"]
COPY ["src/NotifyXStudio.Connectors/NotifyXStudio.Connectors.csproj", "src/NotifyXStudio.Connectors/"]

# Restore dependencies
RUN dotnet restore "src/NotifyXStudio.Runtime/NotifyXStudio.Runtime.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src/src/NotifyXStudio.Runtime"
RUN dotnet build "NotifyXStudio.Runtime.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "NotifyXStudio.Runtime.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Copy manifests and registry
COPY manifests/ /app/manifests/
COPY registry/ /app/registry/

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Expose port for health checks
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Start application
ENTRYPOINT ["dotnet", "NotifyXStudio.Runtime.dll"]