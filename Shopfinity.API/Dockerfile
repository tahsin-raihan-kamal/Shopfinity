FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY Shopfinity.sln .
COPY Shopfinity.Domain/Shopfinity.Domain.csproj           Shopfinity.Domain/
COPY Shopfinity.Application/Shopfinity.Application.csproj Shopfinity.Application/
COPY Shopfinity.Infrastructure/Shopfinity.Infrastructure.csproj Shopfinity.Infrastructure/
COPY Shopfinity.API/Shopfinity.API.csproj                 Shopfinity.API/

RUN dotnet restore Shopfinity.API/Shopfinity.API.csproj

# Copy the rest of the source
COPY Shopfinity.Domain/       Shopfinity.Domain/
COPY Shopfinity.Application/  Shopfinity.Application/
COPY Shopfinity.Infrastructure/ Shopfinity.Infrastructure/
COPY Shopfinity.API/          Shopfinity.API/

WORKDIR /src/Shopfinity.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Copy seed data if present
COPY Shopfinity.API/SeedData/ ./SeedData/

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5049

EXPOSE 5049

ENTRYPOINT ["dotnet", "Shopfinity.API.dll"]
