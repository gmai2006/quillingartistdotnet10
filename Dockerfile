# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Cache layer: copy csproj and restore dependencies first
COPY src/ApiService/*.csproj src/ApiService/
RUN dotnet restore src/ApiService/ApiService.csproj

# Copy remaining source code and publish release build
COPY src/ApiService/ src/ApiService/
WORKDIR /source/src/ApiService
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
# Minimal ASP.NET Core Linux runtime image (chiseled/non-root).
# The "-extra" variant ships tzdata + ICU, needed for the America/New_York and
# America/Los_Angeles time zones used by NightlyJob and date filters.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
WORKDIR /app
COPY --from=build /app/publish .

# Port 8003 matches the Spring Boot container that ui/nginx.conf proxies /api to.
# Use the unprefixed URLS variable, not ASPNETCORE_URLS: appsettings.json sets
# "Urls": "http://0.0.0.0:9000", and appsettings outranks ASPNETCORE_-prefixed
# variables, so ASPNETCORE_URLS would be ignored and Kestrel would listen on 9000.
# Clearing ASPNETCORE_HTTP_PORTS (8080 in the base image) avoids an override warning.
EXPOSE 8003
ENV URLS=http://+:8003 \
    ASPNETCORE_HTTP_PORTS=

# ── Runtime configuration via environment variables ───────────────────────────
#   ConnectionStrings__Default  Host=<host>;Port=5432;Database=app_db;Username=...;Password=...;Search Path=quillingartist
#   Stripe__SecretKey           Stripe secret key
#   Email__Brevo__ApiKey        Brevo API key
#   Jwt__SigningKey             HS256 secret for bearer tokens (>= 32 bytes; startup fails without it)
# ──────────────────────────────────────────────────────────────────────────────

ENTRYPOINT ["dotnet", "ApiService.dll"]
