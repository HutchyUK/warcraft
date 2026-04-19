# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Warcraft.Api/Warcraft.Api.csproj Warcraft.Api/
RUN dotnet restore Warcraft.Api/Warcraft.Api.csproj

COPY Warcraft.Api/ Warcraft.Api/
RUN dotnet publish Warcraft.Api/Warcraft.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN useradd -r -s /bin/false appuser && chown -R appuser /app
USER appuser

COPY --from=build --chown=appuser /app/publish .

# Railway and docker-compose both expect port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Warcraft.Api.dll"]
