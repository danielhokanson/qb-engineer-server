FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY qb-engineer.api/qb-engineer.api.csproj qb-engineer.api/
COPY qb-engineer.core/qb-engineer.core.csproj qb-engineer.core/
COPY qb-engineer.data/qb-engineer.data.csproj qb-engineer.data/
COPY qb-engineer.integrations/qb-engineer.integrations.csproj qb-engineer.integrations/
RUN dotnet restore qb-engineer.api/qb-engineer.api.csproj

# Copy everything and build
COPY . .
RUN dotnet publish qb-engineer.api/qb-engineer.api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "qb-engineer.api.dll"]
