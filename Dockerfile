# ── Build Stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Shopfinity.sln ./
COPY Shopfinity.Domain/Shopfinity.Domain.csproj Shopfinity.Domain/
COPY Shopfinity.Application/Shopfinity.Application.csproj Shopfinity.Application/
COPY Shopfinity.Infrastructure/Shopfinity.Infrastructure.csproj Shopfinity.Infrastructure/
COPY Shopfinity.API/Shopfinity.API.csproj Shopfinity.API/
COPY Shopfinity.Tests/Shopfinity.Tests.csproj Shopfinity.Tests/

RUN dotnet restore Shopfinity.sln

COPY . .

WORKDIR /src/Shopfinity.API
RUN dotnet publish -c Release -o /app/publish

# ── Runtime Stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .


ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Shopfinity.API.dll"]

