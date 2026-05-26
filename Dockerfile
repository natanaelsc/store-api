# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Store.API/Store.API.csproj", "src/Store.API/"]
COPY ["src/Store.Application/Store.Application.csproj", "src/Store.Application/"]
COPY ["src/Store.Domain/Store.Domain.csproj", "src/Store.Domain/"]
COPY ["src/Store.Infrastructure/Store.Infrastructure.csproj", "src/Store.Infrastructure/"]

RUN dotnet restore "src/Store.API/Store.API.csproj"

COPY . .

WORKDIR "/src/src/Store.API"
RUN dotnet build "Store.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Store.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Store.API.dll"]
