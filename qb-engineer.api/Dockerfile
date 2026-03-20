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

# Debian-based runtime (Chromium requires glibc, not available on Alpine)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Install Chromium for PuppeteerSharp pdf.js extraction
RUN apt-get update && apt-get install -y --no-install-recommends \
    chromium \
    fonts-liberation \
    libnss3 \
    libatk-bridge2.0-0 \
    libx11-xcb1 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    libgbm1 \
    libasound2 \
    libpangocairo-1.0-0 \
    libgtk-3-0 \
    && rm -rf /var/lib/apt/lists/*

ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
WORKDIR /app
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

USER appuser
EXPOSE 8080
ENTRYPOINT ["dotnet", "qb-engineer.api.dll"]
