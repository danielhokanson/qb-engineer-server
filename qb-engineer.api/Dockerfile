FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy csproj files and restore (cached layer)
COPY qb-engineer.api/qb-engineer.api.csproj qb-engineer.api/
COPY qb-engineer.core/qb-engineer.core.csproj qb-engineer.core/
COPY qb-engineer.data/qb-engineer.data.csproj qb-engineer.data/
COPY qb-engineer.integrations/qb-engineer.integrations.csproj qb-engineer.integrations/
RUN dotnet restore qb-engineer.api/qb-engineer.api.csproj

# Copy source and publish
COPY . .
RUN dotnet publish qb-engineer.api/qb-engineer.api.csproj -c Release -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
WORKDIR /app
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

USER appuser
EXPOSE 8080
ENTRYPOINT ["dotnet", "qb-engineer.api.dll"]
